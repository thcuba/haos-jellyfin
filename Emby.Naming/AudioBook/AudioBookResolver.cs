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
        private readonly AudioBookFilePathParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBookResolver"/> class.
        /// </summary>
        /// <param name="options"><see cref="NamingOptions"/> containing AudioFileExtensions and also used to pass to AudioBookFilePathParser.</param>
        public AudioBookResolver(NamingOptions options)
        {
            _options = options;
            // Cache stateless parser instance to prevent heap allocation per resolution call.
            _parser = new AudioBookFilePathParser(options);
        }

        /// <summary>
        /// Resolve specifics (path, container, partNumber, chapterNumber) about audiobook file.
        /// </summary>
        /// <param name="path">Path to audiobook file.</param>
        /// <returns>Returns <see cref="AudioBookResolver"/> object.</returns>
        public AudioBookFileInfo? Resolve(string path)
        {
            if (path.Length == 0 || Path.GetFileNameWithoutExtension(path.AsSpan()).IsEmpty)
            {
                // Return null to indicate this path will not be used, instead of stopping whole process with exception
                return null;
            }

            var extension = Path.GetExtension(path.AsSpan());

            // Check supported extensions using ReadOnlySpan<char> extension matching to avoid heap string allocations
            if (!_options.AudioFileExtensions.Contains(extension, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var container = extension.TrimStart('.').ToString();

            var parsingResult = _parser.Parse(path);

            return new AudioBookFileInfo(
                path,
                container,
                chapterNumber: parsingResult.ChapterNumber,
                partNumber: parsingResult.PartNumber);
        }
    }
}
