using DNARocketLeagueReplayParser.ReplayStructure.Mapping;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Describes the Information of a Player in the Replay
    /// </summary>
    public class PlayerInfo
    {
        /// <summary>
        /// The Stats that would be Displayed on the Scoreboard
        /// </summary>
        public static string[] DisplayStats { get; } = [GameProperties.Score, GameProperties.Goals, GameProperties.Assists, GameProperties.Saves, GameProperties.Shots, GameProperties.BallTouchCount, GameProperties.BallTouchPercentage, GameProperties.BallPossessionTime, GameProperties.BallPossessionTimePercentage, GameProperties.AverageBallPossessionTime];

        /// <summary>
        /// The Stats that would be Displayed on the Scoreboard with their Units
        /// </summary>
        public static string[] DisplayStatsWithUnits { get; } = [GameProperties.Score, GameProperties.Goals, GameProperties.Assists, GameProperties.Saves, GameProperties.Shots, GameProperties.BallTouchCount, GameProperties.BallTouchPercentage + " (%)", GameProperties.BallPossessionTime + " (s)", GameProperties.BallPossessionTimePercentage + " (%)", GameProperties.AverageBallPossessionTime + " (s)"];

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
        public int Score => PlayerProperties.TryGetProperty(GameProperties.Score, 0);

        /// <summary>
        /// The Number of Goals the Player Scored 
        /// </summary>
        public int Goals => PlayerProperties.TryGetProperty(GameProperties.Goals, 0);

        /// <summary>
        /// The Number of Assists the Player got
        /// </summary>
        public int Assists => PlayerProperties.TryGetProperty(GameProperties.Assists, 0);

        /// <summary>
        /// The Number of Saves the Player got
        /// </summary>
        public int Saves => PlayerProperties.TryGetProperty(GameProperties.Saves, 0);

        /// <summary>
        /// The Number of Shots the Player took
        /// </summary>
        public int Shots => PlayerProperties.TryGetProperty(GameProperties.Shots, 0);

        //0 = Blue, 1 = Orange
        /// <summary>
        /// The Team ID the Player is on. 0 = Blue, 1 = Orange
        /// </summary>
        public int Team => PlayerProperties.TryGetProperty(GameProperties.Team, 0);

        /// <summary>
        /// The Number of Touches the Player got
        /// </summary>
        public int Touches => PlayerProperties.TryGetProperty(GameProperties.BallTouchCount, 0);

        /// <summary>
        /// List of all the Ball Touches the Player got with their Info
        /// </summary>
        public List<BallTouch> BallTouches => PlayerProperties.TryGetProperty(GameProperties.BallTouches, new List<BallTouch>());

        /// <summary>
        /// The Percentage of Touches the Player got on the Ball compared to all Players
        /// </summary>
        public float BallTouchPossessionPercentage => PlayerProperties.TryGetProperty(GameProperties.BallTouchPercentage, 0f);

        /// <summary>
        /// The Time the Player Possessed the Ball
        /// </summary>
        public float BallPossessionTime => PlayerProperties.TryGetProperty(GameProperties.BallPossessionTime, 0f);

        /// <summary>
        /// The Percentage of Time the Player Possessed the Ball compared to all Players
        /// </summary>
        public float BallPossessionTimePercentage => PlayerProperties.TryGetProperty(GameProperties.BallPossessionTimePercentage, 0f);

        /// <summary>
        /// The Average Ball Possession Time of the Player
        /// </summary>
        public float AverageBallPossessionTime => PlayerProperties.TryGetProperty(GameProperties.AverageBallPossessionTime, 0f);

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
            PlayerProperties = new RocketLeaguePropertyDictionary();

            PlayerID = playerID;
            PlayerProperties.Add(GameProperties.Name, new RocketLeagueProperty(GameProperties.Name, "string", playerName));
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
        }

        /// <summary>
        /// Gets the Information that would displayed on the Scoreboard in the same Format as the Scoreboard
        /// </summary>
        /// <returns> Formatted info that would be Displayed on the Scoreboard</returns>
        public string[] GetScoreboardInfo()
        {
            List<string> scoreboardInfo = [PlayerName];

            foreach (string stat in DisplayStats)
            {
                object value = PlayerProperties.TryGetProperty<object>(stat);

                if (value == null)
                    scoreboardInfo.Add("0");
                else
                    scoreboardInfo.Add(value.ToString());

            }
            return scoreboardInfo.ToArray();
        }

        /// <summary>
        /// Returns a Boolean Flag indicating if the Stat Exists for the Player
        /// </summary>
        /// <param name="stat"> The Stat to check if it Exists </param>
        /// <returns> True if the Player has the Stat, False if it doesn't </returns>
        public bool StatExists(string stat)
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
        /// <param name="teamTouches"> The Number of Touches the Team Got </param>
        /// <param name="teamPossessionTime"> The Time the Team Possessed the Ball </param>
        internal void SetBallTouches(List<BallTouch> touches, int teamTouches, float teamPossessionTime)
        {
            float possessionTime = touches.Sum(touch => touch.TimeUntilNextTouch);

            PlayerProperties.Add(GameProperties.BallTouches, new RocketLeagueProperty(GameProperties.BallTouches, touches.GetType().Name, touches));
            PlayerProperties.Add(GameProperties.BallTouchCount, new RocketLeagueProperty(GameProperties.BallTouchCount, "int", touches.Count));
            PlayerProperties.Add(GameProperties.BallPossessionTime, new RocketLeagueProperty(GameProperties.BallPossessionTime, "float", possessionTime));
            PlayerProperties.Add(GameProperties.BallTouchPercentage, new RocketLeagueProperty(GameProperties.BallTouchPercentage, "float", 100 * (float)Touches / teamTouches));
            PlayerProperties.Add(GameProperties.BallPossessionTimePercentage, new RocketLeagueProperty(GameProperties.BallPossessionTimePercentage, "float", 100 * BallPossessionTime / teamPossessionTime));
            PlayerProperties.Add(GameProperties.AverageBallPossessionTime, new RocketLeagueProperty(GameProperties.AverageBallPossessionTime, "float", GetAveragePossessionTime()));
        }

        /// <summary>
        /// Calculates the Average Possession Time for the Player
        /// </summary>
        /// <returns> Returns the Average Possession Time of the Player  </returns>
        private float GetAveragePossessionTime()
        {
            List<float> possessionTimes = new List<float>();

            float possessionTime = 0;

            for (int i = 0; i < BallTouches.Count - 2; i++)
            {
                possessionTime += BallTouches[i].TimeUntilNextTouch;

                if (BallTouches[i + 1].Time == BallTouches[i].Time + BallTouches[i].TimeUntilNextTouch)
                    continue;

                possessionTimes.Add(possessionTime);
                possessionTime = 0;
            }

            if (possessionTimes.Count == 0)
                return 0;

            return possessionTimes.Average();
        }
    }
}
