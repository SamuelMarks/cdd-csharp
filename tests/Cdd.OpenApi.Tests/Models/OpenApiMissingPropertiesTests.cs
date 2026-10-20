// <copyright file="OpenApiMissingPropertiesTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Xunit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Parse;
using Cdd.OpenApi.Emit;

namespace Cdd.OpenApi.Tests.Models
{
    /// <summary>Verifies or exercises open api missing properties tests.</summary>
    public class OpenApiMissingPropertiesTests
    {
        /// <summary>Verifies or exercises response headers and links round trip.</summary>
        [Fact]
        public void Response_HeadersAndLinks_RoundTrip()
        {
            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/items"] = new OpenApiPathItem
                    {
                        Get = new OpenApiOperation
                        {
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    Description = "OK",
                                    Headers = new Dictionary<string, OpenApiHeader> { ["X-Rate-Limit"] = new OpenApiHeader { Description = "Limit" } },
                                    Links = new Dictionary<string, OpenApiLink>
                                    {
                                        ["GetItem"] = new OpenApiLink
                                        {
                                            Parameters = new Dictionary<string, OpenApiAny> { ["id"] = "123" },
                                            RequestBody = "body_expr",
                                            Server = new OpenApiServer { Url = "http://test" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var json = new OpenApiEmitter().EmitJson(doc);
            var parsedDoc = new OpenApiParser().ParseJson(json);
            var res = parsedDoc.Paths!["/items"].Get!.Responses!["200"];

            Assert.NotNull(res.Headers);
            Assert.Single(res.Headers);
            Assert.NotNull(res.Links);
            Assert.Single(res.Links);
            var link = res.Links["GetItem"];
            Assert.NotNull(link.Parameters);
            Assert.Single(link.Parameters);
            Assert.NotNull(link.RequestBody);
            Assert.Equal("http://test", link.Server?.Url);
        }

        /// <summary>Verifies or exercises security scheme flows and open id round trip.</summary>
        [Fact]
        public void SecurityScheme_FlowsAndOpenId_RoundTrip()
        {
            var doc = new OpenApiDocument
            {
                Components = new OpenApiComponents
                {
                    SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
                    {
                        ["Sec"] = new OpenApiSecurityScheme
                        {
                            Type = "oauth2",
                            OpenIdConnectUrl = "http://openid",
                            Flows = new OpenApiOAuthFlows
                            {
                                Implicit = new OpenApiOAuthFlow { AuthorizationUrl = "http://auth" },
                                Password = new OpenApiOAuthFlow { TokenUrl = "http://token" },
                                ClientCredentials = new OpenApiOAuthFlow { RefreshUrl = "http://refresh" },
                                AuthorizationCode = new OpenApiOAuthFlow { Scopes = new Dictionary<string, string> { ["read"] = "Read access" } }
                            }
                        }
                    }
                }
            };

            var json = new OpenApiEmitter().EmitJson(doc);
            var parsedDoc = new OpenApiParser().ParseJson(json);
            var sec = parsedDoc.Components!.SecuritySchemes!["Sec"];

            Assert.Equal("http://openid", sec.OpenIdConnectUrl);
            Assert.Equal("http://auth", sec.Flows?.Implicit?.AuthorizationUrl);
            Assert.Equal("http://token", sec.Flows?.Password?.TokenUrl);
            Assert.Equal("http://refresh", sec.Flows?.ClientCredentials?.RefreshUrl);
            Assert.NotNull(sec.Flows?.AuthorizationCode?.Scopes);
            Assert.Single(sec.Flows.AuthorizationCode.Scopes);
        }
    }
}
