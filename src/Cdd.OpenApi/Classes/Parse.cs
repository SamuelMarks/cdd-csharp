using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Classes
{
/// <summary>Auto-generated documentation for Parse.</summary>
    public static class Parse
    {
/// <summary>Auto-generated documentation for ToSchema.</summary>
        public static OpenApiSchema ToSchema(ClassDeclarationSyntax classNode)
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Description = Docstrings.Parse.GetSummary(classNode),
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new List<string>()
            };

            foreach (var prop in classNode.Members.OfType<PropertyDeclarationSyntax>())
            {
                var propName = prop.Identifier.Text;
                var typeName = prop.Type.ToString();
                
                var isNullable = typeName.EndsWith("?");
                var baseType = typeName.TrimEnd('?');
                
                var propSchema = new OpenApiSchema
                {
                    Type = MapType(baseType),
                    Description = Docstrings.Parse.GetSummary(prop)
                };

                schema.Properties[propName] = propSchema;

                if (!isNullable)
                {
                    schema.Required.Add(propName);
                }
            }

            if (!schema.Required.Any()) schema.Required = null;

            return schema;
        }

        private static string MapType(string csharpType)
        {
            return csharpType switch
            {
                "int" or "long" or "short" => "integer",
                "float" or "double" or "decimal" => "number",
                "bool" => "boolean",
                "string" => "string",
                _ => "object"
            };
        }
    }
}
