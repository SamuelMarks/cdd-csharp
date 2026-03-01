using System;
using System.IO;
using System.Linq;
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
                    Console.WriteLine("1.0.0"); // Or use reflection to get version
                    return 0;
                }

                // Standardized required subcommands
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
                
                // Legacy / Additional subcommands
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
                else if (command == "generate-spec" && !string.IsNullOrEmpty(inputPath))
                {
                    if (args.Length < 3) return Error("generate-spec requires an output file path.");
                    return RunToOpenApi(inputPath, args[2]);
                }
                else if (command == "generate-code" && !string.IsNullOrEmpty(inputPath))
                {
                    if (args.Length < 3) return Error("generate-code requires an output directory path.");
                    return RunFromOpenApi(inputPath, args[2]);
                }
            }
            catch (Exception ex)
            {
                return Error($"Operation failed: {ex.Message}");
            }

            return Error($"Unknown or incomplete command: {command}");
        }

        private static int HandleFromOpenApi(string[] args)
        {
            string inputPath = string.Empty;
            string outputPath = "./out";

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-i" && i + 1 < args.Length)
                {
                    inputPath = args[++i];
                }
                else if (args[i] == "-o" && i + 1 < args.Length)
                {
                    outputPath = args[++i];
                }
            }

            if (string.IsNullOrEmpty(inputPath))
            {
                return Error("Usage: cdd-csharp from_openapi -i <spec.json> [-o <output-dir>]");
            }

            return RunFromOpenApi(inputPath, outputPath);
        }

        private static int HandleToOpenApi(string[] args)
        {
            string inputPath = string.Empty;
            string outputPath = "openapi.json";

            for (int i = 1; i < args.Length; i++)
            {
                if ((args[i] == "-i" || args[i] == "-f") && i + 1 < args.Length)
                {
                    inputPath = args[++i];
                }
                else if (args[i] == "-o" && i + 1 < args.Length)
                {
                    outputPath = args[++i];
                }
            }

            if (string.IsNullOrEmpty(inputPath))
            {
                return Error("Usage: cdd-csharp to_openapi -f <csharp-dir-or-file> [-o <output.json>]");
            }

            return RunToOpenApi(inputPath, outputPath);
        }

        private static int HandleToDocsJson(string[] args)
        {
            string inputPath = string.Empty;
            bool noImports = false;
            bool noWrapping = false;

            for (int i = 1; i < args.Length; i++)
            {
                if ((args[i] == "-i" || args[i] == "--input") && i + 1 < args.Length)
                {
                    inputPath = args[++i];
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
                return Error("Usage: cdd-csharp to_docs_json -i <spec.json> [--no-imports] [--no-wrapping]");
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
            Console.WriteLine(outputJson);
            return 0;
        }

        private static int RunFromOpenApi(string inputPath, string outputDir)
        {
            if (!File.Exists(inputPath)) return Error($"Error: Input '{inputPath}' not found.");
            
            var doc = new OpenApiParser().ParseJson(File.ReadAllText(inputPath));
            var codes = CodeGenerator.Generate(doc);
            
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
            Console.WriteLine("  cdd-csharp from_openapi -i <spec.json> [-o <output-dir>]");
            Console.WriteLine("  cdd-csharp to_openapi -f <csharp-dir-or-file> [-o <output.json>]");
            Console.WriteLine("  cdd-csharp to_docs_json --no-imports --no-wrapping -i <spec.json>");
            Console.WriteLine("");
            Console.WriteLine("Additional Commands:");
            Console.WriteLine("  cdd-csharp parse <file.json>");
            Console.WriteLine("  cdd-csharp emit <file.json> <output.json>");
        }

        private static int Error(string message)
        {
            Console.Error.WriteLine(message);
            return 1;
        }
    }
}
