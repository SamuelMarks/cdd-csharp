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
    class Program
    {
        static int Main(string[] args)
        {
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
                    File.WriteAllText(args[2], new OpenApiEmitter().EmitJson(doc));
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

            var inputPaths = new List<string>();
            string outputPath = Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? Directory.GetCurrentDirectory();
            bool noGithubActions = Environment.GetEnvironmentVariable("NO_GITHUB_ACTIONS") == "true";
            bool noInstallablePackage = Environment.GetEnvironmentVariable("NO_INSTALLABLE_PACKAGE") == "true";

            string? inputEnv = Environment.GetEnvironmentVariable("INPUT");
            if (!string.IsNullOrEmpty(inputEnv)) inputPaths.Add(inputEnv);
            
            string? inputDirEnv = Environment.GetEnvironmentVariable("INPUT_DIR");
            if (!string.IsNullOrEmpty(inputDirEnv) && Directory.Exists(inputDirEnv))
            {
                inputPaths.AddRange(Directory.GetFiles(inputDirEnv, "*.json", SearchOption.AllDirectories));
                inputPaths.AddRange(Directory.GetFiles(inputDirEnv, "*.yaml", SearchOption.AllDirectories));
            }

            for (int i = startIndex; i < args.Length; i++)
            {
                if (args[i] == "-i" && i + 1 < args.Length)
                {
                    inputPaths.Add(args[++i]);
                }
                else if (args[i] == "--input-dir" && i + 1 < args.Length)
                {
                    var dir = args[++i];
                    if (Directory.Exists(dir))
                    {
                        inputPaths.AddRange(Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories));
                        inputPaths.AddRange(Directory.GetFiles(dir, "*.yaml", SearchOption.AllDirectories));
                    }
                }
                else if (args[i] == "-o" && i + 1 < args.Length)
                {
                    outputPath = args[++i];
                }
                else if (args[i] == "--no-github-actions")
                {
                    noGithubActions = true;
                }
                else if (args[i] == "--no-installable-package")
                {
                    noInstallablePackage = true;
                }
            }

            if (!inputPaths.Any())
            {
                return Error("Usage: cdd-csharp from_openapi [to_sdk|to_sdk_cli|to_server] -i <spec.json> | --input-dir <dir> [-o <output-dir>]");
            }

            int res = RunFromOpenApi(inputPaths, outputPath, type);
            if (res == 0)
            {
                if (!noGithubActions)
                {
                    var ghDir = Path.Combine(outputPath, ".github", "workflows");
                    Directory.CreateDirectory(ghDir);
                    File.WriteAllText(Path.Combine(ghDir, "ci.yml"), "name: CI\n\non: [push]\n\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n    - uses: actions/checkout@v3\n");
                }
                if (!noInstallablePackage)
                {
                    var projContent = "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n";
                    if (type == GenerateType.All || type == GenerateType.Server)
                    {
                        projContent += "  <ItemGroup>\n    <PackageReference Include=\"Microsoft.EntityFrameworkCore\" Version=\"9.0.0\" />\n  </ItemGroup>\n";
                    }
                    projContent += "</Project>";
                    File.WriteAllText(Path.Combine(outputPath, "GeneratedProject.csproj"), projContent);
                }
            }
            return res;
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
                return Error("Usage: cdd-csharp to_openapi -i <csharp-dir-or-file> [-o <output.json>]");
            }

            return RunToOpenApi(inputPath, outputPath);
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
                else if (args[i] == "-o" && i + 1 < args.Length)
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
                return Error("Usage: cdd-csharp to_docs_json -i <spec.json> [-o docs.json] [--no-imports] [--no-wrapping]");
            }

            string jsonContent;
            if (inputPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || inputPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                using var client = new System.Net.Http.HttpClient();
                jsonContent = client.GetStringAsync(inputPath).GetAwaiter().GetResult();
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
                File.WriteAllText(outputPath, outputJson);
            }
            else
            {
                Console.WriteLine(outputJson);
            }
            return 0;
        }

        private static int RunFromOpenApi(IEnumerable<string> inputPaths, string outputDir, GenerateType type)
        {
            var parser = new OpenApiParser();
            foreach (var inputPath in inputPaths)
            {
                if (!File.Exists(inputPath)) return Error($"Error: Input '{inputPath}' not found.");
                
                var doc = parser.ParseJson(File.ReadAllText(inputPath));
                var codes = CodeGenerator.Generate(doc, "Generated", type);
                
                foreach (var code in codes)
                {
                    var fullPath = Path.Combine(outputDir, code.FileName);
                    var dir = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.WriteAllText(fullPath, code.Code);
                }
            }
            
            Console.WriteLine($"Successfully generated C# code in '{outputDir}'.");
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
            
            File.WriteAllText(outputPath, new OpenApiEmitter().EmitJson(doc));
            Console.WriteLine($"Successfully generated spec at '{outputPath}'.");
            return 0;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  cdd-csharp --help");
            Console.WriteLine("  cdd-csharp --version");
            Console.WriteLine("  cdd-csharp from_openapi [to_sdk|to_sdk_cli|to_server] -i <spec.json> | --input-dir <dir> [-o <output-dir>]");
            Console.WriteLine("  cdd-csharp to_openapi -i <csharp-dir-or-file> [-o <output.json>]");
            Console.WriteLine("  cdd-csharp to_docs_json --no-imports --no-wrapping -i <spec.json> [-o docs.json]");
            Console.WriteLine("  cdd-csharp serve_json_rpc --port <port> --listen <ip>");
        }

        private static int Error(string message)
        {
            Console.Error.WriteLine(message);
            return 1;
        }
    }
}
