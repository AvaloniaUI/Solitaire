using System;
using System.Collections.Generic;
using System.Linq;

namespace SolitaireAvalonia.Utils;

public static class LinqExtensions
{
    private static Random randomizer = new ();

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy((item) => randomizer.Next());
    }
}
 