
using DNARocketLeagueReplayParser.ReplayStructure;
using DNARocketLeagueReplayParser.ReplayStructure.Mapping;


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

        /// <summary>
        /// The Key for the Blue Team Score
        /// </summary>
        private const string BLUE_TEAM_SCORE = "Team0Score";

        /// <summary>
        /// The Key for the Orange Team Score
        /// </summary>
        private const string ORANGE_TEAM_SCORE = "Team1Score";

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

        /// <summary>
        /// Initializes the Replay Object from the given Path
        /// </summary>
        /// <param name="path"></param>
        public Replay(string path)
        {
            _pathToFile = path;
            using (FileStream stream = File.Open(path, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
                _replayInfo = PsyonixReplay.Deserialize(reader);

            ExtractPlayers();
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
        private void ExtractPlayers()
        {
            int playerID = 0;

            ArrayProperty playerStatsArray = (ArrayProperty)_replayInfo.Properties[PLAYER_STATS];
            List<PropertyDictionary> playerStats = (List<PropertyDictionary>)playerStatsArray.Value;

            Players = new PlayerInfo[playerStats.Count];

            foreach (PropertyDictionary playerStat in playerStats)
            {
                Players[playerID] = new PlayerInfo(playerStat);
                playerID++;
            }
        }

        /// <summary>
        /// Gets the Number of Saves for a Team
        /// </summary>
        /// <param name="blueTeam"> Flag indicating if the Team to get is Blue </param>
        /// <returns> The Number of Saves for the Team </returns>
        public int GetTeamSaves(bool blueTeam)
        {
            int teamID = blueTeam ? BLUE_TEAM : ORANGE_TEAM;
            int saves = 0;

            foreach (PlayerInfo player in Players)
            {
                if (player.Team == teamID)
                    saves += player.Saves;
            }

            return saves;
        }

        /// <summary>
        /// Renames the Replay and Saves it to the given Path
        /// </summary>
        /// <param name="renamedName"> The new Name of the File, if left empty it defaults to the Name of the Replay </param>
        /// <param name="savePath"> The New Save Path of the Replay, if left Empty it saves in the Same Directory </param>
        /// <exception cref="DirectoryNotFoundException"> Error thrown if the new Directory doesn't exist </exception>
        public void RenameAndSave (string? renamedName = null, string? savePath = null)
        {
            if (string.IsNullOrEmpty(savePath))
                savePath = Path.GetDirectoryName(_pathToFile);

            if (string.IsNullOrEmpty(renamedName))
                renamedName = ReplayName;

            if (Directory.Exists(savePath) == false)
                throw new DirectoryNotFoundException("The Directory to Save the Replay to does not exist");

            string newPath = Path.Combine(savePath, renamedName + ".replay");
            File.WriteAllText(newPath, File.ReadAllText(_pathToFile));
        }
    }
}
