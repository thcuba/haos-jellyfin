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
