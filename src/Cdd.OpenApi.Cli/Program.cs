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
using Cdd.OpenApi;

namespace Cdd.OpenApi.Cli
{
    internal class Program
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private static string[] LoadFallbackArgs()
        {
            if (System.IO.File.Exists("/.cdd_args")) return System.IO.File.ReadAllLines("/.cdd_args");
            if (System.IO.File.Exists(".cdd_args")) return System.IO.File.ReadAllLines(".cdd_args");
            var cddCommand = Environment.GetEnvironmentVariable("CDD_COMMAND");
            var cddArgs = Environment.GetEnvironmentVariable("CDD_ARGS");
            if (!string.IsNullOrEmpty(cddArgs)) return cddArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!string.IsNullOrEmpty(cddCommand)) return new[] { cddCommand };
            return Array.Empty<string>();
        }

        internal static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                args = LoadFallbackArgs();
            }


            if (args.Length < 1)
            {
                PrintUsage();
                return 1;
            }

            var command = args[0].ToLowerInvariant();

            try
            {
                if (command == "--help" || command == "-h" || command == "help")
                {
                    PrintUsage();
                    return 0;
                }
                else if (command == "--version" || command == "-v" || command == "version")
                {
                    Console.WriteLine("0.0.1");
                    return 0;
                }

                if (command == "from_openapi")
                {
                    return HandleFromOpenApi(args);
                }
                else if (command == "to_openapi")
                {
                    return HandleToOpenApi(args);
                }
                else if (command == "to_docs_json")
                {
                    return HandleToDocsJson(args);
                }
                else if (command == "server_json_rpc")
                {
                    return HandleServerJsonRpc(args);
                }

                var inputPath = args.Length > 1 ? args[1] : string.Empty;

                if (command == "parse" && !string.IsNullOrEmpty(inputPath))
                {
                    if (!File.Exists(inputPath)) return Error($"Error: Input '{inputPath}' not found.");
                    var doc = new OpenApiParser().ParseJson(File.ReadAllText(inputPath));
                    Console.WriteLine($"Successfully parsed '{inputPath}'.");
                    Console.WriteLine($"Title: {doc.Info?.Title}");
                    Console.WriteLine($"Version: {doc.Info?.Version}");
                    Console.WriteLine($"Paths count: {doc.Paths?.Count ?? 0}");
                    return 0;
                }
                else if (command == "emit" && !string.IsNullOrEmpty(inputPath))
                {
                    if (args.Length < 3) return Error("Emit requires an output file path.");
                    if (!File.Exists(inputPath)) return Error($"Error: Input '{inputPath}' not found.");
                    var doc = new OpenApiParser().ParseJson(File.ReadAllText(inputPath));
                    File.WriteAllBytes(args[2], System.Text.Encoding.UTF8.GetBytes(new OpenApiEmitter().EmitJson(doc)));
                    Console.WriteLine($"Successfully emitted to '{args[2]}'.");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                return Error($"Operation failed: {ex.Message}");
            }

            return Error($"Unknown or incomplete command: {command}");
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private static int HandleServerJsonRpc(string[] args)
        {
            string portStr = Environment.GetEnvironmentVariable("PORT") ?? "8082";
            string listen = Environment.GetEnvironmentVariable("LISTEN") ?? "0.0.0.0";

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--port" && i + 1 < args.Length)
                {
                    portStr = args[++i];
                }
                else if (args[i] == "--listen" && i + 1 < args.Length)
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
                        var res = new JsonObject { ["jsonrpc"] = "2.0", ["id"] = idNode?.DeepClone(), ["result"] = "0.0.1" };
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
                        if (method == "from_openapi") retCode = HandleFromOpenApi(invokeArgs.ToArray());
                        else if (method == "to_openapi") retCode = HandleToOpenApi(invokeArgs.ToArray());
                        else if (method == "to_docs_json") retCode = HandleToDocsJson(invokeArgs.ToArray());

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

        private static int HandleFromOpenApi(string[] args)
        {
            var subCommand = args.Length > 1 ? args[1].ToLowerInvariant() : "";
            GenerateType type = GenerateType.All;
            int startIndex = 1;

            if (subCommand == "to_sdk") { type = GenerateType.Sdk; startIndex = 2; }
            else if (subCommand == "to_sdk_cli") { type = GenerateType.SdkCli; startIndex = 2; }
            else if (subCommand == "to_server") { type = GenerateType.Server; startIndex = 2; }

            var config = new CddConfig();
            config.OutputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? Directory.GetCurrentDirectory();
            config.NoGithubActions = Environment.GetEnvironmentVariable("NO_GITHUB_ACTIONS") == "true";
            config.NoInstallablePackage = Environment.GetEnvironmentVariable("NO_INSTALLABLE_PACKAGE") == "true";
            config.CreateComposableTestsAndMocks = Environment.GetEnvironmentVariable("CREATE_COMPOSABLE_TESTS_AND_MOCKS") == "true";

            string? inputEnv = Environment.GetEnvironmentVariable("INPUT");
            if (!string.IsNullOrEmpty(inputEnv)) config.InputPath = inputEnv;

            string? inputDirEnv = Environment.GetEnvironmentVariable("INPUT_DIR");
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
            }

            if (string.IsNullOrEmpty(config.InputPath) && string.IsNullOrEmpty(config.InputDir) && !config.InputPaths.Any())
            {
                return Error("Usage: cdd-csharp from_openapi [to_sdk|to_sdk_cli|to_server] -i|--input <spec.json> | --input-dir <dir> [-o|--output <output-dir>] [--no-github-actions] [--no-installable-package] [--tests]");
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
                return Error($"Operation failed: {ex.Message}");
            }
        }

        private static int HandleToOpenApi(string[] args)
        {
            string inputPath = Environment.GetEnvironmentVariable("INPUT_FILE") ?? string.Empty;
            string outputPath = Environment.GetEnvironmentVariable("OUTPUT_FILE") ?? "openapi.json";

            for (int i = 1; i < args.Length; i++)
            {
                if ((args[i] == "-i" || args[i] == "--input") && i + 1 < args.Length)
                {
                    inputPath = args[++i];
                }
                else if ((args[i] == "-o" || args[i] == "--output") && i + 1 < args.Length)
                {
                    outputPath = args[++i];
                }
            }

            if (string.IsNullOrEmpty(inputPath))
            {
                return Error("Usage: cdd-csharp to_openapi -i|--input <csharp-dir-or-file> [-o|--output <output.json>]");
            }

            return RunToOpenApi(inputPath, outputPath);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

        private static int HandleToDocsJson(string[] args)
        {
            string inputPath = Environment.GetEnvironmentVariable("INPUT_FILE") ?? string.Empty;
            string outputPath = Environment.GetEnvironmentVariable("OUTPUT_FILE") ?? string.Empty;
            bool noImports = Environment.GetEnvironmentVariable("NO_IMPORTS") == "true";
            bool noWrapping = Environment.GetEnvironmentVariable("NO_WRAPPING") == "true";

            for (int i = 1; i < args.Length; i++)
            {
                if ((args[i] == "-i" || args[i] == "--input") && i + 1 < args.Length)
                {
                    inputPath = args[++i];
                }
                else if ((args[i] == "-o" || args[i] == "--output") && i + 1 < args.Length)
                {
                    outputPath = args[++i];
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
                return Error("Usage: cdd-csharp to_docs_json -i|--input <spec.json> [-o|--output docs.json] [--no-imports] [--no-wrapping]");
            }

            string jsonContent;
            if (inputPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || inputPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                jsonContent = FetchHttpContent(inputPath);
            }
            else
            {
                if (!File.Exists(inputPath)) return Error($"Error: Input '{inputPath}' not found.");
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


        private static int RunToOpenApi(string inputPath, string outputPath)
        {
            if (!File.Exists(inputPath) && !Directory.Exists(inputPath))
            {
                return Error($"Error: Input '{inputPath}' not found.");
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

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  cdd-csharp --help");
            Console.WriteLine("  cdd-csharp --version");
            Console.WriteLine("  cdd-csharp from_openapi [to_sdk|to_sdk_cli|to_server] -i|--input <spec.json> | --input-dir <dir> [-o|--output <output-dir>] [--no-github-actions] [--no-installable-package] [--tests]");
            Console.WriteLine("  cdd-csharp to_openapi -i|--input <csharp-dir-or-file> [-o|--output <output.json>]");
            Console.WriteLine("  cdd-csharp to_docs_json --no-imports --no-wrapping -i|--input <spec.json> [-o|--output docs.json]");
            Console.WriteLine("  cdd-csharp serve_json_rpc --port <port> --listen <ip>");
        }

        private static int Error(string message)
        {
            Console.Error.WriteLine(message);
            return 1;
        }
    }
}
