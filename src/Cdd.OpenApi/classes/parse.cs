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
            var example = Docstrings.Parse.GetTag(classNode, "example");
            var discriminatorName = Docstrings.Parse.GetTag(classNode, "discriminator");
            var defaultMapping = Docstrings.Parse.GetTag(classNode, "discriminator-defaultMapping");
            var externalDocsUrl = Docstrings.Parse.GetTag(classNode, "externalDocs");
            
            var mappingStr = Docstrings.Parse.GetTag(classNode, "discriminator-mapping");
            Dictionary<string, string>? mapping = null;
            if (!string.IsNullOrEmpty(mappingStr))
            {
                mapping = new Dictionary<string, string>();
                foreach (var m in mappingStr.Split(','))
                {
                    var parts = m.Split(':');
                    if (parts.Length == 2)
                    {
                        mapping[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }

            var xmlName = Docstrings.Parse.GetTag(classNode, "xml-name");
            var xmlNamespace = Docstrings.Parse.GetTag(classNode, "xml-namespace");
            var xmlPrefix = Docstrings.Parse.GetTag(classNode, "xml-prefix");
            var xmlNodeType = Docstrings.Parse.GetTag(classNode, "xml-nodeType");
            var xmlAttributeStr = Docstrings.Parse.GetTag(classNode, "xml-attribute");
            var xmlWrappedStr = Docstrings.Parse.GetTag(classNode, "xml-wrapped");

            OpenApiDiscriminator? discriminator = null;
            if (discriminatorName != null) {
                discriminator = new OpenApiDiscriminator { PropertyName = discriminatorName, DefaultMapping = defaultMapping, Mapping = mapping };
            }

            OpenApiXml? xml = null;
            if (xmlName != null || xmlNamespace != null || xmlPrefix != null || xmlNodeType != null || xmlAttributeStr != null || xmlWrappedStr != null) {
                xml = new OpenApiXml {
                    Name = xmlName,
                    Namespace = xmlNamespace,
                    Prefix = xmlPrefix,
                    NodeType = xmlNodeType,
                    Attribute = xmlAttributeStr != null ? xmlAttributeStr.ToLower() == "true" : null,
                    Wrapped = xmlWrappedStr != null ? xmlWrappedStr.ToLower() == "true" : null
                };
            }

            var schema = new OpenApiSchema
            {
                Type = "object",
                Description = Docstrings.Parse.GetSummary(classNode),
                Example = example,
                Discriminator = discriminator,
                Xml = xml,
                ExternalDocs = externalDocsUrl != null ? new OpenApiExternalDocs { Url = externalDocsUrl } : null,
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new List<string>()
            };

            foreach (var prop in classNode.Members.OfType<PropertyDeclarationSyntax>())
            {
                var propName = prop.Identifier.Text;
                var typeName = prop.Type.ToString();
                
                var isNullable = typeName.EndsWith("?");
                var baseType = typeName.TrimEnd('?');
                
                var propExample = Docstrings.Parse.GetTag(prop, "example");
                var propDiscriminatorName = Docstrings.Parse.GetTag(prop, "discriminator");
                var propDiscriminatorDefault = Docstrings.Parse.GetTag(prop, "discriminator-defaultMapping");
                var propExternalDocsUrl = Docstrings.Parse.GetTag(prop, "externalDocs");

                var propMappingStr = Docstrings.Parse.GetTag(prop, "discriminator-mapping");
                Dictionary<string, string>? propMapping = null;
                if (!string.IsNullOrEmpty(propMappingStr))
                {
                    propMapping = new Dictionary<string, string>();
                    foreach (var m in propMappingStr.Split(','))
                    {
                        var parts = m.Split(':');
                        if (parts.Length == 2)
                        {
                            propMapping[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                }

                OpenApiDiscriminator? propDiscriminator = null;
                if (propDiscriminatorName != null) {
                    propDiscriminator = new OpenApiDiscriminator { PropertyName = propDiscriminatorName, DefaultMapping = propDiscriminatorDefault, Mapping = propMapping };
                }

                var propXmlName = Docstrings.Parse.GetTag(prop, "xml-name");
                var propXmlNamespace = Docstrings.Parse.GetTag(prop, "xml-namespace");
                var propXmlPrefix = Docstrings.Parse.GetTag(prop, "xml-prefix");
                var propXmlNodeType = Docstrings.Parse.GetTag(prop, "xml-nodeType");
                var propXmlAttributeStr = Docstrings.Parse.GetTag(prop, "xml-attribute");
                var propXmlWrappedStr = Docstrings.Parse.GetTag(prop, "xml-wrapped");

                OpenApiXml? propXml = null;
                if (propXmlName != null || propXmlNamespace != null || propXmlPrefix != null || propXmlNodeType != null || propXmlAttributeStr != null || propXmlWrappedStr != null) {
                    propXml = new OpenApiXml {
                        Name = propXmlName,
                        Namespace = propXmlNamespace,
                        Prefix = propXmlPrefix,
                        NodeType = propXmlNodeType,
                        Attribute = propXmlAttributeStr != null ? propXmlAttributeStr.ToLower() == "true" : null,
                        Wrapped = propXmlWrappedStr != null ? propXmlWrappedStr.ToLower() == "true" : null
                    };
                }

                var propSchema = new OpenApiSchema
                {
                    Type = MapType(baseType),
                    Description = Docstrings.Parse.GetSummary(prop),
                    Example = propExample,
                    Discriminator = propDiscriminator,
                    Xml = propXml,
                    ExternalDocs = propExternalDocsUrl != null ? new OpenApiExternalDocs { Url = propExternalDocsUrl } : null
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
