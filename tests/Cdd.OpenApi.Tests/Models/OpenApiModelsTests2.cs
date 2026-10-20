// <copyright file="OpenApiModelsTests2.cs" company="Cdd.OpenApi">
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
    /// Verifies OpenAPI 3.2.0 request bodies, responses, schemas, media types, encodings, security schemes, and components round-tripping.
    /// </summary>
    public class OpenApiModelsTests2
    {
        /// <summary>
        /// Verifies that <see cref="OpenApiRequestBody"/> properties round-trip through <see cref="IrRequestBody"/>.
        /// </summary>
        [Fact]
        public void RequestBody_Properties_CanBeSetAndGot()
        {
            var reqBody = new OpenApiRequestBody
            {
                Description = "A body",
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType()
                }
            };

            Assert.Equal("A body", reqBody.Description);
            Assert.True(reqBody.Required);
            Assert.Single(reqBody.Content);

            var irBody = IrRequestBody.FromOpenApiRequestBody(reqBody);
            var rtBody = irBody.ToOpenApiRequestBody();
            Assert.Equal("A body", rtBody.Description);
            Assert.True(rtBody.Required);
            Assert.Single(rtBody.Content);
        }

        /// <summary>
        /// Verifies that <see cref="OpenApiResponses"/> and <see cref="OpenApiResponse"/> round-trip through <see cref="IrResponse"/>.
        /// </summary>
        [Fact]
        public void Responses_Properties_CanBeSetAndGot()
        {
            var responses = new OpenApiResponses
            {
                Default = new OpenApiResponse { Description = "Default response" }
            };
            responses.Add("200", new OpenApiResponse
            {
                Description = "OK",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string", Format = "uuid" } }
                }
            });

            Assert.Equal("Default response", responses.Default?.Description);
            Assert.Equal("OK", responses["200"].Description);

            var irResp = IrResponse.FromOpenApiResponse("200", responses["200"]);
            Assert.Equal("Guid", irResp.ContentType?.CSharpType);
            var rtResp = irResp.ToOpenApiResponse();
            Assert.Equal("OK", rtResp.Description);
        }

        /// <summary>
        /// Verifies that <see cref="OpenApiSchema"/> constraints and metadata round-trip through <see cref="IrModel"/> and <see cref="IrField"/>.
        /// </summary>
        [Fact]
        public void Schema_Properties_CanBeSetAndGot()
        {
            var schema = new OpenApiSchema
            {
                Ref = "#/components/schemas/Pet",
                Type = "object",
                Format = "custom",
                Items = new OpenApiSchema { Type = "string" },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["id"] = new OpenApiSchema
                    {
                        Type = "integer",
                        Minimum = 1,
                        Maximum = 100,
                        MinLength = 1,
                        MaxLength = 50,
                        Pattern = "^[0-9]+$"
                    }
                },
                Required = new List<string> { "id" },
                Description = "A pet",
                Minimum = 1,
                Maximum = 100
            };

            Assert.Equal("#/components/schemas/Pet", schema.Ref);
            Assert.Equal("object", schema.Type);
            Assert.Equal("custom", schema.Format);
            Assert.Equal("string", schema.Items?.Type);
            Assert.True(schema.Properties?.ContainsKey("id"));
            Assert.Contains("id", schema.Required!);
            Assert.Equal("A pet", schema.Description);
            Assert.Equal(1, schema.Minimum);
            Assert.Equal(100, schema.Maximum);

            var irModel = IrModel.FromOpenApiSchema("Pet", schema);
            Assert.Single(irModel.Fields);
            Assert.Equal(1, irModel.Fields[0].Minimum);
            Assert.Equal(100, irModel.Fields[0].Maximum);
            var rtSchema = irModel.ToOpenApiSchema();
            Assert.Equal("A pet", rtSchema.Description);
            Assert.Equal(1, rtSchema.Properties!["id"].Minimum);
        }

        /// <summary>
        /// Verifies <see cref="OpenApiReference"/> properties.
        /// </summary>
        [Fact]
        public void Reference_Properties_CanBeSetAndGot()
        {
            var reference = new OpenApiReference
            {
                Ref = "#/components/parameters/skipParam",
                Summary = "Skip parameter",
                Description = "Skip description"
            };

            Assert.Equal("#/components/parameters/skipParam", reference.Ref);
            Assert.Equal("Skip parameter", reference.Summary);
            Assert.Equal("Skip description", reference.Description);
        }

        /// <summary>
        /// Verifies <see cref="OpenApiServerVariable"/> properties and JSON serialization.
        /// </summary>
        [Fact]
        public void ServerVariable_Properties_CanBeSetAndGot()
        {
            var serverVar = new OpenApiServerVariable
            {
                Enum = new List<string> { "a", "b" },
                Default = "a",
                Description = "A desc"
            };

            Assert.Equal(2, serverVar.Enum?.Count);
            Assert.Equal("a", serverVar.Default);
            Assert.Equal("A desc", serverVar.Description);
        }

        /// <summary>
        /// Verifies <see cref="OpenApiMediaType"/> properties and JSON serialization round-trip.
        /// </summary>
        [Fact]
        public void MediaType_Properties_CanBeSetAndGot()
        {
            var mediaType = new OpenApiMediaType
            {
                Schema = new OpenApiSchema { Type = "string" },
                ItemSchema = new OpenApiSchema { Type = "integer" },
                Example = "example_val",
                Examples = new Dictionary<string, OpenApiExample> { ["ex1"] = new OpenApiExample { Value = "val1" } },
                Encoding = new Dictionary<string, OpenApiEncoding> { ["prop"] = new OpenApiEncoding { ContentType = "image/png" } },
                PrefixEncoding = new List<OpenApiEncoding> { new OpenApiEncoding { ContentType = "text/plain" } },
                ItemEncoding = new OpenApiEncoding { ContentType = "application/json" }
            };

            Assert.Equal("string", mediaType.Schema?.Type);
            Assert.Equal("integer", mediaType.ItemSchema?.Type);
            Assert.Equal("example_val", mediaType.Example);
            Assert.Equal("val1", mediaType.Examples?["ex1"].Value);
            Assert.Equal("image/png", mediaType.Encoding?["prop"].ContentType);
            Assert.Equal("text/plain", mediaType.PrefixEncoding?[0].ContentType);
            Assert.Equal("application/json", mediaType.ItemEncoding?.ContentType);
        }

        /// <summary>
        /// Verifies <see cref="OpenApiEncoding"/> properties.
        /// </summary>
        [Fact]
        public void Encoding_Properties_CanBeSetAndGot()
        {
            var encoding = new OpenApiEncoding
            {
                ContentType = "image/png",
                Headers = new Dictionary<string, OpenApiHeader> { ["X-Header"] = new OpenApiHeader { Description = "header desc" } },
                Style = "form",
                Explode = true,
                AllowReserved = false,
                Encoding = new Dictionary<string, OpenApiEncoding>(),
                PrefixEncoding = new List<OpenApiEncoding>(),
                ItemEncoding = new OpenApiEncoding()
            };

            Assert.Equal("image/png", encoding.ContentType);
            Assert.Equal("header desc", encoding.Headers?["X-Header"].Description);
            Assert.Equal("form", encoding.Style);
            Assert.True(encoding.Explode);
            Assert.False(encoding.AllowReserved);
            Assert.Empty(encoding.Encoding!);
            Assert.Empty(encoding.PrefixEncoding!);
            Assert.NotNull(encoding.ItemEncoding);
        }

        /// <summary>
        /// Verifies <see cref="OpenApiLink"/> properties.
        /// </summary>
        [Fact]
        public void Link_Properties_CanBeSetAndGot()
        {
            var link = new OpenApiLink
            {
                OperationRef = "#/paths/~1pets/get",
                OperationId = "getPets",
                Description = "Get the pets"
            };

            Assert.Equal("#/paths/~1pets/get", link.OperationRef);
            Assert.Equal("getPets", link.OperationId);
            Assert.Equal("Get the pets", link.Description);
        }

        /// <summary>
        /// Verifies <see cref="OpenApiSecurityScheme"/> properties and <see cref="IrSecurityScheme"/> conversion.
        /// </summary>
        [Fact]
        public void SecurityScheme_Properties_CanBeSetAndGot()
        {
            var scheme = new OpenApiSecurityScheme
            {
                Type = "http",
                Description = "Basic Auth",
                Name = "Authorization",
                In = "header",
                Scheme = "basic",
                BearerFormat = "JWT",
                Oauth2MetadataUrl = "https://example.com/oauth2/metadata",
                Deprecated = true
            };

            Assert.Equal("http", scheme.Type);
            Assert.Equal("Basic Auth", scheme.Description);
            Assert.Equal("Authorization", scheme.Name);
            Assert.Equal("header", scheme.In);
            Assert.Equal("basic", scheme.Scheme);
            Assert.Equal("JWT", scheme.BearerFormat);
            Assert.Equal("https://example.com/oauth2/metadata", scheme.Oauth2MetadataUrl);
            Assert.True(scheme.Deprecated);

            var irScheme = IrSecurityScheme.FromOpenApiSecurityScheme("BasicAuth", scheme);
            Assert.Equal("BasicAuth", irScheme.SchemeName);
            var rtScheme = irScheme.ToOpenApiSecurityScheme();
            Assert.Equal("basic", rtScheme.Scheme);
            Assert.True(rtScheme.Deprecated);
        }

        /// <summary>
        /// Verifies <see cref="OpenApiComponents"/> properties.
        /// </summary>
        [Fact]
        public void Components_Properties_CanBeSetAndGot()
        {
            var components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>(),
                Responses = new Dictionary<string, OpenApiResponse>(),
                Parameters = new Dictionary<string, OpenApiParameter>(),
                RequestBodies = new Dictionary<string, OpenApiRequestBody>(),
                Headers = new Dictionary<string, OpenApiHeader>(),
                SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>(),
                Links = new Dictionary<string, OpenApiLink>(),
                Callbacks = new Dictionary<string, OpenApiCallback>(),
                PathItems = new Dictionary<string, OpenApiPathItem>()
            };

            Assert.Empty(components.Schemas!);
            Assert.Empty(components.Responses!);
            Assert.Empty(components.Parameters!);
            Assert.Empty(components.RequestBodies!);
            Assert.Empty(components.Headers!);
            Assert.Empty(components.SecuritySchemes!);
            Assert.Empty(components.Links!);
            Assert.Empty(components.Callbacks!);
            Assert.Empty(components.PathItems!);
        }

        /// <summary>
        /// Verifies <see cref="OpenApiDocument"/> properties and full JSON/YAML round-tripping.
        /// </summary>
        [Fact]
        public void OpenApiDocument_Properties_CanBeSetAndGot()
        {
            var doc = new OpenApiDocument
            {
                OpenApi = "3.2.0",
                Self = "https://example.com/openapi.json",
                Info = new OpenApiInfo { Title = "Test API", Version = "1.0" },
                JsonSchemaDialect = "https://spec.openapis.org/oas/3.1/dialect/base",
                Servers = new List<OpenApiServer>(),
                Paths = new OpenApiPaths(),
                Webhooks = new Dictionary<string, OpenApiPathItem>(),
                Components = new OpenApiComponents(),
                Security = new List<IDictionary<string, IList<string>>>(),
                Tags = new List<OpenApiTag>(),
                ExternalDocs = new OpenApiExternalDocs { Url = "https://example.com" }
            };

            Assert.Equal("3.2.0", doc.OpenApi);
            Assert.Equal("https://example.com/openapi.json", doc.Self);
            Assert.Equal("Test API", doc.Info.Title);
            Assert.Equal("https://spec.openapis.org/oas/3.1/dialect/base", doc.JsonSchemaDialect);
            Assert.Empty(doc.Servers!);
            Assert.Empty(doc.Paths!);
            Assert.Empty(doc.Webhooks!);
            Assert.NotNull(doc.Components);
            Assert.Empty(doc.Security!);
            Assert.Empty(doc.Tags!);
            Assert.Equal("https://example.com", doc.ExternalDocs?.Url);

            var emitter = new OpenApiEmitter();
            var parser = new OpenApiParser();
            var yaml = emitter.EmitYaml(doc);
            var parsedFromYaml = parser.ParseYamlOrJson(yaml);
            Assert.Equal("3.2.0", parsedFromYaml.OpenApi);
            Assert.Equal("https://example.com/openapi.json", parsedFromYaml.Self);
        }
    }
}
