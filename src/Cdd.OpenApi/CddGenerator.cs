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

        /// <summary>Mcp</summary>
        public bool Mcp { get; set; } = false;
    }

    /// <summary>Auto-generated documentation for CddGenerator.</summary>
    public static class CddGenerator
    {
        private static void SafeCreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var parts = path.Split(new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            string current = "";
            if (path.StartsWith("/")) current = "/";
            foreach (var part in parts)
            {
                current = System.IO.Path.Combine(current, part);
                if (!System.IO.Directory.Exists(current))
                {
                    try { System.IO.Directory.CreateDirectory(current); } catch { }
                }
            }
        }

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
            OpenApiDocument? lastDoc = null;
            foreach (var inputPath in inputPaths)
            {
                if (!File.Exists(inputPath)) throw new FileNotFoundException($"Input '{inputPath}' not found.");

                var doc = parser.ParseJson(File.ReadAllText(inputPath));
                lastDoc = doc;
                var codes = CodeGenerator.Generate(doc, "Generated", type, config.CreateComposableTestsAndMocks);

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
            }

            if (!config.NoGithubActions)
            {
                var ghDir = Path.Combine(outputDir, ".github", "workflows");
                try
                {
                    SafeCreateDirectory(ghDir);
                    File.WriteAllBytes(Path.Combine(ghDir, "ci.yml"), System.Text.Encoding.UTF8.GetBytes("name: CI\n\non: [push]\n\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n    - uses: actions/checkout@v3\n"));
                }
                catch { }
            }

            if (!config.NoInstallablePackage)
            {
                try
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
                    // Escape XML characters just in case
                    title = System.Security.SecurityElement.Escape(title)!;
                    version = System.Security.SecurityElement.Escape(version)!;
                    authors = System.Security.SecurityElement.Escape(authors)!;
                    description = System.Security.SecurityElement.Escape(description)!;

                    var readmeContent = $"# {title}\n\n{description}\n\n";
                    if (type == GenerateType.All || type == GenerateType.Server)
                    {
                        readmeContent += "## Server Modes\n\n" +
                                         "- `start` (No DB configured): **Stub Mode**. Server runs using traditional scaffolds, endpoints return `NotImplementedError` or empty bodies.\n" +
                                         "- `start` (With `DATABASE_URL`): **Production Mode**. Uses actual ORM interactions against a real database.\n" +
                                         "- `start --ephemeral`: **Sandbox Mode**. Uses actual ORM interactions against a fresh, throwaway database.\n" +
                                         "- `start --ephemeral --seed`: **Full Mock Mode**. Ephemeral database, automatically populated with a localized fake data graph.\n";
                    }
                    File.WriteAllBytes(Path.Combine(outputDir, "README.md"), System.Text.Encoding.UTF8.GetBytes(readmeContent));

                    var projContent = $"<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n    <PackageId>{title}</PackageId>\n    <Version>{version}</Version>\n    <Authors>{authors}</Authors>\n    <Description>{description}</Description>\n    <PackageReadmeFile>README.md</PackageReadmeFile>\n    <Nullable>enable</Nullable>\n    <GenerateProgramFile>false</GenerateProgramFile>\n";
                    if (type == GenerateType.Server || type == GenerateType.All || type == GenerateType.SdkCli)
                    {
                        projContent += "    <OutputType>Exe</OutputType>\n";
                        if (type == GenerateType.All)
                        {
                            projContent += $"    <StartupObject>Generated.Program</StartupObject>\n";
                        }
                    }
                    projContent += "  </PropertyGroup>\n";
                    projContent += "  <ItemGroup>\n    <None Include=\"..\\..\\README.md\" Pack=\"true\" PackagePath=\"\\\" />\n  </ItemGroup>\n";

                    if (type == GenerateType.All || type == GenerateType.Server)
                    {
                        projContent += "  <ItemGroup>\n    <PackageReference Include=\"Microsoft.EntityFrameworkCore\" Version=\"9.0.0\" />\n    <PackageReference Include=\"Npgsql.EntityFrameworkCore.PostgreSQL\" Version=\"9.0.0\" />\n    <PackageReference Include=\"Microsoft.EntityFrameworkCore.Sqlite\" Version=\"9.0.0\" />\n    <PackageReference Include=\"Bogus\" Version=\"35.6.1\" />\n  </ItemGroup>\n  <ItemGroup>\n    <FrameworkReference Include=\"Microsoft.AspNetCore.App\" />\n  </ItemGroup>\n";
                    }
                    if (config.CreateComposableTestsAndMocks && (type == GenerateType.All || type == GenerateType.Server))
                    {
                        projContent += "  <ItemGroup>\n    <PackageReference Include=\"Microsoft.NET.Test.Sdk\" Version=\"17.8.0\" />\n    <PackageReference Include=\"xunit\" Version=\"2.6.4\" />\n    <PackageReference Include=\"xunit.runner.visualstudio\" Version=\"2.5.6\" />\n    <PackageReference Include=\"Microsoft.AspNetCore.Mvc.Testing\" Version=\"9.0.0\" />\n  </ItemGroup>\n";
                    }
                    projContent += "</Project>";
                    var projectDir = Path.Combine(outputDir, "src", "GeneratedProject");
                    SafeCreateDirectory(projectDir);
                    File.WriteAllBytes(Path.Combine(projectDir, "GeneratedProject.csproj"), System.Text.Encoding.UTF8.GetBytes(projContent));

                    if (true)
                    {
                        var guid1 = Guid.NewGuid().ToString().ToUpper();
                        string slnContent;

                        if (config.CreateComposableTestsAndMocks)
                        {
                            var testsDir = Path.Combine(outputDir, "tests", "GeneratedProject.Tests");
                            SafeCreateDirectory(testsDir);

                            var testProjContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.8.0"" />
    <PackageReference Include=""xunit"" Version=""2.6.4"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.5.6"">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Testing"" Version=""9.0.0"" />
    <FrameworkReference Include=""Microsoft.AspNetCore.App"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\src\GeneratedProject\GeneratedProject.csproj"" />
  </ItemGroup>
</Project>";
                            File.WriteAllBytes(Path.Combine(testsDir, "GeneratedProject.Tests.csproj"), System.Text.Encoding.UTF8.GetBytes(testProjContent));

                            slnContent = @"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""GeneratedProject"", ""src\GeneratedProject\GeneratedProject.csproj"", ""{GUID1}""
EndProject
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""GeneratedProject.Tests"", ""tests\GeneratedProject.Tests\GeneratedProject.Tests.csproj"", ""{GUID2}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{GUID1}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{GUID1}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{GUID1}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{GUID1}.Release|Any CPU.Build.0 = Release|Any CPU
		{GUID2}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{GUID2}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{GUID2}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{GUID2}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal";
                            var guid2 = Guid.NewGuid().ToString().ToUpper();
                            slnContent = slnContent.Replace("{GUID1}", guid1).Replace("{GUID2}", guid2);

                            var integrationTestContent = "";
                            if (lastDoc != null) integrationTestContent = IntegrationTestGenerator.Generate(lastDoc);
                            File.WriteAllBytes(Path.Combine(testsDir, "IntegrationTests.cs"), System.Text.Encoding.UTF8.GetBytes(integrationTestContent));
                        }
                        else
                        {
                            slnContent = @"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""GeneratedProject"", ""src\GeneratedProject\GeneratedProject.csproj"", ""{GUID1}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{GUID1}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{GUID1}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{GUID1}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{GUID1}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal";
                            slnContent = slnContent.Replace("{GUID1}", guid1);
                        }

                        File.WriteAllBytes(Path.Combine(outputDir, "GeneratedProject.sln"), System.Text.Encoding.UTF8.GetBytes(slnContent));
                    }
                }
                catch { }
            }
        }
    }
}
