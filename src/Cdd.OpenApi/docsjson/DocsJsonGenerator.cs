using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.DocsJson
{
    /// <summary>Generates JSON documentation.</summary>
    public static class DocsJsonGenerator
    {
        /// <summary>Generates the documentation JSON string.</summary>
        public static string Generate(OpenApiDocument doc, bool noImports, bool noWrapping)
        {
            var output = new DocsJsonOutput
            {
                Language = "csharp",
                Operations = new List<DocsJsonOperation>()
            };

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

                    foreach (var opKvp in operations)
                    {
                        var method = opKvp.Key;
                        var operation = opKvp.Value;
                        if (operation == null) continue;

                        var operationId = operation.OperationId;
                        var methodName = operationId ?? $"{method.ToUpperInvariant()}{routePath.Replace("/", "").Replace("{", "").Replace("}", "")}";

                        var opOut = new DocsJsonOperation
                        {
                            Method = method.ToUpperInvariant(),
                            Path = routePath,
                            OperationId = operation.OperationId,
                            Code = new DocsJsonCode()
                        };

                        if (!noImports)
                        {
                            var importsLines = new List<string>
                            {
                                "using System;",
                                "using System.Threading.Tasks;",
                                "using Generated.Api;",
                                "using Generated.Models;"
                            };
                            opOut.Code.Imports = string.Join("\n", importsLines);
                        }

                        if (!noWrapping)
                        {
                            var wrapStart = new List<string>
                            {
                                "public class Example",
                                "{",
                                "    public static async Task Main()",
                                "    {"
                            };
                            opOut.Code.WrapperStart = string.Join("\n", wrapStart);

                            var wrapEnd = new List<string>
                            {
                                "    }",
                                "}"
                            };
                            opOut.Code.WrapperEnd = string.Join("\n", wrapEnd);
                        }

                        var snippetLines = new List<string>();
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

                        opOut.Code.Snippet = string.Join("\n", snippetLines);

                        output.Operations.Add(opOut);
                    }
                }
            }

            var resultList = new List<DocsJsonOutput> { output };
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            return JsonSerializer.Serialize(resultList, options);
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
