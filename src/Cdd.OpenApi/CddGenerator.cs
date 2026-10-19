// <copyright file="CddGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Parse;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi
{
    /// <summary>Configuration options controlling OpenAPI-to-C# code generation and project scaffolding.</summary>
    public class CddConfig
    {
        /// <summary>Gets or sets the single input OpenAPI specification file path.</summary>
        public string InputPath { get; set; } = string.Empty;

        /// <summary>Gets or sets the directory path containing OpenAPI specification files.</summary>
        public string InputDir { get; set; } = string.Empty;

        /// <summary>Gets or sets the collection of input OpenAPI specification file paths.</summary>
        public List<string> InputPaths { get; set; } = new List<string>();

        /// <summary>Gets or sets the target output directory path for generated files.</summary>
        public string OutputDir { get; set; } = string.Empty;

        /// <summary>Gets or sets a value indicating whether GitHub Actions workflow scaffolding should be omitted.</summary>
        public bool NoGithubActions { get; set; } = false;

        /// <summary>Gets or sets a value indicating whether .csproj and .sln package scaffolding should be omitted.</summary>
        public bool NoInstallablePackage { get; set; } = false;

        /// <summary>Gets or sets a value indicating whether composable unit tests, integration tests, and mocks should be generated.</summary>
        public bool CreateComposableTestsAndMocks { get; set; } = false;

        /// <summary>Gets or sets a value indicating whether Model Context Protocol (MCP) server and models should be generated.</summary>
        public bool Mcp { get; set; } = false;
    }

    /// <summary>Orchestrates file-based C# project and code generation from OpenAPI specifications.</summary>
    public static class CddGenerator
    {
        /// <summary>
        /// Creates the specified directory path, wrapping any I/O failure in a <see cref="Cdd.OpenApi.Errors.CddException"/>.
        /// </summary>
        /// <param name="path">The directory path to create.</param>
        private static void SafeCreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw new Cdd.OpenApi.Errors.CddException(
                    Cdd.OpenApi.Errors.CddErrorCode.IoError,
                    $"Failed to create directory '{path}': {ex.Message}",
                    ex);
            }
        }

        /// <summary>Generates a C# client SDK project from the given configuration.</summary>
        /// <param name="config">The generation configuration.</param>
        public static void GenerateSdk(CddConfig config) => Generate(config, GenerateType.Sdk);

        /// <summary>Generates a C# client SDK and CLI project from the given configuration.</summary>
        /// <param name="config">The generation configuration.</param>
        public static void GenerateSdkCli(CddConfig config) => Generate(config, GenerateType.SdkCli);

        /// <summary>Generates an ASP.NET Core server and ORM project from the given configuration.</summary>
        /// <param name="config">The generation configuration.</param>
        public static void GenerateServer(CddConfig config) => Generate(config, GenerateType.Server);

        /// <summary>Generates the full suite of SDK, CLI, Server, ORM, and optional MCP artifacts from the given configuration.</summary>
        /// <param name="config">The generation configuration.</param>
        public static void GenerateAll(CddConfig config) => Generate(config, GenerateType.All);

        /// <summary>
        /// Generates C# workflow artifacts (SDK orchestrators, CLI runners, Server E2E workflow tests, and stateful mocks) from Arazzo 1.1.0 specification files.
        /// </summary>
        /// <param name="config">The generation configuration.</param>
        /// <param name="type">The target generation mode.</param>
        public static void GenerateFromArazzo(CddConfig config, GenerateType type = GenerateType.All)
        {
            var inputPaths = new List<string>();
            if (!string.IsNullOrEmpty(config.InputPath))
            {
                inputPaths.Add(config.InputPath);
            }

            if (config.InputPaths.Any())
            {
                inputPaths.AddRange(config.InputPaths);
            }

            if (!string.IsNullOrEmpty(config.InputDir) && Directory.Exists(config.InputDir))
            {
                inputPaths.AddRange(Directory.GetFiles(config.InputDir, "*.json", SearchOption.AllDirectories));
                inputPaths.AddRange(Directory.GetFiles(config.InputDir, "*.yaml", SearchOption.AllDirectories));
                inputPaths.AddRange(Directory.GetFiles(config.InputDir, "*.yml", SearchOption.AllDirectories));
            }

            if (!inputPaths.Any())
            {
                throw new ArgumentException("No input paths provided in config.");
            }

            var outputDir = string.IsNullOrEmpty(config.OutputDir) ? Directory.GetCurrentDirectory() : config.OutputDir;
            var sourceDir = config.NoInstallablePackage ? outputDir : Path.Combine(outputDir, "src", "GeneratedProject");

            foreach (var inputPath in inputPaths)
            {
                if (!File.Exists(inputPath))
                {
                    throw new FileNotFoundException($"Input '{inputPath}' not found.");
                }

                var arazzoDoc = Arazzo.ArazzoParser.ParseYamlOrJson(File.ReadAllText(inputPath));
                var codes = ArazzoCodeGenerator.Generate(arazzoDoc, "Generated", type, config.CreateComposableTestsAndMocks);
                foreach (var code in codes)
                {
                    var fullPath = Path.Combine(sourceDir, code.FileName);
                    var dir = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        SafeCreateDirectory(dir);
                    }

                    File.WriteAllBytes(fullPath, System.Text.Encoding.UTF8.GetBytes(code.Code));
                }
            }
        }

        /// <summary>
        /// Executes the code and project scaffolding generation for the specified <see cref="GenerateType"/>.
        /// </summary>
        /// <param name="config">The generation configuration.</param>
        /// <param name="type">The target generation mode.</param>
        private static void Generate(CddConfig config, GenerateType type)
        {
            var inputPaths = new List<string>();
            if (!string.IsNullOrEmpty(config.InputPath))
            {
                inputPaths.Add(config.InputPath);
            }
            if (config.InputPaths != null)
            {
                if (config.InputPaths.Any())
                {
                    inputPaths.AddRange(config.InputPaths);
                }
            }

            if (!string.IsNullOrEmpty(config.InputDir) && Directory.Exists(config.InputDir))
            {
                inputPaths.AddRange(Directory.GetFiles(config.InputDir, "*.json", SearchOption.AllDirectories));
                inputPaths.AddRange(Directory.GetFiles(config.InputDir, "*.yaml", SearchOption.AllDirectories));
            }

            if (!inputPaths.Any())
            {
                throw new ArgumentException("No input paths provided in config.");
            }

            var outputDir = string.IsNullOrEmpty(config.OutputDir) ? Directory.GetCurrentDirectory() : config.OutputDir;

            var parser = new OpenApiParser();
            var mergedDoc = new OpenApiDocument { Info = null!, Components = new OpenApiComponents() };
            foreach (var inputPath in inputPaths)
            {
                if (!File.Exists(inputPath)) throw new FileNotFoundException($"Input '{inputPath}' not found.");

                var doc = parser.ParseJson(File.ReadAllText(inputPath));
                mergedDoc.Info = doc.Info;
                mergedDoc.Servers = doc.Servers ?? mergedDoc.Servers;
                mergedDoc.BasePath = doc.BasePath ?? mergedDoc.BasePath;
                mergedDoc.Self = doc.Self ?? mergedDoc.Self;
                mergedDoc.JsonSchemaDialect = doc.JsonSchemaDialect ?? mergedDoc.JsonSchemaDialect;

                if (doc.Components?.Schemas != null)
                {
                    mergedDoc.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();
                    foreach (var kvp in doc.Components.Schemas)
                    {
                        mergedDoc.Components.Schemas[kvp.Key] = kvp.Value;
                    }
                }

                if (doc.Components?.SecuritySchemes != null)
                {
                    mergedDoc.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
                    foreach (var kvp in doc.Components.SecuritySchemes)
                    {
                        mergedDoc.Components.SecuritySchemes[kvp.Key] = kvp.Value;
                    }
                }

                if (doc.Paths != null)
                {
                    mergedDoc.Paths ??= new OpenApiPaths();
                    foreach (var kvp in doc.Paths)
                    {
                        mergedDoc.Paths[kvp.Key] = kvp.Value;
                    }
                }
            }

            OpenApiDocument lastDoc = mergedDoc;
            var codes = CodeGenerator.Generate(mergedDoc, "Generated", type, config.CreateComposableTestsAndMocks, config.Mcp);

            var sourceDir = config.NoInstallablePackage ? outputDir : Path.Combine(outputDir, "src", "GeneratedProject");
            foreach (var code in codes)
            {
                var fullPath = Path.Combine(sourceDir, code.FileName);
                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    SafeCreateDirectory(dir);
                }
                File.WriteAllBytes(fullPath, System.Text.Encoding.UTF8.GetBytes(code.Code));
            }

            if (!config.NoGithubActions)
            {
                var ghDir = Path.Combine(outputDir, ".github", "workflows");
                SafeCreateDirectory(ghDir);
                File.WriteAllBytes(Path.Combine(ghDir, "ci.yml"), System.Text.Encoding.UTF8.GetBytes(ProjectTemplates.GithubActionsCiYaml));
            }

            if (!config.NoInstallablePackage)
            {
                var title = "GeneratedProject";
                var version = "1.0.0";
                var authors = "Generated";
                var description = "Generated OpenApi SDK";
                if (lastDoc!.Info != null)
                {
                    var info = lastDoc.Info;
                    if (!string.IsNullOrWhiteSpace(info.Title)) title = info.Title;
                    if (!string.IsNullOrWhiteSpace(info.Version)) version = info.Version;
                    if (info.Contact != null && !string.IsNullOrWhiteSpace(info.Contact.Name)) authors = info.Contact.Name;
                    if (!string.IsNullOrWhiteSpace(info.Description)) description = info.Description;
                }

                title = System.Security.SecurityElement.Escape(title)!;
                version = System.Security.SecurityElement.Escape(version)!;
                authors = System.Security.SecurityElement.Escape(authors)!;
                description = System.Security.SecurityElement.Escape(description)!;

                var readmeContent = ProjectTemplates.BuildReadme(title, description, type);
                File.WriteAllBytes(Path.Combine(outputDir, "README.md"), System.Text.Encoding.UTF8.GetBytes(readmeContent));

                var projContent = ProjectTemplates.BuildMainCsproj(title, version, authors, description, type, config.CreateComposableTestsAndMocks);
                var projectDir = Path.Combine(outputDir, "src", "GeneratedProject");
                SafeCreateDirectory(projectDir);
                File.WriteAllBytes(Path.Combine(projectDir, "GeneratedProject.csproj"), System.Text.Encoding.UTF8.GetBytes(projContent));

                if (config.CreateComposableTestsAndMocks)
                {
                    var testsDir = Path.Combine(outputDir, "tests", "GeneratedProject.Tests");
                    SafeCreateDirectory(testsDir);

                    var testProjContent = ProjectTemplates.BuildTestsCsproj();
                    File.WriteAllBytes(Path.Combine(testsDir, "GeneratedProject.Tests.csproj"), System.Text.Encoding.UTF8.GetBytes(testProjContent));

                    var integrationTestContent = IntegrationTestGenerator.Generate(lastDoc);
                    File.WriteAllBytes(Path.Combine(testsDir, "IntegrationTests.cs"), System.Text.Encoding.UTF8.GetBytes(integrationTestContent));
                }

                var slnContent = ProjectTemplates.BuildSolution(config.CreateComposableTestsAndMocks);
                File.WriteAllBytes(Path.Combine(outputDir, "GeneratedProject.sln"), System.Text.Encoding.UTF8.GetBytes(slnContent));
            }
        }
    }
}
