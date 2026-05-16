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
            Assert.Equal("string", p.Type);
            Assert.Equal("uuid", p.Format);
            Assert.NotNull(p.Items);
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
                OpenIdConnectUrl = "url"
            };
            Assert.Equal("oauth2", s.Type);
            Assert.Equal("desc", s.Description);
            Assert.Equal("name", s.Name);
            Assert.Equal("header", s.In);
            Assert.Equal("bearer", s.Scheme);
            Assert.Equal("JWT", s.BearerFormat);
            Assert.NotNull(s.Flows);
            Assert.Equal("url", s.OpenIdConnectUrl);
        }
    }
}