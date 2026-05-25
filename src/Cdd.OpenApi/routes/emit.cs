using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Routes
{
    /// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
        /// <summary>Auto-generated documentation for ToInterface.</summary>
        public static InterfaceDeclarationSyntax ToInterface(string interfaceName, OpenApiPaths paths)
        {
            var interfaceNode = SyntaxFactory.InterfaceDeclaration(interfaceName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var members = new List<MemberDeclarationSyntax>();
            foreach (var pathKvp in paths)
            {
                var routePath = pathKvp.Key;
                var pathItem = pathKvp.Value;

                var operations = new Dictionary<string, OpenApiOperation?>
                {
                    { "Get", pathItem.Get },
                    { "Put", pathItem.Put },
                    { "Post", pathItem.Post },
                    { "Delete", pathItem.Delete },
                    { "Options", pathItem.Options },
                    { "Head", pathItem.Head },
                    { "Patch", pathItem.Patch },
                    { "Trace", pathItem.Trace },
                    { "Query", pathItem.Query }
                };

                if (pathItem.AdditionalOperations != null)
                {
                    foreach (var addOp in pathItem.AdditionalOperations)
                    {
                        var normVerb = addOp.Key.Substring(0, 1).ToUpperInvariant() + addOp.Key.Substring(1).ToLowerInvariant();
                        operations[normVerb] = addOp.Value;
                    }
                }

                foreach (var opKvp in operations)
                {
                    var httpMethod = opKvp.Key;
                    var operation = opKvp.Value;
                    if (operation == null) continue;

                    var methodName = operation.OperationId ?? $"{httpMethod}{routePath.Replace("/", "").Replace("{", "").Replace("}", "")}";

                    var returnTypeName = "void";
                    if (operation.Responses != null)
                        if (operation.Responses.Any())
                        {
                            var successResponse = operation.Responses.FirstOrDefault(r => r.Key.StartsWith("2"));
                            if (successResponse.Value != null && successResponse.Value.Content != null && successResponse.Value.Content.TryGetValue("application/json", out var mediaType))
                            {
                                var schema = mediaType.Schema;
                                if (schema != null)
                                    if (schema.Type != null)
                                    {
                                        returnTypeName = $"System.Threading.Tasks.Task<{MapTypeToCSharp(schema.Type)}>";
                                    }
                                    else
                                    {
                                        returnTypeName = "System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult>";
                                    }
                            }
                            else
                            {
                                returnTypeName = "System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult>";
                            }
                        }

                    var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnTypeName), methodName)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                    // Add Route Attribute e.g. [HttpGet("/pets")]
                    var attrList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName($"Http{httpMethod}"))
                            .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.AttributeArgument(
                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(routePath))
                                    )
                                )
                            ))
                    ));

                    methodNode = methodNode.AddAttributeLists(attrList);
                    var methodParams = new List<ParameterSyntax>();

                    if (operation.Parameters != null)
                    {
                        var parameters = operation.Parameters.Select(p =>
                        {
                            var param = SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                                .WithType(SyntaxFactory.ParseTypeName(MapTypeToCSharp(p.Schema?.Type)));

                            if (p.In == "query")
                            {
                                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FromQuery"))
                                )));
                            }
                            else if (p.In == "path")
                            {
                                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FromRoute"))
                                )));
                            }

                            if (p.Deprecated.HasValue)
                                if (p.Deprecated.Value)
                                {
                                    param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("System.Obsolete"))
                                    )));
                                }
                            if (p.AllowEmptyValue.HasValue)
                                if (p.AllowEmptyValue.Value)
                                {
                                    param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("AllowEmptyValue"))
                                    )));
                                }
                            if (p.Example != null)
                            {
                                string exampleStr = p.Example.ToString()!;
                                ExpressionSyntax defaultValue;
                                if (p.Schema?.Type == "string") defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(exampleStr));
                                else if (p.Schema?.Type == "boolean") defaultValue = SyntaxFactory.LiteralExpression(exampleStr.ToLower() == "true" ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                                else if (int.TryParse(exampleStr, out int i)) defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i));
                                else if (double.TryParse(exampleStr, out double d)) defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(d));
                                else defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

                                param = param.WithDefault(SyntaxFactory.EqualsValueClause(defaultValue));
                            }


                            if (p.Examples != null)
                                if (p.Examples.Count > 0)
                                {
                                    var exampleArgs = new List<AttributeArgumentSyntax>();
                                    foreach (var ex in p.Examples)
                                    {
                                        exampleArgs.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(ex.Key))));
                                        var exVal = ex.Value.Value?.ToString() ?? "";
                                        exampleArgs.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(exVal))));
                                    }
                                    param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Examples"))
                                            .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(exampleArgs)))
                                    )));
                                }
                            if (p.Style != null)
                            {
                                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Style"))
                                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(p.Style)))
                                        )))
                                )));
                            }
                            if (p.Explode == true && p.Schema?.Type != "array")
                            {
                                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Explode"))
                                )));
                            }
                            if (p.AllowReserved.HasValue)
                                if (p.AllowReserved.Value)
                                {
                                    param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("AllowReserved"))
                                    )));
                                }
                            if (p.Content != null && p.Content.Count > 0)

                            {
                                var firstContent = p.Content.First();
                                var mediaType = firstContent.Key;
                                var schemaType = "string";
                                if (firstContent.Value.Schema != null && firstContent.Value.Schema.Type != null)
                                {
                                    schemaType = firstContent.Value.Schema.Type;
                                }
                                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Content"))
                                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new[]
                                        {
                                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(mediaType))),
                                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(schemaType)))
                                        })))
                                )));
                            }

                            return param;
                        }).ToList();

                        methodParams.AddRange(parameters);
                    }

                    if (operation.RequestBody != null)
                    {
                        string? schemaType = null;
                        if (operation.RequestBody.Content != null)
                        {
                            if (operation.RequestBody.Content.Count > 0)
                            {
                                var mediaType = operation.RequestBody.Content.Values.FirstOrDefault();
                                if (mediaType != null)
                                {
                                    if (mediaType.Schema != null)
                                    {
                                        schemaType = mediaType.Schema.Type;
                                    }
                                }
                            }
                        }
                        var typeName = MapTypeToCSharp(schemaType);
                        if (typeName == "string") typeName = "object"; // Fallback generic type for bodies

                        var param = SyntaxFactory.Parameter(SyntaxFactory.Identifier("body"))
                            .WithType(SyntaxFactory.ParseTypeName(typeName))
                            .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FromBody"))
                            )));
                        methodParams.Add(param);
                    }

                    if (methodParams.Any())
                    {
                        methodNode = methodNode.AddParameterListParameters(methodParams.ToArray());
                    }

                    if (!string.IsNullOrWhiteSpace(operation.Summary))
                    {
                        methodNode = Docstrings.Emit.WithSummary(methodNode, operation.Summary);
                    }


                    if (operation.Responses != null)
                    {
                        foreach (var respKvp in operation.Responses)
                        {
                            var attrs = new Dictionary<string, string> { { "code", respKvp.Key } };
                            if (respKvp.Value.Headers != null)
                                if (respKvp.Value.Headers.Count > 0)
                                {
                                    var headerKvp = respKvp.Value.Headers.First();
                                    attrs["header"] = headerKvp.Key;
                                    var h = headerKvp.Value;
                                    if (!string.IsNullOrEmpty(h.Description)) attrs["header-description"] = h.Description;
                                    if (h.Required.HasValue) attrs["header-required"] = h.Required.Value.ToString().ToLower();
                                    if (h.Deprecated.HasValue) attrs["header-deprecated"] = h.Deprecated.Value.ToString().ToLower();
                                    if (h.Example != null) attrs["header-example"] = h.Example.ToString()!;
                                    if (h.Examples != null)
                                        if (h.Examples.Count > 0)
                                        {
                                            attrs["header-examples"] = string.Join(",", h.Examples.Select(e => $"{e.Key}:{e.Value.Value}"));
                                        }
                                    if (!string.IsNullOrEmpty(h.Style)) attrs["header-style"] = h.Style;
                                    if (h.Explode.HasValue) attrs["header-explode"] = h.Explode.Value.ToString().ToLower();
                                    if (h.Schema?.Type != null) attrs["header-schema"] = h.Schema.Type;
                                    if (h.Content != null)
                                    {
                                        if (h.Content.Count > 0)
                                        {
                                            var c = h.Content.First();
                                            string? schemaType = null;
                                            if (c.Value.Schema != null)
                                            {
                                                schemaType = c.Value.Schema.Type;
                                            }
                                            attrs["header-content"] = $"{c.Key}:{schemaType}";
                                        }
                                    }
                                }
                            string respDesc = "Response";
                            if (respKvp.Value.Description != null)
                            {
                                respDesc = respKvp.Value.Description;
                            }
                            methodNode = Docstrings.Emit.WithTag(methodNode, "response", attrs, respDesc);
                        }
                    }

                    if (operation.Servers != null)
                    {
                        foreach (var s in operation.Servers)
                        {
                            var attrs = new Dictionary<string, string> { { "url", s.Url } };
                            if (!string.IsNullOrEmpty(s.Description)) attrs["description"] = s.Description;
                            methodNode = Docstrings.Emit.WithTag(methodNode, "server", attrs, "");
                        }
                    }

                    if (operation.Callbacks != null)
                    {
                        foreach (var cKvp in operation.Callbacks)
                        {
                            var cName = cKvp.Key;
                            var cbObj = cKvp.Value;
                            if (cbObj.Count > 0)
                            {
                                var expKvp = cbObj.First();
                                var expression = expKvp.Key;
                                var opItem = expKvp.Value;
                                string desc = "Callback";
                                if (opItem.Post != null && opItem.Post.Description != null)
                                {
                                    desc = opItem.Post.Description;
                                }
                                else if (opItem.Get != null && opItem.Get.Description != null)
                                {
                                    desc = opItem.Get.Description;
                                }

                                var attrs = new Dictionary<string, string> { { "name", cName }, { "expression", expression } };
                                methodNode = Docstrings.Emit.WithTag(methodNode, "callback", attrs, desc);
                            }
                        }
                    }

                    members.Add(methodNode);
                }
            }

            return interfaceNode.AddMembers(members.ToArray());
        }

        private static string MapTypeToCSharp(string? openApiType)
        {
            return openApiType switch
            {
                "integer" => "int",
                "number" => "double",
                "boolean" => "bool",
                "string" => "string",
                _ => "string"
            };
        }
    }
}
