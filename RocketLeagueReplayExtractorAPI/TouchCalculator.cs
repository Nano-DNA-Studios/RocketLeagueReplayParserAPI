using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }










    }
}
