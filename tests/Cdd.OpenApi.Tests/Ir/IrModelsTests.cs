using System.Collections.Generic;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Ir;
using Cdd.OpenApi.Models;
using Xunit;

namespace Cdd.OpenApi.Tests.Ir
{
    public class IrModelsTests
    {
        [Fact]
        public void FromOpenApi_MapsModelsAndEndpoints()
        {
            var doc = new OpenApiDocument
            {
                Info = new OpenApiInfo { Title = "PetService", Version = "2.1.0" },
                Components = new OpenApiComponents
                {
                    Schemas = new Dictionary<string, OpenApiSchema>
                    {
                        ["Pet"] = new OpenApiSchema
                        {
                            Description = "A pet entity",
                            Required = new List<string> { "id" },
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["id"] = new OpenApiSchema { Type = "integer", Description = "Primary key" },
                                ["name"] = new OpenApiSchema { Type = null }
                            }
                        },
                        ["EmptySchema"] = new OpenApiSchema(),
                        ["NoReqSchema"] = new OpenApiSchema
                        {
                            Required = null,
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["opt"] = new OpenApiSchema { Type = "boolean" }
                            }
                        }
                    }
                },
                Paths = new OpenApiPaths
                {
                    ["/pets/{id}"] = new OpenApiPathItem
                    {
                        Get = new OpenApiOperation
                        {
                            OperationId = "GetPet",
                            Summary = "Fetch pet",
                            Parameters = new List<OpenApiParameter>
                            {
                                new OpenApiParameter
                                {
                                    Name = "id",
                                    In = "path",
                                    Required = true,
                                    Schema = new OpenApiSchema { Type = "integer" }
                                },
                                new OpenApiParameter
                                {
                                    Name = "filter",
                                    In = "query",
                                    Required = false,
                                    Type = "string"
                                },
                                new OpenApiParameter
                                {
                                    Name = "raw",
                                    In = "query",
                                    Required = false
                                }
                            },
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    Description = "Found",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType
                                        {
                                            Schema = new OpenApiSchema { Type = "string" }
                                        }
                                    }
                                },
                                ["202"] = new OpenApiResponse
                                {
                                    Description = null!,
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType
                                        {
                                            Schema = new OpenApiSchema { Type = null }
                                        }
                                    }
                                },
                                ["204"] = new OpenApiResponse
                                {
                                    Description = "No Content",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["text/plain"] = new OpenApiMediaType()
                                    }
                                },
                                ["404"] = new OpenApiResponse
                                {
                                    Description = "Not Found"
                                },
                                ["500"] = new OpenApiResponse
                                {
                                    Description = "Error",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType { Schema = null }
                                    }
                                }
                            }
                        },
                        Post = new OpenApiOperation(),
                        Put = new OpenApiOperation { OperationId = "PutPet" },
                        Delete = new OpenApiOperation { OperationId = "DeletePet" }
                    }
                }
            };

            var ir = IrService.FromOpenApi(doc);
            Assert.Equal("PetService", ir.Title);
            Assert.Equal("2.1.0", ir.Version);
            Assert.Equal(3, ir.Models.Count);
            Assert.Equal("Pet", ir.Models[0].Name);
            Assert.Equal("A pet entity", ir.Models[0].Description);
            Assert.Equal(2, ir.Models[0].Fields.Count);
            Assert.True(ir.Models[0].Fields[0].IsRequired);
            Assert.Equal("int", ir.Models[0].Fields[0].Type.CSharpType);
            Assert.False(ir.Models[0].Fields[0].Type.IsNullable);
            Assert.Equal("Primary key", ir.Models[0].Fields[0].Description);
            Assert.False(ir.Models[0].Fields[1].IsRequired);
            Assert.True(ir.Models[0].Fields[1].Type.IsNullable);

            Assert.Equal(4, ir.Endpoints.Count);
            var getEp = ir.Endpoints[0];
            Assert.Equal("/pets/{id}", getEp.Path);
            Assert.Equal("Get", getEp.HttpMethod);
            Assert.Equal("GetPet", getEp.OperationId);
            Assert.Equal("Fetch pet", getEp.Summary);
            Assert.Equal(3, getEp.Parameters.Count);
            Assert.Equal("id", getEp.Parameters[0].Name);
            Assert.Equal("path", getEp.Parameters[0].Location);
            Assert.True(getEp.Parameters[0].IsRequired);
            Assert.Equal(5, getEp.Responses.Count);
            Assert.Equal("200", getEp.Responses[0].StatusCode);
            Assert.Equal("Found", getEp.Responses[0].Description);
            Assert.Equal("string", getEp.Responses[0].ContentType?.CSharpType);
            Assert.Equal("string", getEp.Responses[0].ContentType?.TypeName);

            var reconstructed = ir.ToOpenApi();
            Assert.Equal("PetService", reconstructed.Info?.Title);
            Assert.Equal(3, reconstructed.Components?.Schemas?.Count);
            Assert.NotNull(reconstructed.Paths?["/pets/{id}"].Get);
            Assert.NotNull(reconstructed.Paths?["/pets/{id}"].Post);
            Assert.NotNull(reconstructed.Paths?["/pets/{id}"].Put);
            Assert.NotNull(reconstructed.Paths?["/pets/{id}"].Delete);

            var emptyIr = IrService.FromOpenApi(new OpenApiDocument { Info = null!, Components = new OpenApiComponents { Schemas = null } });
            Assert.Equal("API", emptyIr.Title);
            Assert.Equal("1.0.0", emptyIr.Version);
            Assert.Empty(emptyIr.Models);
            Assert.Empty(emptyIr.Endpoints);
            var emptyDoc = emptyIr.ToOpenApi();
            Assert.Null(emptyDoc.Components);
            Assert.Null(emptyDoc.Paths);

            var nullVerIr = IrService.FromOpenApi(new OpenApiDocument { Info = new OpenApiInfo { Title = null!, Version = null! }, Components = null });
            Assert.Equal("API", nullVerIr.Title);
            Assert.Equal("1.0.0", nullVerIr.Version);

            var pathItemWithEmptyAddOp = new OpenApiPathItem
            {
                AdditionalOperations = new Dictionary<string, OpenApiOperation>
                {
                    [""] = new OpenApiOperation()
                }
            };
            var opsDict = OperationHelpers.GetOperationsByPascalVerb(pathItemWithEmptyAddOp);
            Assert.False(opsDict.ContainsKey(""));
        }
    }
}
