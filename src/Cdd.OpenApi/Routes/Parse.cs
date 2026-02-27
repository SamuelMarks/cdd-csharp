using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Routes
{
    public static class Parse
    {
        public static OpenApiPaths ToPaths(ClassDeclarationSyntax classNode)
        {
            var paths = new OpenApiPaths();

            foreach (var method in classNode.Members.OfType<MethodDeclarationSyntax>())
            {
                var routeAttr = method.AttributeLists
                    .SelectMany(a => a.Attributes)
                    .FirstOrDefault(a => a.Name.ToString().StartsWith("Http"));
                    
                if (routeAttr == null) continue;

                var attrName = routeAttr.Name.ToString(); // HttpGet, HttpPost, etc.
                var httpMethod = attrName.Replace("Http", "").ToLowerInvariant();

                // Extract path from [HttpGet("path")]
                var routePath = "/";
                if (routeAttr.ArgumentList != null && routeAttr.ArgumentList.Arguments.Any())
                {
                    var arg = routeAttr.ArgumentList.Arguments.First().Expression as LiteralExpressionSyntax;
                    if (arg != null)
                    {
                        routePath = arg.Token.ValueText;
                        if (!routePath.StartsWith("/")) routePath = "/" + routePath;
                    }
                }

                var operation = new OpenApiOperation
                {
                    OperationId = method.Identifier.Text,
                    Summary = Docstrings.Parse.GetSummary(method),
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse { Description = "Success" }
                    }
                };

                // Add parameters
                var parameters = new List<OpenApiParameter>();
                foreach (var param in method.ParameterList.Parameters)
                {
                    parameters.Add(new OpenApiParameter
                    {
                        Name = param.Identifier.Text,
                        In = routePath.Contains($"{{{param.Identifier.Text}}}") ? "path" : "query",
                        Required = true,
                        Schema = new OpenApiSchema { Type = MapType(param.Type?.ToString()) }
                    });
                }
                if (parameters.Any()) operation.Parameters = parameters;

                if (!paths.ContainsKey(routePath))
                {
                    paths[routePath] = new OpenApiPathItem();
                }

                var pathItem = paths[routePath];
                SetOperation(pathItem, httpMethod, operation);
            }

            return paths;
        }

        private static void SetOperation(OpenApiPathItem pathItem, string method, OpenApiOperation op)
        {
            switch (method)
            {
                case "get": pathItem.Get = op; break;
                case "put": pathItem.Put = op; break;
                case "post": pathItem.Post = op; break;
                case "delete": pathItem.Delete = op; break;
                case "options": pathItem.Options = op; break;
                case "head": pathItem.Head = op; break;
                case "patch": pathItem.Patch = op; break;
                case "trace": pathItem.Trace = op; break;
            }
        }

        private static string MapType(string? csharpType)
        {
            return csharpType switch
            {
                "int" or "long" or "short" => "integer",
                "float" or "double" or "decimal" => "number",
                "bool" => "boolean",
                "string" => "string",
                _ => "string" // Fallback for path/query parameters
            };
        }
    }
}
