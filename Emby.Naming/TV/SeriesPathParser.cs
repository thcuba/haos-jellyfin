using System;
using Emby.Naming.Common;

namespace Emby.Naming.TV
{
    /// <summary>
    /// Used to parse information about series from paths containing more information that only the series name.
    /// Uses the same regular expressions as the EpisodePathParser but have different success criteria.
    /// </summary>
    public static class SeriesPathParser
    {
        /// <summary>
        /// Parses information about series from path.
        /// </summary>
        /// <param name="options"><see cref="NamingOptions"/> object containing EpisodeExpressions and MultipleEpisodeExpressions.</param>
        /// <param name="path">Path.</param>
        /// <returns>Returns <see cref="SeriesPathParserResult"/> object.</returns>
        public static SeriesPathParserResult Parse(NamingOptions options, string path)
        {
            foreach (var expression in options.EpisodeExpressions)
            {
                // Optimistic expressions (bare numbers, "01.blah", etc.) are only meant for
                // episode parsing and produce false series names on release folder names like
                // "Silo.S03.1080p.WEB-DL..." (e.g. reading "264" as S02E64). Skip them here.
                if (expression.IsOptimistic)
                {
                    continue;
                }

                var currentResult = Parse(path, expression);
                if (currentResult.Success)
                {
                    return currentResult;
                }
            }

            return new SeriesPathParserResult();
        }

        private static SeriesPathParserResult Parse(string name, EpisodeExpression expression)
        {
            var result = new SeriesPathParserResult();

            var match = expression.Regex.Match(name);

            if (match.Success && match.Groups.Count >= 3)
            {
                if (expression.IsNamed)
                {
<<<<<<< HEAD
                    var seasonGroup = match.Groups["seasonnumber"];
                    if (!seasonGroup.ValueSpan.IsEmpty)
                    {
                        var seriesGroup = match.Groups["seriesname"];
                        if (seriesGroup.Success)
                        {
                            // Optimization: Use span trimming on ReadOnlySpan<char> to trim trailing/leading chars (" _.-")
                            // and allocate only a single resulting string when parsing succeeds, avoiding intermediate string
                            // allocations and char[] params array heap allocations.
                            var trimmedSeriesName = seriesGroup.ValueSpan.Trim(" _.-");
                            if (!trimmedSeriesName.IsEmpty)
                            {
                                result.SeriesName = trimmedSeriesName.ToString();
                                result.Success = true;
                            }
                        }
                    }
=======
                    // Reject implausible season numbers (e.g. resolutions like 1280x720
                    // read as S1280E720), mirroring EpisodePathParser.
                    var seasonNumberGroup = match.Groups["seasonnumber"];
                    if (seasonNumberGroup.Success
                        && int.TryParse(seasonNumberGroup.ValueSpan, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var seasonNumber)
                        && ((seasonNumber >= 200 && seasonNumber < 1928) || seasonNumber > 2500))
                    {
                        return result;
                    }

                    result.SeriesName = match.Groups["seriesname"].Value;
                    result.Success = !string.IsNullOrEmpty(result.SeriesName) && !seasonNumberGroup.ValueSpan.IsEmpty;
>>>>>>> upstream/release-12.z
                }
            }

            return result;
        }
    }
}
