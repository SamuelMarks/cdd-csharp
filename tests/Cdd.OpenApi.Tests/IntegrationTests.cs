using System.IO;
using Xunit;
using Cdd.OpenApi.Parse;
using Cdd.OpenApi.Emit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void ParseAndEmit_PreservesInformation()
        {
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
            var doc = parser.ParseJson(json);

            var emitter = new OpenApiEmitter();
            var emittedJson = emitter.EmitJson(doc);

            // Re-parse to verify properties remain
            var doc2 = parser.ParseJson(emittedJson);

            Assert.Equal(doc.OpenApi, doc2.OpenApi);
            Assert.Equal(doc.Info.Title, doc2.Info.Title);
            Assert.Equal(doc.Info.Contact?.Name, doc2.Info.Contact?.Name);
            Assert.Equal(doc.Servers?[0].Url, doc2.Servers?[0].Url);
            Assert.Equal(doc.Paths?["/pets"].Get?.OperationId, doc2.Paths?["/pets"].Get?.OperationId);
            Assert.Equal(doc.Components?.Schemas?["Pet"].Type, doc2.Components?.Schemas?["Pet"].Type);
        }
    }
}
