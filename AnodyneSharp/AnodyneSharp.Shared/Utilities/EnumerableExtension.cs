#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Utilities
{
    public static class EnumerableExtension
    {
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source)
            where T : class
        {
            foreach (var item in source)
            {
                if (item is not null)
                    yield return item;
            }
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source)
            where T : struct
        {
            foreach (var item in source)
            {
                if (item is not null)
                    yield return item.Value;
            }
        }
    }
}
