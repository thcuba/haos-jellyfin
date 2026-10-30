using System;
using System.Collections.Generic;
using System.IO;
using Jellyfin.Extensions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;

namespace MediaBrowser.LocalMetadata.Images
{
    /// <summary>
    /// Episode local image provider.
    /// </summary>
    public class EpisodeLocalImageProvider : ILocalImageProvider, IHasOrder
    {
        /// <inheritdoc />
        public string Name => "Local Images";

        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        public bool Supports(BaseItem item)
        {
            return item is Episode && item.SupportsLocalMetadata;
        }

        /// <inheritdoc />
        public IEnumerable<LocalImageInfo> GetImages(BaseItem item, IDirectoryService directoryService)
        {
            var parentPath = Path.GetDirectoryName(item.Path);
            if (parentPath is null)
            {
                return Array.Empty<LocalImageInfo>();
            }

            var parentPathFiles = directoryService.GetFiles(parentPath);
            // Use ReadOnlySpan<char> directly to avoid string allocation for episode filename without extension
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(item.Path.AsSpan());

            var images = GetImageFilesFromFolder(nameWithoutExtension, parentPathFiles);

            // Use an indexed loop instead of LINQ FirstOrDefault to avoid delegate and closure allocations
            var subDirs = directoryService.GetDirectories(parentPath);
            FileSystemMetadata? metadataSubDir = null;
            for (var i = 0; i < subDirs.Count; i++)
            {
                var dir = subDirs[i];
                if (dir.Name.Equals("metadata", StringComparison.Ordinal))
                {
                    metadataSubDir = dir;
                    break;
                }
            }

            if (metadataSubDir is not null)
            {
                var files = directoryService.GetFiles(metadataSubDir.FullName);
                images.AddRange(GetImageFilesFromFolder(nameWithoutExtension, files));
            }

            return images;
        }

        private List<LocalImageInfo> GetImageFilesFromFolder(ReadOnlySpan<char> filenameWithoutExtension, List<FileSystemMetadata> filePaths)
        {
            var list = new List<LocalImageInfo>(1);

            foreach (var i in filePaths)
            {
                if (i.IsDirectory)
                {
                    continue;
                }

                if (BaseItem.SupportedImageExtensions.Contains(i.Extension.AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    var currentNameWithoutExtension = Path.GetFileNameWithoutExtension(i.FullName.AsSpan());

                    if (filenameWithoutExtension.Equals(currentNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(new LocalImageInfo { FileInfo = i, Type = ImageType.Primary });
                    }
                    // Perform zero-allocation span checks for "-thumb" suffix matching instead of string concatenation
                    else if (currentNameWithoutExtension.EndsWith("-thumb", StringComparison.OrdinalIgnoreCase)
                        && currentNameWithoutExtension.Length == filenameWithoutExtension.Length + 6
                        && currentNameWithoutExtension.StartsWith(filenameWithoutExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(new LocalImageInfo { FileInfo = i, Type = ImageType.Primary });
                    }
                }
            }

            return list;
        }
    }
}
