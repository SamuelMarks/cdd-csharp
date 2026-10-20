// <copyright file="IrModelsTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Ir;
using Cdd.OpenApi.Models;
using Xunit;

namespace Cdd.OpenApi.Tests.Ir
{
    /// <summary>Verifies or exercises ir models tests.</summary>
    public class IrModelsTests
    {
        /// <summary>Verifies or exercises from open api maps models and endpoints.</summary>
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

            var manualIr = new IrService
            {
                Models = new List<IrModel>
                {
                    new IrModel
                    {
                        Name = "ManualModel",
                        Description = "Manual",
                        Schema = null,
                        Fields = new List<IrField>
                        {
                            new IrField
                            {
                                Name = "id",
                                IsRequired = true,
                                Schema = null,
                                Type = new IrTypeRef { TypeName = "integer", Format = "int64", CSharpType = "long" }
                            }
                        }
                    }
                },
                Endpoints = new List<IrEndpoint>
                {
                    new IrEndpoint
                    {
                        Path = "/manual",
                        HttpMethod = "Get",
                        OperationId = "GetManual",
                        Parameters = new List<IrParameter>
                        {
                            new IrParameter
                            {
                                Name = "q",
                                Location = "query",
                                Schema = new OpenApiSchema { Type = "string" },
                                Type = new IrTypeRef { TypeName = "string" }
                            }
                        }
                    }
                }
            };
            var manualDoc = manualIr.ToOpenApi();
            Assert.NotNull(manualDoc.Components?.Schemas?["ManualModel"]);
            Assert.Equal("int64", manualDoc.Components!.Schemas!["ManualModel"].Properties!["id"].Format);
            Assert.NotNull(manualDoc.Paths?["/manual"].Get);
            Assert.Equal("string", manualDoc.Paths!["/manual"].Get!.Parameters![0].Schema!.Type);

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

