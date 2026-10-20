// <copyright file="SeederGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates fake data seeders using Bogus, managing referential integrity.</summary>
    public static class SeederGenerator
    {
        /// <summary>Generates the FakeDataSeeder class from IR models.</summary>
        /// <param name="models">The IR models to seed.</param>
        /// <param name="baseNamespace">The base C# namespace for the generated classes.</param>
        /// <param name="tests">Whether to generate unit tests for the seeders.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> GenerateSeeder(IReadOnlyList<Cdd.OpenApi.Ir.IrModel> models, string baseNamespace, bool tests)
        {
            var dict = new Dictionary<string, OpenApiSchema>();
            foreach (var m in models)
            {
                dict[m.Name] = m.ToOpenApiSchema();
            }

            return GenerateSeeder(dict, baseNamespace, tests);
        }

        /// <summary>Generates the FakeDataSeeder class.</summary>
        /// <param name="schemas">The OpenAPI schemas to seed.</param>
        /// <param name="baseNamespace">The base C# namespace for the generated classes.</param>
        /// <param name="tests">Whether to generate unit tests for the seeders.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> GenerateSeeder(IDictionary<string, OpenApiSchema> schemas, string baseNamespace, bool tests)
        {
            var results = new List<GeneratedCode>();

            var seedStmts = new List<StatementSyntax>
            {
                SyntaxFactory.ParseStatement("using var scope = _serviceProvider.CreateScope();")
            };

            var orderedSchemas = OrderSchemasByDependencies(schemas);
            foreach (var schemaKvp in orderedSchemas)
            {
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
                if (hasId)
                {
                    seedStmts.Add(SyntaxFactory.ParseStatement($"await Seed{schemaKvp.Key}Async(scope.ServiceProvider);"));
                }
            }

            var mainSeederClass = SyntaxFactory.ClassDeclaration("FakeDataSeeder")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(
                    SyntaxFactory.ParseMemberDeclaration("private readonly IServiceProvider _serviceProvider;")!,
                    SyntaxFactory.ConstructorDeclaration("FakeDataSeeder")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("serviceProvider"))
                                .WithType(SyntaxFactory.ParseTypeName("IServiceProvider")))
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement("_serviceProvider = serviceProvider;")
                        )),
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "SeedDatabaseAsync")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                        .WithBody(SyntaxFactory.Block(seedStmts))
                );

            var mainSeederNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Seeder"))
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection"))
                )
                .AddMembers(mainSeederClass);

            results.Add(new GeneratedCode
            {
                FileName = "src/Seeder/FakeDataSeeder.cs",
                Code = WasmSafeRoslyn.FormatSafe(mainSeederNs)
            });

            var factAttr = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Fact"))));

            foreach (var schemaKvp in orderedSchemas)
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

                var fakerExpr = new StringBuilder();
                fakerExpr.Append($"var {schemaName.ToLowerInvariant()}Faker = new Faker<{schemaName}>()");
                fakerExpr.Append(".RuleForType(typeof(string), f => f.Lorem.Word())");
                fakerExpr.Append(".RuleForType(typeof(int), f => f.Random.Int(1, 100))");
                fakerExpr.Append(".RuleForType(typeof(long), f => f.Random.Long(1, 100))");
                fakerExpr.Append(".RuleForType(typeof(bool), f => f.Random.Bool())");

                if (schema.Properties != null)
                {
                    foreach (var prop in schema.Properties)
                    {
                        var propName = prop.Key;
                        var propNameLower = propName.ToLowerInvariant();
                        if (prop.Value.Type == "string")
                        {
                            if (propNameLower.Contains("email"))
                            {
                                fakerExpr.Append($".RuleFor(x => x.{propName}, f => f.Internet.Email())");
                            }
                            else if (propNameLower.Contains("name"))
                            {
                                fakerExpr.Append($".RuleFor(x => x.{propName}, f => f.Name.FullName())");
                            }
                            else if (propNameLower == "id")
                            {
                                fakerExpr.Append($".RuleFor(x => x.{propName}, f => Guid.NewGuid().ToString())");
                            }
                            else
                            {
                                fakerExpr.Append($".RuleFor(x => x.{propName}, f => f.Lorem.Word())");
                            }
                        }
                        else if (prop.Value.Type == "integer")
                        {
                            if (propNameLower != "id")
                            {
                                fakerExpr.Append($".RuleFor(x => x.{propName}, f => f.Random.Int(1, 100))");
                            }
                        }
                        else if (prop.Value.Type == "boolean")
                        {
                            fakerExpr.Append($".RuleFor(x => x.{propName}, f => f.Random.Bool())");
                        }
                    }
                }

                fakerExpr.Append(";");

                var seedEntityMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), $"Seed{schemaName}Async")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("provider"))
                            .WithType(SyntaxFactory.ParseTypeName("IServiceProvider")))
                    .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement($"var {schemaName.ToLowerInvariant()}Dao = provider.GetRequiredService<I{schemaName}Dao>();"),
                        SyntaxFactory.ParseStatement(fakerExpr.ToString()),
                        SyntaxFactory.ParseStatement($"var {schemaName.ToLowerInvariant()}List = {schemaName.ToLowerInvariant()}Faker.Generate(10);"),
                        SyntaxFactory.ParseStatement($"foreach (var entity in {schemaName.ToLowerInvariant()}List) {{ await {schemaName.ToLowerInvariant()}Dao.CreateAsync(entity); }}")
                    ));

                var seederClass = SyntaxFactory.ClassDeclaration("FakeDataSeeder")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                    .AddMembers(seedEntityMethod);

                var seederNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Seeder"))
                    .AddUsings(
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Bogus")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Models")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Daos"))
                    )
                    .AddMembers(seederClass);

                results.Add(new GeneratedCode
                {
                    FileName = $"src/Seeder/Fake{schemaName}Seeder.cs",
                    Code = WasmSafeRoslyn.FormatSafe(seederNs)
                });

                if (tests)
                {
                    var testMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), $"Seed{schemaName}Async_PopulatesData")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                        .AddAttributeLists(factAttr)
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement("var services = new ServiceCollection();"),
                            SyntaxFactory.ParseStatement("services.AddDaos(true, null);"),
                            SyntaxFactory.ParseStatement("services.AddScoped<FakeDataSeeder>();"),
                            SyntaxFactory.ParseStatement("var provider = services.BuildServiceProvider();"),
                            SyntaxFactory.ParseStatement("var dbContext = provider.GetRequiredService<AppDbContext>();"),
                            SyntaxFactory.ParseStatement("await dbContext.Database.OpenConnectionAsync();"),
                            SyntaxFactory.ParseStatement("await dbContext.Database.EnsureCreatedAsync();"),
                            SyntaxFactory.ParseStatement("var seeder = provider.GetRequiredService<FakeDataSeeder>();"),
                            SyntaxFactory.ParseStatement("await seeder.SeedDatabaseAsync();"),
                            SyntaxFactory.ParseStatement("using var scope = provider.CreateScope();"),
                            SyntaxFactory.ParseStatement($"var {schemaName.ToLowerInvariant()}Dao = scope.ServiceProvider.GetRequiredService<I{schemaName}Dao>();"),
                            SyntaxFactory.ParseStatement($"var {schemaName.ToLowerInvariant()}s = await {schemaName.ToLowerInvariant()}Dao.GetAllAsync();"),
                            SyntaxFactory.ParseStatement($"Assert.NotEmpty({schemaName.ToLowerInvariant()}s);")
                        ));

                    var testClass = SyntaxFactory.ClassDeclaration($"Fake{schemaName}SeederTests")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddMembers(testMethod);

                    var testNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                        .AddUsings(
                            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Xunit")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Seeder")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Daos")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Models")),
                            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore"))
                        )
                        .AddMembers(testClass);

                    results.Add(new GeneratedCode
                    {
                        FileName = $"src/Tests/Fake{schemaName}SeederTests.cs",
                        Code = WasmSafeRoslyn.FormatSafe(testNs)
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// Topologically sorts schemas so referenced parent entities are seeded before dependent child entities.
        /// </summary>
        /// <param name="schemas">The input dictionary of schemas.</param>
        /// <returns>An ordered list of schema key-value pairs respecting referential dependencies.</returns>
        private static List<KeyValuePair<string, OpenApiSchema>> OrderSchemasByDependencies(IDictionary<string, OpenApiSchema> schemas)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var ordered = new List<KeyValuePair<string, OpenApiSchema>>();

            void Visit(string name)
            {
                if (!visited.Add(name)) return;
                if (!schemas.TryGetValue(name, out var schema)) return;

                if (schema.Properties != null)
                {
                    foreach (var prop in schema.Properties)
                    {
                        if (!string.IsNullOrEmpty(prop.Value.Ref))
                        {
                            var refTarget = prop.Value.Ref.Split('/').Last();
                            if (schemas.ContainsKey(refTarget))
                            {
                                Visit(refTarget);
                            }
                        }
                        else if (prop.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && prop.Key.Length > 2)
                        {
                            var parentCandidate = prop.Key.Substring(0, prop.Key.Length - 2);
                            var match = schemas.Keys.FirstOrDefault(k => string.Equals(k, parentCandidate, StringComparison.OrdinalIgnoreCase));
                            if (match != null)
                            {
                                Visit(match);
                            }
                        }
                    }
                }

                ordered.Add(new KeyValuePair<string, OpenApiSchema>(name, schema));
            }

            foreach (var key in schemas.Keys)
            {
                Visit(key);
            }

            return ordered;
        }
    }
}
