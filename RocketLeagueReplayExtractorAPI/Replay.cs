using DNARocketLeagueReplayParser.ReplayStructure;
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
    public class Replay : RLAnalysisObject<Replay>
    {
        /// <summary>
        /// The Recording FPS of the Replay
        /// </summary>
        public float RecordFPS => Properties.TryGetProperty(GameProperties.RecordFPS, 30f);

        /// <summary>
        /// The Length of the Match in Seconds
        /// </summary>
        public float MatchLength { get; private set; }

        /// <summary>
        /// The Replay Info Object
        /// </summary>
        public PsyonixReplay _replayInfo { get; set; }

        /// <summary>
        /// The Path to the File Being Parsed
        /// </summary>
        private string _pathToFile;

        /// <summary>
        /// The Blue Teams Number of Goals At the end of the replay
        /// </summary>
        public int BlueTeamGoals => Properties.TryGetProperty(GameProperties.BlueTeamScore, GameProperties.ZeroGoals);

        /// <summary>
        /// The Orange Teams Number of Goals At the end of the replay
        /// </summary>
        public int OrangeTeamGoals => Properties.TryGetProperty(GameProperties.OrangeTeamScore, GameProperties.ZeroGoals);

        /// <summary>
        /// The Name of the Replay set by the Player When Saved
        /// </summary>
        public string ReplayName => Properties.TryGetProperty(GameProperties.ReplayName, GameProperties.UnamedReplay);

        /// <summary>
        /// List of all the Balls State in the Replay
        /// </summary>
        public List<GameObjectState> BallPositions { get; private set; }

        /// <summary>
        /// Dictionary of all the Car States in the Replay
        /// </summary>
        public Dictionary<string, List<GameObjectState>> CarPositions { get; private set; }

        /// <summary>
        /// The Matches Roster, including all the Players and their Stats
        /// </summary>
        public Roster MatchRoster { get; private set; }

        /// <summary>
        /// Initializes the Replay Object from the given Path
        /// </summary>
        /// <param name="path"> The Path to the Replay File </param>
        public Replay(string path)
        {
            Properties = new RocketLeaguePropertyDictionary();
            _pathToFile = path;
            using (FileStream stream = File.Open(path, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
                _replayInfo = PsyonixReplay.Deserialize(reader);

            foreach (string key in _replayInfo.Properties.Keys)
            {
                Property property = _replayInfo.Properties[key];
                RocketLeagueProperty RLProperty = new RocketLeagueProperty(property.Name, property.Type, property.Value);
                if (RLProperty.Name != GameProperties.None)
                    Properties.Add(key, RLProperty);
            }

            SetMatchLength();
            MatchRoster = new Roster(this);
            ExtractRigidBodies(_replayInfo);
            TouchCalculator.SetBallTouchStats(this);
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
        /// Extracts all the Rigid Bodies from the Replay Info Object
        /// </summary>
        private void ExtractRigidBodies(PsyonixReplay replay)
        {
            List<GameObjectState> ballPosition = new List<GameObjectState>();
            Dictionary<string, List<GameObjectState>> carPositions = new Dictionary<string, List<GameObjectState>>();
            uint frameNumber = 0;

            foreach (PsyonixFrame frame in replay.Frames)
            {
                frameNumber++;
                foreach (ActorState actorState in frame.ActorStates)
                {
                    foreach (ActorStateProperty property in actorState.Properties.Values)
                    {
                        if (property.PropertyId == GameProperties.RigidBody)
                        {
                            GameObjectState gameObjectState = new GameObjectState
                            {
                                RigidBody = (RigidBodyState)property.Data,
                                FrameNumber = (int)frameNumber,
                                Time = frame.Time,
                                ActorID = actorState.Id
                            };

                            if (replay.Objects[property.GetClassCache().ObjectIndex] == GameProperties.Car)
                            {
                                if (MatchRoster.ActorIDToName.TryGetValue(actorState.Id, out string playerName))
                                {
                                    if (carPositions.TryGetValue(playerName, out List<GameObjectState> positions))
                                        positions.Add(gameObjectState);
                                    else
                                    {
                                        carPositions.Add(MatchRoster.ActorIDToName[actorState.Id], new List<GameObjectState>());
                                        carPositions[MatchRoster.ActorIDToName[actorState.Id]].Add(gameObjectState);
                                    }
                                }
                            }

                            if (replay.Objects[property.GetClassCache().ObjectIndex] == GameProperties.Ball)
                                ballPosition.Add(gameObjectState);
                        }
                    }
                }
            }

            BallPositions = ReplayInterpolator.InterpolateFrames(ballPosition);
            CarPositions = ReplayInterpolator.InterpolateAllFrames(carPositions);
        }

        /// <summary>
        /// Sets the Replays Match Length
        /// </summary>
        private void SetMatchLength()
        {
            foreach (PsyonixFrame frame in _replayInfo.Frames)
            {
                if (frame.Time > MatchLength)
                    MatchLength = frame.Time;
            }
        }
    }
}
