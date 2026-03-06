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

                if (!routePath.StartsWith("/")) routePath = "/" + routePath;

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

                if (returnTypeStr != "string" && returnTypeStr != "Task")
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
                        resp.Content = defaultResponse.Content;
                    }

                    responses[code] = resp;
                }

                if (!has200)
                {
                    responses["200"] = defaultResponse;
                }

                var operationId = method.Identifier.Text;
                if (operationId.EndsWith("Async")) operationId = operationId.Substring(0, operationId.Length - 5);

                var operation = new OpenApiOperation
                {
                    OperationId = operationId,
                    Summary = Docstrings.Parse.GetSummary(method),
                    Responses = responses
                };

                var descTag = Docstrings.Parse.GetTag(method, "description");
                if (!string.IsNullOrEmpty(descTag)) operation.Description = descTag;

                var extDocs = Docstrings.Parse.GetTag(method, "externalDocs");
                if (!string.IsNullOrEmpty(extDocs)) operation.ExternalDocs = new OpenApiExternalDocs { Url = extDocs };

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

                var isMethodDeprecated = method.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString() == "Obsolete");
                if (isMethodDeprecated) operation.Deprecated = true;

                                var parameters = new List<OpenApiParameter>();
                foreach (var param in method.ParameterList.Parameters)
                {
                    var paramName = param.Identifier.Text;
                    var typeStr = param.Type?.ToString();
                    bool isFromBody = param.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains("FromBody"));
                    

                    bool isDeprecated = param.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains("Obsolete"));
                    bool isAllowEmpty = param.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains("AllowEmptyValue"));
                    object? exampleValue = null;
                    if (param.Default != null) {
                        exampleValue = param.Default.Value.ToString().Trim('"');
                    }
                    
                    if (isFromBody || ((httpMethod == "post" || httpMethod == "put" || httpMethod == "patch") && !routePath.Contains($"{{{paramName}}}")))

                    {
                        operation.RequestBody = new OpenApiRequestBody
                        {
                            Description = "Request body",
                            Required = true,
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = MapType(typeStr) } } }
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
                            Schema = new OpenApiSchema { Type = MapType(typeStr) }
                        };
                        
                        if (isDeprecated) paramObj.Deprecated = true;
                        if (isAllowEmpty) paramObj.AllowEmptyValue = true;
                        if (exampleValue != null) paramObj.Example = exampleValue;

                        
                        var styleAttr = param.AttributeLists.SelectMany(a => a.Attributes).FirstOrDefault(a => a.Name.ToString() == "Style");

                        var examplesAttr = param.AttributeLists.SelectMany(a => a.Attributes).FirstOrDefault(a => a.Name.ToString() == "Examples");
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

                        if (styleAttr?.ArgumentList?.Arguments.Count > 0)
                        {
                            paramObj.Style = (styleAttr.ArgumentList.Arguments[0].Expression as LiteralExpressionSyntax)?.Token.ValueText;
                        }

                        if (param.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString() == "Explode"))
                            paramObj.Explode = true;

                        if (param.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString() == "AllowReserved"))
                            paramObj.AllowReserved = true;

                        var paramTags = Docstrings.Parse.GetTagsWithAttributes(method, "param");
                        var paramDoc = paramTags.FirstOrDefault(t => t.Attributes.TryGetValue("name", out var n) && n == paramName);
                        if (paramDoc.Text != null && !string.IsNullOrWhiteSpace(paramDoc.Text))
                        {
                            paramObj.Description = paramDoc.Text;
                        }

                        var contentAttr = param.AttributeLists.SelectMany(a => a.Attributes).FirstOrDefault(a => a.Name.ToString() == "Content");                        if (contentAttr?.ArgumentList?.Arguments.Count >= 2)
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
                _ => "string" // Fallback
            };
        }
    }
}
