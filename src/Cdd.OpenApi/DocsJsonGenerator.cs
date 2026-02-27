using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.DocsJson
{
    /// <summary>DocsJsonGenerator</summary>
    public static class DocsJsonGenerator
    {
        /// <summary>Generate</summary>
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
                        { "GET", pathItem.Get },
                        { "PUT", pathItem.Put },
                        { "POST", pathItem.Post },
                        { "DELETE", pathItem.Delete },
                        { "OPTIONS", pathItem.Options },
                        { "HEAD", pathItem.Head },
                        { "PATCH", pathItem.Patch },
                        { "TRACE", pathItem.Trace }
                    };

                    foreach (var opKvp in operations)
                    {
                        var method = opKvp.Key;
                        var operation = opKvp.Value;
                        if (operation == null) continue;

                        var operationId = operation.OperationId;
                        var methodName = operationId ?? $"{method}{routePath.Replace("/", "").Replace("{", "").Replace("}", "")}";

                        var docsOp = new DocsJsonOperation
                        {
                            Method = method,
                            Path = routePath,
                            OperationId = operationId
                        };

                        if (!noImports)
                        {
                            docsOp.Code.Imports = "using System;\nusing System.Threading.Tasks;\nusing Generated.Api;\nusing Generated.Models;";
                        }

                        if (!noWrapping)
                        {
                            docsOp.Code.WrapperStart = "public class Example\n{\n    public static async Task Main()\n    {";
                            docsOp.Code.WrapperEnd = "    }\n}";
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

                        // We can also have a request body (but we'll keep it simple if it's not well supported in the generator)
                        if (operation.RequestBody != null)
                        {
                            snippetLines.Add($"{indent}var requestBody = new object(); // TODO: Initialize request body");
                            parameters.Add("requestBody");
                        }

                        var paramsString = string.Join(", ", parameters);
                        snippetLines.Add($"{indent}await client.{methodName}Async({paramsString});");

                        docsOp.Code.Snippet = string.Join("\n", snippetLines);

                        output.Operations.Add(docsOp);
                    }
                }
            }

            var jsonArray = new List<DocsJsonOutput> { output };
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            return JsonSerializer.Serialize(jsonArray, options);
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