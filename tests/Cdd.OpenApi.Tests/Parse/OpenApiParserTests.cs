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

            var yaml = "openapi: 3.2.0\ninfo:\n  title: Yaml API\n  version: 2.0.0\n";
            var yamlDoc = parser.ParseYamlOrJson(yaml);
            Assert.Equal("Yaml API", yamlDoc.Info.Title);
            Assert.Equal("2.0.0", yamlDoc.Info.Version);

            var jsonViaYamlOrJson = parser.ParseYamlOrJson(json);
            Assert.Equal("Test API", jsonViaYamlOrJson.Info.Title);

            var nullDoc = parser.ParseYamlOrJson("null");
            Assert.Equal("3.2.0", nullDoc.OpenApi);

            Assert.Throws<System.ArgumentException>(() => parser.ParseYamlOrJson(""));
        }
    }
}
