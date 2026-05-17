using System.Collections.Generic;
using Xunit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Models
{
    public class CoverageTests
    {
        [Fact]
        public void OpenApiParameter_Properties()
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
                Schema = new OpenApiSchema(),
                Example = "example",
                Examples = new Dictionary<string, OpenApiExample>(),
                Content = new Dictionary<string, OpenApiMediaType>()
            };
            Assert.Equal("name", p.Name);
            Assert.Equal("query", p.In);
            Assert.Equal("desc", p.Description);
            Assert.True(p.Required);
            Assert.True(p.Deprecated);
            Assert.True(p.AllowEmptyValue);
            Assert.Equal("form", p.Style);
            Assert.True(p.Explode);
            Assert.True(p.AllowReserved);
            Assert.NotNull(p.Schema);
            Assert.Equal("example", p.Example);
            Assert.NotNull(p.Examples);
            Assert.NotNull(p.Content);

            p.Type = "string";
            p.Format = "uuid";
            p.Items = new OpenApiSchema();
            p.CollectionFormat = "csv";
            p.Default = "def";
            p.Maximum = 10;
            p.ExclusiveMaximum = true;
            p.Minimum = 1;
            p.ExclusiveMinimum = true;
            p.MaxLength = 10;
            p.MinLength = 1;
            p.Pattern = ".*";
            p.MaxItems = 10;
            p.MinItems = 1;
            p.UniqueItems = true;
            p.Enum = new List<object> { "a", "b" };
            p.MultipleOf = 2;

            Assert.Equal("string", p.Type);
            Assert.Equal("uuid", p.Format);
            Assert.NotNull(p.Items);
            Assert.Equal("csv", p.CollectionFormat);
            Assert.Equal("def", p.Default);
            Assert.Equal(10, p.Maximum);
            Assert.True(p.ExclusiveMaximum);
            Assert.Equal(1, p.Minimum);
            Assert.True(p.ExclusiveMinimum);
            Assert.Equal(10, p.MaxLength);
            Assert.Equal(1, p.MinLength);
            Assert.Equal(".*", p.Pattern);
            Assert.Equal(10, p.MaxItems);
            Assert.Equal(1, p.MinItems);
            Assert.True(p.UniqueItems);
            Assert.NotNull(p.Enum);
            Assert.Equal(2, p.MultipleOf);
        }

        [Fact]
        public void OpenApiSecurityScheme_Properties()
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
            Assert.Equal("oauth2", s.Type);
            Assert.Equal("desc", s.Description);
            Assert.Equal("name", s.Name);
            Assert.Equal("header", s.In);
            Assert.Equal("bearer", s.Scheme);
            Assert.Equal("JWT", s.BearerFormat);
            Assert.NotNull(s.Flows);
            Assert.Equal("url", s.OpenIdConnectUrl);
            Assert.Equal("oauth_url", s.Oauth2MetadataUrl);
            Assert.True(s.Deprecated);
            Assert.Equal("implicit", s.Flow);
            Assert.Equal("auth_url", s.AuthorizationUrl);
            Assert.Equal("token_url", s.TokenUrl);
            Assert.NotNull(s.Scopes);
        }
    }
}
