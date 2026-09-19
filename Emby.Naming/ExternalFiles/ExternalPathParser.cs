using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emby.Naming.Common;
using Jellyfin.Extensions;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Globalization;

namespace Emby.Naming.ExternalFiles
{
    /// <summary>
    /// External media file parser class.
    /// </summary>
    public class ExternalPathParser
    {
        private readonly NamingOptions _namingOptions;
        private readonly DlnaProfileType _type;
        private readonly ILocalizationManager _localizationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalPathParser"/> class.
        /// </summary>
        /// <param name="localizationManager">The localization manager.</param>
        /// <param name="namingOptions">The <see cref="NamingOptions"/> object containing FileExtensions, MediaDefaultFlags, MediaForcedFlags and MediaFlagDelimiters.</param>
        /// <param name="type">The <see cref="DlnaProfileType"/> of the parsed file.</param>
        public ExternalPathParser(NamingOptions namingOptions, ILocalizationManager localizationManager, DlnaProfileType type)
        {
            _localizationManager = localizationManager;
            _namingOptions = namingOptions;
            _type = type;
        }

        /// <summary>
        /// Parse filename and extract information.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="extraString">Part of the filename only containing the extra information.</param>
        /// <returns>Returns null or an <see cref="ExternalPathParserResult"/> object if parsing is successful.</returns>
        public ExternalPathParserResult? ParseFile(string path, string? extraString)
        {
            if (path.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(path.AsSpan());

            // .idx carries VobSub per-track language metadata. Recognize it here rather
            // than adding it to NamingOptions.SubtitleFileExtensions, which also gates
            // subtitle uploads/saves.
            var isVobSubIndex = _type == DlnaProfileType.Subtitle && extension.Equals(".idx", StringComparison.OrdinalIgnoreCase);

            if (!isVobSubIndex
                && !(_type == DlnaProfileType.Subtitle && _namingOptions.SubtitleFileExtensions.Contains(extension, StringComparison.OrdinalIgnoreCase))
                && !(_type == DlnaProfileType.Audio && _namingOptions.AudioFileExtensions.Contains(extension, StringComparison.OrdinalIgnoreCase))
                && !(_type == DlnaProfileType.Lyric && _namingOptions.LyricFileExtensions.Contains(extension, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            var pathInfo = new ExternalPathParserResult(path);

            if (string.IsNullOrEmpty(extraString))
            {
                return pathInfo;
            }

            foreach (var separator in _namingOptions.MediaFlagDelimiters)
            {
                var languageString = extraString;
                var titleString = string.Empty;
                const int SeparatorLength = 1;

                while (languageString.Length > 0)
                {
                    int lastSeparator = languageString.LastIndexOf(separator);

                    if (lastSeparator == -1)
                    {
                        break;
                    }

                    var currentSliceSpan = languageString.AsSpan(lastSeparator);
                    var currentSliceWithoutSeparatorSpan = currentSliceSpan[SeparatorLength..];

                    // Optimization: Use ReadOnlySpan<char> flag matching and slice operations to eliminate
                    // intermediate string allocations and LINQ delegate/enumerator allocations during flag parsing.
                    if (ContainsFlag(currentSliceWithoutSeparatorSpan, _namingOptions.MediaDefaultFlags))
                    {
                        pathInfo.IsDefault = true;
                        var currentSlice = currentSliceSpan.ToString();
                        extraString = extraString.Replace(currentSlice, string.Empty, StringComparison.OrdinalIgnoreCase);
                        languageString = languageString[..lastSeparator];
                        continue;
                    }

                    if (ContainsFlag(currentSliceWithoutSeparatorSpan, _namingOptions.MediaForcedFlags))
                    {
                        pathInfo.IsForced = true;
                        var currentSlice = currentSliceSpan.ToString();
                        extraString = extraString.Replace(currentSlice, string.Empty, StringComparison.OrdinalIgnoreCase);
                        languageString = languageString[..lastSeparator];
                        continue;
                    }

                    // Try to translate to three character code
                    var currentSliceWithoutSeparator = currentSliceWithoutSeparatorSpan.ToString();
                    var culture = _localizationManager.FindLanguageInfo(currentSliceWithoutSeparator);

                    if (culture is not null && pathInfo.Language is null)
                    {
                        pathInfo.Language = culture.Name.Contains('-', StringComparison.OrdinalIgnoreCase)
                                          ? culture.Name
                                          : culture.ThreeLetterISOLanguageName;
                        var currentSlice = currentSliceSpan.ToString();
                        extraString = extraString.Replace(currentSlice, string.Empty, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (culture is not null && pathInfo.Language == "hin")
                    {
                        // Hindi language code "hi" collides with a hearing impaired flag - use as Hindi only if no other language is set
                        pathInfo.IsHearingImpaired = true;
                        pathInfo.Language = culture.Name.Contains('-', StringComparison.OrdinalIgnoreCase)
                                          ? culture.Name
                                          : culture.ThreeLetterISOLanguageName;
                        var currentSlice = currentSliceSpan.ToString();
                        extraString = extraString.Replace(currentSlice, string.Empty, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (EqualsFlag(currentSliceWithoutSeparatorSpan, _namingOptions.MediaHearingImpairedFlags))
                    {
                        pathInfo.IsHearingImpaired = true;
                        var currentSlice = currentSliceSpan.ToString();
                        extraString = extraString.Replace(currentSlice, string.Empty, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        titleString = currentSliceSpan.ToString() + titleString;
                    }

                    languageString = languageString[..lastSeparator];
                }

                pathInfo.Title = titleString.Length >= SeparatorLength ? titleString[SeparatorLength..] : null;
            }

            return pathInfo;
        }

        private static bool ContainsFlag(ReadOnlySpan<char> slice, IReadOnlyList<string> flags)
        {
            for (var i = 0; i < flags.Count; i++)
            {
                if (slice.Contains(flags[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool EqualsFlag(ReadOnlySpan<char> slice, IReadOnlyList<string> flags)
        {
            for (var i = 0; i < flags.Count; i++)
            {
                if (slice.Equals(flags[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
