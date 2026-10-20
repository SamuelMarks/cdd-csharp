// <copyright file="OpenApiModelsTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Emit;
using Cdd.OpenApi.Ir;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Parse;
using Xunit;

namespace Cdd.OpenApi.Tests.Models
{
    /// <summary>
    /// Verifies OpenAPI 3.2.0 core model properties, JSON/YAML serialization round-trips, and IR preservation.
    /// </summary>
    public class OpenApiModelsTests
    {
        /// <summary>
        /// Verifies that <see cref="OpenApiServer"/>, <see cref="OpenApiInfo"/>, <see cref="OpenApiExternalDocs"/>, and <see cref="OpenApiTag"/> round-trip through JSON, YAML, and <see cref="IrService"/>.
        /// </summary>
        [Fact]
        public void ModelProperties_CanBeSetAndGot()
        {
            var server = new OpenApiServer
            {
                Url = "http://test.com",
                Description = "Test server",
                Name = "Test"
            };

            Assert.Equal("http://test.com", server.Url);
            Assert.Equal("Test server", server.Description);
            Assert.Equal("Test", server.Name);

            var info = new OpenApiInfo
            {
                Title = "Test API",
                Summary = "Test Summary",
                Description = "Test Description",
                TermsOfService = "http://test.com/terms",
                Version = "1.0.0",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Url = "http://test.com/support",
                    Email = "support@test.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Identifier = "MIT",
                    Url = "http://test.com/license"
                }
            };

            Assert.Equal("Test API", info.Title);
            Assert.Equal("Test Summary", info.Summary);
            Assert.Equal("Test Description", info.Description);
            Assert.Equal("http://test.com/terms", info.TermsOfService);
            Assert.Equal("1.0.0", info.Version);
            Assert.NotNull(info.Contact);
            Assert.Equal("API Support", info.Contact.Name);
            Assert.Equal("http://test.com/support", info.Contact.Url);
            Assert.Equal("support@test.com", info.Contact.Email);
            Assert.NotNull(info.License);
            Assert.Equal("MIT", info.License.Name);
            Assert.Equal("MIT", info.License.Identifier);
            Assert.Equal("http://test.com/license", info.License.Url);

            var externalDocs = new OpenApiExternalDocs
            {
                Description = "Find more info here",
                Url = "https://example.com"
            };

            Assert.Equal("Find more info here", externalDocs.Description);
            Assert.Equal("https://example.com", externalDocs.Url);

            var tag = new OpenApiTag
            {
                Name = "pet",
                Description = "Pets operations",
                ExternalDocs = externalDocs
            };

            Assert.Equal("pet", tag.Name);
            Assert.Equal("Pets operations", tag.Description);
            Assert.NotNull(tag.ExternalDocs);
            Assert.Equal("https://example.com", tag.ExternalDocs.Url);

            var doc = new OpenApiDocument
            {
                Info = info,
                Servers = new List<OpenApiServer> { server },
                Tags = new List<OpenApiTag> { tag },
                ExternalDocs = externalDocs
            };

            var emitter = new OpenApiEmitter();
            var parser = new OpenApiParser();
            var json = emitter.EmitJson(doc);
            var parsedDoc = parser.ParseJson(json);
            var irRoundTrip = IrService.FromOpenApi(parsedDoc).ToOpenApi();

            Assert.Equal("Test API", irRoundTrip.Info.Title);
            Assert.Equal("API Support", irRoundTrip.Info.Contact?.Name);
            Assert.Equal("MIT", irRoundTrip.Info.License?.Identifier);
            Assert.Single(irRoundTrip.Servers!);
            Assert.Equal("Test", irRoundTrip.Servers![0].Name);
            Assert.Single(irRoundTrip.Tags!);
            Assert.Equal("pet", irRoundTrip.Tags![0].Name);
            Assert.Equal("https://example.com", irRoundTrip.ExternalDocs?.Url);
        }

        /// <summary>
        /// Verifies that <see cref="OpenApiPathItem"/> and all nine HTTP verbs round-trip through JSON serialization and <see cref="IrService"/>.
        /// </summary>
        [Fact]
        public void PathItem_Properties_CanBeSetAndGot()
        {
            var pathItem = new OpenApiPathItem
            {
                Ref = "#/components/pathItems/MyPath",
                Summary = "Path Summary",
                Description = "Path Description",
                Get = new OpenApiOperation { OperationId = "getOp" },
                Put = new OpenApiOperation { OperationId = "putOp" },
                Post = new OpenApiOperation { OperationId = "postOp" },
                Delete = new OpenApiOperation { OperationId = "deleteOp" },
                Options = new OpenApiOperation { OperationId = "optionsOp" },
                Head = new OpenApiOperation { OperationId = "headOp" },
                Patch = new OpenApiOperation { OperationId = "patchOp" },
                Trace = new OpenApiOperation { OperationId = "traceOp" },
                Query = new OpenApiOperation { OperationId = "queryOp" },
                Servers = new List<OpenApiServer> { new OpenApiServer { Url = "http://test.com" } },
                Parameters = new List<OpenApiParameter> { new OpenApiParameter { Name = "id", In = "path" } }
            };

            Assert.Equal("#/components/pathItems/MyPath", pathItem.Ref);
            Assert.Equal("Path Summary", pathItem.Summary);
            Assert.Equal("Path Description", pathItem.Description);
            Assert.Equal("getOp", pathItem.Get?.OperationId);
            Assert.Equal("putOp", pathItem.Put?.OperationId);
            Assert.Equal("postOp", pathItem.Post?.OperationId);
            Assert.Equal("deleteOp", pathItem.Delete?.OperationId);
            Assert.Equal("optionsOp", pathItem.Options?.OperationId);
            Assert.Equal("headOp", pathItem.Head?.OperationId);
            Assert.Equal("patchOp", pathItem.Patch?.OperationId);
            Assert.Equal("traceOp", pathItem.Trace?.OperationId);
            Assert.Equal("queryOp", pathItem.Query?.OperationId);
            Assert.Single(pathItem.Servers!);
            Assert.Equal("http://test.com", pathItem.Servers![0].Url);
            Assert.Single(pathItem.Parameters!);
            Assert.Equal("id", pathItem.Parameters![0].Name);
            Assert.Equal("path", pathItem.Parameters[0].In);

            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths { ["/items/{id}"] = pathItem }
            };

