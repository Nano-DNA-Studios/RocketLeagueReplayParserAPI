
using DNARocketLeagueReplayParser.ReplayStructure;
using DNARocketLeagueReplayParser.ReplayStructure.Actors;
using DNARocketLeagueReplayParser.ReplayStructure.Frames;
using DNARocketLeagueReplayParser.ReplayStructure.Mapping;
using DNARocketLeagueReplayParser.ReplayStructure.UnrealEngineObjects;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using System.Xml.Linq;


namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Represents a Rocket League Replay Object
    /// </summary>
    public class Replay
    {
        public const int BLUE_TEAM = 0;
        public const int ORANGE_TEAM = 1;

        public const int ZERO_GOALS = 0;


        public const string UNNAMED_REPLAY = "Unnamed";

        //Constants
        private const string CAR = "TAGame.Car_TA";
        private const string BALL = "TAGame.Ball_TA";
        private const string PLAYER_NAME = "Engine.PlayerReplicationInfo:PlayerName";
        private const string PLAYER_REPLICATION_INFO = "Engine.Pawn:PlayerReplicationInfo";
        private const string VEHICLE = "TAGame.CarComponent_TA:Vehicle";
        private const string BALL_HIT = "TAGame.Ball_TA:HitTeamNum"; //Or TAGame.GameEvent_Soccar_TA:bBallHasBeenHit
        private const int RIGID_BODY = 42;
        private const string PLAYER_STATS = "PlayerStats";

        private const string REPLAY_NAME = "ReplayName";

        private const string RECORD_FPS = "RecordFPS";

        /// <summary>
        /// The Key for the Blue Team Score
        /// </summary>
        private const string BLUE_TEAM_SCORE = "Team0Score";

        /// <summary>
        /// The Key for the Orange Team Score
        /// </summary>
        private const string ORANGE_TEAM_SCORE = "Team1Score";

        /// <summary>
        /// The Recording FPS of the Replay
        /// </summary>
        public float RecordFPS => TryGetProperty<float>(RECORD_FPS, 30);

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
        public PlayerInfo[] Players { get; private set; }

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

        public Dictionary<uint, string> ActorIDToName;

        public List<GameObjectState> BallPosition = new List<GameObjectState>();

        private Dictionary<string, List<GameObjectState>> CarPositions = new Dictionary<string, List<GameObjectState>>();

        public List<BallHit> BallHits = new List<BallHit>();


        public struct BallHit
        {
            public float time;
            public int frameNumber;
            public int team;
        }


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

            Players = ExtractPlayers();
            ActorIDToName = GetActorToPlayerMap();

            ExtractRigidBodies();
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

        /// <summary>
        /// Gets the Teams Stat Value 
        /// </summary>
        /// <param name="blueTeam"> Flag determining if the Total stats should be from the Blue Team or not </param>
        /// <param name="stat"> The Game Stat of Interest </param>
        /// <returns> The Teams Stat Value </returns>
        public int GetTeamStat(bool blueTeam, GameStats stat)
        {
            int teamID = blueTeam ? BLUE_TEAM : ORANGE_TEAM;
            int statValue = 0;

            foreach (PlayerInfo player in Players)
            {
                if (player.Team == teamID)
                    statValue += player.GetStat(stat);
            }

            return statValue;
        }

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

        /// <summary>
        /// Gets the Team Scoreboard Array for the given Team
        /// </summary>
        /// <param name="isBlueTeam"> Flag indicating if the Team is Blue or not </param>
        /// <returns> An Array with the Teams cumulative stats formmated like the In Game Player Scoreboard </returns>
        public string[] GetTeamScoreboard(bool isBlueTeam)
        {
            return [GetTeamStat(isBlueTeam, GameStats.Score).ToString(), GetTeamStat(isBlueTeam, GameStats.Goals).ToString(), GetTeamStat(isBlueTeam, GameStats.Assists).ToString(), GetTeamStat(isBlueTeam, GameStats.Saves).ToString(), GetTeamStat(isBlueTeam, GameStats.Shots).ToString()];
        }

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



        public (int blueTouch, int orangeTouch) GetBallTouches()
        {
            int blueTeamTouches = 0;
            int orangeTeamTouches = 0;

            int count = 0;
            GameObjectState lastState = BallPosition[0];
            foreach (GameObjectState ballState in BallPosition)
            {
                if (count != 0)
                {
                    float xDistance = ballState.RigidBody.Position.X - lastState.RigidBody.Position.X;
                    float yDistance = ballState.RigidBody.Position.Y - lastState.RigidBody.Position.Y;
                    float zDistance = ballState.RigidBody.Position.Z - lastState.RigidBody.Position.Z;

                    float distance = (float)Math.Sqrt(xDistance * xDistance + yDistance * yDistance + zDistance * zDistance);

                    int frameDifference = ballState.FrameNumber - lastState.FrameNumber;

                    if (distance > 200)
                        Console.WriteLine($"Distance: {distance}  Frame Difference: {frameDifference}");
                }
                count++;
                lastState = ballState;
            }

            foreach (GameObjectState ballState in BallPosition)
            {
                foreach (GameObjectState carState in CarPositions["MyTyranosaur"])
                {
                    if (ballState.FrameNumber == carState.FrameNumber)
                    {
                        //Console.WriteLine($"Car Position: {carState.RigidBody.Position.ToString()}");
                        //Console.WriteLine($"Ball Position: {ballState.RigidBody.Position.ToString()}");



                        float xDistance = carState.RigidBody.Position.X - ballState.RigidBody.Position.X;
                        float yDistance = carState.RigidBody.Position.Y - ballState.RigidBody.Position.Y;
                        float zDistance = carState.RigidBody.Position.Z - ballState.RigidBody.Position.Z;

                        float distance = (float)Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2) + Math.Pow(zDistance, 2));

                        if (distance < 300)
                        {
                            //Console.WriteLine($"Distance: X : {xDistance}  Y : {yDistance}   Z : {zDistance}");
                            // Console.WriteLine($"Distance: {distance}");
                        }

                    }
                }
            }

            return (blueTeamTouches, orangeTeamTouches);
        }

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

            List<BallHit> ballHits = new List<BallHit>();


            uint frameNumber = 0;

            foreach (PsyonixFrame frame in _replayInfo.Frames)
            {
                frameNumber++;
                foreach (ActorState actorState in frame.ActorStates)
                {
                    foreach (ActorStateProperty property in actorState.Properties.Values)
                    {

                        if (property.PropertyName == BALL_HIT)
                        {
                            byte idk = (byte)property.Data;
                            int idk2 = idk;

                            ballHits.Add(new BallHit { time = frame.Time, frameNumber = (int)frameNumber, team = idk2 });

                            Console.WriteLine($"Ball Hit: {frameNumber}  {frame.Time} {idk2 == 0}");
                        }


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
            }

            BallPosition = ReplayInterpolator.InterpolateFrames(ballPosition);
            CarPositions = InterpolateCarPositions(carPositions);
            BallHits = ballHits;
        }

        /*private List<GameObjectState> InterpolateBallPositions(List<GameObjectState> compressedPositions)
        {
            List<GameObjectState> positions = new List<GameObjectState>();

            for (int i = 0; i < compressedPositions.Count - 1; i++)
            {
                //if one ahead has a delta larger than 0 we try to resolve it, otherwise we just add the frame

                //Add the first frame
                positions.Add(compressedPositions[i]);

                int frameDelta = compressedPositions[i + 1].FrameNumber - compressedPositions[i].FrameNumber;

                if (frameDelta != 1)
                {
                    //Interpolate

                    if (compressedPositions[i].RigidBody.LinearVelocity != null)
                    {
                        double speed = (compressedPositions[i].RigidBody.LinearVelocity.Magnitude / 10000) * 3.6;

                        if (speed >= 0.6)
                        {
                            //Use the same speed and positions
                            for (int j = 1; j < frameDelta; j++)
                            {
                                RigidBodyState rigidBody = (RigidBodyState)compressedPositions[i].RigidBody.Clone();
                                rigidBody.Position = ((Vector3D)rigidBody.Position) + (rigidBody.LinearVelocity * (j * (1f / 30f)));
                                ((Quaternion)rigidBody.Rotation).RotateByAngularVelocity(rigidBody.AngularVelocity, j * (1f / 30f)); //Idk kinda fishy

                                GameObjectState interpolatedState = new GameObjectState
                                {
                                    RigidBody = rigidBody,
                                    FrameNumber = compressedPositions[i].FrameNumber + j,
                                    Time = compressedPositions[i].Time + (j * (1f / 30f)),
                                    ActorID = compressedPositions[i].ActorID,
                                };

                                positions.Add(interpolatedState);
                            }
                        }
                        else
                        {
                            //Set speed to 0 and keep same position
                            for (int j = 1; j < frameDelta; j++)
                            {
                                RigidBodyState rigidBody = new RigidBodyState(true, compressedPositions[i].RigidBody.Position, compressedPositions[i].RigidBody.Rotation, new Vector3D(0, 0, 0), new Vector3D(0, 0, 0));

                                GameObjectState interpolatedState = new GameObjectState
                                {
                                    RigidBody = rigidBody,
                                    FrameNumber = compressedPositions[i].FrameNumber + j,
                                    Time = compressedPositions[i].Time + (j * (1f / 30f)),
                                    ActorID = compressedPositions[i].ActorID,
                                };

                                positions.Add(interpolatedState);
                            }
                        }
                    }
                    else
                    {
                        //Set speed to 0 and keep same position
                        for (int j = 1; j < frameDelta; j++)
                        {
                            RigidBodyState rigidBody = new RigidBodyState(true, compressedPositions[i].RigidBody.Position, compressedPositions[i].RigidBody.Rotation, new Vector3D(0, 0, 0), new Vector3D(0, 0, 0));

                            GameObjectState interpolatedState = new GameObjectState
                            {
                                RigidBody = rigidBody,
                                FrameNumber = compressedPositions[i].FrameNumber + j,
                                Time = compressedPositions[i].Time + (j * (1f / 30f)),
                                ActorID = compressedPositions[i].ActorID,
                            };

                            positions.Add(interpolatedState);
                        }
                    }
                }
            }

            positions.Add(compressedPositions[compressedPositions.Count - 1]);

            return positions;

        }*/


        private Dictionary<string, List<GameObjectState>> InterpolateCarPositions(Dictionary<string, List<GameObjectState>> allCars)
        {
            Dictionary<string, List<GameObjectState>> carPositions = new Dictionary<string, List<GameObjectState>>();

            foreach (string key in allCars.Keys)
            {
                List<GameObjectState> compressedPositions = allCars[key];
                List<GameObjectState> positions = new List<GameObjectState>();

                for (int i = 0; i < compressedPositions.Count - 1; i++)
                {
                    //if one ahead has a delta larger than 0 we try to resolve it, otherwise we just add the frame

                    int frameDelta = compressedPositions[i + 1].FrameNumber - compressedPositions[i].FrameNumber;

                    if (frameDelta > 1)
                    {
                        //Interpolate

                        //Add the first frame
                        positions.Add(compressedPositions[i]);


                        double speed = (compressedPositions[i].RigidBody.LinearVelocity.Magnitude / 10000) * 3.6;

                        if (speed >= 0.6)
                        {
                            //Use the same speed and positions
                            for (int j = 1; j < frameDelta; j++)
                            {
                                GameObjectState interpolatedState = new GameObjectState
                                {
                                    RigidBody = compressedPositions[i].RigidBody,
                                    FrameNumber = compressedPositions[i].FrameNumber + j,
                                    Time = compressedPositions[i].Time + (j * (1f / 30f)),
                                    ActorID = compressedPositions[i].ActorID,
                                };

                                positions.Add(interpolatedState);
                            }
                        }
                        else
                        {
                            //Set speed to 0 and keep same position
                            for (int j = 1; j < frameDelta; j++)
                            {


                                RigidBodyState rigidBody = new RigidBodyState(true, compressedPositions[i].RigidBody.Position, compressedPositions[i].RigidBody.Rotation, new Vector3D(0, 0, 0), new Vector3D(0, 0, 0));


                                GameObjectState interpolatedState = new GameObjectState
                                {
                                    RigidBody = rigidBody,
                                    FrameNumber = compressedPositions[i].FrameNumber + j,
                                    Time = compressedPositions[i].Time + (j * (1f / 30f)),
                                    ActorID = compressedPositions[i].ActorID,
                                };

                                positions.Add(interpolatedState);
                            }
                        }



                    }
                    else
                        positions.Add(compressedPositions[i]);
                }

                positions.Add(compressedPositions[compressedPositions.Count - 1]);

            }



            return carPositions;
        }


    }
}
