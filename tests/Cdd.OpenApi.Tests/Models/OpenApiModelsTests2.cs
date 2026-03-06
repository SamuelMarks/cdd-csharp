using System.Collections.Generic;
using Xunit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Models
{
    public class OpenApiModelsTests2
    {
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
        }

        [Fact]
        public void Responses_Properties_CanBeSetAndGot()
        {
            var responses = new OpenApiResponses
            {
                Default = new OpenApiResponse { Description = "Default response" }
            };
            responses.Add("200", new OpenApiResponse { Description = "OK" });

            Assert.Equal("Default response", responses.Default?.Description);
            Assert.Equal("OK", responses["200"].Description);
        }

        [Fact]
        public void Schema_Properties_CanBeSetAndGot()
        {
            var schema = new OpenApiSchema
            {
                Ref = "#/components/schemas/Pet",
                Type = "object",
                Format = "custom",
                Items = new OpenApiSchema { Type = "string" },
                Properties = new Dictionary<string, OpenApiSchema> { ["id"] = new OpenApiSchema { Type = "integer" } },
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
        }

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

        [Fact]
        public void ServerVariable_Properties_CanBeSetAndGot()
        {
            var var = new OpenApiServerVariable
            {
                Enum = new List<string> { "a", "b" },
                Default = "a",
                Description = "A desc"
            };

            Assert.Equal(2, var.Enum?.Count);
            Assert.Equal("a", var.Default);
            Assert.Equal("A desc", var.Description);
        }

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
        }

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
        }
    }
}
