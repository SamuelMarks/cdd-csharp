using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Clients
{
/// <summary>Auto-generated documentation for Parse.</summary>
    public static class Parse
    {
/// <summary>Auto-generated documentation for ToPaths.</summary>
        public static OpenApiPaths ToPaths(ClassDeclarationSyntax classNode)
        {
            var paths = new OpenApiPaths();

            foreach (var method in classNode.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.Body == null) continue;

                var invocations = method.Body.DescendantNodes().OfType<InvocationExpressionSyntax>();
                InvocationExpressionSyntax? httpCall = null;
                string? httpMethod = null;

                foreach (var inv in invocations)
                {
                    if (inv.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        var methodName = memberAccess.Name.Identifier.Text;
                        if (methodName.EndsWith("Async") && (
                            methodName == "GetAsync" || methodName == "PostAsync" || 
                            methodName == "PutAsync" || methodName == "DeleteAsync" ||
                            methodName == "PatchAsync" || methodName == "OptionsAsync" ||
                            methodName == "HeadAsync" || methodName == "TraceAsync"))
                        {
                            httpCall = inv;
                            httpMethod = methodName.Replace("Async", "").ToLowerInvariant();
                            break;
                        }
                    }
                }

                if (httpCall == null || httpMethod == null) continue;
                if (!httpCall.ArgumentList.Arguments.Any()) continue;

                var routeArg = httpCall.ArgumentList.Arguments.First().Expression;
                string routePath = "/";

                if (routeArg is LiteralExpressionSyntax literalStr)
                {
                    routePath = literalStr.Token.ValueText;
                }
                else if (routeArg is InterpolatedStringExpressionSyntax interpolatedStr)
                {
                    var parts = new List<string>();
                    foreach (var content in interpolatedStr.Contents)
                    {
                        if (content is InterpolatedStringTextSyntax text)
                        {
                            parts.Add(text.TextToken.ValueText);
                        }
                        else if (content is InterpolationSyntax interpolation)
                        {
                            parts.Add($"{{{interpolation.Expression}}}");
                        }
                    }
                    routePath = string.Join("", parts);
                }

                if (!routePath.StartsWith("/")) routePath = "/" + routePath;

                var operationId = method.Identifier.Text;
                if (operationId.EndsWith("Async")) operationId = operationId.Substring(0, operationId.Length - 5);

                var operation = new OpenApiOperation
                {
                    OperationId = operationId,
                    Summary = Docstrings.Parse.GetSummary(method),
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse { Description = "Success" }
                    }
                };

                var parameters = new List<OpenApiParameter>();
                foreach (var param in method.ParameterList.Parameters)
                {
                    var paramName = param.Identifier.Text;
                    parameters.Add(new OpenApiParameter
                    {
                        Name = paramName,
                        In = routePath.Contains($"{{{paramName}}}") ? "path" : "query",
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
                _ => "string" // Fallback
            };
        }
    }
}
