// <copyright file="SpecGeneratorTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Xunit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests
{
    /// <summary>Verifies or exercises spec generator tests.</summary>
    public class SpecGeneratorTests
    {
        /// <summary>Verifies or exercises generate info tags returns correct document.</summary>
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

        /// <summary>Verifies or exercises generate security tags returns security scheme.</summary>
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

        /// <summary>Verifies or exercises generate security tags password flow.</summary>
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
            var scheme = doc.Components!.SecuritySchemes!["Bearer"];
            Assert.NotNull(scheme.Flows!.Password);
            Assert.Equal("pets", scheme.Flows.Password.Scopes!["read"]);
        }

        /// <summary>Verifies or exercises generate security tags implicit flow.</summary>
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
            Assert.NotNull(doc.Components!.SecuritySchemes!["Bearer"].Flows!.Implicit);
        }

        /// <summary>Verifies or exercises generate security tags auth code flow.</summary>
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
            Assert.NotNull(doc.Components!.SecuritySchemes!["Bearer"].Flows!.AuthorizationCode);
        }

        /// <summary>Verifies or exercises generate security tags device flow.</summary>
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
            Assert.NotNull(doc.Components!.SecuritySchemes!["Bearer"].Flows!.DeviceAuthorization);
        }

        /// <summary>Verifies or exercises generate db context returns schemas.</summary>
        [Fact]
        public void Generate_DbContext_ReturnsSchemas()
        {
            var code = @"
            using Microsoft.EntityFrameworkCore;
            public class User { public int Id { get; set; } }
            public class MyDbContext : DbContext
            {
                public DbSet<User> Users { get; set; }
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
            Assert.NotNull(doc.Components);
            Assert.NotNull(doc.Components.Schemas);
            Assert.True(doc.Components.Schemas.ContainsKey("User"));
        }

        /// <summary>Verifies or exercises generate cli returns paths.</summary>
        [Fact]
        public void Generate_Cli_ReturnsPaths()
        {
            var code = @"
            public class MyCli
            {
                public void Run() {
                    switch (args[0]) {
                        case ""test"": break;
                    }
                }
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
            Assert.NotNull(doc.Paths);
            Assert.True(doc.Paths.ContainsKey("/test"));
        }

        /// <summary>Verifies or exercises generate tests returns paths.</summary>
        [Fact]
        public void Generate_Tests_ReturnsPaths()
        {
            var code = @"
            public class MyTests
            {
                public void Test1() {}
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
            Assert.NotNull(doc.Paths);
            Assert.True(doc.Paths.ContainsKey("/test1"));
        }

        /// <summary>Verifies or exercises generate mock returns paths.</summary>
        [Fact]
        public void Generate_Mock_ReturnsPaths()
        {
            var code = @"
            public class MyMock
            {
                public void MockMethod() {}
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
            Assert.NotNull(doc.Paths);
            Assert.True(doc.Paths.ContainsKey("/mockmethod"));
        }

        /// <summary>Verifies or exercises generate client methods returns paths.</summary>
        [Fact]
        public void Generate_ClientMethods_ReturnsPaths()
        {
            var code = @"
            public class MyClient1
            {
                public async Task DoSomething()
                {
                    await this.client.GetAsync(""test"");
                    await this.client.PatchAsync(""test""); // ends with Async, doesn't start with Get/Post/Put/Delete
                    await this.client.GetSomething(""test""); // starts with Get, doesn't end with Async
                    DoLocalAsync(); // not a member access
                }
            }
            ";
            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
            Assert.NotNull(doc.Paths);
            Assert.True(doc.Paths.ContainsKey("/test"));
            Assert.NotNull(doc.Paths["/test"].Get);
            Assert.Equal("DoSomething", doc.Paths["/test"].Get!.OperationId);
        }


        /// <summary>Verifies or exercises generate client methods other method.</summary>
        [Fact]
        public void Generate_ClientMethods_OtherMethod()
        {
            var code = @"
            public class MyClient
            {
                public async Task DoSomething()
                {
                    await GetAsync(""test""); // no member access
                    await this.client.OtherAsync(""test"");
                    await this.client.GetMethod(""test"");
                }
            }
            ";
            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
            Assert.Null(doc.Paths);
            Assert.NotNull(doc.Components?.Schemas);
            Assert.True(doc.Components!.Schemas!.ContainsKey("MyClient"));
        }


        /// <summary>Verifies or exercises generate security tags scope no colon2.</summary>
        [Fact]
        public void Generate_SecurityTags_ScopeNoColon2()
        {
            var code = @"
            /// <security-name>OAuth2</security-name>
            /// <security-type>oauth2</security-type>
            /// <oauth-flow>clientCredentials</oauth-flow>
            /// <oauth-scopes>read:pets:Read pets</oauth-scopes>
            public class Program2
            {
                [Authorize]
                [HttpGet]
                public void Method() {}
            }
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            var scheme = doc.Components!.SecuritySchemes!["OAuth2"];
            Assert.Equal("Read pets", scheme.Flows!.ClientCredentials!.Scopes!["read:pets"]);
        }



        /// <summary>Verifies or exercises generate empty security schemes.</summary>
        [Fact]
        public void Generate_EmptySecuritySchemes()
        {
            var code = @"
            using Microsoft.AspNetCore.Mvc;
            public class EmptyController
            {
                [HttpGet]
                public void Empty() {}
            }
            ";
            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
        }

        /// <summary>Verifies or exercises generate security tags oauth scopes formats.</summary>
        /// <param name="scopesTag">The scopes tag string.</param>
        /// <param name="expectedKey">The expected scope key.</param>
        /// <param name="expectedDescription">The expected scope description.</param>
        [Theory]
        [InlineData("admin", "admin", "Access to admin")]
        [InlineData("admin:admin", "admin", "admin")]
        [InlineData("read:pets:Read pets", "read:pets", "Read pets")]
        public void Generate_SecurityTags_OAuthScopesFormats(string scopesTag, string expectedKey, string expectedDescription)
        {
            var code = $@"
            /// <security-name>OAuth2</security-name>
            /// <security-type>oauth2</security-type>
            /// <oauth-flow>clientCredentials</oauth-flow>
            /// <oauth-scopes>{scopesTag}</oauth-scopes>
            public class Program
            {{
                [Authorize]
                [HttpGet]
                public void Method() {{}}
            }}
            ";

            var doc = SpecGenerator.Generate(new[] { code });
            var scheme = doc.Components!.SecuritySchemes!["OAuth2"];
            Assert.Equal(expectedDescription, scheme.Flows!.ClientCredentials!.Scopes![expectedKey]);
        }


        /// <summary>Verifies or exercises generate empty components sets to null.</summary>
        [Fact]
        public void Generate_EmptyComponents_SetsToNull()
        {
            var code = @"
            using Microsoft.AspNetCore.Mvc;
            public class MyController
            {
                [HttpGet]
                public void Empty() {}
            }
            ";
            var doc = SpecGenerator.Generate(new[] { code });
            Assert.Null(doc.Components);
        }

        /// <summary>Verifies or exercises generate empty doc removes empty collections.</summary>
        [Fact]
        public void Generate_EmptyDoc_RemovesEmptyCollections()
        {
            var code = @"";
            var doc = SpecGenerator.Generate(new[] { code });
            Assert.NotNull(doc);
            Assert.Null(doc.Paths);
            Assert.Null(doc.Components);
        }
    }
}
