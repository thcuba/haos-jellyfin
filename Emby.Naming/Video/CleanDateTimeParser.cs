using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Emby.Naming.Video
{
    /// <summary>
    /// <see href="http://kodi.wiki/view/Advancedsettings.xml#video" />.
    /// </summary>
    public static class CleanDateTimeParser
    {
        /// <summary>
        /// Attempts to clean the name span.
        /// </summary>
        /// <param name="name">Name of video as span.</param>
        /// <param name="cleanDateTimeRegexes">Optional list of regexes to clean the name.</param>
        /// <returns>Returns <see cref="CleanDateTimeResult"/> object.</returns>
        public static CleanDateTimeResult Clean(ReadOnlySpan<char> name, IReadOnlyList<Regex> cleanDateTimeRegexes)
        {
            if (name.IsEmpty)
            {
                return new CleanDateTimeResult(string.Empty);
            }

            // Optimization: Defer string materialization until regex execution,
            // avoiding heap string allocation when inputs or regex collections are empty.
            string? current = null;

            var len = cleanDateTimeRegexes.Count;
            for (int i = 0; i < len; i++)
            {
                current ??= name.ToString();
                if (TryClean(current, cleanDateTimeRegexes[i], out var result))
                {
                    return result;
                }
            }

            return new CleanDateTimeResult(current ?? name.ToString());
        }

        /// <summary>
        /// Attempts to clean the name.
        /// </summary>
        /// <param name="name">Name of video.</param>
        /// <param name="cleanDateTimeRegexes">Optional list of regexes to clean the name.</param>
        /// <returns>Returns <see cref="CleanDateTimeResult"/> object.</returns>
        public static CleanDateTimeResult Clean(string name, IReadOnlyList<Regex> cleanDateTimeRegexes)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new CleanDateTimeResult(name);
            }

            var len = cleanDateTimeRegexes.Count;
            for (int i = 0; i < len; i++)
            {
                if (TryClean(name, cleanDateTimeRegexes[i], out var result))
                {
                    return result;
                }
            }

            return new CleanDateTimeResult(name);
        }

        private static bool TryClean(string name, Regex expression, out CleanDateTimeResult result)
        {
            var match = expression.Match(name);

            if (match.Success
                && match.Groups.Count == 5
                && match.Groups[1].Success
                && match.Groups[2].Success
                && int.TryParse(match.Groups[2].ValueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year))
            {
                // Optimization: Use ValueSpan to trim trailing whitespace on ReadOnlySpan<char> before allocating the single resulting string, avoiding intermediate string allocation.
                result = new CleanDateTimeResult(match.Groups[1].ValueSpan.TrimEnd().ToString(), year);
                return true;
            }

            result = default;
            return false;
        }
    }
}
