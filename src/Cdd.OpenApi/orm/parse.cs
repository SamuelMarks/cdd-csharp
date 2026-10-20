// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Parses Entity Framework Core <c>DbContext</c> class declarations into <see cref="Cdd.OpenApi.Ir.IrModel"/> and <see cref="OpenApiSchema"/> definitions.</summary>
    public static class Parse
    {
        /// <summary>
        /// Extracts a list of <see cref="Cdd.OpenApi.Ir.IrModel"/> definitions from <c>DbSet&lt;T&gt;</c> properties on a <c>DbContext</c> class.
        /// </summary>
        /// <param name="classNode">The C# DbContext class syntax node to analyze.</param>
        /// <returns>A list of extracted <see cref="Cdd.OpenApi.Ir.IrModel"/> definitions.</returns>
        public static List<Cdd.OpenApi.Ir.IrModel> ToIrModels(ClassDeclarationSyntax classNode)
        {
            return ToSchemas(classNode).Select(kvp => Cdd.OpenApi.Ir.IrModel.FromOpenApiSchema(kvp.Key, kvp.Value)).ToList();
        }

        /// <summary>
        /// Extracts a dictionary of <see cref="OpenApiSchema"/> definitions from <c>DbSet&lt;T&gt;</c> properties on a <c>DbContext</c> class.
        /// </summary>
        /// <param name="classNode">The C# DbContext class syntax node to analyze.</param>
        /// <returns>A dictionary mapping entity names to their <see cref="OpenApiSchema"/> definitions.</returns>
        public static IDictionary<string, OpenApiSchema> ToSchemas(ClassDeclarationSyntax classNode)
        {
            var schemas = new Dictionary<string, OpenApiSchema>();

            var properties = classNode.Members.OfType<PropertyDeclarationSyntax>();
            foreach (var prop in properties)
            {
                var typeStr = prop.Type.ToString();
                if (typeStr.StartsWith("DbSet<") && typeStr.EndsWith(">"))
                {
                    var entityName = typeStr.Substring(6, typeStr.Length - 7);

                    var schema = new OpenApiSchema
                    {
                        Type = "object",
                        Description = Docstrings.Parse.GetSummary(prop) ?? $"Entity for {entityName}",
                        Properties = new Dictionary<string, OpenApiSchema>()
                    };
                    schemas[entityName] = schema;
                }
            }

            return schemas;
        }
    }
}
