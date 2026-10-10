using System;
using Emby.Naming.Common;

namespace Emby.Naming.Video
{
    /// <summary>
    /// Parse 3D format related flags.
    /// </summary>
    public static class Format3DParser
    {
        // Static default result to save on allocation costs.
        private static readonly Format3DResult _defaultResult = new(false, null);

        /// <summary>
        /// Parse 3D format related flags.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <returns>Returns <see cref="Format3DResult"/> object.</returns>
        public static Format3DResult Parse(ReadOnlySpan<char> path, NamingOptions namingOptions)
        {
            if (path.IsEmpty || namingOptions.Format3DRules.Length == 0)
            {
                return _defaultResult;
            }

            int oldLen = namingOptions.VideoFlagDelimiters.Length;
            Span<char> delimiters = stackalloc char[oldLen + 1];
            namingOptions.VideoFlagDelimiters.AsSpan().CopyTo(delimiters);
            delimiters[oldLen] = ' ';

            return Parse(path, delimiters, namingOptions);
        }

        private static Format3DResult Parse(ReadOnlySpan<char> path, ReadOnlySpan<char> delimiters, NamingOptions namingOptions)
        {
            // Optimization: Tokenize path once into stack-allocated span buffer.
            // This avoids re-scanning and re-slicing the path string for every 3D format rule (O(N) single-pass tokenization instead of O(N * R)).
            Span<ReadOnlySpan<char>> tokens = stackalloc ReadOnlySpan<char>[128];
            int tokenCount = 0;

            ReadOnlySpan<char> remaining = path;
            while (remaining.Length > 0 && tokenCount < tokens.Length)
            {
                var index = remaining.IndexOfAny(delimiters);
                if (index == -1)
                {
                    tokens[tokenCount++] = remaining;
                    remaining = default;
                    break;
                }

                tokens[tokenCount++] = remaining[..index];
                remaining = remaining[(index + 1)..];
            }

            // If path had more than 128 tokens (extremely rare), fall back to streaming parse per rule.
            if (remaining.Length > 0)
            {
                return ParseFallback(path, delimiters, namingOptions);
            }

            var rules = namingOptions.Format3DRules;
            for (int r = 0; r < rules.Length; r++)
            {
                var rule = rules[r];
                var foundPrefix = string.IsNullOrEmpty(rule.PrecedingToken);

                for (int i = 0; i < tokenCount; i++)
                {
                    var currentSlice = tokens[i];
                    if (!foundPrefix)
                    {
                        foundPrefix = currentSlice.Equals(rule.PrecedingToken, StringComparison.OrdinalIgnoreCase);
                        continue;
                    }

                    if (currentSlice.Equals(rule.Token, StringComparison.OrdinalIgnoreCase))
                    {
                        return new Format3DResult(true, rule.Token);
                    }
                }
            }

            return _defaultResult;
        }

        private static Format3DResult ParseFallback(ReadOnlySpan<char> path, ReadOnlySpan<char> delimiters, NamingOptions namingOptions)
        {
            foreach (var rule in namingOptions.Format3DRules)
            {
                var result = ParseRuleFallback(path, rule, delimiters);
                if (result.Is3D)
                {
                    return result;
                }
            }

            return _defaultResult;
        }

        private static Format3DResult ParseRuleFallback(ReadOnlySpan<char> path, Format3DRule rule, ReadOnlySpan<char> delimiters)
        {
            bool is3D = false;
            string? format3D = null;

            var foundPrefix = string.IsNullOrEmpty(rule.PrecedingToken);
            while (path.Length > 0)
            {
                var index = path.IndexOfAny(delimiters);
                ReadOnlySpan<char> currentSlice;
                if (index == -1)
                {
<<<<<<< HEAD
=======
                    // No delimiter left, the last token is the remainder of the path
>>>>>>> upstream/release-12.z
                    currentSlice = path;
                    path = default;
                }
                else
                {
                    currentSlice = path[..index];
                    path = path[(index + 1)..];
                }

                if (!foundPrefix)
                {
                    foundPrefix = currentSlice.Equals(rule.PrecedingToken, StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                is3D = foundPrefix && currentSlice.Equals(rule.Token, StringComparison.OrdinalIgnoreCase);

                if (is3D)
                {
                    format3D = rule.Token;
                    break;
                }
            }

            return is3D ? new Format3DResult(true, format3D) : _defaultResult;
        }
    }
}
