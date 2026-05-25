using System;
using Xunit;
using Cdd.OpenApi.Parse;

namespace Cdd.OpenApi.Tests.Parse
{
    public class OpenApiParserCoverageTests
    {
        [Fact]
        public void ParseJson_BodyParamWithoutSchema_DoesNotCreateRequestBody()
        {
            var json = @"
            {
                ""paths"": {
                    ""/test"": {
                        ""post"": {
                            ""parameters"": [
                                {
                                    ""in"": ""body"",
                                    ""name"": ""myBodyParam""
                                }
                            ]
                        }
                    }
                }
            }";
            var parser = new OpenApiParser();
            var doc = parser.ParseJson(json); System.Console.WriteLine("P IN IS: " + doc.Paths["/test"].Post.Parameters[0].In);
            var op = doc.Paths["/test"].Post;

            Assert.Null(op.RequestBody);
            Assert.NotNull(op.Parameters);
            Assert.Single(op.Parameters); // Since it was not body-with-schema and not formData, it goes to the 'else' block
        }

        [Fact]
        public void ParseJson_NonBodyParamWithSchemaNullAndTypeNotNull_CreatesSchema()
        {
            var json = @"
            {
                ""paths"": {
                    ""/test"": {
                        ""get"": {
                            ""parameters"": [
                                {
                                    ""in"": ""query"",
                                    ""name"": ""myQueryParam"",
                                    ""type"": ""string"",
                                    ""format"": ""uuid""
                                }
                            ]
                        }
                    }
                }
            }";
            var parser = new OpenApiParser();
            var doc = parser.ParseJson(json);
            var op = doc.Paths["/test"].Get;

            Assert.NotNull(op.Parameters);
            Assert.Single(op.Parameters);
            var p = op.Parameters[0];
            Assert.NotNull(p.Schema);
            Assert.Equal("string", p.Schema.Type);
            Assert.Equal("uuid", p.Schema.Format);
        }
        [Fact]
        public void ParseJson_BodyParamWithSchema_CreatesRequestBody()
        {
            var json = @"
            {
                ""paths"": {
                    ""/test"": {
                        ""post"": {
                            ""parameters"": [
                                {
                                    ""in"": ""body"",
                                    ""name"": ""myBodyParam"",
                                    ""schema"": { ""type"": ""string"" }
                                }
                            ]
                        }
                    }
                }
            }";
            var parser = new OpenApiParser();
            var doc = parser.ParseJson(json);
            var op = doc.Paths["/test"].Post;
            Assert.NotNull(op.RequestBody);
        }

        [Fact]
        public void ParseJson_NonBodyParamWithSchemaNullAndTypeNull_DoesNotCreateSchema()
        {
            var json = @"
            {
                ""paths"": {
                    ""/test"": {
                        ""get"": {
                            ""parameters"": [
                                {
                                    ""in"": ""query"",
                                    ""name"": ""myQueryParam""
                                }
                            ]
                        }
                    }
                }
            }";
            var parser = new OpenApiParser();
            var doc = parser.ParseJson(json);
            var p = doc.Paths["/test"].Get.Parameters[0];
            Assert.Null(p.Schema);
        }

        [Fact]
        public void ParseJson_FormDataWithExistingNullSchema_DoesNotCrash()
        {
            var json = @"
            {
                ""paths"": {
                    ""/test"": {
                        ""post"": {
                            ""requestBody"": {
                                ""content"": {
                                    ""application/x-www-form-urlencoded"": {
                                    }
                                }
                            },
                            ""parameters"": [
                                {
                                    ""in"": ""formData"",
                                    ""name"": ""formParam"",
                                    ""type"": ""string""
                                }
                            ]
                        }
                    }
                }
            }";
            var parser = new OpenApiParser();
            var doc = parser.ParseJson(json);
            var formSchema = doc.Paths["/test"].Post.RequestBody!.Content["application/x-www-form-urlencoded"].Schema;
            Assert.Null(formSchema); // No exception, skipped setting
        }

        [Fact]
        public void ParseJson_FormDataWithExistingSchemaNullProperties_DoesNotCrash()
        {
            var json = @"
            {
                ""paths"": {
                    ""/test"": {
                        ""post"": {
                            ""requestBody"": {
                                ""content"": {
                                    ""application/x-www-form-urlencoded"": {
                                        ""schema"": {
                                            ""type"": ""object""
                                        }
                                    }
                                }
                            },
                            ""parameters"": [
                                {
                                    ""in"": ""formData"",
                                    ""name"": ""formParam"",
                                    ""type"": ""string""
                                }
                            ]
                        }
                    }
                }
            }";
            var parser = new OpenApiParser();
            var doc = parser.ParseJson(json);
            var formSchema = doc.Paths["/test"].Post.RequestBody!.Content["application/x-www-form-urlencoded"].Schema;
            Assert.Null(formSchema!.Properties); // No exception, skipped setting
        }
    }
}
