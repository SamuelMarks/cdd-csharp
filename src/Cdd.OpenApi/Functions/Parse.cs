using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Functions
{
    /// <summary>Auto-generated documentation for Parse.</summary>
    public static class Parse
    {
        /// <summary>Auto-generated documentation for ParseFunction.</summary>
        public static OpenApiOperation ParseFunction(MethodDeclarationSyntax method)
        {
            var operation = new OpenApiOperation
            {
                OperationId = method.Identifier.Text,
                Summary = Docstrings.Parse.GetSummary(method)
            };
            
            return operation;
        }
    }
}