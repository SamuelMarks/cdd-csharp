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
    }
}
