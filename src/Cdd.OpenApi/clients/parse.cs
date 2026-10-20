// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Clients
{
    /// <summary>Parses C# HTTP client class declarations into <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> and <see cref="OpenApiPaths"/> definitions.</summary>
    public static class Parse
    {
        /// <summary>
        /// Extracts a list of <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> instances by analyzing HTTP client method invocations in a C# class.
        /// </summary>
        /// <param name="classNode">The C# client class syntax node to analyze.</param>
        /// <returns>The extracted list of <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> instances.</returns>
        public static List<Cdd.OpenApi.Ir.IrEndpoint> ToEndpoints(ClassDeclarationSyntax classNode)
        {
            return OperationHelpers.ExtractEndpointsFromPaths(ToPaths(classNode));
        }

        /// <summary>
        /// Extracts an <see cref="OpenApiPaths"/> object by analyzing HTTP client method invocations in a C# class.
        /// </summary>
        /// <param name="classNode">The C# client class syntax node to analyze.</param>
        /// <returns>The extracted <see cref="OpenApiPaths"/>.</returns>
        public static OpenApiPaths ToPaths(ClassDeclarationSyntax classNode)
        {
            var paths = new OpenApiPaths();

            foreach (var method in classNode.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.Body == null) continue;

                var invocations = method.Body.GetDescendantNodesSafe().OfType<InvocationExpressionSyntax>();
                InvocationExpressionSyntax? httpCall = null;
                string? httpMethod = null;

                foreach (var inv in invocations)
                {
                    if (inv.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        var methodName = memberAccess.Name.Identifier.Text;
                        if (methodName.EndsWith("Async") &&
                            methodName != "ReadAsStringAsync" &&
                            methodName != "SendAsync" &&
                            methodName != "GetStreamAsync" &&
                            methodName != "GetByteArrayAsync" &&
                            methodName != "GetStringAsync")
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

                if (!routePath.StartsWith("/"))
                {
                    routePath = "/" + routePath;
                }

                var defaultResponse = new OpenApiResponse { Description = "Success" };
                var returnTypeStr = method.ReturnType.ToString();
                if (returnTypeStr.StartsWith("Task<") && returnTypeStr.EndsWith(">"))
                {
                    returnTypeStr = returnTypeStr.Substring(5, returnTypeStr.Length - 6);
                }
                else if (returnTypeStr.StartsWith("System.Threading.Tasks.Task<") && returnTypeStr.EndsWith(">"))
                {
                    returnTypeStr = returnTypeStr.Substring(28, returnTypeStr.Length - 29).Trim();
                }

                if (returnTypeStr != "string" && returnTypeStr != "Task")
                {
                    defaultResponse.Content = new Dictionary<string, OpenApiMediaType>
                    {
                        { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = TypeMapper.MapCSharpToOpenApi(returnTypeStr, "string") } } }
                    };
                }

                var responses = OperationMetadataExtractor.ExtractResponses(method, defaultResponse);

                var operationId = method.Identifier.Text;
                if (operationId.EndsWith("Async")) operationId = operationId.Substring(0, operationId.Length - 5);

                var operation = new OpenApiOperation
                {
                    OperationId = operationId,
                    Summary = Docstrings.Parse.GetSummary(method),
                    Responses = responses
                };

                OperationMetadataExtractor.PopulateOperationMetadata(method, operation);

                var parameters = new List<OpenApiParameter>();
                foreach (var param in method.ParameterList.Parameters)
                {
                    var paramName = param.Identifier.Text;
                    var typeStr = param.Type?.ToString();
                    bool isFromBody = param.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains("FromBody"));
                    bool isImplicitBody = (httpMethod == "post" || httpMethod == "put" || httpMethod == "patch") &&
                                          !routePath.Contains($"{{{paramName}}}");

                    if (isFromBody || isImplicitBody)
                    {
                        operation.RequestBody = new OpenApiRequestBody
                        {
                            Description = "Request body",
                            Required = true,
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = TypeMapper.MapCSharpToOpenApi(typeStr, "string") } } }
                            }
                        };
                    }
                    else
                    {
                        var paramObj = new OpenApiParameter
                        {
                            Name = paramName,
                            In = routePath.Contains($"{{{paramName}}}") ? "path" : "query",
                            Required = true,
                            Schema = new OpenApiSchema { Type = TypeMapper.MapCSharpToOpenApi(typeStr, "string") }
                        };

                        OperationMetadataExtractor.PopulateParameterMetadata(param, method, paramObj);
                        parameters.Add(paramObj);
                    }
                }

                if (parameters.Any()) operation.Parameters = parameters;

                if (!paths.ContainsKey(routePath))
                {
                    paths[routePath] = new OpenApiPathItem();
                }

                OperationHelpers.SetOperation(paths[routePath], httpMethod, operation);
            }

            return paths;
        }
    }
}
