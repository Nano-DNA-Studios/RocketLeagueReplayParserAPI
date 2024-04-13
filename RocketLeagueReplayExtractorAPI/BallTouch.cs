using DNARocketLeagueReplayParser.ReplayStructure.UnrealEngineObjects;

namespace RocketLeagueReplayParserAPI
{
    public class BallTouch
    {
        public int FrameNumber { get; set; }

        public float Time { get; set; }

        public uint ActorID { get; set; }

        public RigidBodyState RigidBody { get; set; }
    }
}
