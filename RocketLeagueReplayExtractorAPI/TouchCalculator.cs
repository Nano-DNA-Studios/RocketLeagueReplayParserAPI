using DNARocketLeagueReplayParser.ReplayStructure.UnrealEngineObjects;
using System.Runtime.CompilerServices;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// A Class that Calculates all the Ball Touches that occur in the Replay File
    /// </summary>
    public class TouchCalculator
    {
        #region Constants
        /// <summary>
        /// The Minimym Distance a Car can be from the Ball to be considered a Touch
        /// </summary>
        private const double MINIMUM_DISTANCE_FROM_BALL = 2.5;

        /// <summary>
        /// The Minimum Acceleration the Ball can have to be considered a Touch
        /// </summary>
        private const double MINIMUM_ACCELERATION = 1;

        /// <summary>
        /// The Number of Surrounding Frames to Check for Car States
        /// </summary>
        private const int SURROUNDING_FRAMES = 2;

        /// <summary>
        /// The Number of Frames to Check for Acceleration
        /// </summary>
        private const int ACCELERATION_FRAMES = 4;

        #endregion

        #region Properties

        /// <summary>
        /// The List of the States of the Ball over the Course of the Match, Ordered by Frame Number
        /// </summary>
        private List<GameObjectState> _ballPositions { get; set; }

        /// <summary>
        /// The Length of the Match in Seconds
        /// </summary>
        private float _matchLength { get; set; }

        /// <summary>
        /// Dictionary of the list of Car Positions by Player Name
        /// </summary>
        private Dictionary<string, List<GameObjectState>> _carPositions { get; set; }

        /// <summary>
        /// Reference to the Match Roster
        /// </summary>
        private PlayerInfo[] _players { get; set; }

        /// <summary>
        /// Mapping of the Actor ID to the Player Name
        /// </summary>
        private Dictionary<uint, string> _actorIDToName { get; set; }

        /// <summary>
        /// Refromated Car Positions by Frame Number
        /// </summary>
        private Dictionary<int, List<GameObjectState>> _carPositionsByFrame { get; set; }

        /// <summary>
        /// Dictionary mapping the Player to all it's Ball Touches in the Replay
        /// </summary>
        private Dictionary<string, List<BallTouch>> _playerTouchDictionary { get; set; }

        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="replay"> The Replay to Analyze </param>
        public TouchCalculator(Replay replay)
        {
            _carPositions = replay.CarPositions;
            _ballPositions = replay.BallPositions;
            _matchLength = replay.MatchLength;
            _players = replay.MatchRoster.GetAllPlayers().ToArray();
            _actorIDToName = replay.MatchRoster.ActorIDToName;
            _carPositionsByFrame = GetCarPositionsByFrameDictionary();
            _playerTouchDictionary = GetPlayerTouchDictionary();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the Dictionary tha reformats Car Positions by Frame Number
        /// </summary>
        private Dictionary<int, List<GameObjectState>> GetCarPositionsByFrameDictionary()
        {
            Dictionary<int, List<GameObjectState>> carPositionsByFrame = new Dictionary<int, List<GameObjectState>>();

            foreach (KeyValuePair<string, List<GameObjectState>> carPosition in _carPositions)
            {
                foreach (GameObjectState gameObjectState in carPosition.Value)
                {
                    if (!carPositionsByFrame.ContainsKey(gameObjectState.FrameNumber))
                        carPositionsByFrame.Add(gameObjectState.FrameNumber, new List<GameObjectState>());

                    carPositionsByFrame[gameObjectState.FrameNumber].Add(gameObjectState);
                }
            }

            return carPositionsByFrame;
        }

        /// <summary>
        /// Gets the Car States that are Closest in the Surrounds Frames of the current Ball State
        /// </summary>
        /// <param name="frameNumber"> The Frame Number of the Ball State </param>
        /// <returns> A List of all the States of the Cars in the Surrounding Frames </returns>
        private List<GameObjectState> GetCarStates(int frameNumber)
        {
            List<GameObjectState> gameObjects = new List<GameObjectState>();

            for (int i = -SURROUNDING_FRAMES; i <= SURROUNDING_FRAMES; i++)
            {
                int targetFrame = frameNumber + i;
                if (targetFrame < 0)
                    continue;

                if (_carPositionsByFrame.ContainsKey(targetFrame))
                    gameObjects.AddRange(_carPositionsByFrame[targetFrame]);
            }

            return gameObjects;
        }

        /// <summary>
        /// Gets the Closest Car and it's Distance to the Ball
        /// </summary>
        /// <param name="ballState"> The Ball State to get the Closest Car to </param>
        /// <returns> A Tuple with the closest Distance to the Ball and the Closest Cars State </returns>
        private (double minDistance, GameObjectState closestCar) GetClosestCar(GameObjectState ballState)
        {
            List<GameObjectState> closestCars = GetCarStates(ballState.FrameNumber);
            GameObjectState closestCar = closestCars[0];
            double minDistance = double.MaxValue;

            foreach (GameObjectState carState in closestCars)
            {
                double distance = (ballState.RigidBody.Position - carState.RigidBody.Position).Magnitude / UnrealEngineToMetric.METER_TO_UNREAL_UNITS;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCar = carState;
                }
            }

            return (minDistance, closestCar);
        }

        /// <summary>
        /// Gets the Max Acceleration of the Ball in the following Frames
        /// </summary>
        /// <param name="index"> The Index of the Ball State to Start at </param>
        /// <returns> The Maximum Acceleration Values in the next few Frames </returns>
        private double GetMaxAcceleration(int index)
        {
            List<double> speeds = _ballPositions.Skip(index).Take(ACCELERATION_FRAMES).Select(ball => ball.RigidBody.GetVelocityKMH()).ToList();
            List<double> accelerations = speeds.Skip(1).Zip(speeds, (first, second) => first - second).ToList();

            return Math.Abs(accelerations.DefaultIfEmpty(0).Max());
        }

        /// <summary>
        /// Gets all the Ball Touches that occur in the Replay File
        /// </summary>
        /// <returns> A List of all the Ball Touches that Occur in the Replay </returns>
        private List<BallTouch> GetBallTouches()
        {
            List<BallTouch> ballTouches = new List<BallTouch>();

            double lastMaxAcceleration = 0;
            for (int i = 0; i < _ballPositions.Count; i++)
            {
                GameObjectState ballState = _ballPositions[i];
                (double minDistance, GameObjectState closestCar) = GetClosestCar(ballState);
                double maxAcceleration = GetMaxAcceleration(i);

                if (minDistance > MINIMUM_DISTANCE_FROM_BALL)
                    continue;

                if (maxAcceleration < MINIMUM_ACCELERATION)
                    continue;

                BallTouch ballTouch = new BallTouch()
                {
                    FrameNumber = ballState.FrameNumber,
                    Time = ballState.Time,
                    ActorID = closestCar.ActorID,
                    RigidBody = ballState.RigidBody,
                    Distance = minDistance,
                };

                if (ballTouches.Count > 0 && lastMaxAcceleration == maxAcceleration)
                {
                    if (minDistance > ballTouches.Last().Distance)
                        continue;

                    ballTouches.RemoveAt(ballTouches.Count - 1);
                    ballTouches.Add(ballTouch);
                }
                else
                    ballTouches.Add(ballTouch);

                lastMaxAcceleration = maxAcceleration;
            }

            for (int i = 0; i < ballTouches.Count - 2; i++)
                ballTouches[i].TimeUntilNextTouch = ballTouches[i + 1].Time - ballTouches[i].Time;

            ballTouches[ballTouches.Count - 1].TimeUntilNextTouch = _matchLength - ballTouches[ballTouches.Count - 1].Time;

            return ballTouches;
        }

        /// <summary>
        /// Calculates and Returns a Dictionary mapping the Player to all it's Ball Touches in the Replay
        /// </summary>
        /// <returns> Dictionary mapping a Players Name to it's associated Ball Touches </returns>
        private Dictionary<string, List<BallTouch>> GetPlayerTouchDictionary()
        {
            Dictionary<string, List<BallTouch>> playerTouchDictionary = new Dictionary<string, List<BallTouch>>();

            foreach (BallTouch ballTouch in GetBallTouches())
            {
                PlayerInfo? player = _players.FirstOrDefault(player => player.PlayerName == _actorIDToName[ballTouch.ActorID]);

                if (player == null)
                    continue;

                if (!playerTouchDictionary.ContainsKey(player.PlayerName))
                    playerTouchDictionary.Add(player.PlayerName, new List<BallTouch>());

                playerTouchDictionary[player.PlayerName].Add(ballTouch);
            }

            return playerTouchDictionary;
        }

        /// <summary>
        /// Sets the Player Stats for the Team
        /// </summary>
        /// <param name="teamID"> The ID of the Team </param>
        private void SetPlayerStats(int teamID)
        {
            (int teamTouches, float teamPossessionTime) = GetTeamStats(teamID);

            foreach (string playerName in _playerTouchDictionary.Keys)
            {
                PlayerInfo? player = _players.FirstOrDefault(player => player.PlayerName == playerName);

                if (player == null)
                    continue;

                if (player.Team == teamID)
                    player.SetBallTouches(_playerTouchDictionary[playerName], teamTouches, teamPossessionTime);
            }
        }

        /// <summary>
        /// Gets the Team Touch and Possession Time Stats
        /// </summary>
        /// <param name="teamID"> The ID of the Team to get the Stats from </param>
        /// <returns> The Number of Touches the Team got and the Total Possession Time in Seconds </returns>
        private (int teamTouches, float teamPossessionTime) GetTeamStats(int teamID)
        {
            int teamTouches = 0;
            float teamPossessionTime = 0;
            foreach (string playerName in _playerTouchDictionary.Keys)
            {
                PlayerInfo? player = _players.FirstOrDefault(player => player.PlayerName == playerName);

                if (player == null)
                    continue;

                if (player.Team == teamID)
                {
                    teamTouches += _playerTouchDictionary[playerName].Count;
                    teamPossessionTime += _playerTouchDictionary[playerName].Sum(touch => touch.TimeUntilNextTouch);
                }
            }

            return (teamTouches, teamPossessionTime);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the Ball Touch Player Stats for both Teams
        /// </summary>
        public void SetBallTouchStats()
        {
            SetPlayerStats(GameProperties.BlueTeamID);
            SetPlayerStats(GameProperties.OrangeTeamID);
        }

        /// <summary>
        /// Sets the Ball Touch Stats for each Player in the Replay
        /// </summary>
        /// <param name="replay"> The Replay to analyze </param>
        public static void SetBallTouchStats(Replay replay)
        {
            TouchCalculator calculator = new TouchCalculator(replay);

            calculator.SetBallTouchStats();
        }

        #endregion
    }
}
