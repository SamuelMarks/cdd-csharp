using System.Linq;
using Xunit;
using Cdd.OpenApi.Parse;

namespace Cdd.OpenApi.Tests.Parse
{
    public class Swagger2ParserTests
    {
        [Fact]
        public void ParseJson_ConvertsSwagger2ToOpenApi3()
        {
            var json = @"
            {
                ""swagger"": ""2.0"",
                ""definitions"": {
                    ""User"": {
                        ""type"": ""object""
                    }
                },
                ""paths"": {
                    ""/user"": {
                        ""post"": {
                            ""parameters"": [
                                {
                                    ""in"": ""body"",
                                    ""name"": ""user"",
                                    ""description"": ""User object"",
                                    ""required"": true,
                                    ""schema"": { ""$ref"": ""#/definitions/User"" }
                                }
                            ],
                            ""responses"": {
                                ""200"": {
                                    ""description"": ""OK"",
                                    ""schema"": { ""type"": ""string"" }
                                }
                            }
                        },
                        ""put"": {
                            ""parameters"": [
                                {
                                    ""in"": ""formData"",
                                    ""name"": ""id"",
                                    ""type"": ""string"",
                                    ""format"": ""uuid""
                                }
                            ]
                        },
                        ""get"": {
                            ""parameters"": [
                                {
                                    ""name"": ""limit"",
                                    ""in"": ""query"",
                                    ""type"": ""integer"",
                                    ""items"": { ""type"": ""string"" }
                                }
                            ]
                        }
                    }
                }
            }";

            var parser = new OpenApiParser();
            var doc = parser.ParseJson(json);

            Assert.NotNull(doc.Components);
            Assert.NotNull(doc.Components.Schemas);
            Assert.True(doc.Components.Schemas.ContainsKey("User"));

            var postOp = doc.Paths["/user"].Post;
            Assert.Null(postOp.Parameters);
            Assert.NotNull(postOp.RequestBody);
            Assert.Equal("User object", postOp.RequestBody.Description);
            Assert.True(postOp.RequestBody.Required);
            Assert.True(postOp.RequestBody.Content.ContainsKey("application/json"));
            Assert.Equal("#/definitions/User", postOp.RequestBody.Content["application/json"].Schema.Ref);

            var postResp = postOp.Responses["200"];
            Assert.NotNull(postResp.Content);
            Assert.True(postResp.Content.ContainsKey("application/json"));
            Assert.Equal("string", postResp.Content["application/json"].Schema.Type);

            var putOp = doc.Paths["/user"].Put;
            Assert.Null(putOp.Parameters);
            Assert.NotNull(putOp.RequestBody);
            Assert.True(putOp.RequestBody.Content.ContainsKey("application/x-www-form-urlencoded"));
            var formSchema = putOp.RequestBody.Content["application/x-www-form-urlencoded"].Schema;
            Assert.True(formSchema.Properties.ContainsKey("id"));
            Assert.Equal("string", formSchema.Properties["id"].Type);

            var getOp = doc.Paths["/user"].Get;
            Assert.NotNull(getOp.Parameters);
            var limitParam = getOp.Parameters[0];
            Assert.NotNull(limitParam.Schema);
            Assert.Equal("integer", limitParam.Schema.Type);
        }
    }
}
