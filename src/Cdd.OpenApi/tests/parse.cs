// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.TestsModule
{
    /// <summary>
    /// Parses C# test suite classes into <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> and OpenAPI path item definitions.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Converts a test <see cref="ClassDeclarationSyntax"/> into a list of <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> instances.
        /// </summary>
        /// <param name="classNode">The Roslyn class syntax node representing an API test suite.</param>
        /// <returns>A list of extracted <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> instances.</returns>
        public static List<Cdd.OpenApi.Ir.IrEndpoint> ToEndpoints(ClassDeclarationSyntax classNode)
        {
            return OperationHelpers.ExtractEndpointsFromPaths(ToPaths(classNode));
        }

        /// <summary>
        /// Converts a test <see cref="ClassDeclarationSyntax"/> into an OpenAPI paths dictionary.
        /// </summary>
        /// <param name="classNode">The Roslyn class syntax node representing an API test suite.</param>
        /// <returns>A dictionary mapping route paths to <see cref="OpenApiPathItem"/> instances.</returns>
        public static IDictionary<string, OpenApiPathItem> ToPaths(ClassDeclarationSyntax classNode)
        {
            return OperationHelpers.ParseHeuristicPaths(classNode, stripTestSuffix: true);
        }
    }
}
