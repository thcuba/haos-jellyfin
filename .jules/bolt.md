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
