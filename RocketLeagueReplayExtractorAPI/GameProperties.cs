
using System.Diagnostics.Contracts;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Static Class that Acts like an Enum and Contains the Properties that can be Tracked in a Rocket League Game
    /// </summary>
    public class GameProperties
    {
        public const int BlueTeamID = 0;
        public const int OrangeTeamID = 1;

        
        public const string None = "None";
        public const string Score = "Score";
        public const string Saves = "Saves";
        public const string Goals = "Goals";
        public const string Assists = "Assists";
        public const string Name = "Name";
        public const string Shots = "Shots";
        public const string Team = "Team";
        public const string BallTouches = "Ball Touches";
        public const string BallTouchPercentage = "Ball Touch Percentage";
        public const string BallPossessionTime = "Ball Possession Time";
        public const string BallPossessionTimePercentage = "Ball Possession Percentage";



        public const string PlayerStats = "PlayerStats";
    }
}
