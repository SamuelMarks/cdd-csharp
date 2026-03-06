using System.IO;
using Xunit;
using Cdd.OpenApi.Parse;
using Cdd.OpenApi.Emit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void ParseAndEmit_PreservesInformation()
        {
            var json = @"
            {
                ""openapi"": ""3.2.0"",
                ""$self"": ""https://example.com/api"",
                ""jsonSchemaDialect"": ""https://spec.openapis.org/oas/3.1/dialect/base"",
                ""info"": {
                    ""title"": ""Test API"",
                    ""version"": ""1.0.0"",
                    ""contact"": {
                        ""name"": ""API Support"",
                        ""email"": ""support@example.com""
                    }
                },
                ""servers"": [
                    {
                        ""url"": ""https://api.example.com/v1"",
                        ""description"": ""Production""
                    }
                ],
                ""paths"": {
                    ""/pets"": {
                        ""get"": {
                            ""summary"": ""List pets"",
                            ""operationId"": ""listPets"",
                            ""tags"": [""pets""],
                            ""responses"": {
                                ""200"": {
                                    ""description"": ""Success""
                                }
                            }
                        }
                    }
                },
                ""webhooks"": {
                    ""newPet"": {
                        ""post"": {
                            ""requestBody"": {
                                ""description"": ""A new pet webhook"",
                                ""content"": {
                                    ""application/json"": {
                                        ""schema"": {
                                            ""$ref"": ""#/components/schemas/Pet""
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                ""components"": {
                    ""schemas"": {
                        ""Pet"": {
                            ""type"": ""object"",
                            ""properties"": {
                                ""id"": { ""type"": ""integer"" },
                                ""name"": { ""type"": ""string"" }
                            },
                            ""allOf"": [ { ""type"": ""object"" } ],
                            ""anyOf"": [ { ""type"": ""object"" } ],
                            ""oneOf"": [ { ""type"": ""object"" } ]
                        }
                    }
                }
            }";

            var parser = new OpenApiParser();
            var doc = parser.ParseJson(json);

            var emitter = new OpenApiEmitter();
            var emittedJson = emitter.EmitJson(doc);

            // Re-parse to verify properties remain
            var doc2 = parser.ParseJson(emittedJson);

            Assert.Equal(doc.OpenApi, doc2.OpenApi);
            Assert.Equal(doc.Self, doc2.Self);
            Assert.Equal(doc.JsonSchemaDialect, doc2.JsonSchemaDialect);
            Assert.Equal(doc.Info.Title, doc2.Info.Title);
            Assert.Equal(doc.Info.Contact?.Name, doc2.Info.Contact?.Name);
            Assert.Equal(doc.Servers?[0].Url, doc2.Servers?[0].Url);
            Assert.Equal(doc.Paths?["/pets"].Get?.OperationId, doc2.Paths?["/pets"].Get?.OperationId);
            Assert.NotNull(doc2.Webhooks?["newPet"].Post);
            Assert.Equal(doc.Components?.Schemas?["Pet"].Type, doc2.Components?.Schemas?["Pet"].Type);
            Assert.NotNull(doc2.Components?.Schemas?["Pet"].AllOf);
            Assert.NotNull(doc2.Components?.Schemas?["Pet"].AnyOf);
            Assert.NotNull(doc2.Components?.Schemas?["Pet"].OneOf);
        }
        [Fact]
        public void GeneratesInfoFromProgramClassTags()
        {
            var code = @"
/// <summary>My API Summary</summary>
/// <description>A cool API</description>
/// <version>1.5.0</version>
/// <contact-name>John Doe</contact-name>
/// <contact-email>john@example.com</contact-email>
/// <license-name>MIT</license-name>
public class Program
{
}
";

            var doc = SpecGenerator.Generate(new[] { code });

            Assert.Equal("My API Summary", doc.Info.Summary);
            Assert.Equal("A cool API", doc.Info.Description);
            Assert.Equal("1.5.0", doc.Info.Version);
            Assert.NotNull(doc.Info.Contact);
            Assert.Equal("John Doe", doc.Info.Contact.Name);
            Assert.Equal("john@example.com", doc.Info.Contact.Email);
            Assert.NotNull(doc.Info.License);
            Assert.Equal("MIT", doc.Info.License.Name);
        }
        [Fact]
        public void GeneratesServersFromProgramClassTags()
        {
            var code = @"
/// <server-url>https://api.example.com/v1</server-url>
/// <server-description>Production Server</server-description>
public class Program
{
}
";

            var doc = SpecGenerator.Generate(new[] { code });

            Assert.NotNull(doc.Servers);
            var server = Assert.Single(doc.Servers);
            Assert.Equal("https://api.example.com/v1", server.Url);
            Assert.Equal("Production Server", server.Description);
        }
        [Fact]
        public void GeneratesExternalDocsFromProgramClassTags()
        {
            var code = @"
/// <externalDocs-url>https://example.com/docs</externalDocs-url>
/// <externalDocs-description>Find more info here</externalDocs-description>
public class Program
{
}
";

            var doc = SpecGenerator.Generate(new[] { code });

            Assert.NotNull(doc.ExternalDocs);
            Assert.Equal("https://example.com/docs", doc.ExternalDocs.Url);
            Assert.Equal("Find more info here", doc.ExternalDocs.Description);
        }
        [Fact]
        public void GeneratesSchemaWithDiscriminatorAndXml()
        {
            var code = @"
            /// <discriminator>type</discriminator>
            /// <discriminator-defaultMapping>fallbackType</discriminator-defaultMapping>
            /// <xml-name>Widget</xml-name>
            /// <xml-attribute>true</xml-attribute>
            public class Widget
            {
                /// <xml-wrapped>true</xml-wrapped>
                public string Name { get; set; }
            }";

            var doc = SpecGenerator.Generate(new[] { code });

            Assert.NotNull(doc.Components?.Schemas);
            var schema = doc.Components.Schemas["Widget"];
            
            Assert.NotNull(schema.Discriminator);
            Assert.Equal("type", schema.Discriminator.PropertyName);
            Assert.Equal("fallbackType", schema.Discriminator.DefaultMapping);

            Assert.NotNull(schema.Xml);
            Assert.Equal("Widget", schema.Xml.Name);
            Assert.True(schema.Xml.Attribute);

            var nameProp = schema.Properties?["Name"];
            Assert.NotNull(nameProp?.Xml);
            Assert.True(nameProp.Xml.Wrapped);
        }
        [Fact]
        public void GeneratesSecuritySchemeFromAuthorizeAttribute()
        {
            var code = @"
            using Microsoft.AspNetCore.Authorization;
            /// <security-name>OAuth2</security-name>
            /// <security-type>openIdConnect</security-type>
            /// <security-openIdConnectUrl>https://example.com/oidc</security-openIdConnectUrl>
            /// <oauth-flow>clientCredentials</oauth-flow>
            /// <oauth-tokenUrl>https://example.com/token</oauth-tokenUrl>
            /// <oauth-scopes>read:pets:Read pets</oauth-scopes>
            [Authorize]
            public class SecureApi
            {
                [HttpGet(""/secure"")]
                public void GetSecure() {}
            }";

            var doc = SpecGenerator.Generate(new[] { code });

            Assert.NotNull(doc.Components?.SecuritySchemes);
            var scheme = doc.Components.SecuritySchemes["OAuth2"];
            Assert.NotNull(scheme);
            Assert.Equal("oauth2", scheme.Type); // Forced to oauth2 by presence of flows
            Assert.Equal("https://example.com/oidc", scheme.OpenIdConnectUrl);
            
            Assert.NotNull(scheme.Flows);
            Assert.NotNull(scheme.Flows.ClientCredentials);
            Assert.Equal("https://example.com/token", scheme.Flows.ClientCredentials.TokenUrl);
            
            Assert.NotNull(scheme.Flows.ClientCredentials.Scopes);
            Assert.True(scheme.Flows.ClientCredentials.Scopes.ContainsKey("read:pets"));
            Assert.Equal("Read pets", scheme.Flows.ClientCredentials.Scopes["read:pets"]);
            
            Assert.NotNull(doc.Security);
            var req = Assert.Single(doc.Security);
            Assert.True(req.ContainsKey("OAuth2"));
        }
        [Fact]
        public void GeneratesSecuritySchemeFromAuthorizeAttribute_WithMoreOAuthFlows()
        {
            var code = @"
            using Microsoft.AspNetCore.Authorization;
            /// <security-name>OAuth2Dev</security-name>
            /// <oauth-flow>deviceAuthorization</oauth-flow>
            /// <oauth-authorizationUrl>https://example.com/auth</oauth-authorizationUrl>
            /// <oauth-tokenUrl>https://example.com/token</oauth-tokenUrl>
            /// <oauth-refreshUrl>https://example.com/refresh</oauth-refreshUrl>
            [Authorize]
            public class SecureApiDev
            {
                [HttpGet(""/secureDev"")]
                public void GetSecure() {}
            }";

            var doc = SpecGenerator.Generate(new[] { code });

            Assert.NotNull(doc.Components?.SecuritySchemes);
            var scheme = doc.Components.SecuritySchemes["OAuth2Dev"];
            Assert.NotNull(scheme.Flows);
            Assert.NotNull(scheme.Flows.DeviceAuthorization);
            Assert.Equal("https://example.com/auth", scheme.Flows.DeviceAuthorization.AuthorizationUrl);
            Assert.Equal("https://example.com/token", scheme.Flows.DeviceAuthorization.TokenUrl);
            Assert.Equal("https://example.com/refresh", scheme.Flows.DeviceAuthorization.RefreshUrl);
        }
        [Fact]
        public void GeneratesSecuritySchemeFromAuthorizeAttribute_WithMetadataAndDeprecated()
        {
            var code = @"
            using Microsoft.AspNetCore.Authorization;
            /// <security-name>OAuth2Meta</security-name>
            /// <security-oauth2MetadataUrl>https://meta.com/.well-known</security-oauth2MetadataUrl>
            /// <security-deprecated>true</security-deprecated>
            [Authorize]
            public class SecureMetaApi
            {
                [HttpGet(""/meta"")]
                public void GetSecure() {}
            }";

            var doc = SpecGenerator.Generate(new[] { code });

            Assert.NotNull(doc.Components?.SecuritySchemes);
            var scheme = doc.Components.SecuritySchemes["OAuth2Meta"];
            Assert.Equal("https://meta.com/.well-known", scheme.Oauth2MetadataUrl);
            Assert.True(scheme.Deprecated);
        }
        [Fact]
        public void GeneratesSecuritySchemeFromAuthorizeAttribute_WithMultipleSecurityRequirements()
        {
            var code = @"
            using Microsoft.AspNetCore.Authorization;
            public class SecureOpApi
            {
                /// <security name=""OAuth2"" scopes=""read:users"">Auth</security>
                /// <security name=""ApiKey"">Key</security>
                [HttpGet(""/secureOp"")]
                public void GetSecure() {}
            }";

            var doc = SpecGenerator.Generate(new[] { code });

            Assert.NotNull(doc.Paths);
            var op = doc.Paths["/secureOp"]?.Get;
            Assert.NotNull(op);
            Assert.NotNull(op.Security);
            Assert.Equal(2, op.Security.Count);
            
            Assert.True(op.Security[0].ContainsKey("OAuth2"));
            Assert.Contains("read:users", op.Security[0]["OAuth2"]);
            
            Assert.True(op.Security[1].ContainsKey("ApiKey"));
        }
    }
}
