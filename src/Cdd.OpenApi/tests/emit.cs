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
        public static ClassDeclarationSyntax ToTests(string name, OpenApiPaths paths)
        {
            var classDecl = SyntaxFactory.ClassDeclaration(name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            foreach (var pathKvp in paths)
            {
                var route = pathKvp.Key;
                var pathItem = pathKvp.Value;
                if (pathItem.Get != null) classDecl = classDecl.AddMembers(CreateTestMethod("Get", route, pathItem.Get));
                if (pathItem.Post != null) classDecl = classDecl.AddMembers(CreateTestMethod("Post", route, pathItem.Post));
                if (pathItem.Put != null) classDecl = classDecl.AddMembers(CreateTestMethod("Put", route, pathItem.Put));
                if (pathItem.Delete != null) classDecl = classDecl.AddMembers(CreateTestMethod("Delete", route, pathItem.Delete));
            }

            return classDecl;
        }

        private static MethodDeclarationSyntax CreateTestMethod(string method, string route, OpenApiOperation op)
        {
            var methodName = op.OperationId ?? $"{method}{route.Replace("/", "").Replace("{", "").Replace("}", "")}";
            var returnType = SyntaxFactory.ParseTypeName("void");
            var methodDecl = SyntaxFactory.MethodDeclaration(returnType, methodName + "Test")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            
            var body = SyntaxFactory.Block();
            return methodDecl.WithBody(body);
        }
    }
}