using DNARocketLeagueReplayParser.ReplayStructure.UnrealEngineObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RocketLeagueReplayParserAPI
{
    internal class TouchCalculator
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

                if (!CarPositions.ContainsKey(targetFrame.ToString()))
                    continue;

                gameObjects.AddRange(CarPositions[targetFrame.ToString()]);
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


        public List<BallTouch> GetBallTouches()
        {
            List<BallTouch> ballTouches = new List<BallTouch>();

            double lastAcceleration = 0;
            double lastMinDistance = 0;
            for (int i = 0; i < BallPosition.Count; i++)
            {
                GameObjectState ballState = BallPosition[i];

                (double minDistance, GameObjectState closestCar) = GetClosestCar(ballState);

                double maxAcceleration = GetMaxAcceleration(ballState.FrameNumber);

                if (minDistance > 2.5)
                    continue;

                if (maxAcceleration < 1)
                    continue;

                BallTouch ballTouch = new BallTouch()
                {
                    FrameNumber = ballState.FrameNumber,
                    Time = ballState.Time,
                    ActorID = closestCar.ActorID,
                    RigidBody = ballState.RigidBody
                };

                if (ballTouches.Count > 1)
                {
                    if (lastAcceleration == maxAcceleration)
                        continue;

                    if (minDistance < lastMinDistance)
                        ballTouches.RemoveAt(ballTouches.Count - 1);
                }

                ballTouches.Add(ballTouch);
                lastAcceleration = maxAcceleration;
                lastMinDistance = minDistance;
            }









            return ballTouches;
        }










    }
}
