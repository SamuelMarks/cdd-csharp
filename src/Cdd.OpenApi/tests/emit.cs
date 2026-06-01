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
            classDecl = Cdd.OpenApi.Docstrings.Emit.WithSummary(classDecl, $"Auto-generated documentation for {name}.");

            var members = new List<MemberDeclarationSyntax>();
            if (tests)
            {
                var field = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("IApi"))
                    .AddVariables(SyntaxFactory.VariableDeclarator("_api"))
                ).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

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
                ctor = Cdd.OpenApi.Docstrings.Emit.WithSummary(ctor, $"Auto-generated documentation for {name}.");

                members.Add(field);
                members.Add(ctor);
            }

            foreach (var pathKvp in paths)
            {
                var route = pathKvp.Key;
                var pathItem = pathKvp.Value;
                if (pathItem.Get != null) members.Add(CreateTestMethod("Get", route, pathItem.Get, tests));
                if (pathItem.Post != null) members.Add(CreateTestMethod("Post", route, pathItem.Post, tests));
                if (pathItem.Put != null) members.Add(CreateTestMethod("Put", route, pathItem.Put, tests));
                if (pathItem.Delete != null) members.Add(CreateTestMethod("Delete", route, pathItem.Delete, tests));
            }

            return classDecl.AddMembers(members.ToArray());
        }

        /// <summary>Auto-generated documentation for ToClientTests.</summary>
        public static ClassDeclarationSyntax ToClientTests(string name, OpenApiPaths paths, bool tests = false)
        {
            var classDecl = SyntaxFactory.ClassDeclaration(name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            classDecl = Cdd.OpenApi.Docstrings.Emit.WithSummary(classDecl, $"Auto-generated documentation for {name}.");

            var members = new List<MemberDeclarationSyntax>();
            if (tests)
            {
                var field = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("ApiClient"))
                    .AddVariables(SyntaxFactory.VariableDeclarator("_client"))
                ).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

                var ctor = SyntaxFactory.ConstructorDeclaration(name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("client")).WithType(SyntaxFactory.ParseTypeName("ApiClient")))
                    .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("_client"),
                            SyntaxFactory.IdentifierName("client")
                        ))
                    ));
                ctor = Cdd.OpenApi.Docstrings.Emit.WithSummary(ctor, $"Auto-generated documentation for {name}.");

                members.Add(field);
                members.Add(ctor);
            }

            foreach (var pathKvp in paths)
            {
                var route = pathKvp.Key;
                var pathItem = pathKvp.Value;
                if (pathItem.Get != null) members.Add(CreateClientTestMethod("Get", route, pathItem.Get, tests));
                if (pathItem.Post != null) members.Add(CreateClientTestMethod("Post", route, pathItem.Post, tests));
                if (pathItem.Put != null) members.Add(CreateClientTestMethod("Put", route, pathItem.Put, tests));
                if (pathItem.Delete != null) members.Add(CreateClientTestMethod("Delete", route, pathItem.Delete, tests));
            }

            return classDecl.AddMembers(members.ToArray());
        }

        private static MethodDeclarationSyntax CreateTestMethod(string method, string route, OpenApiOperation op, bool tests)
        {
            var methodName = op.OperationId ?? $"{method}{route.Replace("/", "").Replace("{", "").Replace("}", "")}";
            var returnType = SyntaxFactory.ParseTypeName(tests ? "System.Threading.Tasks.Task" : "void");
            var methodDecl = SyntaxFactory.MethodDeclaration(returnType, methodName + "Test")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            methodDecl = Cdd.OpenApi.Docstrings.Emit.WithSummary(methodDecl, $"Auto-generated documentation for {methodName}Test.");

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

        private static MethodDeclarationSyntax CreateClientTestMethod(string method, string route, OpenApiOperation op, bool tests)
        {
            var methodName = op.OperationId ?? $"{method}{route.Replace("/", "").Replace("{", "").Replace("}", "")}";
            if (!methodName.EndsWith("Async")) methodName += "Async";
            var returnType = SyntaxFactory.ParseTypeName(tests ? "System.Threading.Tasks.Task" : "void");
            var methodDecl = SyntaxFactory.MethodDeclaration(returnType, methodName + "Test")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            methodDecl = Cdd.OpenApi.Docstrings.Emit.WithSummary(methodDecl, $"Auto-generated documentation for {methodName}Test.");

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
