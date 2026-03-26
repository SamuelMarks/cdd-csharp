using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.DocsJson
{
    public static class DocsJsonGenerator
    {
        public static string Generate(OpenApiDocument doc, bool noImports, bool noWrapping)
        {
            var endpoints = new Dictionary<string, Dictionary<string, string>>();

            if (doc.Paths != null)
            {
                foreach (var pathKvp in doc.Paths)
                {
                    var routePath = pathKvp.Key;
                    var pathItem = pathKvp.Value;

                    var operations = new Dictionary<string, OpenApiOperation?>
                    {
                        { "get", pathItem.Get },
                        { "put", pathItem.Put },
                        { "post", pathItem.Post },
                        { "delete", pathItem.Delete },
                        { "options", pathItem.Options },
                        { "head", pathItem.Head },
                        { "patch", pathItem.Patch },
                        { "trace", pathItem.Trace }
                    };

                    var pathMap = new Dictionary<string, string>();

                    foreach (var opKvp in operations)
                    {
                        var method = opKvp.Key;
                        var operation = opKvp.Value;
                        if (operation == null) continue;

                        var operationId = operation.OperationId;
                        var methodName = operationId ?? $"{method}{routePath.Replace("/", "").Replace("{", "").Replace("}", "")}";

                        var snippetLines = new List<string>();
                        
                        if (!noImports)
                        {
                            snippetLines.Add("using System;");
                            snippetLines.Add("using System.Threading.Tasks;");
                            snippetLines.Add("using Generated.Api;");
                            snippetLines.Add("using Generated.Models;");
                            snippetLines.Add("");
                        }

                        if (!noWrapping)
                        {
                            snippetLines.Add("public class Example");
                            snippetLines.Add("{");
                            snippetLines.Add("    public static async Task Main()");
                            snippetLines.Add("    {");
                        }

                        string indent = noWrapping ? "" : "        ";

                        snippetLines.Add($"{indent}// Initialize the API client");
                        snippetLines.Add($"{indent}var client = new ApiClient();");
                        snippetLines.Add("");

                        // Map parameters
                        var parameters = new List<string>();
                        if (operation.Parameters != null)
                        {
                            foreach (var param in operation.Parameters)
                            {
                                if (param == null || string.IsNullOrEmpty(param.Name)) continue;
                                var pName = param.Name;
                                var pType = MapTypeToCSharp(param.Schema?.Type);
                                var pValue = GetDefaultValueForType(pType);
                                snippetLines.Add($"{indent}{pType} {pName} = {pValue};");
                                parameters.Add(pName);
                            }
                        }

                        if (operation.RequestBody != null)
                        {
                            snippetLines.Add($"{indent}var requestBody = new object(); // TODO: Initialize request body");
                            parameters.Add("requestBody");
                        }

                        var paramsString = string.Join(", ", parameters);
                        snippetLines.Add($"{indent}await client.{methodName}Async({paramsString});");

                        if (!noWrapping)
                        {
                            snippetLines.Add("    }");
                            snippetLines.Add("}");
                        }

                        pathMap[method] = string.Join("\n", snippetLines);
                    }

                    if (pathMap.Count > 0)
                    {
                        endpoints[routePath] = pathMap;
                    }
                }
            }

            var root = new Dictionary<string, object>
            {
                { "endpoints", endpoints }
            };

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            return JsonSerializer.Serialize(root, options);
        }

        private static string MapTypeToCSharp(string? openApiType)
        {
            return openApiType switch
            {
                "integer" => "int",
                "number" => "double",
                "boolean" => "bool",
                "string" => "string",
                "object" => "object",
                _ => "object"
            };
        }

        private static string GetDefaultValueForType(string csharpType)
        {
            return csharpType switch
            {
                "int" => "0",
                "double" => "0.0",
                "bool" => "false",
                "string" => "\"example\"",
                _ => "null"
            };
        }
    }
}
