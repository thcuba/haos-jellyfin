using System.IO;
using System.Linq;
using System.Xml;
using MediaBrowser.Controller.Extensions;
using Xunit;

namespace Jellyfin.Controller.Tests;

public class XmlReaderExtensionsTests
{
    [Theory]
    [InlineData("<genres>Action, Drama, Sci-Fi</genres>", new[] { "Action", "Drama", "Sci-Fi" })]
    [InlineData("<genres>Action / Adventure | Sci-Fi</genres>", new[] { "Action / Adventure", "Sci-Fi" })]
    [InlineData("<genres>Director A; Director B</genres>", new[] { "Director A", "Director B" })]
    [InlineData("<genres>Matthew, Jr. | John Doe</genres>", new[] { "Matthew, Jr.", "John Doe" })]
    [InlineData("<genres> , Action, Drama, </genres>", new[] { "Action", "Drama" })]
    [InlineData("<genres>Action,,Drama</genres>", new[] { "Action", "Drama" })]
    [InlineData("<genres></genres>", new string[0])]
    [InlineData("<genres>   </genres>", new string[0])]
    public void GetStringArray_ValidXml_ReturnsExpectedArray(string xml, string[] expected)
    {
        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader);
        xmlReader.MoveToContent();

        var result = xmlReader.GetStringArray().ToArray();

        Assert.Equal(expected, result);
    }
}
