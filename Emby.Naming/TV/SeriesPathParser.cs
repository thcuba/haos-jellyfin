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
                }
            }

            return result;
        }
    }
}
