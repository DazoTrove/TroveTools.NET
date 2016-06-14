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

        /// <summary>
        /// Returns a boolean value indicating whether the string is missing
        /// </summary>
        public static bool IsStringMissing(this string value)
        {
            return String.IsNullOrEmpty(value) || value.Trim() == String.Empty;
        }
    }
}
