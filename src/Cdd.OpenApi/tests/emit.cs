using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.TestsModule
{
    /// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
        /// <summary>Auto-generated documentation for ToTests.</summary>
        public static ClassDeclarationSyntax ToTests(string name, OpenApiPaths paths, bool tests = false)
        {
            var classDecl = SyntaxFactory.ClassDeclaration(name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (tests)
            {
                var field = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("IApi"))
                    .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator("_api"))))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
                
                var ctor = SyntaxFactory.ConstructorDeclaration(name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("api")).WithType(SyntaxFactory.ParseTypeName("IApi")))
                    .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("_api"),
                            SyntaxFactory.IdentifierName("api")
                        ))
                    ));
                
                classDecl = classDecl.AddMembers(field, ctor);
            }

            foreach (var pathKvp in paths)
            {
                var route = pathKvp.Key;
                var pathItem = pathKvp.Value;
                if (pathItem.Get != null) classDecl = classDecl.AddMembers(CreateTestMethod("Get", route, pathItem.Get, tests));
                if (pathItem.Post != null) classDecl = classDecl.AddMembers(CreateTestMethod("Post", route, pathItem.Post, tests));
                if (pathItem.Put != null) classDecl = classDecl.AddMembers(CreateTestMethod("Put", route, pathItem.Put, tests));
                if (pathItem.Delete != null) classDecl = classDecl.AddMembers(CreateTestMethod("Delete", route, pathItem.Delete, tests));
            }

            return classDecl;
        }

        private static MethodDeclarationSyntax CreateTestMethod(string method, string route, OpenApiOperation op, bool tests)
        {
            var methodName = op.OperationId ?? $"{method}{route.Replace("/", "").Replace("{", "").Replace("}", "")}";
            var returnType = SyntaxFactory.ParseTypeName(tests ? "System.Threading.Tasks.Task" : "void");
            var methodDecl = SyntaxFactory.MethodDeclaration(returnType, methodName + "Test")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            
            var body = SyntaxFactory.Block();
            
            if (tests)
            {
                body = SyntaxFactory.Block(
                    SyntaxFactory.ReturnStatement(SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("System.Threading.Tasks.Task"),
                        SyntaxFactory.IdentifierName("CompletedTask")
                    ))
                );
            }

            return methodDecl.WithBody(body);
        }
    }
}