// <copyright file="OperationMetadataExtractor.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Common
{
    /// <summary>
    /// Shared Roslyn syntax parser for extracting OpenAPI responses, headers, links, security requirements, callbacks, servers, and parameter metadata from C# method declarations.
    /// </summary>
    public static class OperationMetadataExtractor
    {
        /// <summary>
        /// Extracts an <see cref="OpenApiResponses"/> collection from method <c>&lt;response&gt;</c> XML tags and fallback return type metadata.
        /// </summary>
        /// <param name="method">The C# method syntax node to inspect.</param>
        /// <param name="defaultResponse">The default <c>200</c> response constructed from the method's return type.</param>
        /// <returns>The populated <see cref="OpenApiResponses"/> dictionary.</returns>
        public static OpenApiResponses ExtractResponses(MethodDeclarationSyntax method, OpenApiResponse defaultResponse)
        {
            var responses = new OpenApiResponses();
            var responseTags = Docstrings.Parse.GetTagsWithAttributes(method, "response");
            bool has200 = false;

            foreach (var tag in responseTags)
            {
                var code = tag.Attributes.TryGetValue("code", out var c) ? c : "default";
                if (code == "200") has200 = true;

                var resp = new OpenApiResponse { Description = string.IsNullOrEmpty(tag.Text) ? "Response" : tag.Text };

                if (tag.Attributes.TryGetValue("header", out var hdr))
                {
                    var headerObj = new OpenApiHeader { Description = tag.Attributes.TryGetValue("header-description", out var hd) ? hd : "Header " + hdr };
                    if (tag.Attributes.TryGetValue("header-required", out var hr) && bool.TryParse(hr, out var hrb))
                    {
                        headerObj.Required = hrb;
                    }
                    if (tag.Attributes.TryGetValue("header-deprecated", out var hdpr) && bool.TryParse(hdpr, out var hdprb))
                    {
                        headerObj.Deprecated = hdprb;
                    }
                    if (tag.Attributes.TryGetValue("header-example", out var he))
                    {
                        headerObj.Example = he;
                    }
                    if (tag.Attributes.TryGetValue("header-examples", out var hex))
                    {
                        headerObj.Examples = new Dictionary<string, OpenApiExample>();
                        foreach (var pair in hex.Split(','))
                        {
                            var kv = pair.Split(new[] { ':' }, 2);
                            if (kv.Length == 2) headerObj.Examples[kv[0]] = new OpenApiExample { Value = kv[1] };
                        }
                    }
                    if (tag.Attributes.TryGetValue("header-style", out var hs))
                    {
                        headerObj.Style = hs;
                    }
                    if (tag.Attributes.TryGetValue("header-explode", out var hexpl) && bool.TryParse(hexpl, out var hexplb))
                    {
                        headerObj.Explode = hexplb;
                    }
                    if (tag.Attributes.TryGetValue("header-schema", out var hsch))
                    {
                        headerObj.Schema = new OpenApiSchema { Type = hsch };
                    }
                    if (tag.Attributes.TryGetValue("header-content", out var hcnt))
                    {
                        var parts = hcnt.Split(':');
                        if (parts.Length == 2)
                        {
                            headerObj.Content = new Dictionary<string, OpenApiMediaType>
                            {
                                { parts[0], new OpenApiMediaType { Schema = new OpenApiSchema { Type = parts[1] } } }
                            };
                        }
                    }

                    resp.Headers = new Dictionary<string, OpenApiHeader> { { hdr, headerObj } };
                }

                if (tag.Attributes.TryGetValue("link", out var lnk))
                {
                    var linkObj = new OpenApiLink { OperationId = lnk };
                    if (tag.Attributes.TryGetValue("link-operationRef", out var opRef)) linkObj.OperationRef = opRef;
                    if (tag.Attributes.TryGetValue("link-description", out var desc)) linkObj.Description = desc;
                    if (tag.Attributes.TryGetValue("link-requestBody", out var rb)) linkObj.RequestBody = rb;

                    var paramStr = tag.Attributes.TryGetValue("link-parameters", out var pStr) ? pStr : null;
                    if (!string.IsNullOrEmpty(paramStr))
                    {
                        linkObj.Parameters = new Dictionary<string, OpenApiAny>();
                        foreach (var p in paramStr.Split(','))
                        {
                            var parts = p.Split(':');
                            if (parts.Length == 2) linkObj.Parameters[parts[0].Trim()] = parts[1].Trim();
                        }
                    }

                    if (tag.Attributes.TryGetValue("link-serverUrl", out var serverUrl))
                    {
                        linkObj.Server = new OpenApiServer { Url = serverUrl };
                    }

                    resp.Links = new Dictionary<string, OpenApiLink> { { lnk, linkObj } };
                }

                if (code.StartsWith("2"))
                {
                    resp.Content = defaultResponse.Content;
                }

                responses[code] = resp;
            }

            if (!has200)
            {
                responses["200"] = defaultResponse;
            }

            return responses;
        }

        /// <summary>
        /// Populates operation-level metadata (<c>description</c>, <c>externalDocs</c>, <c>deprecated</c>, <c>tags</c>, <c>security</c>, <c>servers</c>, and <c>callbacks</c>) from a <see cref="MethodDeclarationSyntax"/>.
        /// </summary>
        /// <param name="method">The C# method syntax node to inspect.</param>
        /// <param name="operation">The target <see cref="OpenApiOperation"/> to populate.</param>
        public static void PopulateOperationMetadata(MethodDeclarationSyntax method, OpenApiOperation operation)
        {
            var descTag = Docstrings.Parse.GetTag(method, "description");
            if (!string.IsNullOrEmpty(descTag)) operation.Description = descTag;

            var extDocs = Docstrings.Parse.GetTag(method, "externalDocs");
            if (!string.IsNullOrEmpty(extDocs)) operation.ExternalDocs = new OpenApiExternalDocs { Url = extDocs };

            var isDeprecated = method.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString() == "Obsolete");
            if (isDeprecated) operation.Deprecated = true;

            var tagsStr = Docstrings.Parse.GetTag(method, "tags");
            if (!string.IsNullOrEmpty(tagsStr))
            {
                operation.Tags = tagsStr.Split(',').Select(t => t.Trim()).ToList();
            }

            var securityTags = Docstrings.Parse.GetTagsWithAttributes(method, "security");
            if (securityTags.Any())
            {
                operation.Security = new List<IDictionary<string, IList<string>>>();
                foreach (var sec in securityTags)
                {
                    var name = sec.Attributes.TryGetValue("name", out var n) ? n : "default";
                    var scopes = sec.Attributes.TryGetValue("scopes", out var s)
                        ? s.Split(',').Select(st => st.Trim()).ToList()
                        : new List<string>();
                    operation.Security.Add(new Dictionary<string, IList<string>> { { name, scopes } });
                }
            }

            var servers = Docstrings.Parse.GetServers(method).ToList();
            if (servers.Any())
            {
                operation.Servers = servers;
            }

            var callbackTags = Docstrings.Parse.GetTagsWithAttributes(method, "callback");
            if (callbackTags.Any())
            {
                operation.Callbacks = new Dictionary<string, OpenApiCallback>();
                foreach (var ct in callbackTags)
                {
                    var name = ct.Attributes.TryGetValue("name", out var n) ? n : "myCallback";
                    var expression = ct.Attributes.TryGetValue("expression", out var e) ? e : "{$request.body#/callbackUrl}";

                    var cb = new OpenApiCallback();
                    cb[expression] = new OpenApiPathItem
                    {
                        Post = new OpenApiOperation { Description = ct.Text }
                    };
                    operation.Callbacks[name] = cb;
                }
            }
        }

        /// <summary>
        /// Populates parameter-level attributes and docstrings (<c>[Obsolete]</c>, <c>[AllowEmptyValue]</c>, <c>[AllowReserved]</c>, <c>[Style]</c>, <c>[Explode]</c>, <c>[Examples]</c>, <c>[Content]</c>, default value, and <c>&lt;param&gt;</c> tag) onto an <see cref="OpenApiParameter"/>.
        /// </summary>
        /// <param name="param">The Roslyn parameter syntax node.</param>
        /// <param name="method">The enclosing method syntax node.</param>
        /// <param name="paramObj">The target <see cref="OpenApiParameter"/> to populate.</param>
        public static void PopulateParameterMetadata(ParameterSyntax param, MethodDeclarationSyntax method, OpenApiParameter paramObj)
        {
            var allAttrs = param.AttributeLists.SelectMany(a => a.Attributes).ToList();
            var attrNames = allAttrs.Select(a => a.Name.ToString()).ToList();

            if (attrNames.Any(n => n.Contains("Obsolete"))) paramObj.Deprecated = true;
            if (attrNames.Any(n => n.Contains("AllowEmptyValue"))) paramObj.AllowEmptyValue = true;
            if (attrNames.Any(n => n.Contains("AllowReserved"))) paramObj.AllowReserved = true;

            var examplesAttr = allAttrs.FirstOrDefault(a => a.Name.ToString().Contains("Examples"));
            if (examplesAttr?.ArgumentList != null)
            {
                paramObj.Examples = new Dictionary<string, OpenApiExample>();
                var args = examplesAttr.ArgumentList.Arguments;
                for (int i = 0; i + 1 < args.Count; i += 2)
                {
                    var key = (args[i].Expression as LiteralExpressionSyntax)?.Token.ValueText;
                    var val = (args[i + 1].Expression as LiteralExpressionSyntax)?.Token.ValueText;
                    if (key != null && val != null)
                    {
                        paramObj.Examples[key] = new OpenApiExample { Value = val };
                    }
                }
            }

            var styleAttr = allAttrs.FirstOrDefault(a => a.Name.ToString().Contains("Style"));
            if (styleAttr?.ArgumentList != null && styleAttr.ArgumentList.Arguments.Any())
            {
                if (styleAttr.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax styleLit)
                {
                    paramObj.Style = styleLit.Token.ValueText;
                }
            }

            var explodeAttr = allAttrs.FirstOrDefault(a => a.Name.ToString().Contains("Explode"));
            if (explodeAttr != null)
            {
                paramObj.Explode = true;
                if (explodeAttr.ArgumentList != null && explodeAttr.ArgumentList.Arguments.Count > 0 &&
                    explodeAttr.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax argExpr)
                {
                    paramObj.Explode = argExpr.Token.ValueText.ToLowerInvariant() == "true";
                }
            }

            if (param.Default != null)
            {
                paramObj.Example = param.Default.Value.ToString().Trim('"');
            }

            var paramTags = Docstrings.Parse.GetTagsWithAttributes(method, "param");
            var paramDoc = paramTags.FirstOrDefault(t => t.Attributes.TryGetValue("name", out var n) && n == param.Identifier.Text);
            if (!string.IsNullOrWhiteSpace(paramDoc.Text))
            {
                paramObj.Description = paramDoc.Text;
            }

            var contentAttr = allAttrs.FirstOrDefault(a => a.Name.ToString() == "Content");
            if (contentAttr?.ArgumentList != null && contentAttr.ArgumentList.Arguments.Count >= 2)
            {
                var mediaType = (contentAttr.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax)?.Token.ValueText;
                var schemaType = (contentAttr.ArgumentList.Arguments[1].Expression as LiteralExpressionSyntax)?.Token.ValueText;
                if (mediaType != null && schemaType != null)
                {
                    paramObj.Content = new Dictionary<string, OpenApiMediaType>
                    {
                        { mediaType, new OpenApiMediaType { Schema = new OpenApiSchema { Type = schemaType } } }
                    };
                    paramObj.Schema = null;
                }
            }
        }
    }
}
