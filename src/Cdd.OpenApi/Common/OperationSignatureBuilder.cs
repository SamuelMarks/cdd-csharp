// <copyright file="OperationSignatureBuilder.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Common
{
    /// <summary>
    /// Shared Roslyn syntax builder for OpenAPI operation parameter attributes, default values, and XML documentation tags.
    /// </summary>
    public static class OperationSignatureBuilder
    {
        /// <summary>
        /// Decorates a <see cref="ParameterSyntax"/> node with OpenAPI parameter attributes (<c>[Obsolete]</c>, <c>[AllowEmptyValue]</c>, <c>[Examples]</c>, <c>[Style]</c>, <c>[Explode]</c>, <c>[AllowReserved]</c>, <c>[Content]</c>) and default value literals.
        /// </summary>
        /// <param name="param">The base Roslyn parameter syntax node.</param>
        /// <param name="openApiParam">The source <see cref="OpenApiParameter"/> metadata.</param>
        /// <returns>The decorated <see cref="ParameterSyntax"/> node.</returns>
        public static ParameterSyntax ApplyParameterMetadata(ParameterSyntax param, OpenApiParameter openApiParam)
        {
            if (openApiParam.Deprecated == true)
            {
                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("System.Obsolete")))));
            }

            if (openApiParam.AllowEmptyValue == true)
            {
                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("AllowEmptyValue")))));
            }

            if (openApiParam.Example != null)
            {
                string exampleStr = openApiParam.Example.ToString();
                ExpressionSyntax defaultValue;
                if (openApiParam.Schema?.Type == OpenApiDataType.String)
                {
                    defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(exampleStr));
                }
                else if (openApiParam.Schema?.Type == OpenApiDataType.Boolean)
                {
                    defaultValue = SyntaxFactory.LiteralExpression(exampleStr.ToLowerInvariant() == "true" ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                }
                else if (int.TryParse(exampleStr, out int i))
                {
                    defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i));
                }
                else if (double.TryParse(exampleStr, out double d))
                {
                    defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(d));
                }
                else
                {
                    defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
                }

                param = param.WithDefault(SyntaxFactory.EqualsValueClause(defaultValue));
            }

            if (openApiParam.Examples != null && openApiParam.Examples.Count > 0)
            {
                var exampleArgs = new List<AttributeArgumentSyntax>();
                foreach (var ex in openApiParam.Examples)
                {
                    exampleArgs.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(ex.Key))));
                    var exVal = ex.Value.Value?.ToString() ?? string.Empty;
                    exampleArgs.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(exVal))));
                }

                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Examples"))
                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(exampleArgs))))));
            }

            if (openApiParam.Style != null)
            {
                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Style"))
                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(openApiParam.Style)))))))));
            }

            if (openApiParam.Explode == true && openApiParam.Schema?.Type != OpenApiDataType.Array)
            {
                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Explode")))));
            }

            if (openApiParam.AllowReserved == true)
            {
                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("AllowReserved")))));
            }

            if (openApiParam.Content != null && openApiParam.Content.Count > 0)
            {
                var firstContent = openApiParam.Content.First();
                var mediaType = firstContent.Key;
                var schemaType = firstContent.Value.Schema?.Type ?? OpenApiDataType.String;
                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Content"))
                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(mediaType))),
                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(schemaType)))
                        }))))));
            }

            return param;
        }

        /// <summary>
        /// Decorates a <see cref="MethodDeclarationSyntax"/> node with XML documentation tags for summary, responses, headers, servers, and callbacks.
        /// </summary>
        /// <param name="methodNode">The method syntax node to annotate.</param>
        /// <param name="operation">The source <see cref="OpenApiOperation"/> metadata.</param>
        /// <param name="includeParamDocs">Whether to emit <c>&lt;param&gt;</c> XML tags for operation parameters.</param>
        /// <returns>The annotated <see cref="MethodDeclarationSyntax"/> node.</returns>
        public static MethodDeclarationSyntax ApplyOperationDocstrings(MethodDeclarationSyntax methodNode, OpenApiOperation operation, bool includeParamDocs)
        {
            if (!string.IsNullOrWhiteSpace(operation.Summary))
            {
                methodNode = Docstrings.Emit.WithSummary(methodNode, operation.Summary);
            }

            if (includeParamDocs && operation.Parameters != null)
            {
                foreach (var opParam in operation.Parameters)
                {
                    if (!string.IsNullOrEmpty(opParam.Description))
                    {
                        methodNode = Docstrings.Emit.WithTag(methodNode, "param", new Dictionary<string, string> { { "name", opParam.Name } }, opParam.Description);
                    }
                }
            }

            if (operation.Responses != null)
            {
                foreach (var respKvp in operation.Responses)
                {
                    var attrs = new Dictionary<string, string> { { "code", respKvp.Key } };
                    if (respKvp.Value.Headers != null && respKvp.Value.Headers.Count > 0)
                    {
                        var headerKvp = respKvp.Value.Headers.First();
                        attrs["header"] = headerKvp.Key;
                        var h = headerKvp.Value;
                        if (!string.IsNullOrEmpty(h.Description)) attrs["header-description"] = h.Description;
                        if (h.Required.HasValue) attrs["header-required"] = h.Required.Value.ToString().ToLowerInvariant();
                        if (h.Deprecated.HasValue) attrs["header-deprecated"] = h.Deprecated.Value.ToString().ToLowerInvariant();
                        if (h.Example != null) attrs["header-example"] = h.Example.ToString();
                        if (h.Examples != null && h.Examples.Count > 0)
                        {
                            attrs["header-examples"] = string.Join(",", h.Examples.Select(e => $"{e.Key}:{e.Value.Value}"));
                        }
                        if (!string.IsNullOrEmpty(h.Style)) attrs["header-style"] = h.Style;
                        if (h.Explode.HasValue) attrs["header-explode"] = h.Explode.Value.ToString().ToLowerInvariant();
                        var headerSchemaType = h.Schema != null ? h.Schema.Type : null;
                        if (!string.IsNullOrEmpty(headerSchemaType)) attrs["header-schema"] = headerSchemaType;
                        if (h.Content != null && h.Content.Count > 0)
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

                    string respDesc = "Response";
                    if (!string.IsNullOrEmpty(respKvp.Value.Description))
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
                    methodNode = Docstrings.Emit.WithTag(methodNode, "server", attrs, string.Empty);
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
                        var desc = opItem.Post?.Description ?? opItem.Get?.Description ?? "Callback";
                        var attrs = new Dictionary<string, string> { { "name", cName }, { "expression", expression } };
                        methodNode = Docstrings.Emit.WithTag(methodNode, "callback", attrs, desc);
                    }
                }
            }

            return methodNode;
        }
    }
}
