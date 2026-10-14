
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports;

public static class EnumerableReportExtensions
{
    public static IEnumerable<T> TopBy<T>(
        this IEnumerable<T> source,
        Func<T, int> keySelector,
        int count,
        Func<T, string>? tieBreaker = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        IOrderedEnumerable<T> ordered =
            source.OrderByDescending(keySelector);

        if (tieBreaker is not null)
        {
            ordered = ordered.ThenBy(
                tieBreaker,
                StringComparer.Ordinal);
        }

        return ordered.Take(count);
    }
}
