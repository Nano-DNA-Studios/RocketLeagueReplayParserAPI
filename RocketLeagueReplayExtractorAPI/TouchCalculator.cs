using DNARocketLeagueReplayParser.ReplayStructure.UnrealEngineObjects;

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
        /// The List of Ball Positions in the Replay File
        /// </summary>
        public List<GameObjectState> BallPosition { get; set; }

        /// <summary>
        /// The List of Car Positions in the Replay File
        /// </summary>
        public Dictionary<string, List<GameObjectState>> CarPositions { get; set; }

        /// <summary>
        /// Refromated Car Positions by Frame Number
        /// </summary>
        private Dictionary<int, List<GameObjectState>> _carPositionsByFrame { get; set; }

        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="ballPosition"> The Ball Positions </param>
        /// <param name="carPositions"> The Car Positions </param>
        public TouchCalculator(List<GameObjectState> ballPosition, Dictionary<string, List<GameObjectState>> carPositions)
        {
            BallPosition = ballPosition;
            CarPositions = carPositions;

            SetCarPositionsByFrameDictionary();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the Dictionary tha reformats Car Positions by Frame Number
        /// </summary>
        private void SetCarPositionsByFrameDictionary()
        {
            Dictionary<int, List<GameObjectState>> carPositionsByFrame = new Dictionary<int, List<GameObjectState>>();

            foreach (KeyValuePair<string, List<GameObjectState>> carPosition in CarPositions)
            {
                foreach (GameObjectState gameObjectState in carPosition.Value)
                {
                    if (!carPositionsByFrame.ContainsKey(gameObjectState.FrameNumber))
                        carPositionsByFrame.Add(gameObjectState.FrameNumber, new List<GameObjectState>());

                    carPositionsByFrame[gameObjectState.FrameNumber].Add(gameObjectState);
                }
            }

            _carPositionsByFrame = carPositionsByFrame;
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
            List<double> speeds = BallPosition.Skip(index).Take(ACCELERATION_FRAMES).Select(ball => ball.RigidBody.GetVelocityKMH()).ToList();
            List<double> accelerations = speeds.Skip(1).Zip(speeds, (first, second) => first - second).ToList();

            return Math.Abs(accelerations.DefaultIfEmpty(0).Max());
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all the Ball Touches that occur in the Replay File
        /// </summary>
        /// <returns> A List of all the Ball Touches that Occur in the Replay </returns>
        public List<BallTouch> GetBallTouches()
        {
            List<BallTouch> ballTouches = new List<BallTouch>();

            double lastMaxAcceleration = 0;
            for (int i = 0; i < BallPosition.Count; i++)
            {
                GameObjectState ballState = BallPosition[i];
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
                    Distance = minDistance
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

            return ballTouches;
        }

        #endregion
    }
}
