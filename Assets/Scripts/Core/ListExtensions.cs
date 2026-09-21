using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GridLock.Core
{
    public static class ListExtensions
    {
        private static readonly Random kRandom = new Random();

        public static T PopRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                throw new ArgumentException("Cannot call PopRandom on null or empty list");
            }

            int index = kRandom.Next(0, list.Count);
            T output = list[index];
            list.RemoveAt(index);
            return output;
        }

        public static T RandomItem<T>(this List<T> list) => list[kRandom.Next(0, list.Count)];

        public static T RandomItem<T>(this IReadOnlyList<T> list) => list[kRandom.Next(0, list.Count)];

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
