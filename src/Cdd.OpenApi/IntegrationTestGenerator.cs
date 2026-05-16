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
            sb.AppendLine("            var httpClient = new HttpClient { BaseAddress = new Uri(\"http://localhost:8080/v2/\") };");
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
                        { "Trace", pathItem.Trace }
                    };

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
                                if (p.Schema?.Type == "integer") dummyValue = "123";
                                else if (p.Schema?.Type == "number") dummyValue = "123.45";
                                else if (p.Schema?.Type == "boolean") dummyValue = "true";
                                else if (p.Schema?.Items?.Ref != null)
                                {
                                    var refName = p.Schema.Items.Ref.Split('/').Last();
                                    dummyValue = $"new List<{refName}>()";
                                }
                                else if (p.Schema?.Ref != null)
                                {
                                    var refName = p.Schema.Ref.Split('/').Last();
                                    dummyValue = $"new {refName}()";
                                }
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
                                else if (schema.Type == "integer") dummyValue = "123";
                                else if (schema.Type == "number") dummyValue = "123.45";
                                else if (schema.Type == "boolean") dummyValue = "true";
                                else if (schema.Items?.Ref != null)
                                {
                                    var refName = schema.Items.Ref.Split('/').Last();
                                    dummyValue = $"new List<{refName}>()";
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
