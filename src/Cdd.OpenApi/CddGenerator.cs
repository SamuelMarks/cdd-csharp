using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Parse;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi
{
    /// <summary>Auto-generated documentation for CddConfig.</summary>
    public class CddConfig
    {
        /// <summary>InputPath</summary>
        public string InputPath { get; set; } = string.Empty;
        
        /// <summary>InputDir</summary>
        public string InputDir { get; set; } = string.Empty;

        /// <summary>InputPaths</summary>
        public List<string> InputPaths { get; set; } = new List<string>();

        /// <summary>OutputDir</summary>
        public string OutputDir { get; set; } = string.Empty;

        /// <summary>NoGithubActions</summary>
        public bool NoGithubActions { get; set; } = false;

        /// <summary>NoInstallablePackage</summary>
        public bool NoInstallablePackage { get; set; } = false;

        /// <summary>CreateComposableTestsAndMocks</summary>
        public bool CreateComposableTestsAndMocks { get; set; } = false;
    }

    /// <summary>Auto-generated documentation for CddGenerator.</summary>
    public static class CddGenerator
    {
        /// <summary>GenerateSdk</summary>
        public static void GenerateSdk(CddConfig config) => Generate(config, GenerateType.Sdk);

        /// <summary>GenerateSdkCli</summary>
        public static void GenerateSdkCli(CddConfig config) => Generate(config, GenerateType.SdkCli);

        /// <summary>GenerateServer</summary>
        public static void GenerateServer(CddConfig config) => Generate(config, GenerateType.Server);

        /// <summary>GenerateAll</summary>
        public static void GenerateAll(CddConfig config) => Generate(config, GenerateType.All);

        private static void Generate(CddConfig config, GenerateType type)
        {
            var inputPaths = new List<string>();
            if (!string.IsNullOrEmpty(config.InputPath))
            {
                inputPaths.Add(config.InputPath);
            }
            if (config.InputPaths != null && config.InputPaths.Any())
            {
                inputPaths.AddRange(config.InputPaths);
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
            foreach (var inputPath in inputPaths)
            {
                if (!File.Exists(inputPath)) throw new FileNotFoundException($"Input '{inputPath}' not found.");
                
                var doc = parser.ParseJson(File.ReadAllText(inputPath));
                var codes = CodeGenerator.Generate(doc, "Generated", type, config.CreateComposableTestsAndMocks);
                
                foreach (var code in codes)
                {
                    var fullPath = Path.Combine(outputDir, code.FileName);
                    var dir = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        try { Directory.CreateDirectory(dir); } catch {} 
                    }
                    File.WriteAllText(fullPath, code.Code);
                }
            }

            if (!config.NoGithubActions)
            {
                var ghDir = Path.Combine(outputDir, ".github", "workflows");
                try 
                { 
                    Directory.CreateDirectory(ghDir); 
                    File.WriteAllText(Path.Combine(ghDir, "ci.yml"), "name: CI\n\non: [push]\n\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n    - uses: actions/checkout@v3\n"); 
                } 
                catch {} 
            }

            if (!config.NoInstallablePackage) 
            { 
                try 
                { 
                    var projContent = "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n"; 
                    if (type == GenerateType.All || type == GenerateType.Server) 
                    { 
                        projContent += "  <ItemGroup>\n    <PackageReference Include=\"Microsoft.EntityFrameworkCore\" Version=\"9.0.0\" />\n  </ItemGroup>\n"; 
                    } 
                    projContent += "</Project>"; 
                    File.WriteAllText(Path.Combine(outputDir, "GeneratedProject.csproj"), projContent); 
                } 
                catch {} 
            }
        }
    }
}