// <copyright file="DaoGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates Data Access Objects (DAOs) for the OpenAPI and IR models.</summary>
    public static class DaoGenerator
    {
        /// <summary>Generates DAOs and their dependency injection extensions from IR models.</summary>
        /// <param name="models">The IR models to generate DAOs for.</param>
        /// <param name="baseNamespace">The base C# namespace for the generated classes.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> GenerateDaos(IReadOnlyList<Cdd.OpenApi.Ir.IrModel> models, string baseNamespace)
        {
            var dict = new Dictionary<string, OpenApiSchema>();
            foreach (var m in models)
            {
                dict[m.Name] = m.ToOpenApiSchema();
            }

            return GenerateDaos(dict, baseNamespace);
        }

        /// <summary>Generates DAOs and their dependency injection extensions.</summary>
        /// <param name="schemas">The OpenAPI schemas to generate DAOs for.</param>
        /// <param name="baseNamespace">The base C# namespace for the generated classes.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> GenerateDaos(IDictionary<string, OpenApiSchema> schemas, string baseNamespace)
        {
            var results = new List<GeneratedCode>();

            var ephemeralStmts = new List<StatementSyntax>
            {
                SyntaxFactory.ParseStatement(@"var dbName = Guid.NewGuid().ToString();"),
                SyntaxFactory.ParseStatement(@"services.AddDbContext<AppDbContext>(options => options.UseSqlite($""DataSource={dbName};Mode=Memory;Cache=Shared""));")
            };
            var postgresStmts = new List<StatementSyntax>
            {
                SyntaxFactory.ParseStatement(@"services.AddDbContext<AppDbContext>(options => options.UseNpgsql(databaseUrl));")
            };
            var stubStmts = new List<StatementSyntax>();

            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                ephemeralStmts.Add(SyntaxFactory.ParseStatement($"services.AddScoped<I{schemaName}Dao, Concrete{schemaName}Dao>();"));
                postgresStmts.Add(SyntaxFactory.ParseStatement($"services.AddScoped<I{schemaName}Dao, Concrete{schemaName}Dao>();"));
                stubStmts.Add(SyntaxFactory.ParseStatement($"services.AddScoped<I{schemaName}Dao, Stub{schemaName}Dao>();"));
            }

            var ifElseStmt = SyntaxFactory.IfStatement(
                SyntaxFactory.IdentifierName("useEphemeral"),
                SyntaxFactory.Block(ephemeralStmts),
                SyntaxFactory.ElseClause(
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.ParseExpression("!string.IsNullOrEmpty(databaseUrl)"),
                        SyntaxFactory.Block(postgresStmts),
                        SyntaxFactory.ElseClause(SyntaxFactory.Block(stubStmts))
                    )
                )
            );

            var addDaosMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("IServiceCollection"), "AddDaos")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("services"))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.ThisKeyword))
                        .WithType(SyntaxFactory.ParseTypeName("IServiceCollection")),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("useEphemeral"))
                        .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword))),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("databaseUrl"))
                        .WithType(SyntaxFactory.ParseTypeName("string?"))
                )
                .WithBody(SyntaxFactory.Block(
                    ifElseStmt,
                    SyntaxFactory.ParseStatement("return services;")
                ));

            var diClass = SyntaxFactory.ClassDeclaration("DaoServiceCollectionExtensions")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddMembers(addDaosMethod);

            var diNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Daos"))
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Models")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore"))
                )
                .AddMembers(diClass);

            results.Add(new GeneratedCode
            {
                FileName = "src/Daos/DaoServiceCollectionExtensions.cs",
                Code = WasmSafeRoslyn.FormatSafe(diNs)
            });

            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                var schema = schemaKvp.Value;

                string idType = "string";
                if (schema.Properties != null && schema.Properties.TryGetValue("id", out var idProp))
                {
                    if (idProp.Type == "integer") idType = "int";
                }
                else if (schema.Properties != null && schema.Properties.TryGetValue("Id", out var upperIdProp))
                {
                    if (upperIdProp.Type == "integer") idType = "int";
                }

                var daoInterface = SyntaxFactory.InterfaceDeclaration($"I{schemaName}Dao")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddMembers(
                        SyntaxFactory.ParseMemberDeclaration($"Task<IEnumerable<{schemaName}>> GetAllAsync();")!,
                        SyntaxFactory.ParseMemberDeclaration($"Task<{schemaName}?> GetByIdAsync({idType} id);")!,
                        SyntaxFactory.ParseMemberDeclaration($"Task<{schemaName}> CreateAsync({schemaName} entity);")!,
                        SyntaxFactory.ParseMemberDeclaration($"Task<{schemaName}> UpdateAsync({schemaName} entity);")!,
                        SyntaxFactory.ParseMemberDeclaration($"Task DeleteAsync({idType} id);")!
                    );

                var stubClass = SyntaxFactory.ClassDeclaration($"Stub{schemaName}Dao")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                        SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{schemaName}Dao")))))
                    .AddMembers(
                        SyntaxFactory.ParseMemberDeclaration($"public Task<IEnumerable<{schemaName}>> GetAllAsync() => throw new NotImplementedException();")!,
                        SyntaxFactory.ParseMemberDeclaration($"public Task<{schemaName}?> GetByIdAsync({idType} id) => throw new NotImplementedException();")!,
                        SyntaxFactory.ParseMemberDeclaration($"public Task<{schemaName}> CreateAsync({schemaName} entity) => throw new NotImplementedException();")!,
                        SyntaxFactory.ParseMemberDeclaration($"public Task<{schemaName}> UpdateAsync({schemaName} entity) => throw new NotImplementedException();")!,
                        SyntaxFactory.ParseMemberDeclaration($"public Task DeleteAsync({idType} id) => throw new NotImplementedException();")!
                    );

                var concreteClass = SyntaxFactory.ClassDeclaration($"Concrete{schemaName}Dao")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                        SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{schemaName}Dao")))))
                    .AddMembers(
                        SyntaxFactory.ParseMemberDeclaration("private readonly AppDbContext _context;")!,
                        SyntaxFactory.ConstructorDeclaration($"Concrete{schemaName}Dao")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("context"))
                                    .WithType(SyntaxFactory.ParseTypeName("AppDbContext")))
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement("_context = context;")
                            )),
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<IEnumerable<{schemaName}>>"), "GetAllAsync")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement($"return await _context.{schemaName}s.ToListAsync();")
                            )),
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<{schemaName}?>"), "GetByIdAsync")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("id")).WithType(SyntaxFactory.ParseTypeName(idType)))
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement($"return await _context.{schemaName}s.FindAsync(id);")
                            )),
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<{schemaName}>"), "CreateAsync")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity")).WithType(SyntaxFactory.ParseTypeName(schemaName)))
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement($"_context.{schemaName}s.Add(entity);"),
                                SyntaxFactory.ParseStatement("await _context.SaveChangesAsync();"),
                                SyntaxFactory.ParseStatement("return entity;")
                            )),
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName($"Task<{schemaName}>"), "UpdateAsync")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity")).WithType(SyntaxFactory.ParseTypeName(schemaName)))
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement($"_context.{schemaName}s.Update(entity);"),
                                SyntaxFactory.ParseStatement("await _context.SaveChangesAsync();"),
                                SyntaxFactory.ParseStatement("return entity;")
                            )),
                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "DeleteAsync")
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                            .AddParameterListParameters(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier("id")).WithType(SyntaxFactory.ParseTypeName(idType)))
                            .WithBody(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement($"var entity = await _context.{schemaName}s.FindAsync(id);"),
                                SyntaxFactory.ParseStatement($"if (entity != null) {{ _context.{schemaName}s.Remove(entity); await _context.SaveChangesAsync(); }}")
                            ))
                    );

                var daoNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Daos"))
                    .AddUsings(
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                    )
                    .AddMembers(daoInterface, stubClass, concreteClass);

                results.Add(new GeneratedCode
                {
                    FileName = $"src/Daos/{schemaName}Dao.cs",
                    Code = WasmSafeRoslyn.FormatSafe(daoNs)
                });
            }

            return results;
        }
    }
}
