// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.TestsModule
{
    /// <summary>
    /// Parses C# test suite classes into OpenAPI path item definitions.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Converts a test <see cref="ClassDeclarationSyntax"/> into an OpenAPI paths dictionary.
        /// </summary>
        /// <param name="classNode">The Roslyn class syntax node representing an API test suite.</param>
        /// <returns>A dictionary mapping route paths to <see cref="OpenApiPathItem"/> instances.</returns>
        public static IDictionary<string, OpenApiPathItem> ToPaths(ClassDeclarationSyntax classNode)
        {
            var paths = new Dictionary<string, OpenApiPathItem>();
            foreach (var method in classNode.Members.OfType<MethodDeclarationSyntax>())
            {
                var name = method.Identifier.Text;
                if (name.EndsWith("Test")) name = name.Substring(0, name.Length - 4);
                var route = "/" + name.ToLowerInvariant();
                var op = new OpenApiOperation { OperationId = name };
                var pathItem = new OpenApiPathItem();

                if (name.StartsWith("Post") && name.Length > 4)
                {
                    pathItem.Post = op;
                }
                else if (name.StartsWith("Put") && name.Length > 3)
                {
                    pathItem.Put = op;
                }
                else if (name.StartsWith("Delete") && name.Length > 6)
                {
                    pathItem.Delete = op;
                }
                else
                {
                    pathItem.Get = op;
                }

                paths[route] = pathItem;
            }
            return paths;
        }
    }
}
