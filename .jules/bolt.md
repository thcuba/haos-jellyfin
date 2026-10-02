## 2026-09-14 - Zero-allocation parsing in EpisodePathParser

**Learning:** Chained string `.Trim()` calls (e.g., `.Trim().Trim('_', '.', '-').Trim()`) create multiple intermediate string allocations. Using `AsSpan().Trim().Trim("_.-").Trim().ToString()` performs all slicing on `ReadOnlySpan<char>` and allocates only a single string at the end. Additionally, LINQ `.Where().ToList()` and `InsertRange` in expression loop helpers (like `FillAdditional`) allocate lists and enumerators on every parse call.

**Action:** Prefer `ReadOnlySpan<char>` extension methods for multi-step string cleaning, and iterate expression collections directly rather than constructing temporary `List<T>` instances.

## 2026-09-15 - Zero-allocation regex group trimming in SeriesPathParser

**Learning:** Slicing regex groups using `Group.ValueSpan.Trim(" _.-")` before `.ToString()` avoids allocating intermediate string objects when group parsing fails or when trimming characters from matched groups, and eliminates heap-allocated `char[]` params arrays from `string.Trim(params char[])`.

**Action:** Prefer inspecting `Group.ValueSpan` directly and performing span trimming with literal string representations of characters before materializing strings.

## 2026-09-16 - Span flag matching in ExternalPathParser

**Learning:** In string/path token parsers, using LINQ `.Any(s => slice.Contains(s, ...))` or `.Any(s => slice.Equals(s, ...))` on flag lists instantiates delegate closures, enumerators, and substring allocations per loop iteration. Passing `ReadOnlySpan<char>` to a simple `for` loop helper calling `slice.Contains(flag, StringComparison.OrdinalIgnoreCase)` or `slice.Equals(flag, StringComparison.OrdinalIgnoreCase)` is completely zero-allocation.

**Action:** Replace `list.Any(s => span.Contains(s))` with a static indexed loop helper taking `ReadOnlySpan<char>`.

## 2026-09-17 - Allocation-free path extraction in SeasonPathParser

**Learning:** Constructing `new DirectoryInfo(path).Name` during path parsing allocates `DirectoryInfo` heap objects and incurs OS path initialization overhead on every call. Using `Path.GetFileName(parentPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))` extracts directory names entirely through string manipulation with zero extra heap objects or OS overhead.

**Action:** Prefer `Path.GetFileName` on trimmed path strings over `new DirectoryInfo(path).Name` when extracting parent directory names.

## 2026-09-18 - Zero-allocation directory path matching in ExtraRuleResolver

**Learning:** `Path.GetDirectoryName(pathSpan)` returns `ReadOnlySpan<char>`. Calling `.ToString()` on it to compare with `libraryRoot` allocates heap string objects on every item during library scans. `ReadOnlySpan<char>.Equals(libraryRoot, StringComparison.OrdinalIgnoreCase)` compares directory path spans directly with `string?` / `ReadOnlySpan<char>` without allocating heap objects.

**Action:** Use `Path.GetDirectoryName(pathSpan).Equals(libraryRoot, StringComparison.OrdinalIgnoreCase)` directly on `ReadOnlySpan<char>` spans instead of converting directory path spans to `string` with `.ToString()`.

## 2026-09-23 - Zero-allocation file extension resolution in EpisodeResolver

**Learning:** Calling `Path.GetExtension(path)` allocates a heap string for the file extension on every file resolution, and instantiating stateless parser helpers (like `EpisodePathParser`) per resolution call creates unnecessary heap allocations. Using `Path.GetExtension(path.AsSpan())` with `Jellyfin.Extensions.Contains(ReadOnlySpan<char>, StringComparison)` eliminates string allocations during option matching, and caching stateless parser objects as class fields avoids object allocation during library scans.

**Action:** Use `Path.GetExtension(path.AsSpan())` for extension matching and field-cache stateless sub-parsers in resolver classes.

## 2026-09-24 - Zero-allocation prefix check in AlbumParser

