using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.TestsModule
{
    /// <summary>Auto-generated documentation for Parse.</summary>
    public static class Parse
    {
        /// <summary>Auto-generated documentation for ToPaths.</summary>
        public static IDictionary<string, OpenApiPathItem> ToPaths(ClassDeclarationSyntax classNode)
        {
            var paths = new Dictionary<string, OpenApiPathItem>();
            foreach (var method in classNode.Members.OfType<MethodDeclarationSyntax>())
            {
                var name = method.Identifier.Text;
                if (name.EndsWith("Test")) name = name.Substring(0, name.Length - 4);
                var route = "/" + name.ToLowerInvariant();
                var pathItem = new OpenApiPathItem { Get = new OpenApiOperation { OperationId = name } };
                paths[route] = pathItem;
            }
            return paths;
        }
    }
}