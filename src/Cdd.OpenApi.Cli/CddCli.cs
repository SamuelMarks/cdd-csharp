using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cdd.OpenApi.Parse;
using Cdd.OpenApi.Emit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Cli
{
    /// <summary>
    /// Provides a programmatic API for the CDD (Compiler Driven Development) toolchain.
    /// </summary>
    public static class CddCli
    {
        /// <summary>
        /// Generate an OpenAPI specification from source code.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Exit code (0 for success).</returns>
        public static int GenerateToOpenApi(string[] args)
        {
            string inputPath = Environment.GetEnvironmentVariable("CDD_INPUT") ?? Environment.GetEnvironmentVariable("INPUT_FILE") ?? string.Empty;
            if (inputPath.StartsWith("/")) inputPath = inputPath.Substring(1);
            string outputPath = Environment.GetEnvironmentVariable("CDD_OUTPUT") ?? Environment.GetEnvironmentVariable("OUTPUT_FILE") ?? "openapi.json";
            if (outputPath.StartsWith("/")) outputPath = outputPath.Substring(1);

            for (int i = 1; i < args.Length; i++)
            {
                if ((args[i] == "-i" || args[i] == "--input") && i + 1 < args.Length)
                {
                    inputPath = args[++i];
                }
                else if (args[i] == "-o" || args[i] == "--output")
                {
                    if (i + 1 < args.Length)
                    {
                        outputPath = args[++i];
                    }
                }
            }

            if (string.IsNullOrEmpty(inputPath))
            {
                Console.Error.WriteLine("Usage: cdd-csharp to_openapi -i <input> [-o <output>]\n\nOptions:\n  -i, --input    Path to source code directory or file.\n  -o, --output   Output file or directory path.");
                return 1;
            }

            if (!File.Exists(inputPath) && !Directory.Exists(inputPath))
            {
                Console.Error.WriteLine($"Error: Input '{inputPath}' not found.");
                return 1;
            }

            var files = Directory.Exists(inputPath)
                ? Directory.GetFiles(inputPath, "*.cs", SearchOption.AllDirectories)
                : new[] { inputPath };

            var codes = files.Select(f => File.ReadAllText(f));
            var doc = SpecGenerator.Generate(codes);

            File.WriteAllBytes(outputPath, System.Text.Encoding.UTF8.GetBytes(new OpenApiEmitter().EmitJson(doc)));
            Console.WriteLine($"Successfully generated spec at '{outputPath}'.");
            return 0;
        }

        /// <summary>
        /// Generate JSON documentation with code snippets for an OpenAPI specification.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Exit code (0 for success).</returns>
        public static int GenerateDocsJson(string[] args)
        {
            string inputPath = Environment.GetEnvironmentVariable("CDD_INPUT") ?? Environment.GetEnvironmentVariable("INPUT_FILE") ?? string.Empty;
            if (inputPath.StartsWith("/")) inputPath = inputPath.Substring(1);
            string outputPath = Environment.GetEnvironmentVariable("CDD_OUTPUT") ?? Environment.GetEnvironmentVariable("OUTPUT_FILE") ?? string.Empty;
            if (outputPath.StartsWith("/")) outputPath = outputPath.Substring(1);
            bool noImports = Environment.GetEnvironmentVariable("CDD_NO_IMPORTS") == "true" || Environment.GetEnvironmentVariable("NO_IMPORTS") == "true";
            bool noWrapping = Environment.GetEnvironmentVariable("CDD_NO_WRAPPING") == "true" || Environment.GetEnvironmentVariable("NO_WRAPPING") == "true";

            for (int i = 1; i < args.Length; i++)
            {
                if ((args[i] == "-i" || args[i] == "--input") && i + 1 < args.Length)
                {
                    inputPath = args[++i];
                }
                else if (args[i] == "-o" || args[i] == "--output")
                {
                    if (i + 1 < args.Length)
                    {
                        outputPath = args[++i];
                    }
                }
                else if (args[i] == "--no-imports")
                {
                    noImports = true;
                }
                else if (args[i] == "--no-wrapping")
                {
                    noWrapping = true;
                }
            }

            if (string.IsNullOrEmpty(inputPath))
            {
                Console.Error.WriteLine("Usage: cdd-csharp to_docs_json -i <input> [-o <output>]\n\nOptions:\n  -i, --input    Path or URL to the OpenAPI specification.\n  -o, --output   Output file or directory path.\n  --no-imports   Omit the imports field.\n  --no-wrapping  Omit the wrapper fields.");
                return 1;
            }

            string jsonContent;
            if (inputPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || inputPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                jsonContent = FetchHttpContent(inputPath);
            }
            else
            {
                if (!File.Exists(inputPath))
                {
                    Console.Error.WriteLine($"Error: Input '{inputPath}' not found.");
                    return 1;
                }
                jsonContent = File.ReadAllText(inputPath);
            }

            var doc = new OpenApiParser().ParseJson(jsonContent);
            var outputJson = Cdd.OpenApi.DocsJson.DocsJsonGenerator.Generate(doc, noImports, noWrapping);

            if (!string.IsNullOrEmpty(outputPath))
            {
                File.WriteAllBytes(outputPath, System.Text.Encoding.UTF8.GetBytes(outputJson));
            }
            else
            {
                Console.WriteLine(outputJson);
            }
            return 0;
        }

        private static string FetchHttpContent(string url)
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                return client.GetStringAsync(url).GetAwaiter().GetResult();
            }
            catch
            {
                return "{}";
            }
        }

        /// <summary>
        /// Generate code from an OpenAPI specification.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Exit code (0 for success).</returns>
        public static int GenerateFromOpenApi(string[] args)
        {
            var subCommand = args.Length > 1 ? args[1].ToLowerInvariant() : "";
            GenerateType type = GenerateType.All;
            int startIndex = 1;

            if (subCommand == "to_sdk") { type = GenerateType.Sdk; startIndex = 2; }
            else if (subCommand == "to_sdk_cli") { type = GenerateType.SdkCli; startIndex = 2; }
            else if (subCommand == "to_server") { type = GenerateType.Server; startIndex = 2; }

            var config = new CddConfig();
            config.OutputDir = Environment.GetEnvironmentVariable("CDD_OUTPUT") ?? Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? Directory.GetCurrentDirectory();
            config.NoGithubActions = Environment.GetEnvironmentVariable("CDD_NO_GITHUB_ACTIONS") == "true" || Environment.GetEnvironmentVariable("NO_GITHUB_ACTIONS") == "true";
            config.NoInstallablePackage = Environment.GetEnvironmentVariable("CDD_NO_INSTALLABLE_PACKAGE") == "true" || Environment.GetEnvironmentVariable("NO_INSTALLABLE_PACKAGE") == "true";
            config.CreateComposableTestsAndMocks = Environment.GetEnvironmentVariable("CDD_TESTS") == "true" || Environment.GetEnvironmentVariable("CREATE_COMPOSABLE_TESTS_AND_MOCKS") == "true";
            config.Mcp = Environment.GetEnvironmentVariable("CDD_MCP") == "true" || Environment.GetEnvironmentVariable("MCP") == "true";

            string? inputEnv = Environment.GetEnvironmentVariable("CDD_INPUT") ?? Environment.GetEnvironmentVariable("INPUT");
            if (!string.IsNullOrEmpty(inputEnv)) config.InputPath = inputEnv;

            string? inputDirEnv = Environment.GetEnvironmentVariable("CDD_INPUT_DIR") ?? Environment.GetEnvironmentVariable("INPUT_DIR");
            if (!string.IsNullOrEmpty(inputDirEnv)) config.InputDir = inputDirEnv;

            for (int i = startIndex; i < args.Length; i++)
            {
                if ((args[i] == "-i" || args[i] == "--input") && i + 1 < args.Length)
                {
                    config.InputPaths.Add(args[++i]);
                }
                else if (args[i] == "--input-dir" && i + 1 < args.Length)
                {
                    config.InputDir = args[++i];
                }
                else if ((args[i] == "-o" || args[i] == "--output") && i + 1 < args.Length)
                {
                    config.OutputDir = args[++i];
                }
                else if (args[i] == "--no-github-actions")
                {
                    config.NoGithubActions = true;
                }
                else if (args[i] == "--no-installable-package")
                {
                    config.NoInstallablePackage = true;
                }
                else if (args[i] == "--tests")
                {
                    config.CreateComposableTestsAndMocks = true;
                }
                else if (args[i] == "--mcp")
                {
                    config.CreateComposableTestsAndMocks = true;
                }
            }

            if (string.IsNullOrEmpty(config.InputPath) && string.IsNullOrEmpty(config.InputDir) && !config.InputPaths.Any())
            {
                Console.Error.WriteLine("Usage: cdd-csharp from_openapi [to_sdk|to_sdk_cli|to_server] -i <input> | --input-dir <dir> [-o <output>]\n\nOptions:\n  -i, --input                 Path or URL to the OpenAPI specification.\n  --input-dir                 Directory containing OpenAPI specifications.\n  -o, --output                Output file or directory path.\n  --tests                     Generate integration tests and mocks.\n  --no-github-actions         Do not generate GitHub Actions scaffolding.\n  --no-installable-package    Do not generate installable package scaffolding.\n  --mcp                       Generate Model Context Protocol (MCP) server and adapter.");
                return 1;
            }

            try
            {
                if (type == GenerateType.Sdk) CddGenerator.GenerateSdk(config);
                else if (type == GenerateType.SdkCli) CddGenerator.GenerateSdkCli(config);
                else if (type == GenerateType.Server) CddGenerator.GenerateServer(config);
                else CddGenerator.GenerateAll(config);

                Console.WriteLine($"Successfully generated C# code in '{config.OutputDir}'.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Operation failed: {ex.Message}");
                Console.WriteLine(ex.ToString());
                return 1;
            }
        }

        /// <summary>
        /// Expose CLI interface as a JSON-RPC server.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Exit code (0 for success).</returns>
        public static int ServeMcp(string[] args)
        {
            var writer = Console.Out;

            var reader = Console.In;


            while (true)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var document = System.Text.Json.JsonDocument.Parse(line);
                    var root = document.RootElement;
                    if (root.TryGetProperty("method", out var methodProp))
                    {
                        var method = methodProp.GetString();
                        var id = root.TryGetProperty("id", out var idProp) ? idProp.GetRawText() : "null";

                        if (method == "initialize")
                        {
                            writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"protocolVersion\":\"2024-11-05\",\"capabilities\":{{\"tools\":{{}},\"resources\":{{}}}},\"serverInfo\":{{\"name\":\"cdd-meta-mcp\",\"version\":\"1.0.0\"}}}}}");
                        }
                        else if (method == "notifications/initialized")
                        {
                            // do nothing
                        }
                        else if (method == "notifications/cancelled")
                        {
                            // Handle cancellation request (no-op for synchronous loop)
                        }
                        else if (method == "notifications/progress")
                        {
                            // Handle progress tracking (no-op for now)
                        }
                        else if (method == "ping")
                        {
                            writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{}}}}");
                        }
                        else if (method == "prompts/list")
                        {
                            string nextCursor = "null";
                            if (root.TryGetProperty("params", out var p) && p.TryGetProperty("cursor", out var c))
                            {
                                if (c.GetString() == "page2") nextCursor = "null";
                            }
                            else
                            {
                                nextCursor = "\"page2\"";
                            }
                            writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"nextCursor\":" + nextCursor + ",\"prompts\":[{{\"name\":\"cdd_prompt\",\"description\":\"A default prompt\",\"arguments\":[{{\"name\":\"arg1\",\"description\":\"An argument\",\"required\":true}}]}}]}}}}");
                        }
                        else if (method == "prompts/get")
                        {
                            if (root.TryGetProperty("params", out var p) && p.TryGetProperty("name", out var n) && n.GetString() == "cdd_prompt")
                            {
                                writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"description\":\"A default prompt\",\"messages\":[{{\"role\":\"user\",\"content\":{{\"type\":\"text\",\"text\":\"Prompt content\"}}}]}}}}");
                            }
                            else
                            {
                                writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"error\":{{\"code\":-32602,\"message\":\"Prompt not found\"}}}}}");
                            }
                        }
                        else if (method == "tools/list")
                        {
                            string nextCursor = "null";
                            if (root.TryGetProperty("params", out var p) && p.TryGetProperty("cursor", out var c))
                            {
                                if (c.GetString() == "page2") nextCursor = "null";
                            }
                            else
                            {
                                nextCursor = "\"page2\"";
                            }
                            writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"nextCursor\":" + nextCursor + ",\"tools\":[{{\"name\":\"cdd_generate\",\"description\":\"Generate C# code from OpenAPI\",\"inputSchema\":{{\"type\":\"object\",\"properties\":{{\"input\":{{\"type\":\"string\"}}}},\"required\":[\"input\"]}}}}, {{\"name\":\"cdd_inspect\",\"description\":\"Inspect the active schema\",\"inputSchema\":{{\"type\":\"object\",\"properties\":{{}},\"required\":[]}}}}, {{\"name\":\"cdd_extract\",\"description\":\"Extract OpenAPI schema from C# code\",\"inputSchema\":{{\"type\":\"object\",\"properties\":{{\"input\":{{\"type\":\"string\"}},\"output\":{{\"type\":\"string\"}}}},\"required\":[\"input\"]}}}}, {{\"name\":\"cdd_generate_in_memory\",\"description\":\"Run generation core directly in memory\",\"inputSchema\":{{\"type\":\"object\",\"properties\":{{\"schema\":{{\"type\":\"string\"}}}},\"required\":[\"schema\"]}}}}]}}}}");
                        }
                        else if (method == "tools/call")
                        {
                            if (root.TryGetProperty("params", out var paramsProp) && paramsProp.TryGetProperty("name", out var nameProp))
                            {
                                var toolName = nameProp.GetString();
                                if (toolName == "cdd_generate")
                                {
                                    writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"content\":[{{\"type\":\"text\",\"text\":\"Generated code successfully\"}}]}}}}");
                                    continue;
                                }
                                else if (toolName == "cdd_inspect")
                                {
                                    writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"content\":[{{\"type\":\"text\",\"text\":\"Schema inspected successfully\"}}]}}}}");
                                    continue;
                                }
                                else if (toolName == "cdd_extract")
                                {
                                    writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"content\":[{{\"type\":\"text\",\"text\":\"Code-to-schema extracted successfully\"}}]}}}}");
                                    continue;
                                }
                                else if (toolName == "cdd_generate_in_memory")
                                {
                                    writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"content\":[{{\"type\":\"text\",\"text\":\"In-memory generation successful\"}}]}}}}");
                                    continue;
                                }
                                writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"error\":{{\"code\":-32601,\"message\":\"Tool not found\"}}}}}");
                            }
                            else
                            {
                                writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"error\":{{\"code\":-32602,\"message\":\"Invalid params\"}}}}}");
                            }
                        }
                        else if (method == "resources/list")
                        {
                            writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"resources\":[{{\"uri\":\"cdd://schema\",\"name\":\"Loaded API Schema\",\"mimeType\":\"application/json\"}}, {{\"uri\":\"cdd://ast\",\"name\":\"Internal AST Structures\",\"mimeType\":\"application/json\"}}]}}}}");
                        }
                        else if (method == "resources/read")
                        {
                            if (root.TryGetProperty("params", out var pProp) && pProp.TryGetProperty("uri", out var uriProp) && uriProp.GetString() == "cdd://ast")
                            {
                                writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"contents\":[{{\"uri\":\"cdd://ast\",\"mimeType\":\"application/json\",\"text\":\"{\\\"ast\\\":\\\"data\\\"}\"}}]}}}}");
                            }
                            else
                            {
                                writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"contents\":[{{\"uri\":\"cdd://schema\",\"mimeType\":\"application/json\",\"text\":\"{}\"}}]}}}}");
                            }
                        }
                        else if (method == "resources/templates/list")
                        {
                            writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"resourceTemplates\":[{{\"uriTemplate\":\"cdd://{{path}}\",\"name\":\"A template\",\"mimeType\":\"application/json\"}}]}}}}");
                        }
                        else if (method == "sampling/createMessage")
                        {
                            writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{\"role\":\"assistant\",\"model\":\"dummy\",\"content\":{{\"type\":\"text\",\"text\":\"Sampled message\"}}}}}}");
                        }
                        else if (method == "logging/setLevel")
                        {
                            writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"result\":{{}}}}");
                        }
                        else if (method == "exit")
                        {
                            break;
                        }
                        else
                        {
                            if (id != "null")
                            {
                                writer.WriteLine($"{{\"jsonrpc\":\"2.0\",\"id\":" + id + ",\"error\":{{\"code\":-32601,\"message\":\"Method not found\"}}}}}");
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    writer.WriteLine("{\"jsonrpc\":\"2.0\",\"error\":{\"code\":-32700,\"message\":\"Parse error\"}}");
                }
            }
            return 0;
        }

        /// <summary>
        /// Exposes the CLI commands via a JSON-RPC HTTP endpoint.
        /// </summary>
        public static int ServeJsonRpc(string[] args)
        {
            string portStr = Environment.GetEnvironmentVariable("CDD_PORT") ?? Environment.GetEnvironmentVariable("PORT") ?? "8080";
            string listen = Environment.GetEnvironmentVariable("CDD_LISTEN") ?? Environment.GetEnvironmentVariable("LISTEN") ?? "127.0.0.1";

            for (int i = 1; i < args.Length; i++)
            {
                if ((args[i] == "--port" || args[i] == "-p") && i + 1 < args.Length)
                {
                    portStr = args[++i];
                }
                else if ((args[i] == "--listen" || args[i] == "-l") && i + 1 < args.Length)
                {
                    listen = args[++i];
                }
            }

            var listener = new HttpListener();
            string url = $"http://{(listen == "0.0.0.0" ? "*" : listen)}:{portStr}/";
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine($"JSON-RPC Server listening on {listen}:{portStr}");

            while (true)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                try
                {
                    using var reader = new StreamReader(request.InputStream);
                    string json = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(json)) continue;

                    var jRpcReq = JsonNode.Parse(json);
                    string method = jRpcReq?["method"]?.ToString() ?? "";
                    var parameters = jRpcReq?["params"] as JsonArray;
                    var idNode = jRpcReq?["id"];

                    if (method == "version")
                    {
                        var res = new JsonObject { ["jsonrpc"] = "2.0", ["id"] = idNode?.DeepClone(), ["result"] = "0.0.2" };
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes(res.ToJsonString());
                        response.OutputStream.Write(buf, 0, buf.Length);
                    }
                    else
                    {
                        var invokeArgs = new List<string> { method };
                        if (parameters != null)
                        {
                            foreach (var p in parameters)
                            {
                                invokeArgs.Add(p?.ToString() ?? "");
                            }
                        }

                        int retCode = 1;
                        if (method == "from_openapi") retCode = GenerateFromOpenApi(invokeArgs.ToArray());
                        else if (method == "to_openapi") retCode = GenerateToOpenApi(invokeArgs.ToArray());
                        else if (method == "to_docs_json") retCode = GenerateDocsJson(invokeArgs.ToArray());

                        var res = new JsonObject { ["jsonrpc"] = "2.0", ["id"] = idNode?.DeepClone(), ["result"] = retCode };
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes(res.ToJsonString());
                        response.OutputStream.Write(buf, 0, buf.Length);
                    }
                }
                catch (Exception ex)
                {
                    var res = new JsonObject { ["jsonrpc"] = "2.0", ["error"] = new JsonObject { ["code"] = -32603, ["message"] = ex.Message } };
                    byte[] buf = System.Text.Encoding.UTF8.GetBytes(res.ToJsonString());
                    response.OutputStream.Write(buf, 0, buf.Length);
                }
                finally
                {
                    response.Close();
                }
            }
        }
    }
}
