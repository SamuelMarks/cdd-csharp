using Xunit;
using Cdd.OpenApi.Parse;
using System.Linq;

namespace Cdd.OpenApi.Tests.Parse
{
    public class OpenApiParserFullTests
    {
        [Fact]
        public void ParseJson_CompleteDocument_ReturnsAllProperties()
        {
            // Arrange
            var json = @"
            {
                ""openapi"": ""3.2.0"",
                ""info"": {
                    ""title"": ""Test API"",
                    ""version"": ""1.0.0"",
                    ""contact"": {
                        ""name"": ""API Support"",
                        ""email"": ""support@example.com""
                    }
                },
                ""servers"": [
                    {
                        ""url"": ""https://api.example.com/v1"",
                        ""description"": ""Production""
                    }
                ],
                ""paths"": {
                    ""/pets"": {
                        ""get"": {
                            ""summary"": ""List pets"",
                            ""operationId"": ""listPets"",
                            ""tags"": [""pets""],
                            ""responses"": {
                                ""200"": {
                                    ""description"": ""Success""
                                }
                            }
                        }
                    }
                },
                ""components"": {
                    ""schemas"": {
                        ""Pet"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""id"": { ""type"": ""integer"" },
                                ""name"": { ""type"": ""string"" }
                            }
                        }
                    }
                }
            }";
            var parser = new OpenApiParser();

            // Act
            var doc = parser.ParseJson(json);

            // Assert
            Assert.NotNull(doc);
            Assert.Equal("3.2.0", doc.OpenApi);
            Assert.Equal("Test API", doc.Info.Title);
            Assert.Equal("API Support", doc.Info.Contact.Name);

            Assert.Single(doc.Servers);
            Assert.Equal("https://api.example.com/v1", doc.Servers.First().Url);

            Assert.NotNull(doc.Paths);
            Assert.True(doc.Paths.ContainsKey("/pets"));
            var getOp = doc.Paths["/pets"].Get;
            Assert.NotNull(getOp);
            Assert.Equal("listPets", getOp.OperationId);
            Assert.Single(getOp.Tags);
            Assert.Equal("pets", getOp.Tags[0]);

            Assert.NotNull(doc.Components);
            Assert.NotNull(doc.Components.Schemas);
            Assert.True(doc.Components.Schemas.ContainsKey("Pet"));
            var petSchema = doc.Components.Schemas["Pet"];
            Assert.Equal("object", petSchema.Type);
            Assert.NotNull(petSchema.Properties);
            Assert.True(petSchema.Properties.ContainsKey("id"));
        }
    }
}
