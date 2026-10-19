using System.IO;
using MediaBrowser.Model.Lyrics;
using MediaBrowser.Providers.Lyric;
using Xunit;

namespace Jellyfin.Providers.Tests.Lyrics;

public static class LrcLyricParserTests
{
    [Fact]
    public static void ParseElrcCues()
    {
        var parser = new LrcLyricParser();
        var fileContents = File.ReadAllText(Path.Combine("Test Data", "Lyrics", "Fleetwood Mac - Rumors.elrc"));
        var parsed = parser.ParseLyrics(new LyricFile("Fleetwood Mac - Rumors.elrc", fileContents));

        Assert.NotNull(parsed);
        Assert.Equal(31, parsed.Lyrics.Count);

        var line1 = parsed.Lyrics[0];
        Assert.Equal("Every night that goes between", line1.Text);
        Assert.NotNull(line1.Cues);
        Assert.Equal(5, line1.Cues.Count);
        Assert.Equal(68400000, line1.Cues[0].Start);
        Assert.Equal(72000000, line1.Cues[0].End);
        Assert.Equal(0, line1.Cues[0].Position);
        Assert.Equal(5, line1.Cues[0].EndPosition);
        Assert.Equal(6, line1.Cues[1].Position);
        Assert.Equal(11, line1.Cues[1].EndPosition);
        Assert.Equal(12, line1.Cues[2].Position);

        var line5 = parsed.Lyrics[4];
        Assert.Equal("Every night you do not come", line5.Text);
        Assert.NotNull(line5.Cues);
        Assert.Equal(6, line5.Cues.Count);
        Assert.Equal(375200000, line5.Cues[2].Start);
        Assert.Equal(377300000, line5.Cues[2].End);

        var lastLine = parsed.Lyrics[^1];
        Assert.Equal("I have always been a storm", lastLine.Text);
        Assert.NotNull(lastLine.Cues);
        Assert.Equal(6, lastLine.Cues.Count);
        Assert.Equal(2358000000, lastLine.Cues[^1].Start);
        Assert.Equal(26, lastLine.Cues[^1].EndPosition);
        Assert.Null(lastLine.Cues[^1].End);
    }

    [Fact]
    public static void ParseStandardLrc()
    {
        var parser = new LrcLyricParser();
        var lrcContent = "[00:12.00]Line 1\n[00:05.00]Line 0\n[00:20.00]Line 2";
        var parsed = parser.ParseLyrics(new LyricFile("test.lrc", lrcContent));

        Assert.NotNull(parsed);
        Assert.Equal(3, parsed.Lyrics.Count);
        Assert.Equal("Line 0", parsed.Lyrics[0].Text);
        Assert.Equal(50000000, parsed.Lyrics[0].Start);
        Assert.Equal("Line 1", parsed.Lyrics[1].Text);
        Assert.Equal(120000000, parsed.Lyrics[1].Start);
        Assert.Equal("Line 2", parsed.Lyrics[2].Text);
        Assert.Equal(200000000, parsed.Lyrics[2].Start);
    }

    [Fact]
    public static void ParseUnsupportedExtensionReturnsNull()
    {
        var parser = new LrcLyricParser();
        var parsed = parser.ParseLyrics(new LyricFile("test.txt", "[00:05.00]Line 0"));

        Assert.Null(parsed);
    }
}
