// <copyright file="ConfigGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates server configuration and database initialization routines.</summary>
    public static class ConfigGenerator
    {
        /// <summary>Generates the ServerConfiguration and AppInitializer classes.</summary>
        /// <param name="baseNamespace">The base C# namespace for the generated classes.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> GenerateConfig(string baseNamespace)
        {
            var results = new List<GeneratedCode>();

            var serverConfigClass = SyntaxFactory.ClassDeclaration("ServerConfiguration")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    SyntaxFactory.ParseMemberDeclaration(@"/// <summary>Gets or sets a value indicating whether to use an ephemeral, in-memory database.</summary>
public bool UseEphemeral { get; set; }")!,
                    SyntaxFactory.ParseMemberDeclaration(@"/// <summary>Gets or sets the connection string for the primary database. Overridden if UseEphemeral is true.</summary>
public string? DatabaseUrl { get; set; }")!,
                    SyntaxFactory.ParseMemberDeclaration(@"/// <summary>Gets or sets a value indicating whether to run the fake data seeder on startup.</summary>
public bool Seed { get; set; }")!,
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("ServerConfiguration"), "FromEnvironment")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement(@"var config = new ServerConfiguration();"),
                            SyntaxFactory.ParseStatement(@"var dbUrl = Environment.GetEnvironmentVariable(""DATABASE_URL"");"),
                            SyntaxFactory.ParseStatement(@"if (!string.IsNullOrEmpty(dbUrl)) { config.DatabaseUrl = dbUrl; }"),
                            SyntaxFactory.ParseStatement(@"var ephemeral = Environment.GetEnvironmentVariable(""EPHEMERAL_DB"");"),
                            SyntaxFactory.ParseStatement(@"if (ephemeral == ""true"" || ephemeral == ""1"") { config.UseEphemeral = true; }"),
                            SyntaxFactory.ParseStatement(@"return config;")
                        )),
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "ApplyArgs")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("args"))
                                .WithType(SyntaxFactory.ParseTypeName("string[]")))
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement(@"if (args != null && System.Linq.Enumerable.Contains(args, ""--ephemeral"")) { UseEphemeral = true; }"),
                            SyntaxFactory.ParseStatement(@"if (args != null && System.Linq.Enumerable.Contains(args, ""--seed"")) { Seed = true; }")
                        ))
                );

            var configNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Configuration"))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddMembers(serverConfigClass);

            results.Add(new GeneratedCode
            {
                FileName = "src/Configuration/ServerConfiguration.cs",
                Code = WasmSafeRoslyn.FormatSafe(configNs)
            });

            var appInitializerClass = SyntaxFactory.ClassDeclaration("AppInitializer")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddMembers(
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "InitializeAsync")
                        .AddModifiers(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                            SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("serviceProvider"))
                                .WithType(SyntaxFactory.ParseTypeName("IServiceProvider")))
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement(@"using var scope = serviceProvider.CreateScope();"),
                            SyntaxFactory.ParseStatement(@"var context = scope.ServiceProvider.GetService<AppDbContext>();"),
                            SyntaxFactory.ParseStatement(@"if (context != null) { if (context.Database.IsSqlite()) { await context.Database.OpenConnectionAsync(); await context.Database.EnsureCreatedAsync(); } else { await context.Database.EnsureCreatedAsync(); } }")
                        ))
                );

            var initializerNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Configuration"))
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Models")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore")))
                .AddMembers(appInitializerClass);

            results.Add(new GeneratedCode
            {
                FileName = "src/Configuration/AppInitializer.cs",
                Code = WasmSafeRoslyn.FormatSafe(initializerNs)
            });

            return results;
        }
    }
}
