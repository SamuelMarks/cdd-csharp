using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi
{
    /// <summary>
    /// Generates integration tests.
    /// </summary>
    public static class IntegrationTestGenerator
    {
        /// <summary>
        /// Generates test code from an OpenAPI document.
        /// </summary>
        public static string Generate(OpenApiDocument doc)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Xunit;");
            sb.AppendLine("using Generated.Client;");
            sb.AppendLine("using Generated.Models;");
            sb.AppendLine("using System.Net.Http;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("namespace GeneratedProject.Tests");
            sb.AppendLine("{");
            sb.AppendLine("    public class IntegrationTests");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly ApiClient _client;");
            sb.AppendLine();
            sb.AppendLine("        public IntegrationTests()");
            sb.AppendLine("        {");
            string basePath = "/v2/";
            if (doc?.Servers != null && doc.Servers.Count > 0 && !string.IsNullOrEmpty(doc.Servers[0].Url))
            {
                var url = doc.Servers[0].Url;
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    basePath = uri.AbsolutePath;
                }
                else
                {
                    basePath = url;
                }
            }
            else if (doc != null && !string.IsNullOrEmpty(doc.BasePath))
            {
                basePath = doc.BasePath;
            }

            if (!basePath.StartsWith("/")) basePath = "/" + basePath;
            if (!basePath.EndsWith("/")) basePath += "/";

            var fullUrl = "http://localhost:8080" + basePath;

            sb.AppendLine($"            var httpClient = new HttpClient {{ BaseAddress = new Uri(\"{fullUrl}\") }};");
            sb.AppendLine("            httpClient.DefaultRequestHeaders.Add(\"api_key\", \"special-key\");");
            sb.AppendLine("            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(\"Bearer\", \"special-key\");");
            sb.AppendLine("            _client = new ApiClient(httpClient);");
            sb.AppendLine("        }");

            if (doc?.Paths != null)
            {
                foreach (var pathKvp in doc.Paths)
                {
                    var route = pathKvp.Key;
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

                        var methodName = operation.OperationId ?? $"{httpMethod}{route.Replace("/", "").Replace("{", "").Replace("}", "")}Async";
                        if (!methodName.EndsWith("Async")) methodName += "Async";

                        var testMethodName = "Test" + methodName;

                        sb.AppendLine();
                        sb.AppendLine("        [Fact]");
                        sb.AppendLine($"        public async Task {testMethodName}()");
                        sb.AppendLine("        {");

                        var args = new List<string>();
                        if (operation.Parameters != null)
                        {
                            foreach (var p in operation.Parameters)
                            {
                                string dummyValue = "\"string\"";
                                if (p.Schema?.Type == "integer") dummyValue = "1";
                                else if (p.Schema?.Type == "number") dummyValue = "123.45";
                                else if (p.Schema?.Type == "boolean") dummyValue = "true";
                                else if (p.Schema?.Type == "array" && p.Schema?.Items?.Ref != null)
                                {
                                    var refName = p.Schema.Items.Ref.Split('/').Last();
                                    dummyValue = $"new List<{refName}>()";
                                }
                                else if (p.Schema?.Type == "array" && p.Schema?.Items?.Type != null)
                                {
                                    var itemType = p.Schema.Items.Type == "integer" ? "int" : (p.Schema.Items.Type == "number" ? "double" : (p.Schema.Items.Type == "boolean" ? "bool" : "string"));
                                    dummyValue = $"new List<{itemType}>()";
                                }
                                else if (p.Schema?.Ref != null)
                                {
                                    var refName = p.Schema.Ref.Split('/').Last();
                                    dummyValue = $"new {refName}()";
                                }
                                if (p.Name == "api_key") dummyValue = "\"special-key\"";
                                args.Add(dummyValue);
                            }
                        }

                        if (operation.RequestBody?.Content != null && operation.RequestBody.Content.ContainsKey("application/json"))
                        {
                            var schema = operation.RequestBody.Content["application/json"].Schema;
                            string dummyValue = "\"string\"";
                            if (schema != null)
                            {
                                if (!string.IsNullOrEmpty(schema.Ref))
                                {
                                    var refName = schema.Ref.Split('/').Last();
                                    dummyValue = $"new {refName}()";
                                }
                                else if (schema.Type == "integer") dummyValue = "1";
                                else if (schema.Type == "number") dummyValue = "123.45";
                                else if (schema.Type == "boolean") dummyValue = "true";
                                else if (schema.Type == "array" && schema.Items?.Ref != null)
                                {
                                    var refName = schema.Items.Ref.Split('/').Last();
                                    dummyValue = $"new List<{refName}>()";
                                }
                                else if (schema.Type == "array" && schema.Items?.Type != null)
                                {
                                    var itemType = schema.Items.Type == "integer" ? "int" : (schema.Items.Type == "number" ? "double" : (schema.Items.Type == "boolean" ? "bool" : "string"));
                                    dummyValue = $"new List<{itemType}>()";
                                }
                            }
                            args.Add(dummyValue);
                        }

                        var argsString = string.Join(", ", args);

                        sb.AppendLine($"            var response = await _client.{methodName}({argsString});");

                        // We shouldn't assert NotNull.
                        // Actually, the client method will EnsureSuccessStatusCode().
                        // The chaos test asserts that if server returns 500 or invalid schema, the test FAILS.
                        // Since EnsureSuccessStatusCode() and JsonSerializer throw exceptions, the test will naturally fail on bad responses.
                        // Therefore, we don't need any catch blocks. We WANT the test to fail.

                        sb.AppendLine("        }");
                    }
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
