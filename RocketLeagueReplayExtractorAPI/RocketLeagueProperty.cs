using DNARocketLeagueReplayParser.ReplayStructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Represents a Property an object has in a Rocket League Replay
    /// </summary>
    public class RocketLeagueProperty
    {
        /// <summary>
        /// Name of the Property
        /// </summary>
        public string Name { get; protected set; }
/*
        /// <summary>
        /// Associated Game Stat of the Property
        /// </summary>
        public GameStats GameStat { get; protected set; }
*/
        /// <summary>
        /// Type of the Property
        /// </summary>
        public string Type { get; protected set; }

        /// <summary>
        /// Value of the Property
        /// </summary>
        public object Value
        {
            get; protected set;
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="name"> The Name of the Property </param>
        /// <param name="type"> The Value Type of the Property </param>
        /// <param name="value"> The Value of the Property </param>
        public RocketLeagueProperty(string name, string type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

/*
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="name"> The Name of the Property </param>
        /// <param name="type"> The Value Type of the Property </param>
        /// <param name="value"> The Value of the Property </param>
        /// <param name="gameStat"> The Game Stat Associated with the Property </param>
        public RocketLeagueProperty(string name, string type, object value, GameStats gameStat)
        {
            Name = name;
            Type = type;
            Value = value;
            GameStat = gameStat;
        }*/
    }
}
