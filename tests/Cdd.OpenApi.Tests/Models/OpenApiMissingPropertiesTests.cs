using System.Collections.Generic;
using Xunit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Models
{
    public class OpenApiMissingPropertiesTests
    {
        [Fact]
        public void Response_HeadersAndLinks_CanBeSet()
        {
            var res = new OpenApiResponse
            {
                Headers = new Dictionary<string, OpenApiHeader> { ["X-Rate-Limit"] = new OpenApiHeader() },
                Links = new Dictionary<string, OpenApiLink> { ["GetItem"] = new OpenApiLink() }
            };
            Assert.Single(res.Headers);
            Assert.Single(res.Links);
        }

        [Fact]
        public void Link_Properties_CanBeSet()
        {
            var link = new OpenApiLink
            {
                Parameters = new Dictionary<string, object> { ["id"] = "123" },
                RequestBody = new { Name = "test" },
                Server = new OpenApiServer { Url = "http://test" }
            };
            Assert.Single(link.Parameters);
            Assert.NotNull(link.RequestBody);
            Assert.Equal("http://test", link.Server.Url);
        }

        [Fact]
        public void SecurityScheme_FlowsAndOpenId_CanBeSet()
        {
            var sec = new OpenApiSecurityScheme
            {
                OpenIdConnectUrl = "http://openid",
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow { AuthorizationUrl = "http://auth" },
                    Password = new OpenApiOAuthFlow { TokenUrl = "http://token" },
                    ClientCredentials = new OpenApiOAuthFlow { RefreshUrl = "http://refresh" },
                    AuthorizationCode = new OpenApiOAuthFlow { Scopes = new Dictionary<string, string> { ["read"] = "Read access" } }
                }
            };
            Assert.Equal("http://openid", sec.OpenIdConnectUrl);
            Assert.Equal("http://auth", sec.Flows.Implicit.AuthorizationUrl);
            Assert.Equal("http://token", sec.Flows.Password.TokenUrl);
            Assert.Equal("http://refresh", sec.Flows.ClientCredentials.RefreshUrl);
            Assert.Single(sec.Flows.AuthorizationCode.Scopes);
        }
    }
}
