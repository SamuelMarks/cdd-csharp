// <copyright file="CliArgumentParser.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.IO;

namespace Cdd.OpenApi.Cli
{
    /// <summary>
    /// Parses command-line arguments and environment variables into strongly-typed CLI command configurations.
    /// </summary>
    public static class CliArgumentParser
    {
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
            GenerateType type = GenerateType.All;
            int startIndex = 1;

            if (subCommand == "to_sdk") { type = GenerateType.Sdk; startIndex = 2; }
            else if (subCommand == "to_sdk_cli") { type = GenerateType.SdkCli; startIndex = 2; }
            else if (subCommand == "to_server") { type = GenerateType.Server; startIndex = 2; }

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
