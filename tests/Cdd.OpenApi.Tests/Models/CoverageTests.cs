using System.Collections.Generic;
using Xunit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Parse;
using Cdd.OpenApi.Emit;

namespace Cdd.OpenApi.Tests.Models
{
    public class CoverageTests
    {
        [Fact]
        public void OpenApiParameter_PropertiesAndRoundTrip()
        {
            var p = new OpenApiParameter
            {
                Name = "name",
                In = "query",
                Description = "desc",
                Required = true,
                Deprecated = true,
                AllowEmptyValue = true,
                Style = "form",
                Explode = true,
                AllowReserved = true,
                Schema = new OpenApiSchema { Type = "string" },
                Example = "example",
                Examples = new Dictionary<string, OpenApiExample> { ["ex1"] = new OpenApiExample { Value = "val" } },
                Content = new Dictionary<string, OpenApiMediaType> { ["text/plain"] = new OpenApiMediaType() },
                Type = "string",
                Format = "uuid",
                Items = new OpenApiSchema { Type = "string" },
                CollectionFormat = "csv",
                Default = "def",
                Maximum = 10,
                ExclusiveMaximum = true,
                Minimum = 1,
                ExclusiveMinimum = true,
                MaxLength = 10,
                MinLength = 1,
                Pattern = ".*",
                MaxItems = 10,
                MinItems = 1,
                UniqueItems = true,
                Enum = new List<OpenApiAny> { "a", "b" },
                MultipleOf = 2
            };

            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/items"] = new OpenApiPathItem
                    {
                        Get = new OpenApiOperation { Parameters = new List<OpenApiParameter> { p } }
                    }
                }
            };

            var json = new OpenApiEmitter().EmitJson(doc);
            var parsedDoc = new OpenApiParser().ParseJson(json);
            var parsedParam = parsedDoc.Paths!["/items"].Get!.Parameters![0];

            Assert.Equal("name", parsedParam.Name);
            Assert.Equal("query", parsedParam.In);
            Assert.Equal("desc", parsedParam.Description);
            Assert.True(parsedParam.Required);
            Assert.True(parsedParam.Deprecated);
            Assert.True(parsedParam.AllowEmptyValue);
            Assert.Equal("form", parsedParam.Style);
            Assert.True(parsedParam.Explode);
            Assert.True(parsedParam.AllowReserved);
            Assert.NotNull(parsedParam.Schema);
            Assert.NotNull(parsedParam.Example);
            Assert.NotNull(parsedParam.Examples);
            Assert.NotNull(parsedParam.Content);
            Assert.Equal("string", parsedParam.Type);
            Assert.Equal("uuid", parsedParam.Format);
            Assert.NotNull(parsedParam.Items);
            Assert.Equal("csv", parsedParam.CollectionFormat);
            Assert.NotNull(parsedParam.Default);
            Assert.Equal(10, parsedParam.Maximum);
            Assert.True(parsedParam.ExclusiveMaximum);
            Assert.Equal(1, parsedParam.Minimum);
            Assert.True(parsedParam.ExclusiveMinimum);
            Assert.Equal(10, parsedParam.MaxLength);
            Assert.Equal(1, parsedParam.MinLength);
            Assert.Equal(".*", parsedParam.Pattern);
            Assert.Equal(10, parsedParam.MaxItems);
            Assert.Equal(1, parsedParam.MinItems);
            Assert.True(parsedParam.UniqueItems);
            Assert.NotNull(parsedParam.Enum);
            Assert.Equal(2, parsedParam.MultipleOf);
        }

        [Fact]
        public void OpenApiSecurityScheme_PropertiesAndRoundTrip()
        {
            var s = new OpenApiSecurityScheme
            {
                Type = "oauth2",
                Description = "desc",
                Name = "name",
                In = "header",
                Scheme = "bearer",
                BearerFormat = "JWT",
                Flows = new OpenApiOAuthFlows(),
                OpenIdConnectUrl = "url",
                Oauth2MetadataUrl = "oauth_url",
                Deprecated = true,
                Flow = "implicit",
                AuthorizationUrl = "auth_url",
                TokenUrl = "token_url",
                Scopes = new Dictionary<string, string> { { "read", "read access" } }
            };

            var doc = new OpenApiDocument
            {
                Components = new OpenApiComponents
                {
                    SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme> { ["OAuth"] = s }
                }
            };

            var json = new OpenApiEmitter().EmitJson(doc);
            var parsedDoc = new OpenApiParser().ParseJson(json);
            var parsedScheme = parsedDoc.Components!.SecuritySchemes!["OAuth"];

            Assert.Equal("oauth2", parsedScheme.Type);
            Assert.Equal("desc", parsedScheme.Description);
            Assert.Equal("name", parsedScheme.Name);
            Assert.Equal("header", parsedScheme.In);
            Assert.Equal("bearer", parsedScheme.Scheme);
            Assert.Equal("JWT", parsedScheme.BearerFormat);
            Assert.NotNull(parsedScheme.Flows);
            Assert.Equal("url", parsedScheme.OpenIdConnectUrl);
            Assert.Equal("oauth_url", parsedScheme.Oauth2MetadataUrl);
            Assert.True(parsedScheme.Deprecated);
            Assert.Equal("implicit", parsedScheme.Flow);
            Assert.Equal("auth_url", parsedScheme.AuthorizationUrl);
            Assert.Equal("token_url", parsedScheme.TokenUrl);
            Assert.NotNull(parsedScheme.Scopes);
        }
    }
}
