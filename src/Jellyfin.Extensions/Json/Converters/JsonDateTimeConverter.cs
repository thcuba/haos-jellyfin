using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jellyfin.Extensions.Json.Converters
{
    /// <summary>
    /// Legacy DateTime converter.
    /// Milliseconds aren't output if zero by default.
    /// </summary>
    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        private const string ZeroMillisecondFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffZ";
        private const int MaxDateTimeCharLength = 35;

        /// <inheritdoc />
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetDateTime();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            if (value.Millisecond == 0)
            {
                // Remaining ticks value will be 0, manually format.
                // Stack-allocate a character buffer to format directly without heap string allocation.
                Span<char> buffer = stackalloc char[MaxDateTimeCharLength];
                if (value.TryFormat(buffer, out int charsWritten, ZeroMillisecondFormat, CultureInfo.InvariantCulture))
                {
                    writer.WriteStringValue(buffer[..charsWritten]);
                }
                else
                {
                    writer.WriteStringValue(value.ToString(ZeroMillisecondFormat, CultureInfo.InvariantCulture));
                }
            }
            else
            {
                writer.WriteStringValue(value);
            }
        }
    }
}
