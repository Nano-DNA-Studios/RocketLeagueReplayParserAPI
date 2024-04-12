using DNARocketLeagueReplayParser.ReplayStructure.UnrealEngineObjects;

namespace RocketLeagueReplayParserAPI
{
    internal class ReplayInterpolator
    {
        /// <summary>
        /// The Minimum Speed in KM/H a GameObject can have before it is considered to be at rest
        /// </summary>
        private const double MINIMUM_SPEED_KMH = 0.6;

        /// <summary>
        /// The Record FPS of the Replay File
        /// </summary>
        private const float RECORD_FPS = 30f;

        /// <summary>
        /// Interpolates a single GameObject State
        /// </summary>
        /// <param name="previousGameObjectState"> The Previous Game Object State that has occured </param>
        /// <param name="interpolationIndex"> The Interpolation Index </param>
        /// <returns> The New interpolated state of the Game Object </returns>
        public static GameObjectState InterpolateGameObjectState(GameObjectState previousGameObjectState, int interpolationIndex)
        {
            RigidBodyState rigidBody = (RigidBodyState)previousGameObjectState.RigidBody.Clone();
            int frameNumber = previousGameObjectState.FrameNumber + interpolationIndex;
            uint actorID = previousGameObjectState.ActorID;
            float interpolatedTimeDelta = interpolationIndex * (1f / RECORD_FPS);
            float time = previousGameObjectState.Time + interpolatedTimeDelta;

            if (rigidBody.LinearVelocity == null)
                rigidBody.LinearVelocity = Vector3D.Zero;

            if (rigidBody.AngularVelocity == null)
                rigidBody.AngularVelocity = Vector3D.Zero;

            if (rigidBody.VelocityKMH < MINIMUM_SPEED_KMH)
            {
                rigidBody.LinearVelocity = Vector3D.Zero;
                rigidBody.AngularVelocity = Vector3D.Zero;
            }

            rigidBody.Position = ((Vector3D)rigidBody.Position) + (rigidBody.LinearVelocity * interpolatedTimeDelta);
            ((Quaternion)rigidBody.Rotation).RotateByAngularVelocity(rigidBody.AngularVelocity, interpolatedTimeDelta); //Idk kinda fishy

            return new GameObjectState(rigidBody, frameNumber, time, actorID);
        }

        /// <summary>
        /// Interpolates all the missing Frames / GameObject States in a List of GameObject States
        /// </summary>
        /// <param name="states"> The List of States that need to be interpolated </param>
        /// <returns> The fully interpolated List of Game Object States </returns>
        public static List<GameObjectState> InterpolateFrames(List<GameObjectState> states)
        {
            List<GameObjectState> interpolatedStates = new List<GameObjectState>();

            for (int i = 0; i < states.Count - 1; i++)
            {
                interpolatedStates.Add(states[i]);

                int frameDelta = states[i + 1].FrameNumber - states[i].FrameNumber;

                if (frameDelta == 1)
                    continue;

                for (int j = 1; j < frameDelta; j++)
                    interpolatedStates.Add(InterpolateGameObjectState(states[i], j));
            }

            interpolatedStates.Add(states[states.Count - 1]);

            return interpolatedStates;
        }





    }
}
