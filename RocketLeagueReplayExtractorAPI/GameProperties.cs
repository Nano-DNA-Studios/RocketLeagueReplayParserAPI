
using System.Diagnostics.Contracts;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Static Class that Acts like an Enum and Contains the Properties that can be Tracked in a Rocket League Game
    /// </summary>
    public class GameProperties
    {
        /// <summary>
        /// The Rigid Body Property ID
        /// </summary>
        public const int RigidBody = 42;

        /// <summary>
        /// Zero Goals Scored
        /// </summary>
        public const int ZeroGoals = 0;



        public const int BlueTeamID = 0;
        public const int OrangeTeamID = 1;

        public const string BlueTeam = "Blue Team";
        public const string OrangeTeam = "Orange Team";

        public const string ActorIDToNameMap = "ActorIDToNameMap";

        
        public const string None = "None";
        public const string Score = "Score";
        public const string Saves = "Saves";
        public const string Goals = "Goals";
        public const string Assists = "Assists";
        public const string Name = "Name";
        public const string Shots = "Shots";
        public const string Team = "Team";
        public const string BallTouches = "Ball Touches";
        public const string BallTouchCount = "Ball Touches Count";
        public const string BallTouchPercentage = "Ball Touch Percentage";
        public const string BallPossessionTime = "Ball Possession Time";
        public const string BallPossessionTimePercentage = "Ball Possession Percentage";



        public const string BlueTeamScore = "Team0Score";
        public const string OrangeTeamScore = "Team1Score";

        public const string PlayerStats = "PlayerStats";

        /// <summary>
        /// The Car Object Class Name
        /// </summary>
        public const string Car = "TAGame.Car_TA";

        /// <summary>
        /// The Ball Object Class Name
        /// </summary>
        public const string Ball = "TAGame.Ball_TA";

        /// <summary>
        /// The Vehicle Property
        /// </summary>
        public const string Vehicle = "TAGame.CarComponent_TA:Vehicle";

        /// <summary>
        /// The Player Name Property
        /// </summary>
        public const string PlayerName = "Engine.PlayerReplicationInfo:PlayerName";

        /// <summary>
        /// The Player Replication Info Property
        /// </summary>
        public const string PlayerReplicationInfo = "Engine.Pawn:PlayerReplicationInfo";

        /// <summary>
        /// The Replay Name Property Name
        /// </summary>
        public const string ReplayName = "ReplayName";

        /// <summary>
        /// Title given to Unnamed Replays
        /// </summary>
        public const string UnamedReplay = "Unnamed";

        /// <summary>
        /// The Record FPS Property Name
        /// </summary>
        public const string RecordFPS = "RecordFPS";
    }
}
