using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GridLock.Utility
{
    public static class ListExtensions
    {
        /// <summary>
        /// Returns a randoms item from the list and removes it
        /// </summary>
        public static T PopRandom<T>(this List<T> list)
        {
            Debug.Assert(list != null && list.Count > 0, "Error: trying to call PopRandom on null or empty list");
            
            int index = Random.Range(0, list.Count);
            T output = list[index];
            list.RemoveAt(index);
            return output;
        }
        
        public static T RandomItem<T>(this List<T> list) => list[Random.Range(0, list.Count)];

        public static T RandomItem<T>(this IReadOnlyList<T> list) => list[Random.Range(0, list.Count)];
        
        public static string EnumerableToString(this IEnumerable enumerable)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in enumerable)
            {
                builder.Append(item).Append(", ");
            }

            if (builder.Length > 1)
            {
                builder.Length = builder.Length - 2;
            }

            return builder.ToString();
        }
    }
}
