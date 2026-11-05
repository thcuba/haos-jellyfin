using System;
using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jellyfin.Extensions.Json.Converters
{
    /// <summary>
    /// Converts a GUID object or value to/from JSON.
    /// </summary>
    public class JsonGuidConverter : JsonConverter<Guid>
    {
        /// <inheritdoc />
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.IsNull()
                ? Guid.Empty
                : ReadInternal(ref reader);

        // Optimized to parse UTF-8 bytes directly without intermediate string allocation.
        // TryGetGuid fast-paths standard RFC 4122 dashed GUIDs (36 bytes).
        // Utf8Parser.TryParse fast-paths Jellyfin's standard "N" format undashed GUIDs (32 bytes).
        internal static Guid ReadInternal(ref Utf8JsonReader reader)
        {
            if (reader.TryGetGuid(out var value))
            {
                return value;
            }

            ReadOnlySpan<byte> utf8Span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
            if (Utf8Parser.TryParse(utf8Span, out Guid guid, out int bytesConsumed, 'N') && bytesConsumed == utf8Span.Length)
            {
                return guid;
            }

            return Guid.Parse(reader.GetString()!); // null got handled higher up the call stack
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
            => WriteInternal(writer, value);

        // Optimized to write UTF-8 formatted bytes directly to avoid intermediate string heap allocation.
        internal static void WriteInternal(Utf8JsonWriter writer, Guid value)
        {
            Span<byte> utf8Span = stackalloc byte[32];
            Utf8Formatter.TryFormat(value, utf8Span, out _, 'N');
            writer.WriteStringValue(utf8Span);
        }
    }
}
