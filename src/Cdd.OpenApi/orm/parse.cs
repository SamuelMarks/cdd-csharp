using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Auto-generated documentation for Parse.</summary>
    public static class Parse
    {
        /// <summary>Auto-generated documentation for ToSchemas.</summary>
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
