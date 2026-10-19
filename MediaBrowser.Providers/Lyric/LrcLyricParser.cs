using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Jellyfin.Extensions;
using LrcParser.Model;
using LrcParser.Parser;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Model.Lyrics;

namespace MediaBrowser.Providers.Lyric;

/// <summary>
/// LRC Lyric Parser.
/// </summary>
public partial class LrcLyricParser : ILyricParser
{
    private readonly LyricParser _lrcLyricParser;

    private static readonly string[] _supportedMediaTypes = [".lrc", ".elrc"];

    /// <summary>
    /// Initializes a new instance of the <see cref="LrcLyricParser"/> class.
    /// </summary>
    public LrcLyricParser()
    {
        _lrcLyricParser = new LrcParser.Parser.Lrc.LrcParser();
    }

    /// <inheritdoc />
    public string Name => "LrcLyricProvider";

    /// <summary>
    /// Gets the priority.
    /// </summary>
    /// <value>The priority.</value>
    public ResolverPriority Priority => ResolverPriority.Fourth;

    /// <inheritdoc />
    public LyricDto? ParseLyrics(LyricFile lyrics)
    {
        if (!_supportedMediaTypes.Contains(Path.GetExtension(lyrics.Name.AsSpan()), StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        Song lyricData;

        try
        {
            lyricData = _lrcLyricParser.Decode(lyrics.Content);
        }
        catch (Exception)
        {
            // Failed to parse, return null so the next parser will be tried
            return null;
        }

        if (lyricData.Lyrics.Count == 0)
        {
            return null;
        }

        IReadOnlyList<LrcParser.Model.Lyric> sortedLyricData;
        if (IsSorted(lyricData.Lyrics))
        {
            sortedLyricData = lyricData.Lyrics;
        }
        else
        {
            var list = lyricData.Lyrics.ToList();
            list.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            sortedLyricData = list;
        }

        // Bolt optimization: Pre-allocate list capacity and avoid heap allocations during cue parsing
        List<LyricLine> lyricList = new(sortedLyricData.Count);
        for (var lineIndex = 0; lineIndex < sortedLyricData.Count; lineIndex++)
        {
            var lyric = sortedLyricData[lineIndex];

            // Extract cues from time tags
            var cues = new List<LyricLineCue>(lyric.TimeTags.Count);
            if (lyric.TimeTags.Count > 0)
            {
                // Bolt optimization: Enumerate SortedDictionary directly using struct enumerator to avoid
                // Keys.ToList() heap allocations and O(log k) dictionary lookups. Use ReadOnlySpan.Trim() to avoid string slice allocations.
                using var enumerator = lyric.TimeTags.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    var prevTag = enumerator.Current;

                    while (enumerator.MoveNext())
                    {
                        var currentTag = enumerator.Current;

                        var currentKey = prevTag.Key;
                        var nextKey = currentTag.Key;
                        var currentMs = prevTag.Value ?? 0;
                        var nextMs = currentTag.Value ?? 0;

                        var currentPos = currentKey.State == IndexState.End ? currentKey.Index + 1 : currentKey.Index;
                        var nextPos = nextKey.State == IndexState.End ? nextKey.Index + 1 : nextKey.Index;

                        if (!lyric.Text.AsSpan(currentPos, nextPos - currentPos).Trim().IsEmpty)
                        {
                            cues.Add(new LyricLineCue(
                                position: currentPos,
                                endPosition: nextPos,
                                start: TimeSpan.FromMilliseconds(currentMs).Ticks,
                                end: TimeSpan.FromMilliseconds(nextMs).Ticks));
                        }

                        prevTag = currentTag;
                    }

                    var lastKey = prevTag.Key;
                    var lastMs = prevTag.Value ?? 0;
                    var lastPos = lastKey.State == IndexState.End ? lastKey.Index + 1 : lastKey.Index;

                    if (!lyric.Text.AsSpan(lastPos).Trim().IsEmpty)
                    {
                        cues.Add(new LyricLineCue(
                            position: lastPos,
                            endPosition: lyric.Text.Length,
                            start: TimeSpan.FromMilliseconds(lastMs).Ticks,
                            end: lineIndex + 1 < sortedLyricData.Count ? TimeSpan.FromMilliseconds(sortedLyricData[lineIndex + 1].StartTime).Ticks : null));
                    }
                }
            }

            long lyricStartTicks = TimeSpan.FromMilliseconds(lyric.StartTime).Ticks;
            lyricList.Add(new LyricLine(lyric.Text, lyricStartTicks, cues));
        }

        return new LyricDto { Lyrics = lyricList };
    }

    private static bool IsSorted(IReadOnlyList<LrcParser.Model.Lyric> lyrics)
    {
        for (var i = 1; i < lyrics.Count; i++)
        {
            if (lyrics[i - 1].StartTime > lyrics[i].StartTime)
            {
                return false;
            }
        }

        return true;
    }
}
