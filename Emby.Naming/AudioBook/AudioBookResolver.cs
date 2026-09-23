using System;
using System.IO;
using Emby.Naming.Common;
using Jellyfin.Extensions;

namespace Emby.Naming.AudioBook
{
    /// <summary>
    /// Resolve specifics (path, container, partNumber, chapterNumber) about audiobook file.
    /// </summary>
    public class AudioBookResolver
    {
        private readonly NamingOptions _options;
        private readonly AudioBookFilePathParser _audioBookFilePathParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBookResolver"/> class.
        /// </summary>
        /// <param name="options"><see cref="NamingOptions"/> containing AudioFileExtensions and also used to pass to AudioBookFilePathParser.</param>
        public AudioBookResolver(NamingOptions options)
        {
            _options = options;
            _audioBookFilePathParser = new AudioBookFilePathParser(options);
        }

        /// <summary>
        /// Resolve specifics (path, container, partNumber, chapterNumber) about audiobook file.
        /// </summary>
        /// <param name="path">Path to audiobook file.</param>
        /// <returns>Returns <see cref="AudioBookResolver"/> object.</returns>
        public AudioBookFileInfo? Resolve(string path)
        {
            // Optimization: Use Path.GetFileNameWithoutExtension(AsSpan()).IsEmpty to avoid allocating heap strings.
            if (path.Length == 0 || Path.GetFileNameWithoutExtension(path.AsSpan()).IsEmpty)
            {
                // Return null to indicate this path will not be used, instead of stopping whole process with exception
                return null;
            }

            // Optimization: Use Path.GetExtension(AsSpan()) to get ReadOnlySpan<char> extension without string allocation.
            var extension = Path.GetExtension(path.AsSpan());

            // Check supported extensions
            if (!_options.AudioFileExtensions.Contains(extension, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var containerSpan = extension.TrimStart('.');
            var container = containerSpan.IsEmpty ? string.Empty : containerSpan.ToString();

            // Optimization: Use cached AudioBookFilePathParser instance to avoid heap object allocation per call.
            var parsingResult = _audioBookFilePathParser.Parse(path);

            return new AudioBookFileInfo(
                path,
                container,
                chapterNumber: parsingResult.ChapterNumber,
                partNumber: parsingResult.PartNumber);
        }
    }
}
