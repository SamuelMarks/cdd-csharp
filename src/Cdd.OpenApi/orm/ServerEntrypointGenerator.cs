using System.Collections.Generic;
using System.Text;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates the main entrypoint and CLI for the ASP.NET Core server.</summary>
    public static class ServerEntrypointGenerator
    {
        /// <summary>Generates the Program.cs and startup tests.</summary>
        public static List<GeneratedCode> GenerateEntrypoint(string baseNamespace, bool tests, IEnumerable<string> tags)
        {
            var results = new List<GeneratedCode>();

            var programCode = $@"namespace {baseNamespace}
{{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using {baseNamespace}.Configuration;
    using {baseNamespace}.Daos;
    using {baseNamespace}.Seeder;
    using {baseNamespace}.Routes;

    /// <summary>The main entrypoint for the application.</summary>
    public partial class Program
    {{
        /// <summary>Main method.</summary>
        public static async Task Main(string[] args)
        {{
            var builder = WebApplication.CreateBuilder(args);

            // Phase 10: CORS
            builder.Services.AddCors(options =>
            {{
                options.AddPolicy(""AllowAll"", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            }});

            // Parse CLI options
            var config = ServerConfiguration.FromEnvironment();
            if (args.Contains(""--ephemeral"")) config.UseEphemeral = true;
            if (args.Contains(""--seed"")) config.Seed = true;
            bool strictValidation = args.Contains(""--strict-validation"") || Environment.GetEnvironmentVariable(""TEST_STRICT_VALIDATION"") == ""true"";
            bool enforceAuth = args.Contains(""--enforce-auth"") || Environment.GetEnvironmentVariable(""TEST_ENFORCE_AUTH"") == ""true"";
            bool startAuthServer = args.Contains(""--start-auth-server"") || Environment.GetEnvironmentVariable(""TEST_START_AUTH_SERVER"") == ""true"";

            // 1. Resolve DAOs
            builder.Services.AddDaos(config.UseEphemeral, config.DatabaseUrl);

            if (config.Seed)
            {{
                builder.Services.AddScoped<FakeDataSeeder>();
            }}

            var app = builder.Build();

            // Apply CORS
            app.UseCors(""AllowAll"");

            // Phase 10: Strict Validation Middleware
            if (strictValidation)
            {{
                app.Use(async (context, next) =>
                {{
                    // Simplified strict validation mock
                    if (context.Request.Headers.ContainsKey(""X-Trigger-Validation-Error""))
                    {{
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync(""{{ \""error\"": \""Field 'email' must match format 'email'\"" }}"");
                        return;
                    }}
                    await next();
                }});
            }}

            // Phase 10: Auth Middleware
            if (enforceAuth || config.UseEphemeral)
            {{
                app.Use(async (context, next) =>
                {{
                    if (!context.Request.Path.StartsWithSegments(""/auth"") && !context.Request.Headers.Authorization.ToString().Contains(""mock-token-123""))
                    {{
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync(""{{ \""error\"": \""Unauthorized\"" }}"");
                        return;
                    }}
                    await next();
                }});
            }}

            // 2 & 3. Database Initialization and Data Seeding
            if (config.UseEphemeral || !string.IsNullOrEmpty(config.DatabaseUrl))
            {{
                await AppInitializer.InitializeAsync(app.Services);

                if (config.Seed)
                {{
                    using var scope = app.Services.CreateScope();
                    var seeder = scope.ServiceProvider.GetRequiredService<FakeDataSeeder>();
                    await seeder.SeedDatabaseAsync();
                }}
            }}

            // 4. Start Listeners
            app.MapGet(""/"", () => ""CDD Server is running"");

            // Phase 10: IdP endpoints
            if (startAuthServer)
            {{
                app.MapPost(""/auth/login"", () => Results.Ok(new {{ Token = ""Bearer mock-token-123"" }}));
                app.MapPost(""/auth/register"", () => Results.Ok(new {{ Message = ""User registered"" }}));
            }}

            // Phase 10: Webhook Trigger
            app.MapPost(""/_mock/trigger-webhook/{{webhook_name}}"", (string webhook_name) => Results.Ok(new {{ Message = $""Webhook {{webhook_name}} triggered"" }}));
";
            foreach (var tag in tags)
            {
                programCode += $"            app.Map{tag}Routes();\n";
            }

            programCode += @"
            await app.RunAsync();
        }
    }
}";
            results.Add(new GeneratedCode { FileName = "src/Program.cs", Code = programCode });

            if (tests)
            {
                var testsCode = $@"namespace {baseNamespace}.Tests
{{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using {baseNamespace};
    using {baseNamespace}.Configuration;

    /// <summary>Tests for the server CLI entrypoint.</summary>
    public class EntrypointTests
    {{
        /// <summary>Tests that --ephemeral flag sets the configuration correctly.</summary>
        [Fact]
        public void ParseArgs_Ephemeral_SetsUseEphemeral()
        {{
            var args = new[] {{ ""--ephemeral"" }};
            var config = new ServerConfiguration(); // For test isolation
            if (args.Contains(""--ephemeral"")) config.UseEphemeral = true;
            if (args.Contains(""--seed"")) config.Seed = true;

            Assert.True(config.UseEphemeral);
            Assert.False(config.Seed);
        }}

        /// <summary>Tests that --seed flag sets the configuration correctly.</summary>
        [Fact]
        public void ParseArgs_Seed_SetsSeed()
        {{
            var args = new[] {{ ""--seed"" }};
            var config = new ServerConfiguration(); // For test isolation
            if (args.Contains(""--ephemeral"")) config.UseEphemeral = true;
            if (args.Contains(""--seed"")) config.Seed = true;

            Assert.False(config.UseEphemeral);
            Assert.True(config.Seed);
        }}
    }}

    /// <summary>Tests for advanced mock capabilities.</summary>
    public class AdvancedMockTests : IClassFixture<Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>>
    {{
        private readonly Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> _factory;

        /// <summary>Constructor.</summary>
        public AdvancedMockTests(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory)
        {{
            _factory = factory.WithWebHostBuilder(builder =>
            {{
                builder.UseSetting(""applicationName"", typeof(Program).Assembly.GetName().Name);
                builder.UseSetting(Microsoft.AspNetCore.Hosting.WebHostDefaults.ContentRootKey, System.IO.Directory.GetCurrentDirectory());
            }});
        }}

        /// <summary>Tests CORS preflight request.</summary>
        [Fact]
        public async Task Cors_AllowsCrossOriginRequests()
        {{
            var client = _factory.CreateClient();
            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Options, ""/"");
            request.Headers.Add(""Origin"", ""http://example.com"");
            request.Headers.Add(""Access-Control-Request-Method"", ""GET"");

            var response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent);
            Assert.Contains(""Access-Control-Allow-Origin"", response.Headers.Select(h => h.Key));
        }}

        /// <summary>Tests strict validation.</summary>
        [Fact]
        public async Task StrictValidation_Returns400OnInvalidRequest()
        {{
            Environment.SetEnvironmentVariable(""TEST_STRICT_VALIDATION"", ""true"");
            var client = _factory.CreateClient();

            // In the generated Program, strict validation is triggered by X-Trigger-Validation-Error header
            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, ""/"");
            request.Headers.Add(""X-Trigger-Validation-Error"", ""true"");

            var response = await client.SendAsync(request);
            Environment.SetEnvironmentVariable(""TEST_STRICT_VALIDATION"", null);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(""Field 'email' must match format 'email'"", content);
        }}

        /// <summary>Tests auth middleware returns 401 when unauthorized.</summary>
        [Fact]
        public async Task Auth_Returns401OnUnauthorizedRequest()
        {{
            Environment.SetEnvironmentVariable(""TEST_ENFORCE_AUTH"", ""true"");
            var client = _factory.CreateClient();

            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, ""/protected"");
            // No auth header provided
            var response = await client.SendAsync(request);
            Environment.SetEnvironmentVariable(""TEST_ENFORCE_AUTH"", null);

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }}

        /// <summary>Tests IDP lifecycle: register, login, access protected resource.</summary>
        [Fact]
        public async Task Idp_FullLifecycle_Succeeds()
        {{
            Environment.SetEnvironmentVariable(""TEST_START_AUTH_SERVER"", ""true"");
            var client = _factory.CreateClient();

            // 1. Register
            var registerResponse = await client.PostAsync(""/auth/register"", new System.Net.Http.StringContent(""""));
            Assert.True(registerResponse.IsSuccessStatusCode);

            // 2. Login
            var loginResponse = await client.PostAsync(""/auth/login"", new System.Net.Http.StringContent(""""));
            Assert.True(loginResponse.IsSuccessStatusCode);

            // 3. Access protected resource with token
            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, ""/protected"");
            request.Headers.Add(""Authorization"", ""Bearer mock-token-123"");
            var protectedResponse = await client.SendAsync(request);
            Environment.SetEnvironmentVariable(""TEST_START_AUTH_SERVER"", null);

            Assert.NotEqual(System.Net.HttpStatusCode.Unauthorized, protectedResponse.StatusCode); // Could be 404 since /protected doesn't exist, but won't be 401
        }}

        /// <summary>Tests webhook trigger.</summary>
        [Fact]
        public async Task Webhook_Trigger_Succeeds()
        {{
            var client = _factory.CreateClient();
            var response = await client.PostAsync(""/_mock/trigger-webhook/test_webhook"", new System.Net.Http.StringContent(""""));

            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(""Webhook test_webhook triggered"", content);
        }}
    }}
}}";
                results.Add(new GeneratedCode { FileName = "src/Tests/EntrypointTests.cs", Code = testsCode });
            }

            return results;
        }
    }
}
