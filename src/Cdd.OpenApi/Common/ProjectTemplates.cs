// <copyright file="ProjectTemplates.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Text;

namespace Cdd.OpenApi.Common
{
    /// <summary>
    /// Provides reusable project scaffolding templates (<c>.csproj</c>, <c>.sln</c>, <c>README.md</c>, and GitHub Actions CI workflows) for generated C# projects.
    /// </summary>
    public static class ProjectTemplates
    {
        /// <summary>
        /// Default GitHub Actions CI workflow YAML content.
        /// </summary>
        public const string GithubActionsCiYaml = """
            name: CI

            on: [push]

            jobs:
              build:
                runs-on: ubuntu-latest
                steps:
                - uses: actions/checkout@v3

            """;

        /// <summary>
        /// Builds the <c>README.md</c> content for a generated project.
        /// </summary>
        /// <param name="title">The escaped project title.</param>
        /// <param name="description">The escaped project description.</param>
        /// <param name="type">The target generation mode.</param>
        /// <returns>The formatted Markdown string.</returns>
        public static string BuildReadme(string title, string description, GenerateType type)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {title}");
            sb.AppendLine();
            sb.AppendLine(description);
            sb.AppendLine();

            if (type == GenerateType.All || type == GenerateType.Server)
            {
                sb.AppendLine("## Server Modes");
                sb.AppendLine();
                sb.AppendLine("- `start` (No DB configured): **Stub Mode**. Server runs using traditional scaffolds, endpoints return `NotImplementedError` or empty bodies.");
                sb.AppendLine("- `start` (With `DATABASE_URL`): **Production Mode**. Uses actual ORM interactions against a real database.");
                sb.AppendLine("- `start --ephemeral`: **Sandbox Mode**. Uses actual ORM interactions against a fresh, throwaway database.");
                sb.AppendLine("- `start --ephemeral --seed`: **Full Mock Mode**. Ephemeral database, automatically populated with a localized fake data graph.");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds the main <c>GeneratedProject.csproj</c> XML content.
        /// </summary>
        /// <param name="title">The escaped package ID / title.</param>
        /// <param name="version">The escaped package version.</param>
        /// <param name="authors">The escaped package authors.</param>
        /// <param name="description">The escaped package description.</param>
        /// <param name="type">The target generation mode.</param>
        /// <param name="includeTests">Whether composable test packages should be referenced.</param>
        /// <param name="targetFramework">The target .NET framework moniker (defaults to <c>"net10.0"</c>).</param>
        /// <returns>The XML string for <c>GeneratedProject.csproj</c>.</returns>
        public static string BuildMainCsproj(
            string title,
            string version,
            string authors,
            string description,
            GenerateType type,
            bool includeTests,
            string targetFramework = "net10.0")
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"<Project Sdk=""Microsoft.NET.Sdk"">");
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine($"    <TargetFramework>{targetFramework}</TargetFramework>");
            sb.AppendLine($"    <PackageId>{title}</PackageId>");
            sb.AppendLine($"    <Version>{version}</Version>");
            sb.AppendLine($"    <Authors>{authors}</Authors>");
            sb.AppendLine($"    <Description>{description}</Description>");
            sb.AppendLine("    <PackageReadmeFile>README.md</PackageReadmeFile>");
            sb.AppendLine("    <Nullable>enable</Nullable>");
            sb.AppendLine("    <GenerateProgramFile>false</GenerateProgramFile>");
            sb.AppendLine("    <IsTestProject>false</IsTestProject>");

            if (type == GenerateType.Server || type == GenerateType.All || type == GenerateType.SdkCli)
            {
                sb.AppendLine("    <OutputType>Exe</OutputType>");
                if (type == GenerateType.All)
                {
                    sb.AppendLine("    <StartupObject>Generated.Program</StartupObject>");
                }
            }

            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine("  <ItemGroup>");
            sb.AppendLine(@"    <None Include=""..\..\README.md"" Pack=""true"" PackagePath="""" />");
            sb.AppendLine("  </ItemGroup>");

            if (type == GenerateType.All || type == GenerateType.Server)
            {
                sb.AppendLine("  <ItemGroup>");
                sb.AppendLine(@"    <PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""9.0.0"" />");
                sb.AppendLine(@"    <PackageReference Include=""Npgsql.EntityFrameworkCore.PostgreSQL"" Version=""9.0.0"" />");
                sb.AppendLine(@"    <PackageReference Include=""Microsoft.EntityFrameworkCore.Sqlite"" Version=""9.0.0"" />");
                sb.AppendLine(@"    <PackageReference Include=""Bogus"" Version=""35.6.1"" />");
                sb.AppendLine("  </ItemGroup>");
                sb.AppendLine("  <ItemGroup>");
                sb.AppendLine(@"    <FrameworkReference Include=""Microsoft.AspNetCore.App"" />");
                sb.AppendLine("  </ItemGroup>");
            }

            if (includeTests)
            {
                sb.AppendLine("  <ItemGroup>");
                sb.AppendLine(@"    <PackageReference Include=""xunit"" Version=""2.6.4"" />");
                sb.AppendLine(@"    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Testing"" Version=""9.0.0"" />");
                sb.AppendLine("  </ItemGroup>");
            }

            sb.Append("</Project>");
            return sb.ToString();
        }

        /// <summary>
        /// Builds the <c>GeneratedProject.Tests.csproj</c> XML content.
        /// </summary>
        /// <param name="targetFramework">The target .NET framework moniker (defaults to <c>"net10.0"</c>).</param>
        /// <returns>The XML string for <c>GeneratedProject.Tests.csproj</c>.</returns>
        public static string BuildTestsCsproj(string targetFramework = "net10.0")
        {
            return $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>{targetFramework}</TargetFramework>
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
        }

        /// <summary>
        /// Builds the Visual Studio solution (<c>GeneratedProject.sln</c>) content.
        /// </summary>
        /// <param name="includeTests">Whether to include the <c>GeneratedProject.Tests</c> project in the solution.</param>
        /// <returns>The formatted <c>.sln</c> file content.</returns>
        public static string BuildSolution(bool includeTests)
        {
            var guid1 = Guid.NewGuid().ToString().ToUpperInvariant();
            if (includeTests)
            {
                var guid2 = Guid.NewGuid().ToString().ToUpperInvariant();
                return @"Microsoft Visual Studio Solution File, Format Version 12.00
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
EndGlobal".Replace("{GUID1}", guid1).Replace("{GUID2}", guid2);
            }

            return @"Microsoft Visual Studio Solution File, Format Version 12.00
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
EndGlobal".Replace("{GUID1}", guid1);
        }
    }
}
