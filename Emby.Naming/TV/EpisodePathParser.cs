using System;
using System.Collections.Generic;
using System.Globalization;
using Emby.Naming.Common;

namespace Emby.Naming.TV
{
    /// <summary>
    /// Used to parse information about episode from path.
    /// </summary>
    public class EpisodePathParser
    {
        private readonly NamingOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="EpisodePathParser"/> class.
        /// </summary>
        /// <param name="options"><see cref="NamingOptions"/> object containing EpisodeExpressions and MultipleEpisodeExpressions.</param>
        public EpisodePathParser(NamingOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Parses information about episode from path.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="isDirectory">Is path for a directory or file.</param>
        /// <param name="isNamed">Do we want to use IsNamed expressions.</param>
        /// <param name="isOptimistic">Do we want to use Optimistic expressions.</param>
        /// <param name="supportsAbsoluteNumbers">Do we want to use expressions supporting absolute episode numbers.</param>
        /// <param name="fillExtendedInfo">Should we attempt to retrieve extended information.</param>
        /// <returns>Returns <see cref="EpisodePathParserResult"/> object.</returns>
        public EpisodePathParserResult Parse(
            string path,
            bool isDirectory,
            bool? isNamed = null,
            bool? isOptimistic = null,
            bool? supportsAbsoluteNumbers = null,
            bool fillExtendedInfo = true)
        {
            // Added to be able to use regex patterns which require a file extension.
            // There were no failed tests without this block, but to be safe, we can keep it until
            // the regex which require file extensions are modified so that they don't need them.
            if (isDirectory)
            {
                path += ".mp4";
            }

            EpisodePathParserResult? result = null;

            foreach (var expression in _options.EpisodeExpressions)
            {
                if (supportsAbsoluteNumbers.HasValue
                    && expression.SupportsAbsoluteEpisodeNumbers != supportsAbsoluteNumbers.Value)
                {
                    continue;
                }

                if (isNamed.HasValue && expression.IsNamed != isNamed.Value)
                {
                    continue;
                }

                if (isOptimistic.HasValue && expression.IsOptimistic != isOptimistic.Value)
                {
                    continue;
                }

                var currentResult = Parse(path, expression);
                if (currentResult.Success)
                {
                    result = currentResult;
                    break;
                }
            }

            if (result is not null && fillExtendedInfo)
            {
                FillAdditional(path, result);
            }

            return result ?? new EpisodePathParserResult();
        }

        private static EpisodePathParserResult Parse(string name, EpisodeExpression expression)
        {
            var result = new EpisodePathParserResult();

            // This is a hack to handle wmc naming
            if (expression.IsByDate)
            {
                name = name.Replace('_', '-');
            }

            var match = expression.Regex.Match(name);

            // (Full)(Season)(Episode)(Extension)
            if (match.Success && match.Groups.Count >= 3)
            {
                if (expression.IsByDate)
                {
                    DateTime date;
                    if (expression.DateTimeFormats.Length > 0)
                    {
                        if (DateTime.TryParseExact(
                            match.Groups[0].ValueSpan,
                            expression.DateTimeFormats,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out date))
                        {
                            result.Year = date.Year;
                            result.Month = date.Month;
                            result.Day = date.Day;
                            result.Success = true;
                        }
                    }
                    else if (DateTime.TryParse(match.Groups[0].ValueSpan, CultureInfo.InvariantCulture, out date))
                    {
                        result.Year = date.Year;
                        result.Month = date.Month;
                        result.Day = date.Day;
                        result.Success = true;
                    }

                    // TODO: Only consider success if date successfully parsed?
                    result.Success = true;
                }
                else if (expression.IsNamed)
                {
                    if (int.TryParse(match.Groups["seasonnumber"].ValueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                    {
                        result.SeasonNumber = num;
                    }

                    if (int.TryParse(match.Groups["epnumber"].ValueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
                    {
                        result.EpisodeNumber = num;
                    }

                    var endingNumberGroup = match.Groups["endingepnumber"];
                    if (endingNumberGroup.Success)
                    {
                        // Will only set EndingEpisodeNumber if the captured number is not followed by additional numbers
                        // or a 'p' or 'i' as what you would get with a pixel resolution specification.
                        // It avoids erroneous parsing of something like "series-s09e14-1080p.mkv" as a multi-episode from E14 to E108
                        int nextIndex = endingNumberGroup.Index + endingNumberGroup.Length;
                        if (nextIndex >= name.Length
                            || !(char.IsAsciiDigit(name[nextIndex]) || name[nextIndex] is 'i' or 'I' or 'p' or 'P'))
                        {
                            // A range cannot end before it starts, so a lower number belongs to the episode title rather than to a range.
                            if (int.TryParse(endingNumberGroup.ValueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out num)
                                && num >= result.EpisodeNumber)
                            {
                                result.EndingEpisodeNumber = num;
                            }
                        }
                    }

                    var seriesNameGroup = match.Groups["seriesname"];
                    if (seriesNameGroup.Success)
                    {
                        // Optimization: Perform span trimming directly on ValueSpan before allocating string to eliminate intermediate untrimmed string allocation.
                        var trimmedSpan = seriesNameGroup.ValueSpan.Trim().Trim("_.-").Trim();
                        result.SeriesName = trimmedSpan.ToString();
                    }

                    result.Success = result.EpisodeNumber.HasValue;
                }
                else
                {
                    if (int.TryParse(match.Groups[1].ValueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num))
                    {
                        result.SeasonNumber = num;
                    }

                    if (int.TryParse(match.Groups[2].ValueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
                    {
                        result.EpisodeNumber = num;
                    }

                    result.Success = result.EpisodeNumber.HasValue;
                }

                // Invalidate match when the season is 200 through 1927 or above 2500
                // because it is an error unless the TV show is intentionally using false season numbers.
                // It avoids erroneous parsing of something like "Series Special (1920x1080).mkv" as being season 1920 episode 1080.
                if ((result.SeasonNumber >= 200 && result.SeasonNumber < 1928)
                    || result.SeasonNumber > 2500)
                {
                    result.Success = false;
                }

                result.IsByDate = expression.IsByDate;
            }

            return result;
        }

        private void FillAdditional(string path, EpisodePathParserResult info)
        {
            // Optimization: Iterate expression lists directly to avoid allocating temporary lists, LINQ enumerators, and List.InsertRange operations.
            if (string.IsNullOrEmpty(info.SeriesName))
            {
                foreach (var expression in _options.EpisodeExpressions)
                {
                    if (expression.IsNamed && ProcessAdditionalExpression(path, info, expression))
                    {
                        return;
                    }
                }
            }

            foreach (var expression in _options.MultipleEpisodeExpressions)
            {
                if (expression.IsNamed && ProcessAdditionalExpression(path, info, expression))
                {
                    return;
                }
            }
        }

        private static bool ProcessAdditionalExpression(string path, EpisodePathParserResult info, EpisodeExpression expression)
        {
            var result = Parse(path, expression);

            if (!result.Success)
            {
                return false;
            }

            if (string.IsNullOrEmpty(info.SeriesName))
            {
                info.SeriesName = result.SeriesName ?? string.Empty;
            }

            if (!info.EndingEpisodeNumber.HasValue && result.EndingEpisodeNumber >= info.EpisodeNumber)
            {
                info.EndingEpisodeNumber = result.EndingEpisodeNumber;
            }

            return !string.IsNullOrEmpty(info.SeriesName)
                && (!info.EpisodeNumber.HasValue || info.EndingEpisodeNumber.HasValue);
        }
    }
}
