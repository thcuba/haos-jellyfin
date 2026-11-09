using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jellyfin.Extensions.Json.Converters;

/// <summary>
/// Enum flag to json array converter.
/// </summary>
/// <typeparam name="T">The type of enum.</typeparam>
public class JsonFlagEnumConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    // Cache both the enum value and its string representation statically per enum type.
    // Calling enumValue.ToString() inside Write() allocates a heap string on every matched flag.
    // Pre-caching pairs eliminates runtime string allocations during JSON serialization.
    private static readonly (T Value, string Name)[] _enumEntries = InitializeEnumEntries();

    private static (T Value, string Name)[] InitializeEnumEntries()
    {
        var values = Enum.GetValues<T>();
        var names = Enum.GetNames<T>();
        var entries = new (T Value, string Name)[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            entries[i] = (values[i], names[i]);
        }

        return entries;
    }

    /// <inheritdoc />
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var (enumValue, enumName) in _enumEntries)
        {
            if (value.HasFlag(enumValue))
            {
                writer.WriteStringValue(enumName);
            }
        }

        writer.WriteEndArray();
    }
}
