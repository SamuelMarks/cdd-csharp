using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Mocks
{
    /// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
        /// <summary>Auto-generated documentation for ToMock.</summary>
        public static ClassDeclarationSyntax ToMock(string name, OpenApiPaths paths, bool tests = false)
        {
            var classDecl = SyntaxFactory.ClassDeclaration(name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (tests)
            {
                classDecl = classDecl.AddBaseListTypes(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IApi"))
                );

                var interfaceNode = Routes.Emit.ToInterface("IApi", paths);
                foreach (var member in interfaceNode.Members.OfType<MethodDeclarationSyntax>())
                {
                    var methodDecl = member.WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>());

                    var newParams = methodDecl.ParameterList.Parameters.Select(p => p.WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>()));
                    methodDecl = methodDecl.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(newParams)));

                    BlockSyntax body;
                    if (methodDecl.ReturnType.ToString() == "void")
                    {
                        body = SyntaxFactory.Block();
                    }
                    else
                    {
                        body = SyntaxFactory.Block(
                            SyntaxFactory.ReturnStatement(SyntaxFactory.DefaultExpression(methodDecl.ReturnType))
                        );
                    }

                    methodDecl = methodDecl.WithBody(body)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                    classDecl = classDecl.AddMembers(methodDecl);
                }
            }
            else
            {
                foreach (var pathKvp in paths)
                {
                    var route = pathKvp.Key;
                    var pathItem = pathKvp.Value;
                    if (pathItem.Get != null) classDecl = classDecl.AddMembers(CreateMockMethod("Get", route, pathItem.Get));
                    if (pathItem.Post != null) classDecl = classDecl.AddMembers(CreateMockMethod("Post", route, pathItem.Post));
                    if (pathItem.Put != null) classDecl = classDecl.AddMembers(CreateMockMethod("Put", route, pathItem.Put));
                    if (pathItem.Delete != null) classDecl = classDecl.AddMembers(CreateMockMethod("Delete", route, pathItem.Delete));
                }
            }

            return classDecl;
        }

        private static MethodDeclarationSyntax CreateMockMethod(string method, string route, OpenApiOperation op)
        {
            var methodName = op.OperationId ?? $"{method}{route.Replace("/", "").Replace("{", "").Replace("}", "")}";
            var returnType = SyntaxFactory.ParseTypeName("object");
            var methodDecl = SyntaxFactory.MethodDeclaration(returnType, methodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var body = SyntaxFactory.Block(
                SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))
            );
            return methodDecl.WithBody(body);
        }
    }
}
