## 2026-09-14 - Zero-allocation parsing in EpisodePathParser

**Learning:** Chained string `.Trim()` calls (e.g., `.Trim().Trim('_', '.', '-').Trim()`) create multiple intermediate string allocations. Using `AsSpan().Trim().Trim("_.-").Trim().ToString()` performs all slicing on `ReadOnlySpan<char>` and allocates only a single string at the end. Additionally, LINQ `.Where().ToList()` and `InsertRange` in expression loop helpers (like `FillAdditional`) allocate lists and enumerators on every parse call.

**Action:** Prefer `ReadOnlySpan<char>` extension methods for multi-step string cleaning, and iterate expression collections directly rather than constructing temporary `List<T>` instances.

## 2026-09-15 - Zero-allocation regex group trimming in SeriesPathParser

**Learning:** Slicing regex groups using `Group.ValueSpan.Trim(" _.-")` before `.ToString()` avoids allocating intermediate string objects when group parsing fails or when trimming characters from matched groups, and eliminates heap-allocated `char[]` params arrays from `string.Trim(params char[])`.

**Action:** Prefer inspecting `Group.ValueSpan` directly and performing span trimming with literal string representations of characters before materializing strings.
