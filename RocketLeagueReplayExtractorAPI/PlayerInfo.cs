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


        private GameStats[] DisplayStats = new GameStats[] { GameStats.Score, GameStats.Goals, GameStats.Assists, GameStats.Saves, GameStats.Shots, GameStats.BallTouches, GameStats.BallTouchPossession, GameStats.BallPossessionTime, GameStats.BallPossessionTimePercentage };

        private string[] DisplayStatsString = new string[] { "Score", "Goals", "Assists", "Saves", "Shots", "Touches", "Touch Possession", "Ball Possession Time", "Ball Possession Percentage" };


        /// <summary>
        /// The ID assigned to the Player at the Start of the Match
        /// </summary>
        public int PlayerID { get; private set; }

        /// <summary>
        /// The Name of the Player
        /// </summary>
        public string PlayerName => PlayerProperties.TryGetProperty(GameProperties.Name, "PlayerName");

        /// <summary>
        /// The Number of Points the Player got
        /// </summary>
        public int Score => PlayerProperties.TryGetProperty(SCORE, 0);

        /// <summary>
        /// The Number of Goals the Player Scored 
        /// </summary>
        public int Goals => PlayerProperties.TryGetProperty(GOALS, 0);

        /// <summary>
        /// The Number of Assists the Player got
        /// </summary>
        public int Assists => PlayerProperties.TryGetProperty(ASSISTS, 0);

        /// <summary>
        /// The Number of Saves the Player got
        /// </summary>
        public int Saves => PlayerProperties.TryGetProperty(SAVES, 0);

        /// <summary>
        /// The Number of Shots the Player took
        /// </summary>
        public int Shots => PlayerProperties.TryGetProperty(SHOTS, 0);

        //0 = Blue, 1 = Orange
        /// <summary>
        /// The Team ID the Player is on. 0 = Blue, 1 = Orange
        /// </summary>
        public int Team => PlayerProperties.TryGetProperty(TEAM, 0);

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

        /// <summary>
        /// The Players Rocket League Properties
        /// </summary>
        public RocketLeaguePropertyDictionary PlayerProperties { get; private set; }


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
            BallTouches = new List<BallTouch>();
            PlayerProperties = new RocketLeaguePropertyDictionary();

            PlayerID = playerID;
            PlayerProperties.Add(NAME, new RocketLeagueProperty(NAME, "string", playerName));
        }

        /// <summary>
        /// Initializes a new Player Info Object from the given Property Dictionary
        /// </summary>
        /// <param name="playerStats"> The Property Dictionary containing the Players Stats </param>
        public PlayerInfo(PropertyDictionary playerStats)
        {
            PlayerProperties = new RocketLeaguePropertyDictionary();

            foreach (string key in playerStats.Keys)
            {
                Property property = playerStats[key];
                RocketLeagueProperty RLProperty = new RocketLeagueProperty(property.Name, property.Type, property.Value);
                if (RLProperty.Name != GameProperties.None)
                    PlayerProperties.Add(key, RLProperty);
            }

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
        /// Returns a Boolean Flag indicating if the Stat Exists for the Player
        /// </summary>
        /// <param name="stat"> The Stat to check if it Exists </param>
        /// <returns> True if the Player has the Stat, False if it doesn't </returns>
        public bool StatExists (string stat)
        {
            return PlayerProperties.ContainsKey(stat);
        }

        /// <summary>
        /// Returns the Game Stat Value for the Player based on the Stat Type
        /// </summary>
        /// <param name="stat"> The Stat Type to get </param>
        /// <returns> The Game Stat Value </returns>
        public T GetStat<T>(string stat)
        {
            if (!StatExists(stat))
                throw new KeyNotFoundException($"Stat {stat} not found in the Player Info");

            return PlayerProperties.TryGetProperty<T>(stat);
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
