using System;
using System.Collections.Generic;
using System.Linq;

namespace SolitaireAvalonia.Utils;

public static class LinqExtensions
{ 
    /// </summary>
    private static Random random = new ();

    /// <summary>
    /// Shuffles the specified list.
    /// </summary> 
    public static void Shuffle<T>(this IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
 