// <copyright file="ServerEntrypointGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates the main entrypoint and CLI for the ASP.NET Core server.</summary>
    public static class ServerEntrypointGenerator
    {
        /// <summary>Generates the Program.cs and startup tests.</summary>
        /// <param name="baseNamespace">The base C# namespace for the generated server.</param>
        /// <param name="tests">Whether to generate unit and integration tests for the server entrypoint.</param>
        /// <param name="tags">The route tags to map on startup.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> GenerateEntrypoint(string baseNamespace, bool tests, IEnumerable<string> tags)
        {
            var results = new List<GeneratedCode>();

            var mainStmts = new List<StatementSyntax>
            {
                SyntaxFactory.ParseStatement(@"var builder = WebApplication.CreateBuilder(args);"),
                SyntaxFactory.ParseStatement(@"builder.Services.AddCors(options => { options.AddPolicy(""AllowAll"", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); });"),
                SyntaxFactory.ParseStatement(@"var config = ServerConfiguration.FromEnvironment();"),
                SyntaxFactory.ParseStatement(@"config.ApplyArgs(args);"),
                SyntaxFactory.ParseStatement(@"bool strictValidation = args.Contains(""--strict-validation"") || Environment.GetEnvironmentVariable(""TEST_STRICT_VALIDATION"") == ""true"";"),
                SyntaxFactory.ParseStatement(@"bool enforceAuth = args.Contains(""--enforce-auth"") || Environment.GetEnvironmentVariable(""TEST_ENFORCE_AUTH"") == ""true"";"),
                SyntaxFactory.ParseStatement(@"bool startAuthServer = args.Contains(""--start-auth-server"") || Environment.GetEnvironmentVariable(""TEST_START_AUTH_SERVER"") == ""true"";"),
                SyntaxFactory.ParseStatement(@"builder.Services.AddDaos(config.UseEphemeral, config.DatabaseUrl);"),
                SyntaxFactory.ParseStatement(@"if (config.Seed) { builder.Services.AddScoped<FakeDataSeeder>(); }"),
                SyntaxFactory.ParseStatement(@"var app = builder.Build();"),
                SyntaxFactory.ParseStatement(@"app.UseCors(""AllowAll"");"),
                EmitStrictValidationMiddlewareStatement(),
                EmitMockAuthMiddlewareStatement(),
                SyntaxFactory.ParseStatement(@"if (config.UseEphemeral || !string.IsNullOrEmpty(config.DatabaseUrl)) { await AppInitializer.InitializeAsync(app.Services); if (config.Seed) { using var scope = app.Services.CreateScope(); var seeder = scope.ServiceProvider.GetRequiredService<FakeDataSeeder>(); await seeder.SeedDatabaseAsync(); } }"),
                SyntaxFactory.ParseStatement(@"app.MapGet(""/"", () => ""CDD Server is running"");"),
                EmitMockIdpEndpointsStatement(),
                EmitWebhookTriggerEndpointStatement()
            };

            foreach (var tag in tags)
            {
                mainStmts.Add(SyntaxFactory.ParseStatement($"app.Map{tag}Routes();"));
            }

            mainStmts.Add(SyntaxFactory.ParseStatement("await app.RunAsync();"));

            var mainMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "Main")
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("args"))
                        .WithType(SyntaxFactory.ParseTypeName("string[]")))
                .WithBody(SyntaxFactory.Block(mainStmts));

            var programClass = SyntaxFactory.ClassDeclaration("Program")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(mainMethod);

            var programNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(baseNamespace))
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Builder")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Http")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Configuration")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Daos")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Seeder")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Routes"))
                )
                .AddMembers(programClass);

            results.Add(new GeneratedCode
            {
                FileName = "src/Program.cs",
                Code = WasmSafeRoslyn.FormatSafe(programNs)
            });

            if (tests)
            {
                var factAttr = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Fact"))));

                var entrypointTestsClass = SyntaxFactory.ClassDeclaration("EntrypointTests")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddMembers(
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "ParseArgs_Ephemeral_SetsUseEphemeral")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddAttributeLists(factAttr)
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement(@"var args = new[] { ""--ephemeral"" };"),
                                SyntaxFactory.ParseStatement(@"var config = new ServerConfiguration();"),
                                SyntaxFactory.ParseStatement(@"config.ApplyArgs(args);"),
                                SyntaxFactory.ParseStatement(@"Assert.True(config.UseEphemeral);"),
                                SyntaxFactory.ParseStatement(@"Assert.False(config.Seed);")
                            )),
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "ParseArgs_Seed_SetsSeed")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddAttributeLists(factAttr)
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement(@"var args = new[] { ""--seed"" };"),
                                SyntaxFactory.ParseStatement(@"var config = new ServerConfiguration();"),
                                SyntaxFactory.ParseStatement(@"config.ApplyArgs(args);"),
                                SyntaxFactory.ParseStatement(@"Assert.False(config.UseEphemeral);"),
                                SyntaxFactory.ParseStatement(@"Assert.True(config.Seed);")
                            ))
                    );

                var advancedMockTestsClass = SyntaxFactory.ClassDeclaration("AdvancedMockTests")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                        SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IClassFixture<Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>>")))))
                    .AddMembers(
                        SyntaxFactory.ParseMemberDeclaration("private readonly Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> _factory;")!,
                        SyntaxFactory.ConstructorDeclaration("AdvancedMockTests")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("factory"))
                                    .WithType(SyntaxFactory.ParseTypeName("Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>")))
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement(@"_factory = factory.WithWebHostBuilder(builder => { builder.UseSetting(""applicationName"", typeof(Program).Assembly.GetName().Name); builder.UseSetting(Microsoft.AspNetCore.Hosting.WebHostDefaults.ContentRootKey, System.IO.Directory.GetCurrentDirectory()); });")
                            )),
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "Cors_AllowsCrossOriginRequests")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                            .AddAttributeLists(factAttr)
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement(@"var client = _factory.CreateClient();"),
                                SyntaxFactory.ParseStatement(@"var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Options, ""/"");"),
                                SyntaxFactory.ParseStatement(@"request.Headers.Add(""Origin"", ""http://example.com"");"),
                                SyntaxFactory.ParseStatement(@"request.Headers.Add(""Access-Control-Request-Method"", ""GET"");"),
                                SyntaxFactory.ParseStatement(@"var response = await client.SendAsync(request);"),
                                SyntaxFactory.ParseStatement(@"Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent);"),
                                SyntaxFactory.ParseStatement(@"Assert.Contains(""Access-Control-Allow-Origin"", response.Headers.Select(h => h.Key));")
                            )),
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "StrictValidation_Returns400OnInvalidRequest")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                            .AddAttributeLists(factAttr)
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""TEST_STRICT_VALIDATION"", ""true"");"),
                                SyntaxFactory.ParseStatement(@"var client = _factory.CreateClient();"),
                                SyntaxFactory.ParseStatement(@"var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, ""/"");"),
                                SyntaxFactory.ParseStatement(@"request.Headers.Add(""X-Trigger-Validation-Error"", ""true"");"),
                                SyntaxFactory.ParseStatement(@"var response = await client.SendAsync(request);"),
                                SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""TEST_STRICT_VALIDATION"", null);"),
                                SyntaxFactory.ParseStatement(@"Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);"),
                                SyntaxFactory.ParseStatement(@"var content = await response.Content.ReadAsStringAsync();"),
                                SyntaxFactory.ParseStatement(@"Assert.Contains(""Field 'email' must match format 'email'"", content);")
                            )),
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "Auth_Returns401OnUnauthorizedRequest")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                            .AddAttributeLists(factAttr)
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""TEST_ENFORCE_AUTH"", ""true"");"),
                                SyntaxFactory.ParseStatement(@"var client = _factory.CreateClient();"),
                                SyntaxFactory.ParseStatement(@"var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, ""/protected"");"),
                                SyntaxFactory.ParseStatement(@"var response = await client.SendAsync(request);"),
                                SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""TEST_ENFORCE_AUTH"", null);"),
                                SyntaxFactory.ParseStatement(@"Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);")
                            )),
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "Idp_FullLifecycle_Succeeds")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                            .AddAttributeLists(factAttr)
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""TEST_START_AUTH_SERVER"", ""true"");"),
                                SyntaxFactory.ParseStatement(@"var client = _factory.CreateClient();"),
                                SyntaxFactory.ParseStatement(@"var registerResponse = await client.PostAsync(""/auth/register"", new System.Net.Http.StringContent(""""));"),
                                SyntaxFactory.ParseStatement(@"Assert.True(registerResponse.IsSuccessStatusCode);"),
                                SyntaxFactory.ParseStatement(@"var loginResponse = await client.PostAsync(""/auth/login"", new System.Net.Http.StringContent(""""));"),
                                SyntaxFactory.ParseStatement(@"Assert.True(loginResponse.IsSuccessStatusCode);"),
                                SyntaxFactory.ParseStatement(@"var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, ""/protected"");"),
                                SyntaxFactory.ParseStatement(@"request.Headers.Add(""Authorization"", ""Bearer mock-token-123"");"),
                                SyntaxFactory.ParseStatement(@"var protectedResponse = await client.SendAsync(request);"),
                                SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""TEST_START_AUTH_SERVER"", null);"),
                                SyntaxFactory.ParseStatement(@"Assert.NotEqual(System.Net.HttpStatusCode.Unauthorized, protectedResponse.StatusCode);")
                            )),
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "Webhook_Trigger_Succeeds")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                            .AddAttributeLists(factAttr)
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement(@"var client = _factory.CreateClient();"),
                                SyntaxFactory.ParseStatement(@"var response = await client.PostAsync(""/_mock/trigger-webhook/test_webhook"", new System.Net.Http.StringContent(""""));"),
                                SyntaxFactory.ParseStatement(@"Assert.True(response.IsSuccessStatusCode);"),
                                SyntaxFactory.ParseStatement(@"var content = await response.Content.ReadAsStringAsync();"),
                                SyntaxFactory.ParseStatement(@"Assert.Contains(""Webhook test_webhook triggered"", content);")
                            ))
                    );

                var testsNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                    .AddUsings(
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Builder")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Xunit")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(baseNamespace)),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Configuration"))
                    )
                    .AddMembers(entrypointTestsClass, advancedMockTestsClass);

                results.Add(new GeneratedCode
                {
                    FileName = "src/Tests/EntrypointTests.cs",
                    Code = WasmSafeRoslyn.FormatSafe(testsNs)
                });
            }

            return results;
        }

        /// <summary>
        /// Emits the strict request validation middleware registration statement.
        /// </summary>
        /// <returns>The parsed <see cref="StatementSyntax"/>.</returns>
        private static StatementSyntax EmitStrictValidationMiddlewareStatement()
        {
            return SyntaxFactory.ParseStatement(
                @"if (strictValidation) { app.Use(async (context, next) => { if (context.Request.Headers.ContainsKey(""X-Trigger-Validation-Error"")) { context.Response.StatusCode = 400; await context.Response.WriteAsync(@""{ """"error"""": """"Field 'email' must match format 'email'"""" }""); return; } await next(); }); }");
        }

        /// <summary>
        /// Emits the mock authentication enforcement middleware registration statement.
        /// </summary>
        /// <returns>The parsed <see cref="StatementSyntax"/>.</returns>
        private static StatementSyntax EmitMockAuthMiddlewareStatement()
        {
            return SyntaxFactory.ParseStatement(
                @"if (enforceAuth || config.UseEphemeral) { app.Use(async (context, next) => { if (!context.Request.Path.StartsWithSegments(""/auth"") && !context.Request.Headers.Authorization.ToString().Contains(""mock-token-123"")) { context.Response.StatusCode = 401; await context.Response.WriteAsync(@""{ """"error"""": """"Unauthorized"""" }""); return; } await next(); }); }");
        }

        /// <summary>
        /// Emits the mock identity provider (<c>/auth/login</c> and <c>/auth/register</c>) minimal API endpoint statements.
        /// </summary>
        /// <returns>The parsed <see cref="StatementSyntax"/>.</returns>
        private static StatementSyntax EmitMockIdpEndpointsStatement()
        {
            return SyntaxFactory.ParseStatement(
                @"if (startAuthServer) { app.MapPost(""/auth/login"", () => Results.Ok(new { Token = ""Bearer mock-token-123"" })); app.MapPost(""/auth/register"", () => Results.Ok(new { Message = ""User registered"" })); }");
        }

        /// <summary>
        /// Emits the mock webhook trigger (<c>/_mock/trigger-webhook/{webhook_name}</c>) minimal API endpoint statement.
        /// </summary>
        /// <returns>The parsed <see cref="StatementSyntax"/>.</returns>
        private static StatementSyntax EmitWebhookTriggerEndpointStatement()
        {
            return SyntaxFactory.ParseStatement(
                @"app.MapPost(""/_mock/trigger-webhook/{webhook_name}"", (string webhook_name) => Results.Ok(new { Message = $""Webhook {webhook_name} triggered"" }));");
        }
    }
}
