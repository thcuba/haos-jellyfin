using System;
using MediaBrowser.Controller.Entities;
using MediaBrowser.LocalMetadata.Images;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.Images;

public class LocalImageProviderExtensionTests
{
    [Theory]
    [InlineData(".png", 0)]
    [InlineData(".PNG", 0)]
    [InlineData(".jpg", 1)]
    [InlineData(".JPG", 1)]
    [InlineData(".jpeg", 2)]
    [InlineData(".JPEG", 2)]
    [InlineData(".webp", 3)]
    [InlineData(".WebP", 3)]
    [InlineData(".tbn", 4)]
    [InlineData(".TBN", 4)]
    [InlineData(".gif", 5)]
    [InlineData(".GIF", 5)]
    [InlineData(".svg", 6)]
    [InlineData(".SVG", 6)]
    public void GetImageExtensionRank_SupportedExtensions_MatchesExpectedRank(string extension, int expectedRank)
    {
        var rank = LocalImageProvider.GetImageExtensionRank(extension);
        Assert.Equal(expectedRank, rank);
        Assert.Equal(expectedRank, Array.IndexOf(BaseItem.SupportedImageExtensions, extension.ToLowerInvariant()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(".txt")]
    [InlineData(".mp4")]
    [InlineData(".mkv")]
    [InlineData(".nfo")]
    [InlineData(".bmp")]
    [InlineData("png")] // missing dot
    public void GetImageExtensionRank_UnsupportedOrNull_ReturnsNegativeOne(string? extension)
    {
        var rank = LocalImageProvider.GetImageExtensionRank(extension);
        Assert.Equal(-1, rank);
    }
}
