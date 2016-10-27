using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroveTools.NET.Framework
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Adds a key and value to a dictionary if it does not already contain the given key
        /// </summary>
        public static void AddIfMissing<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key)) dictionary.Add(key, value);
        }

        public static void AddIfMissing<T>(this IList<T> list, T value)
        {
            if (!list.Contains(value)) list.Add(value);
        }

        /// <summary>
        /// Returns a boolean value indicating whether the string is missing
        /// </summary>
        public static bool IsStringMissing(this string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim() == string.Empty;
        }
    }
}
