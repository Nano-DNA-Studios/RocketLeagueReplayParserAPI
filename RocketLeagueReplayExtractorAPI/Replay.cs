﻿using DNARocketLeagueReplayParser.ReplayStructure;
using DNARocketLeagueReplayParser.ReplayStructure.Actors;
using DNARocketLeagueReplayParser.ReplayStructure.Frames;
using DNARocketLeagueReplayParser.ReplayStructure.Mapping;
using DNARocketLeagueReplayParser.ReplayStructure.UnrealEngineObjects;
using Newtonsoft.Json;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Represents a Rocket League Replay Object
    /// </summary>
    public class Replay
    {
        #region Constants

        /// <summary>
        /// The Blue Team ID
        /// </summary>
        public const int BLUE_TEAM = 0;

        /// <summary>
        /// The Orange Team ID
        /// </summary>
        public const int ORANGE_TEAM = 1;

        /// <summary>
        /// Zero Goals Scored
        /// </summary>
        public const int ZERO_GOALS = 0;

        /// <summary>
        /// Title given to Unnamed Replays
        /// </summary>
        public const string UNNAMED_REPLAY = "Unnamed";

        /// <summary>
        /// The Car Object Class Name
        /// </summary>
        private const string CAR = "TAGame.Car_TA";

        /// <summary>
        /// The Ball Object Class Name
        /// </summary>
        private const string BALL = "TAGame.Ball_TA";

        /// <summary>
        /// The Player Name Property
        /// </summary>
        private const string PLAYER_NAME = "Engine.PlayerReplicationInfo:PlayerName";

        /// <summary>
        /// The Player Replication Info Property
        /// </summary>
        private const string PLAYER_REPLICATION_INFO = "Engine.Pawn:PlayerReplicationInfo";

        /// <summary>
        /// The Vehicle Property
        /// </summary>
        private const string VEHICLE = "TAGame.CarComponent_TA:Vehicle";

        /// <summary>
        /// The Ball Hit Property
        /// </summary>
        private const string BALL_HIT = "TAGame.Ball_TA:HitTeamNum"; //Or TAGame.GameEvent_Soccar_TA:bBallHasBeenHit

        /// <summary>
        /// The Rigid Body Property ID
        /// </summary>
        private const int RIGID_BODY = 42;

        /// <summary>
        /// The Player Stats Property Name
        /// </summary>
        private const string PLAYER_STATS = "PlayerStats";

        /// <summary>
        /// The Replay Name Property Name
        /// </summary>
        private const string REPLAY_NAME = "ReplayName";

        /// <summary>
        /// The Record FPS Property Name
        /// </summary>
        private const string RECORD_FPS = "RecordFPS";

        /// <summary>
        /// The Key for the Blue Team Score
        /// </summary>
        private const string BLUE_TEAM_SCORE = "Team0Score";

        /// <summary>
        /// The Key for the Orange Team Score
        /// </summary>
        private const string ORANGE_TEAM_SCORE = "Team1Score";

        #endregion

        /// <summary>
        /// The Recording FPS of the Replay
        /// </summary>
        public float RecordFPS => TryGetProperty<float>(RECORD_FPS, 30);

        /// <summary>
        /// The Length of the Match in Seconds
        /// </summary>
        public float MatchLength { get; private set; }

        /// <summary>
        /// The Replay Info Object
        /// </summary>
        private static PsyonixReplay _replayInfo;

        /// <summary>
        /// The Path to the File Being Parsed
        /// </summary>
        private string _pathToFile;

        /// <summary>
        /// Array of PlayerInfo Objects
        /// </summary>
        //public PlayerInfo[] Players { get; private set; }

        /// <summary>
        /// The Blue Teams Number of Goals At the end of the replay
        /// </summary>
        public int BlueTeamGoals => TryGetProperty(BLUE_TEAM_SCORE, ZERO_GOALS);

        /// <summary>
        /// The Orange Teams Number of Goals At the end of the replay
        /// </summary>
        public int OrangeTeamGoals => TryGetProperty(ORANGE_TEAM_SCORE, ZERO_GOALS);

        /// <summary>
        /// The Name of the Replay set by the Player When Saved
        /// </summary>
        public string ReplayName => TryGetProperty(REPLAY_NAME, UNNAMED_REPLAY);

        /// <summary>
        /// Mapping of Actor ID to Player Name
        /// </summary>
        public Dictionary<uint, string> ActorIDToName { get; private set; }

        /// <summary>
        /// List of all the Balls State in the Replay
        /// </summary>
        public List<GameObjectState> BallPosition { get; private set; }

        /// <summary>
        /// Dictionary of all the Car States in the Replay
        /// </summary>
        public Dictionary<string, List<GameObjectState>> CarPositions { get; private set; }


        public Roster MatchRoster { get; private set; }

        /// <summary>
        /// Initializes the Replay Object from the given Path
        /// </summary>
        /// <param name="path"> The Path to the Replay File </param>
        public Replay(string path)
        {
            _pathToFile = path;
            using (FileStream stream = File.Open(path, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
                _replayInfo = PsyonixReplay.Deserialize(reader);

            MatchRoster = new Roster(_replayInfo);
            ActorIDToName = GetActorToPlayerMap();

            foreach (uint key in ActorIDToName.Keys)
                Console.WriteLine(key + " " + ActorIDToName[key]);

            ExtractRigidBodies();
            CalculateBallTouches();
        }

        /// <summary>
        /// Tries to the Get the Property from the Replay Info Object <paramref name="defaultValue"/> if it is not found
        /// </summary>
        /// <typeparam name="T"> The Return Type of the Object </typeparam>
        /// <param name="key"> The Key for the Value </param>
        /// <param name="defaultValue"> The Default Value to Return if value is not Found </param>
        /// <returns> The Value of the Property being searched, or <paramref name="defaultValue"/> if not found </returns>
        private T TryGetProperty<T>(string key, T defaultValue)
        {
            if (_replayInfo.Properties.TryGetValue(key, out Property value))
                return (T)value.Value;
            else
                return defaultValue;
        }

        /// <summary>
        /// Extracts the Players from the Replay Info Object and Formats them into PlayerInfo Objects
        /// </summary>
        private PlayerInfo[] ExtractPlayers()
        {
            ArrayProperty playerStatsArray = (ArrayProperty)_replayInfo.Properties[PLAYER_STATS];
            List<PropertyDictionary> playerStats = (List<PropertyDictionary>)playerStatsArray.Value;
            List<PlayerInfo> players = new List<PlayerInfo>();

            foreach (PropertyDictionary playerStat in playerStats)
                players.Add(new PlayerInfo(playerStat));

            return players.ToArray();
        }

        /*/// <summary>
        /// Gets the Teams Stat Value 
        /// </summary>
        /// <param name="blueTeam"> Flag determining if the Total stats should be from the Blue Team or not </param>
        /// <param name="stat"> The Game Stat of Interest </param>
        /// <returns> The Teams Stat Value </returns>
        public float GetTeamStat(bool blueTeam, GameStats stat)
        {
            int teamID = blueTeam ? BLUE_TEAM : ORANGE_TEAM;
            float statValue = 0;

            foreach (PlayerInfo player in Players)
            {
                if (player.Team == teamID)
                    statValue += player.GetStat(stat);
            }

            return statValue;
        }*/

        /// <summary>
        /// Renames the Replay and Saves it to the given Path
        /// </summary>
        /// <param name="renamedName"> The new Name of the File, if left empty it defaults to the Name of the Replay </param>
        /// <param name="savePath"> The New Save Path of the Replay, if left Empty it saves in the Same Directory </param>
        /// <exception cref="DirectoryNotFoundException"> Error thrown if the new Directory doesn't exist </exception>
        public void RenameAndSave(string? renamedName = null, string? savePath = null)
        {
            if (string.IsNullOrEmpty(savePath))
                savePath = Path.GetDirectoryName(_pathToFile);

            if (string.IsNullOrEmpty(renamedName))
                renamedName = ReplayName;

            if (Directory.Exists(savePath) == false)
                throw new DirectoryNotFoundException("The Directory to Save the Replay to does not exist");

            string newPath = Path.Combine(savePath, renamedName + ".replay");
            File.WriteAllBytes(newPath, File.ReadAllBytes(_pathToFile));
        }

        /// <summary>
        /// Gets the Replay Files Name on the Device
        /// </summary>
        /// <returns> The File Name stored on the Device </returns>
        public string GetReplayFileName()
        {
            return Path.GetFileName(_pathToFile);
        }

        /*/// <summary>
        /// Gets the Team Scoreboard Array for the given Team
        /// </summary>
        /// <param name="isBlueTeam"> Flag indicating if the Team is Blue or not </param>
        /// <returns> An Array with the Teams cumulative stats formmated like the In Game Player Scoreboard </returns>
        public string[] GetTeamScoreboard(bool isBlueTeam)
        {
            return [GetTeamStat(isBlueTeam, GameStats.Score).ToString(), GetTeamStat(isBlueTeam, GameStats.Goals).ToString(), GetTeamStat(isBlueTeam, GameStats.Assists).ToString(), GetTeamStat(isBlueTeam, GameStats.Saves).ToString(), GetTeamStat(isBlueTeam, GameStats.Shots).ToString(), GetTeamStat(isBlueTeam, GameStats.BallTouches).ToString(), GetTeamStat(isBlueTeam, GameStats.BallTouchPossession).ToString(), GetTeamStat(isBlueTeam, GameStats.BallPossessionTime).ToString(), GetTeamStat(isBlueTeam, GameStats.BallPossessionTimePercentage).ToString()];
        }*/

        /// <summary>
        /// Saves the Psyonix Replay Object as a JSON File
        /// </summary>
        /// <param name="name"> The Name of the JSON File, if left empty it defaults to the Replay Name </param>
        /// <param name="savePath"> The Save Path for the JSON File </param>
        public void SavePsyonixReplayAsJSON(string? name = null, string? savePath = null)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
            };

            string fullFilePath;

            if (string.IsNullOrEmpty(savePath))
                fullFilePath = Path.GetDirectoryName(_pathToFile);
            else
                fullFilePath = Path.GetFullPath(savePath);

            if (!Directory.Exists(fullFilePath))
                throw new DirectoryNotFoundException("The Directory does not exist");

            if (name == null)
                fullFilePath = Path.Combine(fullFilePath, $"{ReplayName}.json");
            else
                fullFilePath = Path.Combine(fullFilePath, $"{name}.json");

            string json = JsonConvert.SerializeObject(_replayInfo, settings);
            File.WriteAllText(fullFilePath, json);
        }

        /// <summary>
        /// Saves the Current <see cref="Replay"/> Object as a JSON File
        /// </summary>
        /// <param name="name"> The Name of the File </param>
        /// <param name="savePath"> The Path to Save the <see cref="Replay"/> Object </param>
        /// <exception cref="DirectoryNotFoundException"> Error thrown if the Directory doesn't exist </exception>
        public void SaveReplayAsJSON(string? name = null, string? savePath = null)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
            };

            string fullFilePath;

            if (string.IsNullOrEmpty(savePath))
                fullFilePath = Path.GetDirectoryName(_pathToFile);
            else
                fullFilePath = Path.GetFullPath(savePath);

            if (!Directory.Exists(fullFilePath))
                throw new DirectoryNotFoundException("The Directory does not exist");

            if (name == null)
                fullFilePath = Path.Combine(fullFilePath, $"{ReplayName}.json");
            else
                fullFilePath = Path.Combine(fullFilePath, $"{name}.json");

            string json = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(fullFilePath, json);
        }

        /// <summary>
        /// Calculates the Ball Touches that Occur during the Replay and Assigns them to the Players 
        /// </summary>
        public void CalculateBallTouches()
        {
            TouchCalculator touchCalculator = new TouchCalculator(this);
            List<BallTouch> ballTouches = touchCalculator.GetBallTouches();
            Dictionary<string, List<BallTouch>> playerTouchDictionary = new Dictionary<string, List<BallTouch>>();

            foreach (BallTouch ballTouch in ballTouches)
            {
                PlayerInfo? player = MatchRoster.GetAllPlayers().FirstOrDefault(player => player.PlayerName == ActorIDToName[ballTouch.ActorID]);

                if (player == null)
                    continue;

                if (!playerTouchDictionary.ContainsKey(player.PlayerName))
                    playerTouchDictionary.Add(player.PlayerName, new List<BallTouch>());

                playerTouchDictionary[player.PlayerName].Add(ballTouch);
            }

            int blueTeamTouches = 0;
            int orangeTeamTouches = 0;
            float blueTeamPossessionTime = 0;
            float orangeTeamPossessionTime = 0;

            foreach (string playerName in playerTouchDictionary.Keys)
            {
                PlayerInfo? player = MatchRoster.GetAllPlayers().FirstOrDefault(player => player.PlayerName == playerName);

                if (player == null)
                    continue;

                if (player.Team == BLUE_TEAM)
                {
                    blueTeamTouches += playerTouchDictionary[playerName].Count;
                    blueTeamPossessionTime += playerTouchDictionary[playerName].Sum(touch => touch.TimeUntilNextTouch);
                }
                else
                {
                    orangeTeamTouches += playerTouchDictionary[playerName].Count;
                    orangeTeamPossessionTime += playerTouchDictionary[playerName].Sum(touch => touch.TimeUntilNextTouch);
                }

                player.SetBallTouches(playerTouchDictionary[playerName]);
            }

            int touchTotal = blueTeamTouches + orangeTeamTouches;
            float possessionTimeTotal = blueTeamPossessionTime + orangeTeamPossessionTime;

            foreach (string playerName in playerTouchDictionary.Keys)
            {
                PlayerInfo? player = MatchRoster.GetAllPlayers().FirstOrDefault(player => player.PlayerName == playerName);

                if (player == null)
                    continue;

                player.BallPossessionTime = playerTouchDictionary[playerName].Sum(touch => touch.TimeUntilNextTouch);

                if (player.Team == BLUE_TEAM)
                {
                    player.BallTouchPossessionPercentage = 100 * (float)playerTouchDictionary[playerName].Count / blueTeamTouches;
                    player.BallPossessionPercentage = 100 * player.BallPossessionTime / blueTeamPossessionTime;
                }
                else
                {
                    player.BallTouchPossessionPercentage = 100 * (float)playerTouchDictionary[playerName].Count / orangeTeamTouches;
                    player.BallPossessionPercentage = 100 * player.BallPossessionTime / orangeTeamPossessionTime;
                }
            }
        }

        /// <summary>
        /// Saves an Individual Frame of the Replay as a JSON File
        /// </summary>
        /// <param name="frameNumber"></param>
        /// <param name="frame"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private void SaveFrame(int frameNumber, PsyonixFrame frame)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
            };

            string fullFilePath;

            fullFilePath = Path.GetDirectoryName(_pathToFile);

            if (!Directory.Exists(fullFilePath))
                throw new DirectoryNotFoundException("The Directory does not exist");

            fullFilePath = Path.Combine(fullFilePath, $"{ReplayName}_Frame{frameNumber}.json");

            string json = JsonConvert.SerializeObject(frame, settings);
            File.WriteAllText(fullFilePath, json);
        }

        /// <summary>
        /// Extracts the Info necessary to map an Actor / GameObject to a Player Name
        /// </summary>
        /// <returns> A Dictionary Map of ActorID to Player Name </returns>
        private Dictionary<uint, string> GetActorToPlayerMap()
        {
            Dictionary<uint, string> ActorToPlayerNameMap = new Dictionary<uint, string>();

            Dictionary<uint, string> IDtoName = new Dictionary<uint, string>();

            Dictionary<uint, int> ActorIDtoNameID = new Dictionary<uint, int>();

            foreach (PsyonixFrame frame in _replayInfo.Frames)
            {
                foreach (ActorState actorState in frame.ActorStates)
                {
                    foreach (ActorStateProperty property in actorState.Properties.Values)
                    {
                        if (property.PropertyName == PLAYER_REPLICATION_INFO)
                        {
                            if (!ActorIDtoNameID.ContainsKey(actorState.Id))
                                ActorIDtoNameID.Add(actorState.Id, ((ActiveActor)property.Data).ActorId);
                        }

                        if (property.PropertyName == PLAYER_NAME)
                        {
                            if (!IDtoName.ContainsKey(actorState.Id))
                                IDtoName.Add(actorState.Id, (string)property.Data);
                        }
                    }
                }
            }

            foreach (uint actorID in ActorIDtoNameID.Keys)
                ActorToPlayerNameMap.Add(actorID, IDtoName[(uint)ActorIDtoNameID[actorID]]);

            return ActorToPlayerNameMap;
        }

        /// <summary>
        /// Extracts all the Rigid Bodies from the Replay Info Object
        /// </summary>
        private void ExtractRigidBodies()
        {
            List<GameObjectState> ballPosition = new List<GameObjectState>();
            Dictionary<string, List<GameObjectState>> carPositions = new Dictionary<string, List<GameObjectState>>();
            uint frameNumber = 0;

            foreach (PsyonixFrame frame in _replayInfo.Frames)
            {
                frameNumber++;
                foreach (ActorState actorState in frame.ActorStates)
                {
                    foreach (ActorStateProperty property in actorState.Properties.Values)
                    {
                        if (property.PropertyId == RIGID_BODY)
                        {
                            GameObjectState gameObjectState = new GameObjectState
                            {
                                RigidBody = (RigidBodyState)property.Data,
                                FrameNumber = (int)frameNumber,
                                Time = frame.Time,
                                ActorID = actorState.Id
                            };

                            if (_replayInfo.Objects[property.GetClassCache().ObjectIndex] == CAR)
                            {
                                if (carPositions.TryGetValue(ActorIDToName[actorState.Id], out List<GameObjectState> positions))
                                    positions.Add(gameObjectState);
                                else
                                {
                                    carPositions.Add(ActorIDToName[actorState.Id], new List<GameObjectState>());
                                    carPositions[ActorIDToName[actorState.Id]].Add(gameObjectState);
                                }
                            }

                            if (_replayInfo.Objects[property.GetClassCache().ObjectIndex] == BALL)
                                ballPosition.Add(gameObjectState);
                        }
                    }
                }

                if (frame.Time > MatchLength)
                    MatchLength = frame.Time;
            }

            BallPosition = ReplayInterpolator.InterpolateFrames(ballPosition);
            CarPositions = ReplayInterpolator.InterpolateAllFrames(carPositions);
        }
    }
}
