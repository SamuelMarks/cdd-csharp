// <copyright file="CSharpRoleClassifier.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Common
{
    /// <summary>
    /// Enumeration of architectural roles that a C# class declaration can fulfill when reverse-compiling into an OpenAPI specification.
    /// </summary>
    public enum CSharpClassRole
    {
        /// <summary>A domain data model or DTO class mapped to <c>components.schemas</c>.</summary>
        Model,

        /// <summary>An ASP.NET Core controller or route interface with <c>[Http*]</c> method attributes.</summary>
        RouteController,

        /// <summary>An asynchronous HTTP API client class making <c>HttpClient</c> calls.</summary>
        HttpClient,

        /// <summary>A mock API service implementation class ending in <c>Mock</c> with operation methods.</summary>
        MockService,

        /// <summary>A command-line interface entry point class ending in <c>Cli</c>.</summary>
        CliEntrypoint,

        /// <summary>An xUnit or API test suite class ending in <c>Tests</c> with test methods.</summary>
        TestSuite,

        /// <summary>An Entity Framework Core <c>DbContext</c> subclass containing <c>DbSet&lt;T&gt;</c> properties.</summary>
        DbContext,

        /// <summary>A Model Context Protocol (MCP) server class exposing tools and resources.</summary>
        McpServer
    }

    /// <summary>
    /// Classifies Roslyn <see cref="ClassDeclarationSyntax"/> nodes into their corresponding <see cref="CSharpClassRole"/>.
    /// </summary>
    public static class CSharpRoleClassifier
    {
        /// <summary>
        /// Determines whether the specified class syntax node carries top-level API document metadata (e.g., <c>Program</c>, <c>Startup</c>, or <c>*Api</c>).
        /// </summary>
        /// <param name="classNode">The class syntax node to inspect.</param>
        /// <returns><c>true</c> if the class is a document metadata host; otherwise <c>false</c>.</returns>
        public static bool IsDocumentMetadataHost(ClassDeclarationSyntax classNode)
        {
            var name = classNode.Identifier.Text;
            return name == "Program" || name == "Startup" || name.EndsWith("Api");
        }

        /// <summary>
        /// Classifies a <see cref="ClassDeclarationSyntax"/> node into its <see cref="CSharpClassRole"/>.
        /// </summary>
        /// <param name="classNode">The C# class syntax node to classify.</param>
        /// <returns>The determined <see cref="CSharpClassRole"/>.</returns>
        public static CSharpClassRole Classify(ClassDeclarationSyntax classNode)
        {
            var methods = classNode.GetDescendantNodesSafe().OfType<MethodDeclarationSyntax>().ToList();

            var hasRoutes = methods.Any(m => m.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().StartsWith("Http")));
            if (hasRoutes)
            {
                return CSharpClassRole.RouteController;
            }

            var hasClientMethods = classNode.GetDescendantNodesSafe().OfType<InvocationExpressionSyntax>()
                .Any(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess &&
                            memberAccess.Name.Identifier.Text.EndsWith("Async") &&
                            (memberAccess.Name.Identifier.Text.StartsWith("Get") ||
                             memberAccess.Name.Identifier.Text.StartsWith("Post") ||
                             memberAccess.Name.Identifier.Text.StartsWith("Put") ||
                             memberAccess.Name.Identifier.Text.StartsWith("Delete")));
            if (hasClientMethods)
            {
                return CSharpClassRole.HttpClient;
            }

            var isDbContext = classNode.BaseList?.Types.Any(t => t.Type.ToString().EndsWith("DbContext")) == true;
            if (isDbContext)
            {
                return CSharpClassRole.DbContext;
            }

            var className = classNode.Identifier.Text;
            if (className.EndsWith("Mock"))
            {
                return CSharpClassRole.MockService;
            }

            if (className.EndsWith("Cli"))
            {
                return CSharpClassRole.CliEntrypoint;
            }

            if (className.EndsWith("Tests"))
            {
                return CSharpClassRole.TestSuite;
            }

            if (Mcp.Parse.IsMcpServerClass(classNode))
            {
                return CSharpClassRole.McpServer;
            }

            return CSharpClassRole.Model;
        }
    }
}
