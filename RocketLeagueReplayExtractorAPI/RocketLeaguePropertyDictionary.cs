
namespace RocketLeagueReplayParserAPI
{
    public class RocketLeaguePropertyDictionary : Dictionary<string, RocketLeagueProperty>
    {
        /// <summary>
        /// Tries to the Get the Property from the Replay Info Object <paramref name="defaultValue"/> if it is not found
        /// </summary>
        /// <typeparam name="T"> The Return Type of the Object </typeparam>
        /// <param name="key"> The Key for the Value </param>
        /// <param name="defaultValue"> The Default Value to Return if value is not Found </param>
        /// <returns> The Value of the Property being searched, or <paramref name="defaultValue"/> if not found </returns>
        public T TryGetProperty<T>(string key, T defaultValue)
        {
            if (TryGetValue(key, out RocketLeagueProperty value))
                try
                {
                    return (T)value.Value;
                }
                catch
                {
                    throw new Exception($"Value {value.Value} is not of type {typeof(T)}");
                }
            else
                return defaultValue;
        }

        /// <summary>
        /// Tries to the Get the Property from the Replay Info Object, otherwise throws a KeyNotFoundException
        /// </summary>
        /// <typeparam name="T"> The Return Type of the Object </typeparam>
        /// <param name="key"> The Key for the Value </param>
        /// <returns> The Value of the Property being searched, or a KeyNotFoundException </returns>
        public T TryGetProperty<T>(string key)
        {
            if (TryGetValue(key, out RocketLeagueProperty value))
                try
                {
                    return (T)value.Value;
                }
                catch
                {
                    throw new Exception($"Value {value.Value} is not of type {typeof(T)}");
                }
            else
                throw new KeyNotFoundException($"Key {key} not found in the Dictionary");
        }
    }
}
