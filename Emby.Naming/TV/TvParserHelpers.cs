using System;
using MediaBrowser.Model.Entities;

namespace Emby.Naming.TV;

/// <summary>
/// Helper class for TV metadata parsing.
/// </summary>
public static class TvParserHelpers
{
    private static readonly string[] _continuingState = ["Pilot", "Returning Series", "Returning"];
    private static readonly string[] _endedState = ["Cancelled", "Canceled"];

    /// <summary>
    /// Tries to parse a string into <see cref="SeriesStatus"/>.
    /// </summary>
    /// <param name="status">The status string.</param>
    /// <param name="enumValue">The <see cref="SeriesStatus"/>.</param>
    /// <returns>Returns true if parsing was successful.</returns>
    public static bool TryParseSeriesStatus(string? status, out SeriesStatus? enumValue)
    {
        if (status is not null)
        {
            if (Enum.TryParse(status, true, out SeriesStatus seriesStatus))
            {
                enumValue = seriesStatus;
                return true;
            }

            // Optimization: Replace LINQ .Contains with direct loops using StringComparison.OrdinalIgnoreCase to eliminate LINQ allocation and StringComparer interface dispatch.
            for (var i = 0; i < _continuingState.Length; i++)
            {
                if (string.Equals(status, _continuingState[i], StringComparison.OrdinalIgnoreCase))
                {
                    enumValue = SeriesStatus.Continuing;
                    return true;
                }
            }

            for (var i = 0; i < _endedState.Length; i++)
            {
                if (string.Equals(status, _endedState[i], StringComparison.OrdinalIgnoreCase))
                {
                    enumValue = SeriesStatus.Ended;
                    return true;
                }
            }
        }

        enumValue = null;
        return false;
    }
}
