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
                    Console.WriteLine("0.0.3");
                    return 0;
                }

                if (args.Any(a => a == "--help" || a == "-h"))
                {
                    if (command == "from_openapi")
                        Console.WriteLine(CddCli.FromOpenApiHelp);
                    else if (command == "to_openapi")
                        Console.WriteLine(CddCli.ToOpenApiHelp);
                    else if (command == "to_docs_json")
                        Console.WriteLine(CddCli.ToDocsJsonHelp);
                    else if (command == "sync")
                        Console.WriteLine(CddCli.SyncHelp);
                    else if (command == "serve_json_rpc")
                        Console.WriteLine(CddCli.ServeJsonRpcHelp);
                    else if (command == "mcp")
                        Console.WriteLine(CddCli.McpHelp);
                    else
                        PrintUsage();
                    return 0;
                }

                if (command == "from_openapi")
                {
                    var parsed = CddCli.ParseFromOpenApiArgs(args);
                    return CddCli.FromOpenApi(parsed.type, parsed.config);
                }
                else if (command == "to_openapi")
                {
                    var parsed = CddCli.ParseToOpenApiArgs(args);
                    return CddCli.ToOpenApi(parsed.inputPath, parsed.outputPath);
                }
                else if (command == "to_docs_json")
                {
                    var parsed = CddCli.ParseDocsJsonArgs(args);
                    return CddCli.ToDocsJson(parsed.inputPath, parsed.outputPath, parsed.noImports, parsed.noWrapping);
                }
                else if (command == "sync")
                {
                    var parsedSync = CddCli.ParseSyncArgs(args);
                    return CddCli.Sync(parsedSync.truth, parsedSync.inputPath, parsedSync.outputPath);
                }
                else if (command == "serve_json_rpc")
                {
                    return CddCli.ServeJsonRpc(args);
                }
                else if (command == "mcp")
                {
                    return CddCli.ServeMcp(args);
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
                Console.WriteLine(ex.ToString()); return Error($"Operation failed: {ex.Message}");
            }

            return Error($"Error: Unknown or incomplete command: {command}");
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  cdd-csharp [subcommand] [options]");
            Console.WriteLine("\nSubcommands:");
            Console.WriteLine("  from_openapi    Generate code from an OpenAPI specification.");
            Console.WriteLine("  to_openapi      Generate an OpenAPI specification from source code.");
            Console.WriteLine("  to_docs_json    Generate JSON documentation with code snippets for an OpenAPI specification.");
            Console.WriteLine("  sync            Synchronize an OpenAPI specification with source code.");
            Console.WriteLine("  serve_json_rpc  Expose CLI interface as a JSON-RPC server.");
            Console.WriteLine("  mcp             Run the generator as an MCP server over stdio.");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("  --help, -h      Show this message");
            Console.WriteLine("  --version, -v   Show version information");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  cdd-csharp from_openapi [to_sdk|to_sdk_cli|to_server] -i|--input <spec.json> | -d|--input-dir <dir> [-o|--output <output-dir>] [--no-github-actions] [--no-installable-package] [--tests] [-m|--mcp]");
            Console.WriteLine("  cdd-csharp to_openapi -i|--input <csharp-dir-or-file> [-o|--output <output.json>]");
            Console.WriteLine("  cdd-csharp to_docs_json --no-imports --no-wrapping -i|--input <spec.json> [-o|--output docs.json]");
            Console.WriteLine("  cdd-csharp serve_json_rpc [-p|--port <port>] [-l|--listen <address>]");
        }

        private static int Error(string message)
        {
            Console.Error.WriteLine(message);
            return 1;
        }
    }
}
