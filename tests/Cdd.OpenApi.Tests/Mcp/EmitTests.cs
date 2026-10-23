// <copyright file="EmitTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Linq;
using Cdd.OpenApi.Mcp;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Cdd.OpenApi.Tests.Mcp
{
    /// <summary>Verifies or exercises emit tests.</summary>
    public class EmitTests
    {
        /// <summary>Verifies or exercises to mcp server generates class.</summary>
        [Fact]
        public void ToMcpServer_GeneratesClass()
        {
            var paths = new OpenApiPaths
            {
                {
                    "/test", new OpenApiPathItem
                    {
                        Get = new OpenApiOperation
                        {
                            OperationId = "test_get",
                            Summary = "Test Get",
                            Parameters = new System.Collections.Generic.List<OpenApiParameter>
                            {
                                new OpenApiParameter { Name = "param1", Schema = new OpenApiSchema { Type = "integer" }, Required = true },
                                new OpenApiParameter { Name = "param2", Schema = new OpenApiSchema { Type = "string" }, Required = false, Description = "param2 desc" }
                            }
                        }
                    }
                }
            };

            var classDeclaration = Cdd.OpenApi.Mcp.Emit.ToMcpServer("TestMcpServer", paths);
            Assert.NotNull(classDeclaration);
            Assert.Equal("TestMcpServer", classDeclaration.Identifier.Text);

            var code = classDeclaration.ToFullString();
            Assert.Contains("TestMcpServer", code);
            Assert.Contains("param1", code);
            Assert.Contains("test_get", code);
            Assert.Contains("Console.OpenStandardOutput", code);
            Assert.Contains("Console.OpenStandardInput", code);
            Assert.Contains("ApiClientCli.Main", code);
        }

        /// <summary>Verifies or exercises to mcp server null paths generates empty server.</summary>
        [Fact]
        public void ToMcpServer_NullPaths_GeneratesEmptyServer()
        {
            var classDeclaration = Cdd.OpenApi.Mcp.Emit.ToMcpServer("EmptyServer", null!);
            Assert.NotNull(classDeclaration);
            Assert.Equal("EmptyServer", classDeclaration.Identifier.Text);

            var code = classDeclaration.ToFullString();
            Assert.Contains("EmptyServer", code);
            Assert.Contains("Console.OpenStandardOutput", code);
            Assert.Contains("Console.OpenStandardInput", code);
        }

        /// <summary>Verifies or exercises to mcp transport generates interface.</summary>
        [Fact]
        public void ToMcpTransport_GeneratesInterface()
        {
            var transportInterface = Cdd.OpenApi.Mcp.Emit.ToMcpTransport();
            Assert.NotNull(transportInterface);
            Assert.Equal("IMcpTransport", transportInterface.Identifier.Text);
            var code = transportInterface.ToFullString();
            Assert.Contains("SendAsync", code);
            Assert.Contains("ReceiveAsync", code);
        }

        /// <summary>Verifies or exercises to mcp models generates models.</summary>
        [Fact]
        public void ToMcpModels_GeneratesModels()
        {
            var models = Cdd.OpenApi.Mcp.Emit.ToMcpModels().ToList();
            Assert.NotEmpty(models);

            var hasTextContent = models.Any(m => m is ClassDeclarationSyntax c && c.Identifier.Text == "TextContent");
            Assert.True(hasTextContent);

            var hasTool = models.Any(m => m is ClassDeclarationSyntax c && c.Identifier.Text == "Tool");
            Assert.True(hasTool);
        }

        /// <summary>Verifies or exercises to mcp server snake case coverage.</summary>
        [Fact]
        public void ToMcpServer_SnakeCase_Coverage()
        {
            // Snake case is called inside ToMcpServer for tool names
            var paths = new OpenApiPaths
            {
                {
                    "/test2", new OpenApiPathItem
                    {
                        Post = new OpenApiOperation
                        {
                            OperationId = "TestPostOperation",
                            Summary = "Test Post"
                        }
                    }
                }
            };
            var classDeclaration = Cdd.OpenApi.Mcp.Emit.ToMcpServer("TestMcpServer2", paths);
            var code = classDeclaration.ToFullString();
            Assert.Contains("test_post_operation", code); // snake case
        }

        /// <summary>Verifies or exercises to mcp server no operation id fallback.</summary>
        [Fact]
        public void ToMcpServer_NoOperationId_Fallback()
        {
            var paths = new OpenApiPaths
            {
                {
                    "/test3/{id}", new OpenApiPathItem
                    {
                        Put = new OpenApiOperation
                        {
                            Summary = "Test Put"
                        }
                    }
                }
            };
            var classDeclaration = Cdd.OpenApi.Mcp.Emit.ToMcpServer("TestMcpServer3", paths);
            var code = classDeclaration.ToFullString();
            Assert.Contains("puttest3id", code); // fallback name from path
        }

        /// <summary>Verifies or exercises to mcp server no parameters no summary coverage.</summary>
        [Fact]
        public void ToMcpServer_NoParameters_NoSummary_Coverage()
        {
            var paths = new OpenApiPaths
            {
                {
                    "/test4", new OpenApiPathItem
                    {
                        Get = new OpenApiOperation
                        {
                            OperationId = "",
                            Summary = null,
                            Parameters = new System.Collections.Generic.List<OpenApiParameter>
                            {
                                new OpenApiParameter { Name = "param1", Schema = null },
                                new OpenApiParameter { Name = "param2", Schema = new OpenApiSchema { Type = null } }
                            }
                        }
                    }
                }
            };
            var classDeclaration = Cdd.OpenApi.Mcp.Emit.ToMcpServer("TestMcpServer4", paths);
            var code = classDeclaration.ToFullString();
            Assert.Contains("TestMcpServer4", code);
            Assert.Contains("No description", code);
            Assert.Contains("param1", code);
        }

        /// <summary>Verifies or exercises to snake case empty string coverage.</summary>
        [Fact]
        public void ToSnakeCase_EmptyString_Coverage()
        {
            Assert.Null(Cdd.OpenApi.Mcp.Emit.ToSnakeCase(null!));
            Assert.Equal("", Cdd.OpenApi.Mcp.Emit.ToSnakeCase(""));
            Assert.Equal("get_user_by_id", Cdd.OpenApi.Mcp.Emit.ToSnakeCase("GetUserById"));
        }

        /// <summary>Verifies or exercises mcp parse and spec generator integration.</summary>
        [Fact]
        public void McpParse_ExtractsPathsAndEndpoints()
        {
            Assert.False(Cdd.OpenApi.Mcp.Parse.IsMcpServerClass(null!));
            Assert.Empty(Cdd.OpenApi.Mcp.Parse.ToPaths(null!));
            Cdd.OpenApi.Mcp.Parse.TryExtractToolsFromRawJson("", new OpenApiPaths());
            Cdd.OpenApi.Mcp.Parse.TryExtractToolsFromRawJson("{}", null!);

            var nonMcpClass = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseMemberDeclaration("public class RegularModel { public int Id { get; set; } }") as ClassDeclarationSyntax;
            Assert.False(Cdd.OpenApi.Mcp.Parse.IsMcpServerClass(nonMcpClass!));

            var bodyDetectedMcpClass = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseMemberDeclaration(
                "public class CustomRunner { public void Execute() { var s = \"tools/list\"; } }") as ClassDeclarationSyntax;
            Assert.True(Cdd.OpenApi.Mcp.Parse.IsMcpServerClass(bodyDetectedMcpClass!));
            Assert.Empty(Cdd.OpenApi.Mcp.Parse.ToPaths(bodyDetectedMcpClass!));

            var paths = new OpenApiPaths
            {
                ["/users"] = new OpenApiPathItem
                {
                    Post = new OpenApiOperation
                    {
                        OperationId = "create_user",
                        Summary = "Create user",
                        Parameters = new System.Collections.Generic.List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "age", In = "query", Required = true, Description = "User age", Schema = new OpenApiSchema { Type = "integer" } },
                            new OpenApiParameter { Name = "name", In = "query", Required = false, Description = "", Schema = new OpenApiSchema { Type = "string" } }
                        }
                    }
                }
            };

            var mcpClass = Cdd.OpenApi.Mcp.Emit.ToMcpServer("UsersMcpServer", paths);
            Assert.True(Cdd.OpenApi.Mcp.Parse.IsMcpServerClass(mcpClass));

            var extractedPaths = Cdd.OpenApi.Mcp.Parse.ToPaths(mcpClass);
            Assert.True(extractedPaths.ContainsKey("/create_user"));
            var op = extractedPaths["/create_user"].Post!;
            Assert.Equal("create_user", op.OperationId);
            Assert.Equal("Create user", op.Summary);
            Assert.Equal(2, op.Parameters!.Count);
            Assert.Equal("age", op.Parameters[0].Name);
            Assert.True(op.Parameters[0].Required);
            Assert.Equal("integer", op.Parameters[0].Schema!.Type);

            var targetPaths = new OpenApiPaths();
            Cdd.OpenApi.Mcp.Parse.TryExtractToolsFromRawJson("[{\"description\":\"missing name\"},{\"name\":\"\"},{\"name\":\"minimal_tool\",\"inputSchema\":{\"type\":\"object\"}}]", targetPaths);
            Assert.True(targetPaths.ContainsKey("/minimal_tool"));
            Assert.Null(targetPaths["/minimal_tool"].Post!.Parameters);

            var specDoc = SpecGenerator.Generate(new[] { mcpClass.ToFormattedString() });
            Assert.NotNull(specDoc.Paths);
            Assert.True(specDoc.Paths!.ContainsKey("/create_user"));
        }

        /// <summary>Verifies or exercises presence, absence, and skipped conformance behavior across OpenAPI 3.2.0, Swagger 2.0, Arazzo 1.1.0, and MCP 1.0.0 targets.</summary>
        [Fact]
        public void Conformance_PresenceAbsenceAndSkipped_AcrossSpecsAndTargets()
        {
            var minimalOpenApi = new OpenApiDocument
            {
                OpenApi = "3.2.0",
                Info = new OpenApiInfo { Title = "Minimal", Version = "1.0.0" }
            };
            var sdkOnlyFiles = CodeGenerator.Generate(
                minimalOpenApi,
                baseNamespace: "MinimalNs",
                type: GenerateType.Sdk,
                tests: false,
                mcp: false);
            Assert.Empty(sdkOnlyFiles);

            var emittedMinimalJson = new Cdd.OpenApi.Emit.OpenApiEmitter().EmitJson(minimalOpenApi);
            Assert.DoesNotContain("\"paths\"", emittedMinimalJson);
            Assert.DoesNotContain("\"components\"", emittedMinimalJson);
            Assert.DoesNotContain("\"webhooks\"", emittedMinimalJson);

            var minimalSwagger = new Cdd.OpenApi.Parse.OpenApiParser().ParseJson(
                "{\"swagger\":\"2.0\",\"info\":{\"title\":\"SwagMin\",\"version\":\"1.0\"},\"paths\":{}}");
            Assert.Equal("3.2.0", minimalSwagger.OpenApi);
            Assert.Empty(minimalSwagger.Paths!);

            var minimalArazzo = Cdd.OpenApi.Arazzo.ArazzoParser.ParseJson(
                "{\"arazzo\":\"1.1.0\",\"info\":{\"title\":\"ArazzoMin\",\"version\":\"1.0.0\"},\"sourceDescriptions\":[{\"name\":\"api\",\"url\":\"./openapi.json\"}],\"workflows\":[{\"workflowId\":\"wf1\",\"steps\":[{\"stepId\":\"s1\",\"operationId\":\"op1\"}]}]}");
            var arazzoClientFiles = ArazzoCodeGenerator.Generate(minimalArazzo, "ArazzoNs", type: GenerateType.Sdk, tests: false);
            Assert.NotEmpty(arazzoClientFiles);
            var extractedArazzo = ArazzoSpecGenerator.Generate(arazzoClientFiles.Select(f => f.Code));
            Assert.Equal("ArazzoMin", extractedArazzo.Info.Title);
            Assert.Null(extractedArazzo.Components);
        }
    }
}
