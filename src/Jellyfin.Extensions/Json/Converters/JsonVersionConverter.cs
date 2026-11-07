using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jellyfin.Extensions.Json.Converters
{
    /// <summary>
    /// Converts a Version object or value to/from JSON.
    /// </summary>
    /// <remarks>
    /// Required to send <see cref="Version"/> as a string instead of an object.
    /// </remarks>
    public class JsonVersionConverter : JsonConverter<Version>
    {
        private const int MaxVersionCharLength = 64;

        /// <inheritdoc />
        public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && !reader.HasValueSequence)
            {
                ReadOnlySpan<byte> span = reader.ValueSpan;
                if (span.Length <= MaxVersionCharLength)
                {
                    Span<char> charBuffer = stackalloc char[MaxVersionCharLength];
                    if (Encoding.UTF8.TryGetChars(span, charBuffer, out int charsWritten)
                        && Version.TryParse(charBuffer[..charsWritten], out var parsedVersion))
                    {
                        return parsedVersion;
                    }
                }
            }

            return new Version(reader.GetString()!); // Will throw ArgumentNullException on null
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(value);

            // Stack-allocate a character buffer to format the Version directly without intermediate string heap allocation.
            Span<char> charBuffer = stackalloc char[MaxVersionCharLength];
            if (value.TryFormat(charBuffer, out int charsWritten))
            {
                writer.WriteStringValue(charBuffer[..charsWritten]);
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
