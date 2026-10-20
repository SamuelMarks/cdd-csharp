// <copyright file="CliArgumentParser.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.IO;

namespace Cdd.OpenApi.Cli
{
    /// <summary>
    /// Enumeration of supported single-source-of-truth targets for the <c>sync</c> CLI command.
    /// </summary>
    public enum SyncTruthSource
    {
        /// <summary>Unknown or unsupported truth source.</summary>
        Unknown = 0,

        /// <summary>C# model classes (<c>class</c>).</summary>
        Class = 1,

        /// <summary>C# static functions (<c>function</c>).</summary>
        Function = 2,

        /// <summary>OpenAPI 3.2.0 specification (<c>openapi</c>).</summary>
        OpenApi = 3,

        /// <summary>Arazzo 1.1.0 workflow specification (<c>arazzo</c>).</summary>
        Arazzo = 4,

        /// <summary>C# workflow orchestrator code (<c>workflow</c>).</summary>
        Workflow = 5
    }

    /// <summary>
    /// Parses command-line arguments and environment variables into strongly-typed CLI command configurations.
    /// </summary>
    public static class CliArgumentParser
    {
        /// <summary>
        /// Canonical version string of the CDD OpenAPI CLI and library.
        /// </summary>
        public const string Version = "0.0.4";

        /// <summary>
        /// Parses a target sub-command string (<c>to_sdk</c>, <c>to_sdk_cli</c>, <c>to_server</c>, or <c>all</c>) into a <see cref="GenerateType"/> enum value.
        /// </summary>
        /// <param name="target">The target sub-command string.</param>
        /// <returns>The resolved <see cref="GenerateType"/> value.</returns>
        public static GenerateType ParseGenerateType(string target)
        {
            return target.Trim().ToLowerInvariant() switch
            {
                "to_sdk" => GenerateType.Sdk,
                "to_sdk_cli" => GenerateType.SdkCli,
                "to_server" => GenerateType.Server,
                _ => GenerateType.All
            };
        }

        /// <summary>
        /// Parses a truth source string into a strongly-typed <see cref="SyncTruthSource"/> enum value.
        /// </summary>
        /// <param name="truth">The truth source string (<c>class</c>, <c>function</c>, <c>openapi</c>, <c>arazzo</c>, or <c>workflow</c>).</param>
        /// <returns>The resolved <see cref="SyncTruthSource"/> value.</returns>
        public static SyncTruthSource ParseSyncTruthSource(string truth)
        {
            return truth.Trim().ToLowerInvariant() switch
            {
                "class" => SyncTruthSource.Class,
                "function" => SyncTruthSource.Function,
                "openapi" => SyncTruthSource.OpenApi,
                "arazzo" => SyncTruthSource.Arazzo,
                "workflow" => SyncTruthSource.Workflow,
                _ => SyncTruthSource.Unknown
            };
        }

        /// <summary>
        /// Parses arguments for generating an OpenAPI or Arazzo specification from C# source code.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A tuple of <c>(inputPath, outputPath)</c>.</returns>
        public static (string inputPath, string outputPath) ParseToOpenApiArgs(string[] args)
        {
            string inputPath = Environment.GetEnvironmentVariable("CDD_INPUT") ?? Environment.GetEnvironmentVariable("INPUT_FILE") ?? string.Empty;
            string outputPath = Environment.GetEnvironmentVariable("CDD_OUTPUT") ?? Environment.GetEnvironmentVariable("OUTPUT_FILE") ?? "openapi.json";

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

            return (inputPath, outputPath);
        }

        /// <summary>
        /// Parses arguments for generating JSON documentation with code snippets for an OpenAPI specification.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A tuple of <c>(inputPath, outputPath, noImports, noWrapping)</c>.</returns>
        public static (string inputPath, string outputPath, bool noImports, bool noWrapping) ParseDocsJsonArgs(string[] args)
        {
            string inputPath = Environment.GetEnvironmentVariable("CDD_INPUT") ?? Environment.GetEnvironmentVariable("INPUT_FILE") ?? string.Empty;
            string outputPath = Environment.GetEnvironmentVariable("CDD_OUTPUT") ?? Environment.GetEnvironmentVariable("OUTPUT_FILE") ?? string.Empty;
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

            return (inputPath, outputPath, noImports, noWrapping);
        }

        /// <summary>
        /// Parses arguments for generating C# code from an OpenAPI or Arazzo specification.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A tuple of <see cref="GenerateType"/> and <see cref="CddConfig"/>.</returns>
        public static (GenerateType type, CddConfig config) ParseFromOpenApiArgs(string[] args)
        {
            var subCommand = args.Length > 1 ? args[1].ToLowerInvariant() : string.Empty;
            GenerateType type = ParseGenerateType(subCommand);
            int startIndex = (subCommand == "to_sdk" || subCommand == "to_sdk_cli" || subCommand == "to_server") ? 2 : 1;

            var config = new CddConfig
            {
                OutputDir = Environment.GetEnvironmentVariable("CDD_OUTPUT") ?? Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? Directory.GetCurrentDirectory(),
                NoGithubActions = Environment.GetEnvironmentVariable("CDD_NO_GITHUB_ACTIONS") == "true" || Environment.GetEnvironmentVariable("NO_GITHUB_ACTIONS") == "true",
                NoInstallablePackage = Environment.GetEnvironmentVariable("CDD_NO_INSTALLABLE_PACKAGE") == "true" || Environment.GetEnvironmentVariable("NO_INSTALLABLE_PACKAGE") == "true",
                CreateComposableTestsAndMocks = Environment.GetEnvironmentVariable("CDD_TESTS") == "true" || Environment.GetEnvironmentVariable("CREATE_COMPOSABLE_TESTS_AND_MOCKS") == "true",
                Mcp = Environment.GetEnvironmentVariable("CDD_MCP") == "true" || Environment.GetEnvironmentVariable("MCP") == "true"
            };

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
                else if ((args[i] == "-d" || args[i] == "--input-dir") && i + 1 < args.Length)
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
                else if (args[i] == "-m" || args[i] == "--mcp")
                {
                    config.Mcp = true;
                }
            }

            return (type, config);
        }

        /// <summary>
        /// Parses arguments for the <c>sync</c> command.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A tuple containing the truth source, input path, and output path.</returns>
        public static (string truth, string inputPath, string outputPath) ParseSyncArgs(string[] args)
        {
            string truth = string.Empty;
            string inputPath = string.Empty;
            string outputPath = string.Empty;

            for (int i = 1; i < args.Length; i++)
            {
                if ((args[i] == "-t" || args[i] == "--truth") && i + 1 < args.Length)
                {
                    truth = args[++i];
                }
                else if ((args[i] == "-i" || args[i] == "--input") && i + 1 < args.Length)
                {
                    inputPath = args[++i];
                }
                else if ((args[i] == "-o" || args[i] == "--output") && i + 1 < args.Length)
                {
                    outputPath = args[++i];
                }
            }

            return (truth, inputPath, outputPath);
        }
    }
}
