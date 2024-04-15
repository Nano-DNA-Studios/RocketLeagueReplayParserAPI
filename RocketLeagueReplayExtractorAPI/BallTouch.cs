using DNARocketLeagueReplayParser.ReplayStructure.UnrealEngineObjects;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// A Class Describing a Ball Touch
    /// </summary>
    public class BallTouch
    {
        /// <summary>
        /// The Frame Number the Ball was Touched
        /// </summary>
        public int FrameNumber { get; internal set; }

        /// <summary>
        /// The Time in Seconds at which the Ball was Touched
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// The Actor ID of the Object that Touched the Ball
        /// </summary>
        public uint ActorID { get; set; }

        /// <summary>
        /// The RigidBody State of the Ball when it was Touched
        /// </summary>
        public RigidBodyState RigidBody { get; set; }

        /// <summary>
        /// The Distance the Object was from the Ball when it was Touched
        /// </summary>
        public double Distance { get; set; }
        
        /// <summary>
        /// The Time until the next Ball Touch Occurs
        /// </summary>
        public float TimeUntilNextTouch { get; set; }
    }
}
