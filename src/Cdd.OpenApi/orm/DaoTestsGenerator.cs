// <copyright file="DaoTestsGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates unit tests for DAOs.</summary>
    public static class DaoTestsGenerator
    {
        /// <summary>Generates DAO tests from IR models.</summary>
        /// <param name="models">The IR models to generate DAO tests for.</param>
        /// <param name="baseNamespace">The base C# namespace for the generated test classes.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> GenerateDaoTests(IReadOnlyList<Cdd.OpenApi.Ir.IrModel> models, string baseNamespace)
        {
            var dict = new Dictionary<string, OpenApiSchema>();
            foreach (var m in models)
            {
                dict[m.Name] = m.ToOpenApiSchema();
            }

            return GenerateDaoTests(dict, baseNamespace);
        }

        /// <summary>Generates DAO tests.</summary>
        /// <param name="schemas">The OpenAPI schemas to generate DAO tests for.</param>
        /// <param name="baseNamespace">The base C# namespace for the generated test classes.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> GenerateDaoTests(IDictionary<string, OpenApiSchema> schemas, string baseNamespace)
        {
            var results = new List<GeneratedCode>();

            var defaultTestStmts = new List<StatementSyntax>
            {
                SyntaxFactory.ParseStatement("var services = new ServiceCollection();"),
                SyntaxFactory.ParseStatement("services.AddDaos(false, null);"),
                SyntaxFactory.ParseStatement("var provider = services.BuildServiceProvider();")
            };
            var ephemeralTestStmts = new List<StatementSyntax>
            {
                SyntaxFactory.ParseStatement("var services = new ServiceCollection();"),
                SyntaxFactory.ParseStatement("services.AddDaos(true, null);"),
                SyntaxFactory.ParseStatement("var provider = services.BuildServiceProvider();")
            };
            var postgresTestStmts = new List<StatementSyntax>
            {
                SyntaxFactory.ParseStatement("var services = new ServiceCollection();"),
                SyntaxFactory.ParseStatement(@"services.AddDaos(false, ""Host=localhost;Database=test"");"),
                SyntaxFactory.ParseStatement("var provider = services.BuildServiceProvider();")
            };

            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                var varName = $"{schemaName.ToLowerInvariant()}Dao";
                defaultTestStmts.Add(SyntaxFactory.ParseStatement($"var {varName} = provider.GetRequiredService<I{schemaName}Dao>();"));
                defaultTestStmts.Add(SyntaxFactory.ParseStatement($"Assert.IsType<Stub{schemaName}Dao>({varName});"));

                ephemeralTestStmts.Add(SyntaxFactory.ParseStatement($"var {varName} = provider.GetRequiredService<I{schemaName}Dao>();"));
                ephemeralTestStmts.Add(SyntaxFactory.ParseStatement($"Assert.IsType<Concrete{schemaName}Dao>({varName});"));

                postgresTestStmts.Add(SyntaxFactory.ParseStatement($"var {varName} = provider.GetRequiredService<I{schemaName}Dao>();"));
                postgresTestStmts.Add(SyntaxFactory.ParseStatement($"Assert.IsType<Concrete{schemaName}Dao>({varName});"));
            }

            var factAttr = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Fact"))));

            var diTestClass = SyntaxFactory.ClassDeclaration("DaoServiceCollectionExtensionsTests")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "AddDaos_Default_RegistersStubs")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAttributeLists(factAttr)
                        .WithBody(SyntaxFactory.Block(defaultTestStmts)),
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "AddDaos_Ephemeral_RegistersConcrete")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAttributeLists(factAttr)
                        .WithBody(SyntaxFactory.Block(ephemeralTestStmts)),
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "AddDaos_Postgres_RegistersConcrete")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAttributeLists(factAttr)
                        .WithBody(SyntaxFactory.Block(postgresTestStmts))
                );

            var diTestNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Xunit")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Daos")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Models")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore"))
                )
                .AddMembers(diTestClass);

            results.Add(new GeneratedCode
            {
                FileName = "src/Tests/DaoServiceCollectionExtensionsTests.cs",
                Code = WasmSafeRoslyn.FormatSafe(diTestNs)
            });

            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                var schema = schemaKvp.Value;

                bool hasId = false;
                if (schema.Properties != null)
                {
                    foreach (var prop in schema.Properties)
                    {
                        var propNameLower = prop.Key.ToLowerInvariant();
                        if (propNameLower == "id") hasId = true;
                    }
                }

                if (!hasId) continue;

                var crudMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "Crud_Works")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                    .AddAttributeLists(factAttr)
                    .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement("var services = new ServiceCollection();"),
                        SyntaxFactory.ParseStatement("services.AddDaos(true, null);"),
                        SyntaxFactory.ParseStatement("var provider = services.BuildServiceProvider();"),
                        SyntaxFactory.ParseStatement("var dbContext = provider.GetRequiredService<AppDbContext>();"),
                        SyntaxFactory.ParseStatement("await dbContext.Database.OpenConnectionAsync();"),
                        SyntaxFactory.ParseStatement("await dbContext.Database.EnsureCreatedAsync();"),
                        SyntaxFactory.ParseStatement($"var dao = provider.GetRequiredService<I{schemaName}Dao>();"),
                        SyntaxFactory.ParseStatement($"var faker = new Bogus.Faker<{schemaName}>().RuleForType(typeof(string), f => f.Lorem.Word()).RuleForType(typeof(int), f => f.Random.Int(1, 100)).RuleForType(typeof(long), f => f.Random.Long(1, 100)).RuleForType(typeof(bool), f => f.Random.Bool());"),
                        SyntaxFactory.ParseStatement("var newEntity = faker.Generate();"),
                        SyntaxFactory.ParseStatement("if (newEntity != null) { await dao.CreateAsync(newEntity); }"),
                        SyntaxFactory.ParseStatement("var all = await dao.GetAllAsync();"),
                        SyntaxFactory.ParseStatement("Assert.NotEmpty(all);"),
                        SyntaxFactory.ParseStatement("await dbContext.Database.EnsureDeletedAsync();")
                    ));

                var schemaTestClass = SyntaxFactory.ClassDeclaration($"{schemaName}DaoTests")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddMembers(crudMethod);

                var schemaTestNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                    .AddUsings(
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Xunit")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Daos")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Models")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore"))
                    )
                    .AddMembers(schemaTestClass);

                results.Add(new GeneratedCode
                {
                    FileName = $"src/Tests/{schemaName}DaoTests.cs",
                    Code = WasmSafeRoslyn.FormatSafe(schemaTestNs)
                });
            }

            return results;
        }
    }
}
