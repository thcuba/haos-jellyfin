using System;
using System.IO;
using Emby.Naming.Common;
using Emby.Server.Implementations.Library.Resolvers;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.Library;

public class PhotoResolverTests
{
    private static readonly string[] _supportedFormats = ["jpg", "jpeg", "png", "webp", "gif", "bmp", "tif", "tiff"];

    [Theory]
    [InlineData("vacation.jpg", true)]
    [InlineData("family_photo.PNG", true)]
    [InlineData("portrait.webp", true)]
    [InlineData("landscape_trip.jpg", false)] // Starts with "landscape" (ignored)
    [InlineData("folder.jpg", false)] // Starts with "folder" (ignored)
    [InlineData("thumb.png", false)] // Starts with "thumb" (ignored)
    [InlineData("poster.jpeg", false)] // Starts with "poster" (ignored)
    [InlineData("cover.jpg", false)] // Starts with "cover" (ignored)
    [InlineData("backdrop.png", false)] // Starts with "backdrop" (ignored)
    [InlineData("fanart.webp", false)] // Starts with "fanart" (ignored)
    [InlineData("logo.png", false)] // Starts with "logo" (ignored)
    [InlineData("default.jpg", false)] // Starts with "default" (ignored)
    [InlineData("document.pdf", false)] // Unsupported extension
    [InlineData("video.mkv", false)] // Unsupported extension
    public void IsImageFile_VariousPaths_ReturnsExpected(string filename, bool expected)
    {
        var imageProcessorMock = new Mock<IImageProcessor>();
        imageProcessorMock.Setup(x => x.SupportedInputFormats).Returns(_supportedFormats);

        var path = Path.Combine("/media/photos", filename);

        // Test string overload
        Assert.Equal(expected, PhotoResolver.IsImageFile(path, imageProcessorMock.Object));

        // Test ReadOnlySpan<char> overload
        Assert.Equal(expected, PhotoResolver.IsImageFile(path.AsSpan(), imageProcessorMock.Object));
    }

    [Fact]
    public void Resolve_PhotoInPhotosCollection_ReturnsPhoto()
    {
        var imageProcessorMock = new Mock<IImageProcessor>();
        imageProcessorMock.Setup(x => x.SupportedInputFormats).Returns(_supportedFormats);

        var namingOptions = new NamingOptions();
        var directoryServiceMock = new Mock<IDirectoryService>();
        directoryServiceMock.Setup(x => x.GetFiles(It.IsAny<string>()))
            .Returns([]);

        var resolver = new PhotoResolver(imageProcessorMock.Object, namingOptions, directoryServiceMock.Object);

        var args = new ItemResolveArgs(
            Mock.Of<IServerApplicationPaths>(),
            null)
        {
            CollectionType = CollectionType.photos,
            FileInfo = new FileSystemMetadata
            {
                FullName = "/media/photos/vacation.jpg",
                IsDirectory = false
            }
        };

        var item = resolver.Resolve(args);

        Assert.NotNull(item);
        Assert.Equal("/media/photos/vacation.jpg", item.Path);
    }

    [Fact]
    public void Resolve_Directory_ReturnsNull()
    {
        var imageProcessorMock = new Mock<IImageProcessor>();
        imageProcessorMock.Setup(x => x.SupportedInputFormats).Returns(_supportedFormats);

        var namingOptions = new NamingOptions();
        var directoryServiceMock = new Mock<IDirectoryService>();

        var resolver = new PhotoResolver(imageProcessorMock.Object, namingOptions, directoryServiceMock.Object);

        var args = new ItemResolveArgs(
            Mock.Of<IServerApplicationPaths>(),
            null)
        {
            CollectionType = CollectionType.photos,
            FileInfo = new FileSystemMetadata
            {
                FullName = "/media/photos/Album",
                IsDirectory = true
            }
        };

        var item = resolver.Resolve(args);

        Assert.Null(item);
    }

    [Theory]
    [InlineData("video.mkv", "video-poster.jpg", true)]
    [InlineData("video.mkv", "video.jpg", true)]
    [InlineData("video.mp4", "other.jpg", false)]
    [InlineData("photo.jpg", "photo-thumb.jpg", false)] // Sibling is not a video file
    public void IsOwnedByMedia_ReadOnlySpan_ReturnsExpected(string videoFile, string imageFile, bool expected)
    {
        var namingOptions = new NamingOptions();
        var imageFilename = Path.GetFileNameWithoutExtension(imageFile.AsSpan());

        // Test string overload
        Assert.Equal(expected, PhotoResolver.IsOwnedByMedia(namingOptions, videoFile, imageFilename));

        // Test ReadOnlySpan<char> overload
        Assert.Equal(expected, PhotoResolver.IsOwnedByMedia(namingOptions, videoFile.AsSpan(), imageFilename));
    }
}
