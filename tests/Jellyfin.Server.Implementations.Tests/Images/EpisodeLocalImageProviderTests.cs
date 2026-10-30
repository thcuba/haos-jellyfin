using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.LocalMetadata.Images;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.Images;

public class EpisodeLocalImageProviderTests
{
    [Fact]
    public void Supports_EpisodeWithLocalMetadata_ReturnsTrue()
    {
        var provider = new EpisodeLocalImageProvider();
        var episodeMock = new Mock<Episode> { CallBase = true };
        episodeMock.Setup(e => e.SupportsLocalMetadata).Returns(true);
        episodeMock.Object.Path = "/media/TV/Show/Season 1/S01E01.mkv";

        Assert.True(provider.Supports(episodeMock.Object));
    }

    [Fact]
    public void GetImages_ParentPathFilesMatching_ReturnsPrimaryImages()
    {
        var provider = new EpisodeLocalImageProvider();
        var episode = new Episode
        {
            Path = Path.Combine("media", "TV", "Show", "Season 1", "S01E01 - Pilot.mkv")
        };

        var parentPath = Path.GetDirectoryName(episode.Path)!;

        var directoryService = new Mock<IDirectoryService>();
        directoryService.Setup(d => d.GetFiles(parentPath))
            .Returns(new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = Path.Combine(parentPath, "S01E01 - Pilot.jpg"),
                    Extension = ".jpg",
                    IsDirectory = false
                },
                new FileSystemMetadata
                {
                    FullName = Path.Combine(parentPath, "S01E01 - Pilot-thumb.png"),
                    Extension = ".png",
                    IsDirectory = false
                },
                new FileSystemMetadata
                {
                    FullName = Path.Combine(parentPath, "S01E02 - Pilot-thumb.png"),
                    Extension = ".png",
                    IsDirectory = false
                },
                new FileSystemMetadata
                {
                    FullName = Path.Combine(parentPath, "S01E01 - Pilot-poster.jpg"),
                    Extension = ".jpg",
                    IsDirectory = false
                }
            });

        directoryService.Setup(d => d.GetDirectories(parentPath))
            .Returns(new List<FileSystemMetadata>());

        var images = provider.GetImages(episode, directoryService.Object).ToList();

        Assert.Equal(2, images.Count);
        Assert.All(images, img => Assert.Equal(ImageType.Primary, img.Type));
        Assert.Contains(images, img => img.FileInfo.FullName.EndsWith("S01E01 - Pilot.jpg", StringComparison.Ordinal));
        Assert.Contains(images, img => img.FileInfo.FullName.EndsWith("S01E01 - Pilot-thumb.png", StringComparison.Ordinal));
    }

    [Fact]
    public void GetImages_MetadataSubfolderMatching_ReturnsPrimaryImages()
    {
        var provider = new EpisodeLocalImageProvider();
        var episode = new Episode
        {
            Path = Path.Combine("media", "TV", "Show", "Season 1", "S01E01.mkv")
        };

        var parentPath = Path.GetDirectoryName(episode.Path)!;
        var metadataDir = Path.Combine(parentPath, "metadata");

        var directoryService = new Mock<IDirectoryService>();
        directoryService.Setup(d => d.GetFiles(parentPath))
            .Returns(new List<FileSystemMetadata>());

        directoryService.Setup(d => d.GetDirectories(parentPath))
            .Returns(new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = metadataDir,
                    Name = "metadata",
                    IsDirectory = true
                }
            });

        directoryService.Setup(d => d.GetFiles(metadataDir))
            .Returns(new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = Path.Combine(metadataDir, "S01E01-thumb.jpg"),
                    Extension = ".jpg",
                    IsDirectory = false
                }
            });

        var images = provider.GetImages(episode, directoryService.Object).ToList();

        Assert.Single(images);
        Assert.Equal(ImageType.Primary, images[0].Type);
        Assert.EndsWith("S01E01-thumb.jpg", images[0].FileInfo.FullName, StringComparison.Ordinal);
    }
}
