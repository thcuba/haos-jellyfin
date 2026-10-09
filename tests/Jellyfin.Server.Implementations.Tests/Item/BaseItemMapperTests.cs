using System;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Server.Implementations.Item;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.Item;

public class BaseItemMapperTests
{
    [Theory]
    [InlineData(null, new string[0])]
    [InlineData("", new string[0])]
    [InlineData("   ", new string[0])]
    [InlineData("Action", new[] { "Action" })]
    [InlineData("Action|Comedy", new[] { "Action", "Comedy" })]
    [InlineData("Action|Comedy|Sci-Fi", new[] { "Action", "Comedy", "Sci-Fi" })]
    public void Map_Genres_ReturnsExpectedArray(string? genresInput, string[] expected)
    {
        var entity = new BaseItemEntity
        {
            Id = Guid.NewGuid(),
            Type = typeof(Movie).ToString(),
            Genres = genresInput
        };

        var dto = new Movie();
        BaseItemMapper.Map(entity, dto, null);

        Assert.Equal(expected, dto.Genres);
    }

    [Theory]
    [InlineData(null, new string[0])]
    [InlineData("", new string[0])]
    [InlineData("   ", new string[0])]
    [InlineData("USA", new[] { "USA" })]
    [InlineData("USA|Canada", new[] { "USA", "Canada" })]
    [InlineData("USA||Canada", new[] { "USA", "Canada" })]
    public void Map_ProductionLocations_ReturnsExpectedArray(string? input, string[] expected)
    {
        var entity = new BaseItemEntity
        {
            Id = Guid.NewGuid(),
            Type = typeof(Movie).ToString(),
            ProductionLocations = input
        };

        var dto = new Movie();
        BaseItemMapper.Map(entity, dto, null);

        Assert.Equal(expected, dto.ProductionLocations);
    }

    [Theory]
    [InlineData(null, new string[0])]
    [InlineData("", new string[0])]
    [InlineData("Warner Bros", new[] { "Warner Bros" })]
    [InlineData("Warner Bros|Universal", new[] { "Warner Bros", "Universal" })]
    public void Map_Studios_ReturnsExpectedArray(string? input, string[] expected)
    {
        var entity = new BaseItemEntity
        {
            Id = Guid.NewGuid(),
            Type = typeof(Movie).ToString(),
            Studios = input
        };

        var dto = new Movie();
        BaseItemMapper.Map(entity, dto, null);

        Assert.Equal(expected, dto.Studios);
    }

    [Theory]
    [InlineData(null, new string[0])]
    [InlineData("", new string[0])]
    [InlineData("4K", new[] { "4K" })]
    [InlineData("4K|HDR", new[] { "4K", "HDR" })]
    public void Map_Tags_ReturnsExpectedArray(string? input, string[] expected)
    {
        var entity = new BaseItemEntity
        {
            Id = Guid.NewGuid(),
            Type = typeof(Movie).ToString(),
            Tags = input
        };

        var dto = new Movie();
        BaseItemMapper.Map(entity, dto, null);

        Assert.Equal(expected, dto.Tags);
    }

    [Theory]
    [InlineData(null, new string[0])]
    [InlineData("", new string[0])]
    [InlineData("Artist One", new[] { "Artist One" })]
    [InlineData("Artist One|Artist Two", new[] { "Artist One", "Artist Two" })]
    [InlineData("Artist One||Artist Two", new[] { "Artist One", "Artist Two" })]
    public void Map_AudioArtists_ReturnsExpectedArray(string? input, string[] expected)
    {
        var entity = new BaseItemEntity
        {
            Id = Guid.NewGuid(),
            Type = typeof(Audio).ToString(),
            Artists = input,
            AlbumArtists = input
        };

        var dto = new Audio();
        BaseItemMapper.Map(entity, dto, null);

        Assert.Equal(expected, dto.Artists);
        Assert.Equal(expected, dto.AlbumArtists);
    }
}
