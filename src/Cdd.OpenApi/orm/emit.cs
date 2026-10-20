// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Emits Entity Framework Core <c>DbContext</c> classes from <see cref="Cdd.OpenApi.Ir.IrModel"/> and <see cref="OpenApiSchema"/> definitions.</summary>
    public static class Emit
    {
        /// <summary>
        /// Generates a Roslyn <see cref="ClassDeclarationSyntax"/> representing an Entity Framework Core <c>DbContext</c> from IR models.
        /// </summary>
        /// <param name="className">The name of the generated DbContext class.</param>
        /// <param name="models">The IR models to expose as <c>DbSet&lt;T&gt;</c> properties.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
        public static ClassDeclarationSyntax ToDbContext(string className, IReadOnlyList<Cdd.OpenApi.Ir.IrModel> models)
        {
            var dict = new Dictionary<string, OpenApiSchema>();
            foreach (var m in models)
            {
                dict[m.Name] = m.ToOpenApiSchema();
            }

            return ToDbContext(className, dict);
        }

        /// <summary>
        /// Generates a Roslyn <see cref="ClassDeclarationSyntax"/> representing an Entity Framework Core <c>DbContext</c>.
        /// </summary>
        /// <param name="className">The name of the generated DbContext class.</param>
        /// <param name="schemas">The OpenAPI schemas to expose as <c>DbSet&lt;T&gt;</c> properties.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
        public static ClassDeclarationSyntax ToDbContext(string className, IDictionary<string, OpenApiSchema> schemas)
        {
            var classNode = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("DbContext"))
                )));

            var constructorNode = SyntaxFactory.ConstructorDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("options"))
                        .WithType(SyntaxFactory.ParseTypeName($"DbContextOptions<{className}>"))
                )
                .WithInitializer(
                    SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                        .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("options")))
                )
                .WithBody(SyntaxFactory.Block());

            classNode = classNode.AddMembers(constructorNode);

            var onModelCreatingMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "OnModelCreating")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("modelBuilder")).WithType(SyntaxFactory.ParseTypeName("ModelBuilder")))
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.BaseExpression(), SyntaxFactory.IdentifierName("OnModelCreating"))
                    ).AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("modelBuilder"))))
                ));

            var statements = new List<StatementSyntax>();
            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                var schema = schemaKvp.Value;

                // Only create tables for object schemas
                if (schema.Type != "object" && schema.Properties == null)
                {
                    continue;
                }

                bool hasId = false;

                if (schema.Properties != null)
                {
                    foreach (var prop in schema.Properties)
                    {
                        var propNameLower = prop.Key.ToLowerInvariant();
                        if (propNameLower == "id") hasId = true;

                        var isObject = prop.Value.Type == "object" || prop.Value.Type == "array" || (prop.Value.Type == null && prop.Value.Ref == null);
                        if (isObject)
                        {
                            statements.Add(SyntaxFactory.ParseStatement($"modelBuilder.Entity<{schemaName}>().Ignore(e => e.{prop.Key});"));
                        }
                    }
                }

                if (!hasId)
                {
                    statements.Add(SyntaxFactory.ParseStatement($"modelBuilder.Entity<{schemaName}>().HasNoKey();"));
                }
            }

            if (statements.Any())
            {
                onModelCreatingMethod = onModelCreatingMethod.WithBody(SyntaxFactory.Block(statements.Prepend(
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.BaseExpression(), SyntaxFactory.IdentifierName("OnModelCreating"))
                    ).AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("modelBuilder"))))
                )));
                classNode = classNode.AddMembers(onModelCreatingMethod);
            }

            var members = new List<MemberDeclarationSyntax>();
            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                var schema = schemaKvp.Value;

                // Only create tables for object schemas
                if (schema.Type != "object" && schema.Properties == null)
                {
                    continue;
                }

                var propNode = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName($"DbSet<{schemaName}>"), schemaName + "s")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName($"Set<{schemaName}>"))))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                members.Add(propNode);
            }

            return classNode.AddMembers(members.ToArray());
        }
    }
}
