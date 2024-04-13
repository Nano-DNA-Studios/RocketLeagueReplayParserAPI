using DNARocketLeagueReplayParser.ReplayStructure.UnrealEngineObjects;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Describes a Game Object State
    /// </summary>
    public class GameObjectState
    {
        /// <summary>
        /// Rigid Body State of the Object
        /// </summary>
        public RigidBodyState RigidBody { get; set; }

        /// <summary>
        /// The Frame Number the Object is in
        /// </summary>
        public int FrameNumber { get; set; }

        /// <summary>
        /// The Time in the Match the Object is in
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// The Actor ID of the Object, for Players only
        /// </summary>
        public uint ActorID { get; set; }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public GameObjectState()
        {
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="rigidBody"> The RigidBodyState of the Game Object </param>
        /// <param name="frameNumber"> The Frame Number </param>
        /// <param name="time"> The Time in the Match </param>
        /// <param name="actorID"> The Actor ID of the Game Object </param>
        public GameObjectState(RigidBodyState rigidBody, int frameNumber, float time, uint actorID)
        {
            RigidBody = rigidBody;
            FrameNumber = frameNumber;
            Time = time;
            ActorID = actorID;
        }

        /// <summary>
        /// Gets the Remaining Time in the Match at which the State Occurs
        /// </summary>
        /// <param name="matchLength"> The Match Length in Seconds </param>
        /// <returns> The Remaining time of the before it ends </returns>
        public float GetRemainingTime (float matchLength)
        {
            return matchLength - Time;
        }
    }
}
