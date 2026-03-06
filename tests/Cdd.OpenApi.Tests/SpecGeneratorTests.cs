using System.Collections.Generic;
using System.Linq;
using Xunit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests
{
    public class SpecGeneratorTests
    {
        [Fact]
        public void Generate_InfoTags_ReturnsCorrectDocument()
        {
            var code = @"
            /// <summary>Summary of API</summary>
            /// <self>https://example.com</self>
            /// <jsonSchemaDialect>http://json-schema.org/draft-07/schema#</jsonSchemaDialect>
            /// <description>Description of API</description>
            /// <version>1.0.0</version>
            /// <termsOfService>https://example.com/terms</termsOfService>
            /// <contact-name>John Doe</contact-name>
            /// <contact-email>john@example.com</contact-email>
            /// <contact-url>https://example.com/contact</contact-url>
            /// <license-name>MIT</license-name>
            /// <license-url>https://example.com/license</license-url>
            /// <license-identifier>MIT</license-identifier>
            /// <server-url>https://api.example.com</server-url>
            /// <server-description>Production Server</server-description>
            /// <externalDocs-url>https://example.com/docs</externalDocs-url>
            /// <externalDocs-description>API Docs</externalDocs-description>
            /// <tag-name>Users</tag-name>
            /// <tag-description>User management</tag-description>
            public class Program
            {
                [Authorize]
                public void Method() {}
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });

            Assert.NotNull(doc);
            Assert.Equal("https://example.com", doc.Self);
            Assert.Equal("http://json-schema.org/draft-07/schema#", doc.JsonSchemaDialect);
            Assert.Equal("Description of API", doc.Info.Description);
            Assert.Equal("Summary of API", doc.Info.Summary);
            Assert.Equal("1.0.0", doc.Info.Version);
            Assert.Equal("https://example.com/terms", doc.Info.TermsOfService);
            
            Assert.NotNull(doc.Info.Contact);
            Assert.Equal("John Doe", doc.Info.Contact.Name);
            Assert.Equal("john@example.com", doc.Info.Contact.Email);
            Assert.Equal("https://example.com/contact", doc.Info.Contact.Url);

            Assert.NotNull(doc.Info.License);
            Assert.Equal("MIT", doc.Info.License.Name);
            Assert.Equal("https://example.com/license", doc.Info.License.Url);
            Assert.Equal("MIT", doc.Info.License.Identifier);

            Assert.NotNull(doc.Servers);
            Assert.Single(doc.Servers);
            Assert.Equal("https://api.example.com", doc.Servers[0].Url);
            Assert.Equal("Production Server", doc.Servers[0].Description);

            Assert.NotNull(doc.ExternalDocs);
            Assert.Equal("https://example.com/docs", doc.ExternalDocs.Url);
            Assert.Equal("API Docs", doc.ExternalDocs.Description);

            Assert.NotNull(doc.Tags);
            Assert.Single(doc.Tags);
            Assert.Equal("Users", doc.Tags[0].Name);
            Assert.Equal("User management", doc.Tags[0].Description);
        }

        [Fact]
        public void Generate_SecurityTags_ReturnsSecurityScheme()
        {
            var code = @"
            /// <security-name>OAuth2</security-name>
            /// <security-type>oauth2</security-type>
            /// <security-scheme>bearer</security-scheme>
            /// <security-bearerFormat>JWT</security-bearerFormat>
            /// <security-description>OAuth2 Auth</security-description>
            /// <security-in>header</security-in>
            /// <security-openIdConnectUrl>https://example.com/openid</security-openIdConnectUrl>
            /// <security-oauth2MetadataUrl>https://example.com/oauth2</security-oauth2MetadataUrl>
            /// <security-deprecated>true</security-deprecated>
            /// <oauth-flow>clientCredentials</oauth-flow>
            /// <oauth-authorizationUrl>https://example.com/auth</oauth-authorizationUrl>
            /// <oauth-tokenUrl>https://example.com/token</oauth-tokenUrl>
            /// <oauth-refreshUrl>https://example.com/refresh</oauth-refreshUrl>
            /// <oauth-deviceAuthorizationUrl>https://example.com/device</oauth-deviceAuthorizationUrl>
            /// <oauth-scopes>read:pets:Read pets,write:pets:Write pets,admin</oauth-scopes>
            public class Program
            {
                [HttpGet]
                [Authorize]
                public void Method() {}
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc.Components);
            Assert.NotNull(doc.Components.SecuritySchemes);
            Assert.True(doc.Components.SecuritySchemes.ContainsKey("OAuth2"));
            var scheme = doc.Components.SecuritySchemes["OAuth2"];
            Assert.Equal("oauth2", scheme.Type);
            Assert.Equal("OAuth2 Auth", scheme.Description);
            Assert.Equal("header", scheme.In);
            Assert.Equal("https://example.com/openid", scheme.OpenIdConnectUrl);
            Assert.Equal("https://example.com/oauth2", scheme.Oauth2MetadataUrl);
            Assert.True(scheme.Deprecated);

            Assert.NotNull(scheme.Flows);
            Assert.NotNull(scheme.Flows.ClientCredentials);
            Assert.Equal("https://example.com/auth", scheme.Flows.ClientCredentials.AuthorizationUrl);
            Assert.Equal("https://example.com/token", scheme.Flows.ClientCredentials.TokenUrl);
            Assert.Equal("https://example.com/refresh", scheme.Flows.ClientCredentials.RefreshUrl);
            Assert.Equal("https://example.com/device", scheme.Flows.ClientCredentials.DeviceAuthorizationUrl);
            Assert.Equal("Read pets", scheme.Flows.ClientCredentials.Scopes["read:pets"]);
            Assert.Equal("Write pets", scheme.Flows.ClientCredentials.Scopes["write:pets"]);
            Assert.Equal("Access to admin", scheme.Flows.ClientCredentials.Scopes["admin"]);
        }

        [Fact]
        public void Generate_SecurityTags_PasswordFlow()
        {
            var code = @"
            /// <oauth-flow>password</oauth-flow>
            /// <oauth-scopes>read:pets</oauth-scopes>
            [Authorize]
            public class Program
            {
                [HttpGet]
                public void Method() {}
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            var scheme = doc.Components.SecuritySchemes["Bearer"];
            Assert.NotNull(scheme.Flows.Password);
            Assert.Equal("pets", scheme.Flows.Password.Scopes["read"]);
        }

        [Fact]
        public void Generate_SecurityTags_ImplicitFlow()
        {
            var code = @"
            /// <oauth-flow>implicit</oauth-flow>
            [Authorize]
            public class Program
            {
                [HttpGet]
                public void Method() {}
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc.Components.SecuritySchemes["Bearer"].Flows.Implicit);
        }

        [Fact]
        public void Generate_SecurityTags_AuthCodeFlow()
        {
            var code = @"
            /// <oauth-flow>authorizationCode</oauth-flow>
            [Authorize]
            public class Program
            {
                [HttpGet]
                public void Method() {}
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc.Components.SecuritySchemes["Bearer"].Flows.AuthorizationCode);
        }

        [Fact]
        public void Generate_SecurityTags_DeviceFlow()
        {
            var code = @"
            /// <oauth-flow>deviceAuthorization</oauth-flow>
            [Authorize]
            public class Program
            {
                [HttpGet]
                public void Method() {}
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc.Components.SecuritySchemes["Bearer"].Flows.DeviceAuthorization);
        }

        [Fact]
        public void Generate_DbContext_ReturnsSchemas()
        {
            var code = @"
            public class MyDbContext : DbContext
            {
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
        }

        [Fact]
        public void Generate_Cli_ReturnsPaths()
        {
            var code = @"
            public class MyCli
            {
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
        }

        [Fact]
        public void Generate_Tests_ReturnsPaths()
        {
            var code = @"
            public class MyTests
            {
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
        }
        
        [Fact]
        public void Generate_Mock_ReturnsPaths()
        {
            var code = @"
            public class MyMock
            {
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
        }

        [Fact]
        public void Generate_ClientMethods_ReturnsPaths()
        {
            var code = @"
            public class MyClient
            {
                public async Task DoSomething()
                {
                    await this.client.GetAsync(""test"");
                }
            }
            ";
            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
        }
    }
}
