using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Routes
{
/// <summary>Auto-generated documentation for Parse.</summary>
    public static class Parse
    {
/// <summary>Auto-generated documentation for ToPaths.</summary>
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

                var responses = new OpenApiResponses();

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

                if (returnTypeStr != "void" && returnTypeStr != "Task" && returnTypeStr != "IActionResult" && returnTypeStr != "Microsoft.AspNetCore.Mvc.IActionResult")
                {
                    defaultResponse.Content = new Dictionary<string, OpenApiMediaType>
                    {
                        { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = MapType(returnTypeStr) } } }
                    };
                }

                var responseTags = Docstrings.Parse.GetTagsWithAttributes(method, "response");
                bool has200 = false;
                foreach (var tag in responseTags)
                {
                    var code = tag.Attributes.TryGetValue("code", out var c) ? c : "default";
                    if (code == "200") has200 = true;
                    
                    var resp = new OpenApiResponse { Description = string.IsNullOrEmpty(tag.Text) ? "Response" : tag.Text };
                    
                    // headers & links could be fetched here via other nested tags if expanded, but for simplicity:

                    if (tag.Attributes.TryGetValue("header", out var hdr))
                    {
                        var headerObj = new OpenApiHeader { Description = tag.Attributes.TryGetValue("header-description", out var hd) ? hd : "Header " + hdr };
                        if (tag.Attributes.TryGetValue("header-required", out var hr) && bool.TryParse(hr, out var hrb)) headerObj.Required = hrb;
                        if (tag.Attributes.TryGetValue("header-deprecated", out var hdpr) && bool.TryParse(hdpr, out var hdprb)) headerObj.Deprecated = hdprb;
                        if (tag.Attributes.TryGetValue("header-example", out var he)) headerObj.Example = he;
                        
                        if (tag.Attributes.TryGetValue("header-examples", out var hex))
                        {
                            headerObj.Examples = new Dictionary<string, OpenApiExample>();
                            foreach (var pair in hex.Split(','))
                            {
                                var kv = pair.Split(new[] { ':' }, 2);
                                if (kv.Length == 2) headerObj.Examples[kv[0]] = new OpenApiExample { Value = kv[1] };
                            }
                        }

                        if (tag.Attributes.TryGetValue("header-style", out var hs)) headerObj.Style = hs;
                        if (tag.Attributes.TryGetValue("header-explode", out var hexpl) && bool.TryParse(hexpl, out var hexplb)) headerObj.Explode = hexplb;
                        
                        if (tag.Attributes.TryGetValue("header-schema", out var hsch)) headerObj.Schema = new OpenApiSchema { Type = hsch };
                        
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
                            linkObj.Parameters = new Dictionary<string, object>();
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

                        resp.Links = new Dictionary<string, OpenApiLink>
                        {
                            { lnk, linkObj }
                        };
                    }
                    
                    if (code == "200" || code.StartsWith("2"))
                    {
                        resp.Content = defaultResponse.Content; // Inherit generated content for success
                    }

                    responses[code] = resp;
                }

                if (!has200)
                {
                    responses["200"] = defaultResponse;
                }

                var operation = new OpenApiOperation
                {
                    OperationId = method.Identifier.Text,
                    Summary = Docstrings.Parse.GetSummary(method),
                    Responses = responses
                };

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
                        var scopes = sec.Attributes.TryGetValue("scopes", out var s) ? s.Split(',').Select(st => st.Trim()).ToList() : new List<string>();
                        operation.Security.Add(new Dictionary<string, IList<string>> { { name, scopes } });
                    }
                }

                // Add parameters
                var parameters = new List<OpenApiParameter>();
                foreach (var param in method.ParameterList.Parameters)
                {
                    var inType = routePath.Contains($"{{{param.Identifier.Text}}}") ? "path" : "query";
                    
                    var attrs = param.AttributeLists.SelectMany(al => al.Attributes).Select(a => a.Name.ToString()).ToList();
                    if (attrs.Contains("FromRoute")) inType = "path";
                    else if (attrs.Contains("FromQuery")) inType = "query";
                    else if (attrs.Contains("FromBody")) inType = "body";
                    else if (attrs.Contains("FromHeader")) inType = "header";

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
                                        Schema = new OpenApiSchema { Type = MapType(param.Type?.ToString()) }
                                    }
                                }
                            }
                        };
                        
                        var encodingAttr = param.AttributeLists.SelectMany(a => a.Attributes).FirstOrDefault(a => a.Name.ToString().Contains("Encoding"));
                        if (encodingAttr != null && encodingAttr.ArgumentList != null && encodingAttr.ArgumentList.Arguments.Count >= 2)
                        {
                            var propName = (encodingAttr.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax)?.Token.ValueText;
                            var contentType = (encodingAttr.ArgumentList.Arguments[1].Expression as LiteralExpressionSyntax)?.Token.ValueText;
                            if (propName != null && contentType != null)
                            {
                                var encObj = new OpenApiEncoding { ContentType = contentType };
                                
                                var styleArg = encodingAttr.ArgumentList.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "Style");
                                if (styleArg != null) encObj.Style = (styleArg.Expression as LiteralExpressionSyntax)?.Token.ValueText;
                                
                                var explodeArg = encodingAttr.ArgumentList.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "Explode");
                                if (explodeArg != null) {
                                    if (explodeArg.Expression.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.TrueLiteralExpression) encObj.Explode = true;
                                    else if (explodeArg.Expression.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.FalseLiteralExpression) encObj.Explode = false;
                                }

                                var allowReservedArg = encodingAttr.ArgumentList.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "AllowReserved");
                                if (allowReservedArg != null) {
                                     if (allowReservedArg.Expression.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.TrueLiteralExpression) encObj.AllowReserved = true;
                                    else if (allowReservedArg.Expression.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.FalseLiteralExpression) encObj.AllowReserved = false;
                                }

                                reqBody.Content["application/x-www-form-urlencoded"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Type = "object" },
                                    Encoding = new Dictionary<string, OpenApiEncoding> { { propName, encObj } }
                                };
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
                            Schema = new OpenApiSchema { Type = MapType(param.Type?.ToString()) }
                        };

                        if (attrs.Contains("Obsolete")) paramObj.Deprecated = true;
                        if (attrs.Contains("AllowEmptyValue")) paramObj.AllowEmptyValue = true;
                        if (attrs.Contains("AllowReserved")) paramObj.AllowReserved = true;

                        
                        var styleAttr = param.AttributeLists.SelectMany(a => a.Attributes).FirstOrDefault(a => a.Name.ToString().Contains("Style"));

                        var examplesAttr = param.AttributeLists.SelectMany(a => a.Attributes).FirstOrDefault(a => a.Name.ToString().Contains("Examples"));
                        if (examplesAttr != null && examplesAttr.ArgumentList != null)
                        {
                            paramObj.Examples = new Dictionary<string, OpenApiExample>();
                            var args = examplesAttr.ArgumentList.Arguments;
                            for (int i = 0; i < args.Count; i += 2)
                            {
                                if (i + 1 < args.Count)
                                {
                                    var key = (args[i].Expression as LiteralExpressionSyntax)?.Token.ValueText;
                                    var val = (args[i + 1].Expression as LiteralExpressionSyntax)?.Token.ValueText;
                                    if (key != null && val != null)
                                    {
                                        paramObj.Examples[key] = new OpenApiExample { Value = val };
                                    }
                                }
                            }
                        }

                        if (styleAttr != null && styleAttr.ArgumentList != null && styleAttr.ArgumentList.Arguments.Any())
                        {
                            var arg = styleAttr.ArgumentList.Arguments.First().Expression as LiteralExpressionSyntax;
                            if (arg != null) paramObj.Style = arg.Token.ValueText;
                        }

                        var explodeAttr = param.AttributeLists.SelectMany(a => a.Attributes).FirstOrDefault(a => a.Name.ToString().Contains("Explode"));
                        if (explodeAttr != null)
                        {
                            if (explodeAttr.ArgumentList != null && explodeAttr.ArgumentList.Arguments.Any())
                            {
                                var arg = explodeAttr.ArgumentList.Arguments.First().Expression;
                                if (arg is LiteralExpressionSyntax lit) paramObj.Explode = lit.Token.ValueText.ToLower() == "true";
                                else if (arg.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.TrueLiteralExpression) paramObj.Explode = true;
                                else if (arg.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.FalseLiteralExpression) paramObj.Explode = false;
                            }
                            else
                            {
                                paramObj.Explode = true;
                            }
                        }

                        if (param.Default != null)
                        {
                            paramObj.Example = param.Default.Value.ToString().Trim('"');
                        }

                        var paramTags = Docstrings.Parse.GetTagsWithAttributes(method, "param");
                        var paramDoc = paramTags.FirstOrDefault(t => t.Attributes.TryGetValue("name", out var n) && n == param.Identifier.Text);
                        if (paramDoc.Text != null && !string.IsNullOrWhiteSpace(paramDoc.Text))
                        {
                             paramObj.Description = paramDoc.Text;
                        }

                        var contentAttr = param.AttributeLists.SelectMany(a => a.Attributes).FirstOrDefault(a => a.Name.ToString() == "Content");
                        if (contentAttr?.ArgumentList?.Arguments.Count >= 2)
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

                        parameters.Add(paramObj);
                    }
                }
                if (parameters.Any()) operation.Parameters = parameters;

                var serverTags = Docstrings.Parse.GetServers(method).ToList();
                if (serverTags.Any())
                {
                    operation.Servers = serverTags;
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

                if (!paths.ContainsKey(routePath))
                {
                    paths[routePath] = new OpenApiPathItem
                    {
                        Summary = pathSummary,
                        Description = pathDescription,
                        Ref = pathRef
                    };
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
                case "query": pathItem.Query = op; break;
                default: 
                    if (pathItem.AdditionalOperations == null) pathItem.AdditionalOperations = new Dictionary<string, OpenApiOperation>();
                    pathItem.AdditionalOperations[method.ToUpperInvariant()] = op;
                    break;
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
