using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Orm;

namespace Cdd.OpenApi.Tests.Orm
{
    public class ServerRoutesGeneratorTests
    {
        [Fact]
        public void GenerateRoutes_Coverage()
        {
            var schemas = new Dictionary<string, OpenApiSchema>
            {
                ["MyModel"] = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["id"] = new OpenApiSchema { Type = "integer" }
                    }
                },
                ["MyModelArray"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Ref = "#/components/schemas/MyModel" }
                }
            };

            var paths = new OpenApiPaths
            {
                ["/api/mymodel"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Ref = "#/components/schemas/MyModel" }
                                    }
                                }
                            }
                        }
                    },
                    Post = new OpenApiOperation
                    {
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Ref = "#/components/schemas/MyModel" } }
                                }
                            }
                        }
                    }
                },
                ["/api/mymodelarray"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Ref = "#/components/schemas/MyModelArray" }
                                    }
                                }
                            }
                        }
                    }
                },
                ["/api/mymodel/list"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Ref = "#/components/schemas/MyModel" } }
                                    }
                                }
                            }
                        }
                    }
                },
                ["/api/mymodel/{id}"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "id", In = "path", Required = true, Schema = new OpenApiSchema { Type = "integer" } }
                        }
                    },
                    Put = new OpenApiOperation
                    {
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Ref = "#/components/schemas/MyModel" }
                                }
                            }
                        }
                    },
                    Delete = new OpenApiOperation
                    {
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "id", In = "path", Required = true, Schema = new OpenApiSchema { Type = "integer" } }
                        }
                    },
                    Options = new OpenApiOperation
                    {
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "id", In = "path", Required = true, Schema = new OpenApiSchema { Type = "integer" } }
                        }
                    }
                }
            };

            var groupedPaths = new Dictionary<string, OpenApiPaths> { ["MyModel"] = paths };
            var cd = ServerRoutesGenerator.GenerateRoutes(groupedPaths, "Generated", schemas);
            var code = cd[0].Code;

            Assert.Contains("return Results.Ok(await dao.GetByIdAsync(id));", code);
            Assert.Contains("return Results.Ok(await dao.GetAllAsync());", code);
            Assert.Contains("return Results.StatusCode(501);", code);
            Assert.Contains("return Results.Ok(await dao.UpdateAsync(body));", code);
            Assert.Contains("await dao.DeleteAsync(id);", code);
        }
    }
}
