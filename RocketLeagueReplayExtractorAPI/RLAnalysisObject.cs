
namespace RocketLeagueReplayParserAPI
{
    /// <summary>
    /// Class the Represents the Analysis Object used in the Replay Analysis API. Defines the base properties and methods for all Objects containing useful information in this API
    /// </summary>
    public abstract class RLAnalysisObject<T>
    {
        /// <summary>
        /// Properties of the <typeparamref name="T"/>
        /// </summary>
        public RocketLeaguePropertyDictionary Properties { get; internal set; } = new RocketLeaguePropertyDictionary();
    }
}
