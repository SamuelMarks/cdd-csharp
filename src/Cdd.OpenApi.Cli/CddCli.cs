// <copyright file="CddCli.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using Cdd.OpenApi.Emit;
using Cdd.OpenApi.Parse;

namespace Cdd.OpenApi.Cli
{
    /// <summary>
    /// Provides a programmatic API for the CDD (Compiler Driven Development) toolchain.
    /// </summary>
    public static class CddCli
    {
        /// <summary>Help text for the <c>from_openapi</c> command.</summary>
        public const string FromOpenApiHelp = CliHelpText.FromOpenApiHelp;

        /// <summary>Help text for the <c>to_openapi</c> command.</summary>
        public const string ToOpenApiHelp = CliHelpText.ToOpenApiHelp;

        /// <summary>Help text for the <c>from_arazzo</c> command.</summary>
        public const string FromArazzoHelp = CliHelpText.FromArazzoHelp;

        /// <summary>Help text for the <c>to_arazzo</c> command.</summary>
        public const string ToArazzoHelp = CliHelpText.ToArazzoHelp;

        /// <summary>Help text for the <c>to_docs_json</c> command.</summary>
        public const string ToDocsJsonHelp = CliHelpText.ToDocsJsonHelp;

        /// <summary>Help text for the <c>sync</c> command.</summary>
        public const string SyncHelp = CliHelpText.SyncHelp;

        /// <summary>Help text for the <c>serve_json_rpc</c> command.</summary>
        public const string ServeJsonRpcHelp = CliHelpText.ServeJsonRpcHelp;

        /// <summary>Help text for the <c>mcp</c> command.</summary>
        public const string McpHelp = CliHelpText.McpHelp;

        /// <summary>Help text for the <c>parse</c> command.</summary>
        public const string ParseHelp = CliHelpText.ParseHelp;

        /// <summary>Help text for the <c>emit</c> command.</summary>
        public const string EmitHelp = CliHelpText.EmitHelp;

        /// <summary>
        /// Parse arguments for generating an OpenAPI specification from source code.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Tuple of inputPath and outputPath.</returns>
        public static (string inputPath, string outputPath) ParseToOpenApiArgs(string[] args)
        {
            return CliArgumentParser.ParseToOpenApiArgs(args);
        }

