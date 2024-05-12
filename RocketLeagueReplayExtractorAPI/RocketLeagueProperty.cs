
namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Represents a Property an object has in a Rocket League Replay
    /// </summary>
    public class RocketLeagueProperty
    {
        /// <summary>
        /// Name of the <see cref="RocketLeagueProperty"/>
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Type of the <see cref="RocketLeagueProperty"/>
        /// </summary>
        public string Type { get; protected set; }

        /// <summary>
        /// Value of the <see cref="RocketLeagueProperty"/>
        /// </summary>
        public object Value
        {
            get; protected set;
        }

        /// <summary>
        /// Default Constructor of the <see cref="RocketLeagueProperty"/>
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
    }
}
