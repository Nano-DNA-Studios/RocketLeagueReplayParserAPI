using DNARocketLeagueReplayParser.ReplayStructure.Mapping;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Describes the Information of a Player in the Replay
    /// </summary>
    public class PlayerInfo
    {
        private const string SCORE = "Score";
        private const string SAVES = "Saves";
        private const string GOALS = "Goals";
        private const string ASSISTS = "Assists";
        private const string NAME = "Name";
        private const string SHOTS = "Shots";
        private const string TEAM = "Team";

        /// <summary>
        /// The ID assigned to the Player at the Start of the Match
        /// </summary>
        public int PlayerID { get; private set; }

        /// <summary>
        /// The Name of the Player
        /// </summary>
        public string PlayerName { get; private set; }

        /// <summary>
        /// The Number of Points the Player got
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// The Number of Goals the Player Scored 
        /// </summary>
        public int Goals { get; private set; }

        /// <summary>
        /// The Number of Assists the Player got
        /// </summary>
        public int Assists { get; private set; }

        /// <summary>
        /// The Number of Saves the Player got
        /// </summary>
        public int Saves { get; private set; }

        /// <summary>
        /// The Number of Shots the Player took
        /// </summary>
        public int Shots { get; private set; }

        //0 = Blue, 1 = Orange
        /// <summary>
        /// The Team ID the Player is on. 0 = Blue, 1 = Orange
        /// </summary>
        public int Team { get; private set; }

        /// <summary>
        /// The Number of Touches the Player got
        /// </summary>
        public int Touches => BallTouches.Count;

        /// <summary>
        /// List of all the Ball Touches the Player got with their Info
        /// </summary>
        public List<BallTouch> BallTouches { get; private set; }

        /// <summary>
        /// The Percentage of Touches the Player got on the Ball compared to all Players
        /// </summary>
        public float BallTouchPossessionPercentage { get; internal set; }

        /// <summary>
        /// The Time the Player Possessed the Ball
        /// </summary>
        public float BallPossessionTime { get; internal set; }

        /// <summary>
        /// The Percentage of Time the Player Possessed the Ball compared to all Players
        /// </summary>
        public float BallPossessionPercentage { get; internal set; }


        //Eventually support
        //Clears
        //Demos
        //Epic vs Normal Saves
        //Touches
        //Boost Usage
        //ect

        /// <summary>
        /// Initializes a new Player Info Object
        /// </summary>
        /// <param name="playerID"> The ID Assigned to the Player </param>
        /// <param name="playerName"> The Name of the Player </param>
        public PlayerInfo(int playerID, string playerName)
        {
            PlayerID = playerID;
            PlayerName = playerName;
            BallTouches = new List<BallTouch>();
        }

        /// <summary>
        /// Initializes a new Player Info Object from the given Property Dictionary
        /// </summary>
        /// <param name="playerStats"> The Property Dictionary containing the Players Stats </param>
        public PlayerInfo(PropertyDictionary playerStats)
        {
            PlayerName = (string)playerStats[NAME].Value;
            Score = (int)playerStats[SCORE].Value;
            Goals = (int)playerStats[GOALS].Value;
            Assists = (int)playerStats[ASSISTS].Value;
            Saves = (int)playerStats[SAVES].Value;
            Shots = (int)playerStats[SHOTS].Value;
            Team = (int)playerStats[TEAM].Value;
            BallTouches = new List<BallTouch>();
        }

        /// <summary>
        /// Gets the Information that would displayed on the Scoreboard in the same Format as the Scoreboard
        /// </summary>
        /// <returns> Formatted info that would be Displayed on the Scoreboard</returns>
        public string[] GetScoreboardInfo()
        {
            return [PlayerName, Score.ToString(), Goals.ToString(), Assists.ToString(), Saves.ToString(), Shots.ToString(), Touches.ToString(), BallTouchPossessionPercentage.ToString(), BallPossessionTime.ToString(), BallPossessionPercentage.ToString()];
        }

        /// <summary>
        /// Returns the Game Stat Value for the Player based on the Stat Type
        /// </summary>
        /// <param name="stat"> The Stat Type to get </param>
        /// <returns> The Game Stat Value </returns>
        public float GetStat(GameStats stat)
        {
            switch (stat)
            {
                case GameStats.Score:
                    return Score;
                case GameStats.Goals:
                    return Goals;
                case GameStats.Assists:
                    return Assists;
                case GameStats.Saves:
                    return Saves;
                case GameStats.Shots:
                    return Shots;
                case GameStats.BallTouches:
                    return Touches;
                case GameStats.BallTouchPossession:
                    return BallTouchPossessionPercentage;
                case GameStats.BallPossessionTime:
                    return BallPossessionTime;
                case GameStats.BallPossessionTimePercentage:
                    return BallPossessionPercentage;

                default: return 0;
            }
        }

        /// <summary>
        /// Adds a Ball Touch to the Player Info
        /// </summary>
        /// <param name="touch"> The Ball Touch info </param>
        public void AddBallTouch(BallTouch touch)
        {
            BallTouches.Add(touch);
        }

        /// <summary>
        /// Sets all the BallTouches for the Player
        /// </summary>
        /// <param name="touches"></param>
        public void SetBallTouches(List<BallTouch> touches)
        {
            BallTouches = touches;
        }
    }
}
