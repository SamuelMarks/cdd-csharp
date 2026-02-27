using Xunit;
using Cdd.OpenApi.Parse;

namespace Cdd.OpenApi.Tests.Parse
{
    public class OpenApiParserTests
    {
        [Fact]
        public void ParseJson_ValidBasicDocument_ReturnsDocument()
        {
            // Arrange
            var json = @"
            {
                ""openapi"": ""3.2.0"",
                ""info"": {
                    ""title"": ""Test API"",
                    ""version"": ""1.0.0""
                },
                ""paths"": {}
            }";
            var parser = new OpenApiParser();

            // Act
            var doc = parser.ParseJson(json);

            // Assert
            Assert.NotNull(doc);
            Assert.Equal("3.2.0", doc.OpenApi);
            Assert.NotNull(doc.Info);
            Assert.Equal("Test API", doc.Info.Title);
            Assert.Equal("1.0.0", doc.Info.Version);
            Assert.NotNull(doc.Paths);
        }
    }
}
