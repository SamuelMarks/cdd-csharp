// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Routes
{
    /// <summary>Parses ASP.NET Core route attributes and controller methods into <see cref="OpenApiPaths"/> definitions.</summary>
    public static class Parse
    {
        /// <summary>
        /// Extracts an <see cref="OpenApiPaths"/> object from a C# controller class declaration.
        /// </summary>
        /// <param name="classNode">The C# controller class syntax node to analyze.</param>
        /// <returns>The extracted <see cref="OpenApiPaths"/>.</returns>
        public static OpenApiPaths ToPaths(ClassDeclarationSyntax classNode)
        {
            var paths = new OpenApiPaths();

            var pathSummary = Docstrings.Parse.GetTag(classNode, "path-summary");
            var pathDescription = Docstrings.Parse.GetTag(classNode, "path-description");
            var pathRef = Docstrings.Parse.GetTag(classNode, "path-ref");

            foreach (var method in classNode.Members.OfType<MethodDeclarationSyntax>())
            {
                var routeAttr = method.AttributeLists
                    .SelectMany(a => a.Attributes)
                    .FirstOrDefault(a => a.Name.ToString().StartsWith("Http"));

                if (routeAttr == null) continue;

                var attrName = routeAttr.Name.ToString();
                var httpMethod = attrName.Replace("Http", "").ToLowerInvariant();

                var routePath = "/";
                if (routeAttr.ArgumentList != null && routeAttr.ArgumentList.Arguments.Any())
                {
                    if (routeAttr.ArgumentList.Arguments.First().Expression is LiteralExpressionSyntax arg)
                    {
                        routePath = "/" + arg.Token.ValueText.TrimStart('/');
                    }
                }

                var defaultResponse = new OpenApiResponse { Description = "Success" };
                var returnTypeStr = method.ReturnType.ToString();
                if (returnTypeStr.StartsWith("Task<") && returnTypeStr.EndsWith(">"))
                {
                    returnTypeStr = returnTypeStr.Substring(5, returnTypeStr.Length - 6);
                }
                else if (returnTypeStr.StartsWith("System.Threading.Tasks.Task<") && returnTypeStr.EndsWith(">"))
                {
                    returnTypeStr = returnTypeStr.Substring(28, returnTypeStr.Length - 29);
                }

                if (returnTypeStr != "void" && returnTypeStr != "Task" &&
                    returnTypeStr != "IActionResult" && returnTypeStr != "Microsoft.AspNetCore.Mvc.IActionResult")
                {
                    defaultResponse.Content = new Dictionary<string, OpenApiMediaType>
                    {
                        { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = TypeMapper.MapCSharpToOpenApi(returnTypeStr, "string") } } }
                    };
                }

                var responses = OperationMetadataExtractor.ExtractResponses(method, defaultResponse);

                var operation = new OpenApiOperation
                {
                    OperationId = method.Identifier.Text,
                    Summary = Docstrings.Parse.GetSummary(method),
                    Responses = responses
                };

                OperationMetadataExtractor.PopulateOperationMetadata(method, operation);

                var parameters = new List<OpenApiParameter>();
                foreach (var param in method.ParameterList.Parameters)
                {
                    var inType = routePath.Contains($"{{{param.Identifier.Text}}}") ? "path" : "query";
                    var attrs = param.AttributeLists.SelectMany(al => al.Attributes).Select(a => a.Name.ToString()).ToList();

                    var inTypeDict = new Dictionary<string, string>
                    {
                        { "FromRoute", "path" },
                        { "FromQuery", "query" },
                        { "FromBody", "body" },
                        { "FromHeader", "header" }
                    };

                    foreach (var attr in attrs)
                    {
                        if (inTypeDict.TryGetValue(attr, out var mappedType))
                        {
                            inType = mappedType;
                        }
                    }

                    string? paramTypeStr = param.Type!.ToString();

                    if (inType == "body")
                    {
                        var reqBody = new OpenApiRequestBody
                        {
                            Description = "Request body for " + param.Identifier.Text,
                            Required = true,
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                {
                                    "application/json", new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = TypeMapper.MapCSharpToOpenApi(paramTypeStr, "string") }
                                    }
                                }
                            }
                        };

                        var encodingAttrs = param.AttributeLists.SelectMany(a => a.Attributes).Where(a => a.Name.ToString().Contains("Encoding"));
                        foreach (var encodingAttr in encodingAttrs)
                        {
                            if (encodingAttr.ArgumentList != null && encodingAttr.ArgumentList.Arguments.Count >= 2)
                            {
                                string? propName = null;
                                string? contentType = null;
                                if (encodingAttr.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax propLit) propName = propLit.Token.ValueText;
                                if (encodingAttr.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax cntLit) contentType = cntLit.Token.ValueText;

                                if (propName != null)
                                {
                                    if (contentType != null)
                                    {
                                        var encObj = new OpenApiEncoding { ContentType = contentType };

                                        var styleArg = encodingAttr.ArgumentList.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "Style");
                                        if (styleArg != null)
                                        {
                                            if (styleArg.Expression is LiteralExpressionSyntax styleLiteral)
                                            {
                                                encObj.Style = styleLiteral.Token.ValueText;
                                            }
                                        }

                                        var explodeArg = encodingAttr.ArgumentList.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "Explode");
                                        if (explodeArg != null)
                                        {
                                            encObj.Explode = explodeArg.Expression.Kind() == SyntaxKind.TrueLiteralExpression;
                                        }

                                        var allowReservedArg = encodingAttr.ArgumentList.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "AllowReserved");
                                        if (allowReservedArg != null)
                                        {
                                            encObj.AllowReserved = allowReservedArg.Expression.Kind() == SyntaxKind.TrueLiteralExpression;
                                        }

                                        if (!reqBody.Content.TryGetValue("application/x-www-form-urlencoded", out var urlEncoded))
                                        {
                                            urlEncoded = new OpenApiMediaType
                                            {
                                                Schema = new OpenApiSchema { Type = OpenApiDataType.Object },
                                                Encoding = new Dictionary<string, OpenApiEncoding>()
                                            };
                                            reqBody.Content["application/x-www-form-urlencoded"] = urlEncoded;
                                        }

                                        urlEncoded.Encoding![propName] = encObj;
                                    }
                                }
                            }
                        }

                        operation.RequestBody = reqBody;
                    }
                    else
                    {
                        var paramObj = new OpenApiParameter
                        {
                            Name = param.Identifier.Text,
                            In = inType,
                            Required = true,
                            Schema = new OpenApiSchema { Type = TypeMapper.MapCSharpToOpenApi(paramTypeStr, "string") }
                        };

                        OperationMetadataExtractor.PopulateParameterMetadata(param, method, paramObj);
                        parameters.Add(paramObj);
                    }
                }

                if (parameters.Any()) operation.Parameters = parameters;

                if (!paths.ContainsKey(routePath))
                {
                    paths[routePath] = new OpenApiPathItem
                    {
                        Summary = pathSummary,
                        Description = pathDescription,
                        Ref = pathRef
                    };
                }

                OperationHelpers.SetOperation(paths[routePath], httpMethod, operation);
            }

            return paths;
        }
    }
}