        /// <summary>
        /// Generate an OpenAPI specification from source code.
        /// </summary>
        /// <param name="inputPath">Path to source code directory or file.</param>
        /// <param name="outputPath">Output file or directory path.</param>
        /// <returns>Exit code (0 for success).</returns>
        public static int ToOpenApi(string inputPath, string outputPath)
        {
            if (string.IsNullOrEmpty(inputPath))
            {
                Console.Error.WriteLine(ToOpenApiHelp);
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
        /// Parse arguments for generating JSON documentation with code snippets for an OpenAPI specification.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Tuple of inputPath, outputPath, noImports, and noWrapping.</returns>
        public static (string inputPath, string outputPath, bool noImports, bool noWrapping) ParseDocsJsonArgs(string[] args)
        {
            return CliArgumentParser.ParseDocsJsonArgs(args);
        }

        /// <summary>
        /// Generate JSON documentation with code snippets for an OpenAPI specification.
        /// </summary>
        /// <param name="inputPath">Path or URL to the OpenAPI specification.</param>
        /// <param name="outputPath">Output file or directory path.</param>
        /// <param name="noImports">Omit the imports field.</param>
        /// <param name="noWrapping">Omit the wrapper fields.</param>
        /// <returns>Exit code (0 for success).</returns>
        public static int ToDocsJson(string inputPath, string outputPath, bool noImports, bool noWrapping)
        {
            if (string.IsNullOrEmpty(inputPath))
            {
                Console.Error.WriteLine(ToDocsJsonHelp);
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

        /// <summary>
        /// Downloads the contents of a remote OpenAPI specification over HTTP or HTTPS.
        /// </summary>
        /// <param name="url">The HTTP or HTTPS URL of the OpenAPI specification.</param>
        /// <returns>The downloaded OpenAPI specification content.</returns>
        private static string FetchHttpContent(string url)
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                return client.GetStringAsync(url).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new Cdd.OpenApi.Errors.CddException(
                    Cdd.OpenApi.Errors.CddErrorCode.HttpFetchError,
                    $"Failed to fetch OpenAPI content from '{url}': {ex.Message}",
                    ex);
            }
        }

        /// <summary>
        /// Parse arguments for generating code from an OpenAPI specification.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Exit code (0 for success).</returns>
        public static (GenerateType type, CddConfig config) ParseFromOpenApiArgs(string[] args)
        {
            return CliArgumentParser.ParseFromOpenApiArgs(args);
        }

        /// <summary>
        /// Generate code from an OpenAPI specification.
        /// </summary>
        /// <param name="type">The type of code to generate.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>Exit code (0 for success).</returns>
        public static int FromOpenApi(GenerateType type, CddConfig config)
        {
            if (string.IsNullOrEmpty(config.InputPath) && string.IsNullOrEmpty(config.InputDir) && !config.InputPaths.Any())
            {
                Console.Error.WriteLine(FromOpenApiHelp);
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
        /// Parse arguments for the sync command.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A tuple containing the truth source, input path, and output path.</returns>
        public static (string truth, string inputPath, string outputPath) ParseSyncArgs(string[] args)
        {
            return CliArgumentParser.ParseSyncArgs(args);
        }

        /// <summary>
        /// Parse arguments for synchronizing an OpenAPI specification with source code.
        /// </summary>
        /// <param name="truth">Designate a single source of truth for synchronization.</param>
        /// <param name="inputPath">Path to the source files.</param>
        /// <param name="outputPath">Output file or directory path.</param>
        /// <returns>Exit code (0 for success).</returns>
        public static int Sync(string truth, string inputPath, string outputPath)
        {
            if (string.IsNullOrEmpty(truth))
            {
                Console.Error.WriteLine(SyncHelp);
                return 1;
            }

            var truthSource = CliArgumentParser.ParseSyncTruthSource(truth);
            switch (truthSource)
            {
                case SyncTruthSource.Class:
                case SyncTruthSource.Function:
                    Console.WriteLine($"Synchronizing from truth '{truth}'...");
                    return ToOpenApi(inputPath, outputPath);
                case SyncTruthSource.OpenApi:
                    Console.WriteLine($"Synchronizing from truth '{truth}'...");
                    return FromOpenApi(GenerateType.All, new CddConfig { InputPath = inputPath, OutputDir = outputPath });
                case SyncTruthSource.Arazzo:
                    Console.WriteLine($"Synchronizing from truth '{truth}'...");
                    return FromArazzo(GenerateType.All, new CddConfig { InputPath = inputPath, OutputDir = outputPath, NoGithubActions = true, NoInstallablePackage = true });
                case SyncTruthSource.Workflow:
                    Console.WriteLine($"Synchronizing from truth '{truth}'...");
                    return ToArazzo(inputPath, outputPath);
                default:
                    Console.Error.WriteLine($"Error: Truth source '{truth}' is not supported yet.");
                    return 1;
            }
        }

        /// <summary>
        /// Parses command-line arguments for the <c>to_arazzo</c> command.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A tuple of <c>inputPath</c> and <c>outputPath</c>.</returns>
        public static (string inputPath, string outputPath) ParseToArazzoArgs(string[] args)
        {
            return CliArgumentParser.ParseToOpenApiArgs(args);
        }

        /// <summary>
        /// Generates an Arazzo 1.1.0 specification from C# workflow source code.
        /// </summary>
        /// <param name="inputPath">Path to C# source code directory or file.</param>
        /// <param name="outputPath">Output Arazzo file path (<c>.json</c>, <c>.yaml</c>, or <c>.yml</c>).</param>
        /// <returns>Exit code (0 for success, 1 for error).</returns>
        public static int ToArazzo(string inputPath, string outputPath)
        {
            if (string.IsNullOrEmpty(inputPath))
            {
                Console.Error.WriteLine(ToArazzoHelp);
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
            var doc = ArazzoSpecGenerator.Generate(codes);
            var isYaml = outputPath.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || outputPath.EndsWith(".yml", StringComparison.OrdinalIgnoreCase);
            var serialized = isYaml ? Arazzo.ArazzoEmitter.EmitYaml(doc) : Arazzo.ArazzoEmitter.EmitJson(doc);

            File.WriteAllBytes(outputPath, System.Text.Encoding.UTF8.GetBytes(serialized));
            Console.WriteLine($"Successfully generated Arazzo spec at '{outputPath}'.");
            return 0;
        }

        /// <summary>
        /// Parses command-line arguments for the <c>from_arazzo</c> command.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A tuple of <see cref="GenerateType"/> and <see cref="CddConfig"/>.</returns>
        public static (GenerateType type, CddConfig config) ParseFromArazzoArgs(string[] args)
        {
            return CliArgumentParser.ParseFromOpenApiArgs(args);
        }

        /// <summary>
        /// Generates C# code from an Arazzo 1.1.0 specification.
        /// </summary>
        /// <param name="type">The target generation mode (<c>Sdk</c>, <c>SdkCli</c>, <c>Server</c>, or <c>All</c>).</param>
        /// <param name="config">The generation configuration.</param>
        /// <returns>Exit code (0 for success, 1 for error).</returns>
        public static int FromArazzo(GenerateType type, CddConfig config)
        {
            if (string.IsNullOrEmpty(config.InputPath) && string.IsNullOrEmpty(config.InputDir) && !config.InputPaths.Any())
            {
                Console.Error.WriteLine(FromArazzoHelp);
                return 1;
            }

            try
            {
                CddGenerator.GenerateFromArazzo(config, type);
                Console.WriteLine($"Successfully generated C# workflow code in '{config.OutputDir}'.");
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
        /// Runs the CDD toolchain as a Model Context Protocol (MCP) server over standard input and output.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Exit code (0 for success).</returns>
        public static int ServeMcp(string[] args)
        {
            return JsonRpcServer.ServeMcp(args);
        }

        /// <summary>
        /// Exposes the CLI commands via a JSON-RPC HTTP endpoint.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="cancellationToken">Optional cancellation token to stop the server loop.</param>
        /// <returns>Exit code (0 for success).</returns>
        [System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
        public static int ServeJsonRpc(string[] args, System.Threading.CancellationToken cancellationToken = default)
        {
#pragma warning disable CA1416
            return HttpSpecServer.ServeJsonRpc(args, cancellationToken);
#pragma warning restore CA1416
        }
    }
}
