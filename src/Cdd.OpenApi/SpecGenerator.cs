using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi
{
/// <summary>Auto-generated documentation for SpecGenerator.</summary>
    public static class SpecGenerator
    {
/// <summary>Auto-generated documentation for Generate.</summary>
        public static OpenApiDocument Generate(IEnumerable<string> csharpSourceCodes)
        {
            var doc = new OpenApiDocument
            {
                Info = new OpenApiInfo { Title = "Generated API", Version = "1.0.0" },
                Paths = new OpenApiPaths(),
                Components = new OpenApiComponents { Schemas = new Dictionary<string, OpenApiSchema>() }
            };

            foreach (var code in csharpSourceCodes)
            {
                var tree = CSharpSyntaxTree.ParseText(code);
                var classNodes = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var classNode in classNodes)
                {
                    var hasRoutes = classNode.DescendantNodes().OfType<MethodDeclarationSyntax>()
                        .Any(m => m.AttributeLists.SelectMany(al => al.Attributes).Any(a => a.Name.ToString().StartsWith("Http")));
                        
                    var hasClientMethods = classNode.DescendantNodes().OfType<InvocationExpressionSyntax>()
                        .Any(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess &&
                                    memberAccess.Name.Identifier.Text.EndsWith("Async") &&
                                    (memberAccess.Name.Identifier.Text.StartsWith("Get") ||
                                     memberAccess.Name.Identifier.Text.StartsWith("Post") ||
                                     memberAccess.Name.Identifier.Text.StartsWith("Put") ||
                                     memberAccess.Name.Identifier.Text.StartsWith("Delete")));

                    if (hasRoutes)
                    {
                        var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }
                    }
                    else if (hasClientMethods)
                    {
                        var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }
                    }
                    else
                    {
                        var schema = Cdd.OpenApi.Classes.Parse.ToSchema(classNode);
                        doc.Components!.Schemas![classNode.Identifier.Text] = schema;
                    }
                }
            }

            if (!doc.Paths!.Any()) doc.Paths = null;
            if (!doc.Components!.Schemas!.Any()) doc.Components = null;

            return doc;
        }
    }
}