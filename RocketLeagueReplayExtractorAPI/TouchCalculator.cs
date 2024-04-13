using DNARocketLeagueReplayParser.ReplayStructure.UnrealEngineObjects;

namespace RocketLeagueReplayParserAPI
{
    public class TouchCalculator
    {
        public List<GameObjectState> BallPosition { get; set; }

        public Dictionary<string, List<GameObjectState>> CarPositions { get; set; }

        private Dictionary<int, List<GameObjectState>> _carPositionsByFrame { get; set; }


        public TouchCalculator(List<GameObjectState> ballPosition, Dictionary<string, List<GameObjectState>> carPositions)
        {
            BallPosition = ballPosition;
            CarPositions = carPositions;

            SetCarPositionsByFrameDictionary();
        }

        private void SetCarPositionsByFrameDictionary()
        {
            Dictionary<int, List<GameObjectState>> carPositionsByFrame = new Dictionary<int, List<GameObjectState>>();

            foreach (KeyValuePair<string, List<GameObjectState>> carPosition in CarPositions)
            {
                foreach (GameObjectState gameObjectState in carPosition.Value)
                {
                    if (!carPositionsByFrame.ContainsKey(gameObjectState.FrameNumber))
                        carPositionsByFrame.Add(gameObjectState.FrameNumber, new List<GameObjectState>());

                    carPositionsByFrame[gameObjectState.FrameNumber].Add(gameObjectState);
                }
            }

            _carPositionsByFrame = carPositionsByFrame;
        }

        private List<GameObjectState> GetCarStates(int frameNumber)
        {
            List<GameObjectState> gameObjects = new List<GameObjectState>();

            for (int i = -2; i <= 2; i++)
            {
                int targetFrame = frameNumber + i;
                if (targetFrame < 0)
                    continue;

                if (_carPositionsByFrame.ContainsKey(targetFrame))
                    gameObjects.AddRange(_carPositionsByFrame[targetFrame]);
            }

            return gameObjects;
        }

        private (double minDistance, GameObjectState closestCar) GetClosestCar(GameObjectState ballState)
        {
            List<GameObjectState> closestCars = GetCarStates(ballState.FrameNumber);
            GameObjectState closestCar = closestCars[0];
            double minDistance = double.MaxValue;

            foreach (GameObjectState carState in closestCars)
            {
                double distance = (ballState.RigidBody.Position - carState.RigidBody.Position).Magnitude / UnrealEngineToMetric.METER_TO_UNREAL_UNITS;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCar = carState;
                }
            }

            return (minDistance, closestCar);
        }

        private double GetMaxAcceleration(int frameNumber)
        {
            List<double> speeds = BallPosition.Skip(frameNumber).Take(4).Select(ball => ball.RigidBody.GetVelocityKMH()).ToList();
            List<double> accelerations = speeds.Skip(1).Zip(speeds, (first, second) => first - second).ToList();

            return Math.Abs(accelerations.Max());
        }

        /*private List<BallTouch> CleanupBallTouches (List<BallTouch> touches)
        {
            List < BallTouch> cleanTouches = new List<BallTouch>();

            for (int i = 0; i < touches.Count; i++)
            {
                BallTouch touch = touches[i];

                if (i == 0)
                {
                    cleanTouches.Add(touch);
                    continue;
                }

                BallTouch lastTouch = touches[i - 1];

                if (touch.Distance > lastTouch.Distance)
                    continue;

                if (touch.ActorID != lastTouch.ActorID)
                    continue;

                cleanTouches.Add(touch);
            })




        }*/

        public List<BallTouch> GetBallTouches()
        {
            List<BallTouch> ballTouches = new List<BallTouch>();

            double lastMaxAcceleration = 0;
            for (int i = 0; i < BallPosition.Count; i++)
            {
                GameObjectState ballState = BallPosition[i];

                (double minDistance, GameObjectState closestCar) = GetClosestCar(ballState);

                if (minDistance > 2.5)
                    continue;

                double maxAcceleration = GetMaxAcceleration(i);

                if (maxAcceleration < 1)
                    continue;

                BallTouch ballTouch = new BallTouch()
                {
                    FrameNumber = ballState.FrameNumber,
                    Time = ballState.Time,
                    ActorID = closestCar.ActorID,
                    RigidBody = ballState.RigidBody,
                    Distance = minDistance
                };

                if (ballTouches.Count > 0 && lastMaxAcceleration == maxAcceleration)
                {
                    if (minDistance > ballTouches.Last().Distance)
                        continue;

                    ballTouches.RemoveAt(ballTouches.Count - 1);
                    ballTouches.Add(ballTouch);
                }
                else
                    ballTouches.Add(ballTouch);

                lastMaxAcceleration = maxAcceleration;
            }

            //Cleanup





            return ballTouches;
        }

    }
}
