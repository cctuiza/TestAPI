using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Utils
{
  

public static class EnumerableExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        // error checking etc removed for brevity

        Random rng = new Random();
        T[] sourceArray = source.ToArray();

        for (int n = 0; n < sourceArray.Length; n++)
        {
            int k = rng.Next(n, sourceArray.Length);
            yield return sourceArray[k];

            sourceArray[k] = sourceArray[n];
        }
    }
}
}