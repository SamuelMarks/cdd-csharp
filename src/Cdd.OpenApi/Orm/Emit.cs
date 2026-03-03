using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
        /// <summary>Auto-generated documentation for ToDbContext.</summary>
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

            foreach (var schemaKvp in schemas)
            {
                var schemaName = schemaKvp.Key;
                var propNode = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName($"DbSet<{schemaName}>"), schemaName + "s")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    );

                propNode = propNode.WithInitializer(
                    SyntaxFactory.EqualsValueClause(SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName($"Set<{schemaName}>")))
                ).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                classNode = classNode.AddMembers(propNode);
            }

            return classNode.NormalizeWhitespace();
        }
    }
}