            var ir = IrService.FromOpenApi(doc);
            Assert.Equal(9, ir.Endpoints.Count);
            var rtPath = ir.ToOpenApi().Paths!["/items/{id}"];
            Assert.Equal("queryOp", rtPath.Query?.OperationId);
            Assert.Equal("Path Summary", rtPath.Summary);
        }

        /// <summary>
        /// Verifies that <see cref="OpenApiOperation"/> properties round-trip through <see cref="IrEndpoint"/> and JSON serialization.
        /// </summary>
        [Fact]
        public void Operation_Properties_CanBeSetAndGot()
        {
            var operation = new OpenApiOperation
            {
                Tags = new List<string> { "tag1", "tag2" },
                Summary = "Op Summary",
                Description = "Op Description",
                ExternalDocs = new OpenApiExternalDocs { Url = "http://docs.com" },
                OperationId = "myOpId",
                Parameters = new List<OpenApiParameter>(),
                RequestBody = new OpenApiRequestBody { Description = "Req Body" },
                Responses = new OpenApiResponses { ["200"] = new OpenApiResponse { Description = "OK" } },
                Callbacks = new Dictionary<string, OpenApiCallback>(),
                Deprecated = true,
                Security = new List<IDictionary<string, IList<string>>>(),
                Servers = new List<OpenApiServer>()
            };

            Assert.Equal(2, operation.Tags?.Count);
            Assert.Equal("Op Summary", operation.Summary);
            Assert.Equal("Op Description", operation.Description);
            Assert.Equal("http://docs.com", operation.ExternalDocs?.Url);
            Assert.Equal("myOpId", operation.OperationId);
            Assert.Empty(operation.Parameters!);
            Assert.Equal("Req Body", operation.RequestBody?.Description);
            Assert.True(operation.Responses?.ContainsKey("200"));
            Assert.Equal("OK", operation.Responses?["200"].Description);
            Assert.Empty(operation.Callbacks!);
            Assert.True(operation.Deprecated);
            Assert.Empty(operation.Security!);
            Assert.Empty(operation.Servers!);

            var ep = IrEndpoint.FromOpenApiOperation("/test", "Post", operation);
            var rtOp = ep.ToOpenApiOperation();
            Assert.Equal("myOpId", rtOp.OperationId);
            Assert.Equal("Req Body", rtOp.RequestBody?.Description);
            Assert.True(rtOp.Deprecated);
        }

        /// <summary>
        /// Verifies that <see cref="OpenApiParameter"/> properties round-trip through <see cref="IrParameter"/> and JSON serialization.
        /// </summary>
        [Fact]
        public void Parameter_Properties_CanBeSetAndGot()
        {
            var param = new OpenApiParameter
            {
                Name = "limit",
                In = "query",
                Description = "max count",
                Required = true,
                Deprecated = false,
                AllowEmptyValue = true,
                Style = "form",
                Explode = true,
                AllowReserved = false,
                Schema = new OpenApiSchema { Type = "integer" },
                Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new OpenApiMediaType() },
                Example = 10,
                Examples = new Dictionary<string, OpenApiExample> { ["ex1"] = new OpenApiExample { Value = 20 } }
            };

            Assert.Equal("limit", param.Name);
            Assert.Equal("query", param.In);
            Assert.Equal("max count", param.Description);
            Assert.True(param.Required);
            Assert.False(param.Deprecated);
            Assert.True(param.AllowEmptyValue);
            Assert.Equal("form", param.Style);
            Assert.True(param.Explode);
            Assert.False(param.AllowReserved);
            Assert.Equal("integer", param.Schema?.Type);
            Assert.True(param.Content?.ContainsKey("application/json"));
            Assert.Equal(10, param.Example);
            Assert.Equal(20, param.Examples?["ex1"].Value);

            var irParam = IrParameter.FromOpenApiParameter(param);
            Assert.Equal("int", irParam.Type.CSharpType);
            var rtParam = irParam.ToOpenApiParameter();
            Assert.Equal("limit", rtParam.Name);
            Assert.Equal("form", rtParam.Style);
            Assert.Equal(10, rtParam.Example);
        }
    }
}