        /// <summary>Verifies or exercises operation helpers and type mapper full coverage.</summary>
        [Fact]
        public void OperationHelpers_And_TypeMapper_FullCoverage()
        {
            Assert.Equal(HttpVerb.Get, OperationHelpers.ParseHttpVerb("GET"));
            Assert.Equal(HttpVerb.Put, OperationHelpers.ParseHttpVerb("PUT"));
            Assert.Equal(HttpVerb.Post, OperationHelpers.ParseHttpVerb("POST"));
            Assert.Equal(HttpVerb.Delete, OperationHelpers.ParseHttpVerb("DELETE"));
            Assert.Equal(HttpVerb.Options, OperationHelpers.ParseHttpVerb("OPTIONS"));
            Assert.Equal(HttpVerb.Head, OperationHelpers.ParseHttpVerb("HEAD"));
            Assert.Equal(HttpVerb.Patch, OperationHelpers.ParseHttpVerb("PATCH"));
            Assert.Equal(HttpVerb.Trace, OperationHelpers.ParseHttpVerb("TRACE"));
            Assert.Equal(HttpVerb.Query, OperationHelpers.ParseHttpVerb("QUERY"));
            Assert.Equal(HttpVerb.Custom, OperationHelpers.ParseHttpVerb("PURGE"));
            Assert.Equal(HttpVerb.Custom, OperationHelpers.ParseHttpVerb(null));

            Assert.Equal("Get", OperationHelpers.ToPascalVerb(HttpVerb.Get));
            Assert.Equal("Put", OperationHelpers.ToPascalVerb(HttpVerb.Put));
            Assert.Equal("Post", OperationHelpers.ToPascalVerb(HttpVerb.Post));
            Assert.Equal("Delete", OperationHelpers.ToPascalVerb(HttpVerb.Delete));
            Assert.Equal("Options", OperationHelpers.ToPascalVerb(HttpVerb.Options));
            Assert.Equal("Head", OperationHelpers.ToPascalVerb(HttpVerb.Head));
            Assert.Equal("Patch", OperationHelpers.ToPascalVerb(HttpVerb.Patch));
            Assert.Equal("Trace", OperationHelpers.ToPascalVerb(HttpVerb.Trace));
            Assert.Equal("Query", OperationHelpers.ToPascalVerb(HttpVerb.Query));
            Assert.Equal("Purge", OperationHelpers.ToPascalVerb(HttpVerb.Custom, "PURGE"));
            Assert.Equal("Get", OperationHelpers.ToPascalVerb(HttpVerb.Custom, null));

            Assert.Equal(ParameterLocation.Query, OperationHelpers.ParseParameterLocation("query"));
            Assert.Equal(ParameterLocation.Header, OperationHelpers.ParseParameterLocation("header"));
            Assert.Equal(ParameterLocation.Path, OperationHelpers.ParseParameterLocation("path"));
            Assert.Equal(ParameterLocation.Cookie, OperationHelpers.ParseParameterLocation("cookie"));
            Assert.Equal(ParameterLocation.Body, OperationHelpers.ParseParameterLocation("body"));
            Assert.Equal(ParameterLocation.FormData, OperationHelpers.ParseParameterLocation("formData"));
            Assert.Equal(ParameterLocation.Query, OperationHelpers.ParseParameterLocation(null));

            Assert.Equal("query", OperationHelpers.ToLocationString(ParameterLocation.Query));
            Assert.Equal("header", OperationHelpers.ToLocationString(ParameterLocation.Header));
            Assert.Equal("path", OperationHelpers.ToLocationString(ParameterLocation.Path));
            Assert.Equal("cookie", OperationHelpers.ToLocationString(ParameterLocation.Cookie));
            Assert.Equal("body", OperationHelpers.ToLocationString(ParameterLocation.Body));
            Assert.Equal("formData", OperationHelpers.ToLocationString(ParameterLocation.FormData));

            var pathItem = new OpenApiPathItem();
            var op = new OpenApiOperation { OperationId = "TestOp" };
            OperationHelpers.SetOperation(pathItem, "GET", op);
            OperationHelpers.SetOperation(pathItem, "PUT", op);
            OperationHelpers.SetOperation(pathItem, "POST", op);
            OperationHelpers.SetOperation(pathItem, "DELETE", op);
            OperationHelpers.SetOperation(pathItem, "OPTIONS", op);
            OperationHelpers.SetOperation(pathItem, "HEAD", op);
            OperationHelpers.SetOperation(pathItem, "PATCH", op);
            OperationHelpers.SetOperation(pathItem, "TRACE", op);
            OperationHelpers.SetOperation(pathItem, "QUERY", op);
            OperationHelpers.SetOperation(pathItem, "PURGE", op);
            OperationHelpers.SetOperation(pathItem, "MKCOL", op);
            Assert.NotNull(pathItem.Get);
            Assert.NotNull(pathItem.Put);
            Assert.NotNull(pathItem.Post);
            Assert.NotNull(pathItem.Delete);
            Assert.NotNull(pathItem.Options);
            Assert.NotNull(pathItem.Head);
            Assert.NotNull(pathItem.Patch);
            Assert.NotNull(pathItem.Trace);
            Assert.NotNull(pathItem.Query);
            Assert.Equal(2, pathItem.AdditionalOperations?.Count);

            // TypeMapper coverage
            Assert.Equal("integer", TypeMapper.MapCSharpToOpenApi("Int32"));
            Assert.Equal("integer", TypeMapper.MapCSharpToOpenApi("Int64"));
            Assert.Equal("integer", TypeMapper.MapCSharpToOpenApi("Int16"));
            Assert.Equal("number", TypeMapper.MapCSharpToOpenApi("Single"));
            Assert.Equal("number", TypeMapper.MapCSharpToOpenApi("Double"));
            Assert.Equal("number", TypeMapper.MapCSharpToOpenApi("Decimal"));
            Assert.Equal("boolean", TypeMapper.MapCSharpToOpenApi("Boolean"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("String"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("Guid"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("System.Guid"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("DateTime"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("System.DateTime"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("DateTimeOffset"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("System.DateTimeOffset"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("DateOnly"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("System.DateOnly"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("TimeOnly"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("System.TimeOnly"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("Uri"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("System.Uri"));
            Assert.Equal("string", TypeMapper.MapCSharpToOpenApi("byte[]"));

            Assert.Equal(("integer", null), TypeMapper.MapCSharpToOpenApiTypeAndFormat("int"));
            Assert.Equal(("integer", null), TypeMapper.MapCSharpToOpenApiTypeAndFormat("Int32"));
            Assert.Equal(("integer", null), TypeMapper.MapCSharpToOpenApiTypeAndFormat("short"));
            Assert.Equal(("integer", null), TypeMapper.MapCSharpToOpenApiTypeAndFormat("Int16"));
            Assert.Equal(("integer", "int64"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("long"));
            Assert.Equal(("integer", "int64"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("Int64"));
            Assert.Equal(("number", "float"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("float"));
            Assert.Equal(("number", "float"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("Single"));
            Assert.Equal(("number", "decimal"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("decimal"));
            Assert.Equal(("number", "decimal"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("Decimal"));
            Assert.Equal(("number", null), TypeMapper.MapCSharpToOpenApiTypeAndFormat("double"));
            Assert.Equal(("number", null), TypeMapper.MapCSharpToOpenApiTypeAndFormat("Double"));
            Assert.Equal(("boolean", null), TypeMapper.MapCSharpToOpenApiTypeAndFormat("bool"));
            Assert.Equal(("boolean", null), TypeMapper.MapCSharpToOpenApiTypeAndFormat("Boolean"));
            Assert.Equal(("string", "uuid"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("Guid"));
            Assert.Equal(("string", "uuid"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("System.Guid"));
            Assert.Equal(("string", "date-time"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("DateTime"));
            Assert.Equal(("string", "date-time"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("System.DateTime"));
            Assert.Equal(("string", "date-time"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("DateTimeOffset"));
            Assert.Equal(("string", "date-time"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("System.DateTimeOffset"));
            Assert.Equal(("string", "date"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("DateOnly"));
            Assert.Equal(("string", "date"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("System.DateOnly"));
            Assert.Equal(("string", "time"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("TimeOnly"));
            Assert.Equal(("string", "time"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("System.TimeOnly"));
            Assert.Equal(("string", "uri"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("Uri"));
            Assert.Equal(("string", "uri"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("System.Uri"));
            Assert.Equal(("string", "byte"), TypeMapper.MapCSharpToOpenApiTypeAndFormat("byte[]"));
            Assert.Equal(("string", null), TypeMapper.MapCSharpToOpenApiTypeAndFormat("string"));
            Assert.Equal(("string", null), TypeMapper.MapCSharpToOpenApiTypeAndFormat("String"));
            Assert.Equal(("object", null), TypeMapper.MapCSharpToOpenApiTypeAndFormat("CustomType", "object"));
        }
    }
}
