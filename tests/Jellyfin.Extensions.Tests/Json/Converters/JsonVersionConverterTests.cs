using System;
using System.Text.Json;
using Jellyfin.Extensions.Json.Converters;
using Xunit;

namespace Jellyfin.Extensions.Tests.Json.Converters
{
    public class JsonVersionConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public JsonVersionConverterTests()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new JsonVersionConverter());
        }

        [Theory]
        [InlineData("\"1.0\"", 1, 0, -1, -1)]
        [InlineData("\"1.025.222\"", 1, 25, 222, -1)]
        [InlineData("\"10.8.13.100\"", 10, 8, 13, 100)]
        public void Deserialize_Version_Success(string input, int major, int minor, int build, int revision)
        {
            var expected = build == -1
                ? new Version(major, minor)
                : revision == -1
                    ? new Version(major, minor, build)
                    : new Version(major, minor, build, revision);

            var deserializedInput = JsonSerializer.Deserialize<Version>(input, _options);
            Assert.Equal(expected, deserializedInput);
        }

        [Theory]
        [InlineData(1, 0, -1, -1, "\"1.0\"")]
        [InlineData(1, 9, 59, -1, "\"1.9.59\"")]
        [InlineData(10, 8, 13, 100, "\"10.8.13.100\"")]
        public void Serialize_Version_Success(int major, int minor, int build, int revision, string expected)
        {
            var input = build == -1
                ? new Version(major, minor)
                : revision == -1
                    ? new Version(major, minor, build)
                    : new Version(major, minor, build, revision);

            var serializedInput = JsonSerializer.Serialize(input, _options);
            Assert.Equal(expected, serializedInput);
        }

        [Fact]
        public void Deserialize_InvalidVersion_ThrowsFormatException()
        {
            var input = "\"invalid.version.string\"";
            Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<Version>(input, _options));
        }
    }
}
