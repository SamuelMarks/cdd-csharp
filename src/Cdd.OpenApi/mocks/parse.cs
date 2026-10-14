// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Mocks
{
    /// <summary>
    /// Parses C# mock service classes into OpenAPI path item definitions.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Converts a mock <see cref="ClassDeclarationSyntax"/> into an OpenAPI paths dictionary.
        /// </summary>
        /// <param name="classNode">The Roslyn class syntax node representing a mock controller/service.</param>
        /// <returns>A dictionary mapping route paths to <see cref="OpenApiPathItem"/> instances.</returns>
        public static IDictionary<string, OpenApiPathItem> ToPaths(ClassDeclarationSyntax classNode)
        {
            var paths = new Dictionary<string, OpenApiPathItem>();
            foreach (var method in classNode.Members.OfType<MethodDeclarationSyntax>())
            {
                var name = method.Identifier.Text;
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
