using System;
using System.Text.Json;
using Jellyfin.Extensions.Json.Converters;
using Xunit;

namespace Jellyfin.Extensions.Tests.Json.Converters
{
    public class JsonDateTimeConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public JsonDateTimeConverterTests()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new JsonDateTimeConverter());
        }

        [Fact]
        public void Serialize_ZeroMillisecond_UsesCustomFormatWithSevenFractionsAndZ()
        {
            var dt = new DateTime(2025, 3, 15, 10, 30, 45, 0, DateTimeKind.Utc);
            var json = JsonSerializer.Serialize(dt, _options);

            Assert.Equal("\"2025-03-15T10:30:45.0000000Z\"", json);
        }

        [Fact]
        public void Serialize_NonZeroMillisecond_SerializesDefault()
        {
            var dt = new DateTime(2025, 3, 15, 10, 30, 45, 123, DateTimeKind.Utc);
            var json = JsonSerializer.Serialize(dt, _options);

            Assert.Equal("\"2025-03-15T10:30:45.123Z\"", json);
        }

        [Theory]
        [InlineData("\"2025-03-15T10:30:45.0000000Z\"", 2025, 3, 15, 10, 30, 45, 0)]
        [InlineData("\"2025-03-15T10:30:45.123Z\"", 2025, 3, 15, 10, 30, 45, 123)]
        [InlineData("\"2025-03-15T10:30:45Z\"", 2025, 3, 15, 10, 30, 45, 0)]
        public void Deserialize_DateTime_Success(string input, int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            var expected = new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
            var deserialized = JsonSerializer.Deserialize<DateTime>(input, _options);

            Assert.Equal(expected, deserialized.ToUniversalTime());
        }

        [Fact]
        public void RoundTrip_ZeroMillisecond_Succeeds()
        {
            var original = new DateTime(2026, 11, 7, 12, 0, 0, 0, DateTimeKind.Utc);
            var json = JsonSerializer.Serialize(original, _options);
            var deserialized = JsonSerializer.Deserialize<DateTime>(json, _options);

            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void RoundTrip_NonZeroMillisecond_Succeeds()
        {
            var original = new DateTime(2026, 11, 7, 12, 0, 0, 456, DateTimeKind.Utc);
            var json = JsonSerializer.Serialize(original, _options);
            var deserialized = JsonSerializer.Deserialize<DateTime>(json, _options);

            Assert.Equal(original, deserialized);
        }
    }
}
