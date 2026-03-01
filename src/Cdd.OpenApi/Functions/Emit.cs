using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Functions
{
    /// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
        /// <summary>Auto-generated documentation for EmitFunction.</summary>
        public static MethodDeclarationSyntax EmitFunction(OpenApiOperation operation)
        {
            var returnType = SyntaxFactory.ParseTypeName("void");
            var methodName = operation.OperationId ?? "DefaultFunction";
            
            var methodDecl = SyntaxFactory.MethodDeclaration(returnType, methodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            if (!string.IsNullOrWhiteSpace(operation.Summary))
            {
                methodDecl = Docstrings.Emit.WithSummary(methodDecl, operation.Summary);
            }

            var body = SyntaxFactory.Block();
            return methodDecl.WithBody(body);
        }
    }
}