**Learning:** Running `CleanRegex().Replace(filename, " ")` before checking option prefixes allocates string objects for 100% of audio files during music scans. Checking `filename.AsSpan().TrimStart(" -._()\t").StartsWith(prefix, StringComparison.OrdinalIgnoreCase)` on `ReadOnlySpan<char>` before regex replacement bypasses regex engine execution and eliminates string allocations for all non-multi-part tracks.

**Action:** Check prefix matches on trimmed `ReadOnlySpan<char>` spans before running expensive regex normalizations.

## 2026-09-26 - Pre-compiled regex properties in NamingOptions

**Learning:** Calling static `Regex.Match(input, patternString, RegexOptions)` repeatedly in loop iterations queries internal `RegexCache` string keys or instantiates regex pattern objects on every file. Pre-compiling `string[]` expression arrays into `Regex[]` properties in `NamingOptions.Compile()` and executing `regex.Match(input)` directly completely eliminates string lookup overhead during library scans.

**Action:** Expose pre-compiled `Regex[]` properties populated during `NamingOptions.Compile()` for expression collections, and iterate `Regex[]` arrays in parser classes instead of passing raw string patterns to `Regex.Match`.

## 2026-09-27 - Zero-allocation string creation in VideoListResolver

**Learning:** Using `FormattableString.Invariant($"...")` boxes formatted value types, constructs a `FormattableString` object, and allocates an `object[]` arguments array per invocation. Replacing `FormattableString.Invariant` with `string.Create(CultureInfo.InvariantCulture, $"...")` leverages C# interpolated string handlers to format directly into the string memory without `FormattableString` boxing or argument array allocations.

**Action:** Prefer `string.Create(CultureInfo.InvariantCulture, $"...")` over `FormattableString.Invariant($"...")` when building formatted keys or identifiers in tight processing loops.

## 2026-09-28 - Zero-allocation stream index lookup in EncodingHelper

**Learning:** Using `.Where().ToList().IndexOf()` on a collection allocates a closure delegate, a LINQ iterator object, a temporary `List<T>`, and a heap array. An indexed `for` loop with early `break` calculates stream offsets in a single pass with zero heap allocations.

**Action:** Replace `.Where().ToList().IndexOf()` calls on collections with indexed `for` loops and early termination.

## 2026-09-29 - Zero-allocation array string splitting in XmlReaderExtensions

**Learning:** Calling `.ToString().Split(separator)` on a `ReadOnlySpan<char>` allocates an intermediate heap string for the trimmed node text and a heap `string[]` array containing elements. Iterating span slices directly with `IndexOfAny(separator)` and `ReadOnlySpan<char>.Trim()` materializes `string` objects only for non-whitespace yielded items, eliminating all intermediate string and array allocations during XML NFO metadata parsing.

**Action:** Iterate string span slices with `IndexOfAny` and `ReadOnlySpan<char>.Trim()` instead of calling `.ToString().Split()` when parsing delimited string content in XML readers.

## 2026-09-30 - O(1) stacked file lookup in VideoListResolver

**Learning:** Calling `stackResult.Any(s => s.ContainsFile(current.Path, current.IsDirectory))` inside a loop over video files executes an $O(N \times S \times F)$ linear scan and allocates lambda delegates per file. Collecting stacked file paths into a `HashSet<string>(StringComparer.OrdinalIgnoreCase)` beforehand reduces lookups to $O(1)$ and eliminates all loop delegate allocations.

**Action:** Pre-index file collections into a `HashSet<string>` with appropriate string comparison before checking membership in nested video resolution loops.

## 2026-10-01 - Single-pass stack-allocated tokenization in Format3DParser

**Learning:** Repeatedly tokenizing a path string for each 3D format rule in `Format3DParser` causes $O(N \times R)$ repeated delimiter searches and span slicing. Tokenizing the path span once into a `stackalloc ReadOnlySpan<char>[128]` buffer reduces path scanning to $O(N)$ single-pass tokenization with zero heap allocations. Remember to clear the tracking span (`remaining = default`) when `IndexOfAny` returns `-1` to ensure tokenization terminates cleanly and avoids unnecessary fallback paths.

**Action:** Tokenize path spans once into `stackalloc ReadOnlySpan<char>[]` buffers before checking rules, and make sure `remaining` span state is cleared when processing the final token.
