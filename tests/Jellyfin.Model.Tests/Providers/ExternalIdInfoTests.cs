using MediaBrowser.Model.Providers;
using Xunit;

namespace Jellyfin.Model.Tests.Providers;

public class ExternalIdInfoTests
{
    [Fact]
    public void Constructor_ValidArguments_PropertiesCorrectlyInitialized()
    {
        var info = new ExternalIdInfo("IMDB", "Imdb", ExternalIdMediaType.Movie);

        Assert.Equal("IMDB", info.ProviderName);
        Assert.Equal("Imdb", info.Key);
        Assert.Equal(ExternalIdMediaType.Movie, info.Type);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var info = new ExternalIdInfo("IMDB", "Imdb", null)
        {
            ProviderName = "TheMovieDb",
            Key = "Tmdb",
            Type = ExternalIdMediaType.Series
        };

        Assert.Equal("TheMovieDb", info.ProviderName);
        Assert.Equal("Tmdb", info.Key);
        Assert.Equal(ExternalIdMediaType.Series, info.Type);
    }
}
