namespace OJS.Common.Extensions
{
    using System.Collections.Generic;

    public static class DictionaryExtensions
    {
        public static TValue GetValuerOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key) => dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);

        public static TValue GetValuerOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue) => dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
    }
}