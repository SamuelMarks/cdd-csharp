using Xunit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Classes;
using System.Collections.Generic;

namespace Cdd.OpenApi.Tests.Classes
{
    public class ClassEmitterTests
    {
        [Fact]
        public void ToClass_ValidSchema_GeneratesCorrectClass()
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Description = "A pet schema",
                Example = "examplePet",
                ExternalDocs = new OpenApiExternalDocs { Url = "http://pet.docs" },
                Discriminator = new OpenApiDiscriminator { 
                    PropertyName = "petType",
                    DefaultMapping = "dog",
                    Mapping = new Dictionary<string, string> { { "cat", "#/components/schemas/Cat" } }
                },
                Xml = new OpenApiXml {
                    Name = "Pet",
                    Namespace = "http://pet.xml",
                    Prefix = "p",
                    NodeType = "element",
                    Attribute = true,
                    Wrapped = false
                },
                Required = new List<string> { "Id" },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["Id"] = new OpenApiSchema { 
                        Type = "integer", 
                        Description = "Pet ID",
                        Example = 123,
                        ExternalDocs = new OpenApiExternalDocs { Url = "http://pet.docs/id" },
                        Discriminator = new OpenApiDiscriminator { 
                            PropertyName = "idType",
                            DefaultMapping = "num",
                            Mapping = new Dictionary<string, string> { { "str", "#/components/schemas/StrId" } }
                        },
                        Xml = new OpenApiXml {
                            Name = "Id",
                            Namespace = "http://id.xml",
                            Prefix = "i",
                            NodeType = "attribute",
                            Attribute = false,
                            Wrapped = true
                        }
                    },
                    ["Name"] = new OpenApiSchema { Type = "string" },
                    ["IsVaccinated"] = new OpenApiSchema { Type = "boolean" },
                    ["Data"] = new OpenApiSchema { Type = "unknown_type" }
                }
            };

            var classNode = Cdd.OpenApi.Classes.Emit.ToClass("Pet", schema);
            var code = classNode.ToFullString();

            Assert.Contains("/// <summary>", code);
            Assert.Contains("/// A pet schema", code);
            Assert.Contains("/// <example>\n/// examplePet\n/// </example>", code.Replace("\r", ""));
            Assert.Contains("/// <externalDocs>\n/// http://pet.docs\n/// </externalDocs>", code.Replace("\r", ""));
            Assert.Contains("/// <discriminator>\n/// petType\n/// </discriminator>", code.Replace("\r", ""));
            Assert.Contains("/// <discriminator-defaultMapping>\n/// dog\n/// </discriminator-defaultMapping>", code.Replace("\r", ""));
            Assert.Contains("/// <discriminator-mapping>\n/// cat:#/components/schemas/Cat\n/// </discriminator-mapping>", code.Replace("\r", ""));
            Assert.Contains("/// <xml-name>\n/// Pet\n/// </xml-name>", code.Replace("\r", ""));
            Assert.Contains("/// <xml-namespace>\n/// http://pet.xml\n/// </xml-namespace>", code.Replace("\r", ""));
            Assert.Contains("/// <xml-prefix>\n/// p\n/// </xml-prefix>", code.Replace("\r", ""));
            Assert.Contains("/// <xml-nodeType>\n/// element\n/// </xml-nodeType>", code.Replace("\r", ""));
            Assert.Contains("/// <xml-attribute>\n/// true\n/// </xml-attribute>", code.Replace("\r", ""));
            Assert.Contains("/// <xml-wrapped>\n/// false\n/// </xml-wrapped>", code.Replace("\r", ""));

            Assert.Contains("public class Pet", code);
            
            // Id is required
            Assert.Contains("public int Id { get; set; }", code);
            Assert.Contains("/// <summary>", code);
            Assert.Contains("/// Pet ID", code);
            Assert.Contains("    /// <example>\n    /// 123\n    /// </example>", code.Replace("\r", ""));
            Assert.Contains("    /// <externalDocs>\n    /// http://pet.docs/id\n    /// </externalDocs>", code.Replace("\r", ""));
            Assert.Contains("    /// <discriminator>\n    /// idType\n    /// </discriminator>", code.Replace("\r", ""));
            Assert.Contains("    /// <discriminator-defaultMapping>\n    /// num\n    /// </discriminator-defaultMapping>", code.Replace("\r", ""));
            Assert.Contains("    /// <discriminator-mapping>\n    /// str:#/components/schemas/StrId\n    /// </discriminator-mapping>", code.Replace("\r", ""));
            Assert.Contains("    /// <xml-name>\n    /// Id\n    /// </xml-name>", code.Replace("\r", ""));
            Assert.Contains("    /// <xml-namespace>\n    /// http://id.xml\n    /// </xml-namespace>", code.Replace("\r", ""));
            Assert.Contains("    /// <xml-prefix>\n    /// i\n    /// </xml-prefix>", code.Replace("\r", ""));
            Assert.Contains("    /// <xml-nodeType>\n    /// attribute\n    /// </xml-nodeType>", code.Replace("\r", ""));
            Assert.Contains("    /// <xml-attribute>\n    /// false\n    /// </xml-attribute>", code.Replace("\r", ""));
            Assert.Contains("    /// <xml-wrapped>\n    /// true\n    /// </xml-wrapped>", code.Replace("\r", ""));

            // Name is optional string
            Assert.Contains("public string? Name { get; set; }", code);

            // IsVaccinated is optional bool
            Assert.Contains("public bool? IsVaccinated { get; set; }", code);
            
            // Data is unknown fallback to object
            Assert.Contains("public object Data { get; set; }", code);
        }

        [Fact]
        public void MapTypeToCSharp_MapsNumberToDouble()
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["Weight"] = new OpenApiSchema { Type = "number" }
                }
            };

            var classNode = Cdd.OpenApi.Classes.Emit.ToClass("Pet", schema);
            var code = classNode.ToFullString();

            Assert.Contains("public double? Weight { get; set; }", code);
        }

        [Fact]
        public void ToSchema_FullCoverage()
        {
            var code = @"
            /// <example>MyExample</example>
            /// <discriminator>petType</discriminator>
            /// <discriminator-defaultMapping>dog</discriminator-defaultMapping>
            /// <externalDocs>https://pet.docs</externalDocs>
            /// <discriminator-mapping>cat:#/components/schemas/Cat, bird:#/components/schemas/Bird</discriminator-mapping>
            /// <xml-name>Pet</xml-name>
            /// <xml-namespace>http://pet.xml</xml-namespace>
            /// <xml-prefix>p</xml-prefix>
            /// <xml-nodeType>element</xml-nodeType>
            /// <xml-attribute>true</xml-attribute>
            /// <xml-wrapped>false</xml-wrapped>
            public class FullClass
            {
                /// <xml-wrapped>true</xml-wrapped>
                /// <xml-attribute>false</xml-attribute>
                /// <xml-nodeType>attribute</xml-nodeType>
                /// <xml-prefix>i</xml-prefix>
                /// <xml-namespace>http://id.xml</xml-namespace>
                /// <xml-name>Id</xml-name>
                /// <externalDocs>http://pet.docs/id</externalDocs>
                /// <discriminator-mapping>str:#/components/schemas/StrId</discriminator-mapping>
                /// <discriminator-defaultMapping>num</discriminator-defaultMapping>
                /// <discriminator>idType</discriminator>
                /// <example>123</example>
                public int Id { get; set; }
            }
            ";

            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First();

            var schema = Cdd.OpenApi.Classes.Parse.ToSchema(classNode);

            Assert.NotNull(schema);
            Assert.Equal("MyExample", schema.Example);
            Assert.NotNull(schema.Discriminator);
            Assert.Equal("petType", schema.Discriminator.PropertyName);
            Assert.Equal("dog", schema.Discriminator.DefaultMapping);
            Assert.True(schema.Discriminator.Mapping.ContainsKey("cat"));
            Assert.Equal("https://pet.docs", schema.ExternalDocs?.Url);
            
            Assert.NotNull(schema.Xml);
            Assert.Equal("Pet", schema.Xml.Name);
            Assert.Equal("http://pet.xml", schema.Xml.Namespace);
            Assert.Equal("p", schema.Xml.Prefix);
            Assert.Equal("element", schema.Xml.NodeType);
            Assert.True(schema.Xml.Attribute);
            Assert.False(schema.Xml.Wrapped);

            Assert.NotNull(schema.Properties);
            Assert.True(schema.Properties.ContainsKey("Id"));
            var idProp = schema.Properties["Id"];
            
            Assert.True(idProp.Xml?.Wrapped);
            Assert.False(idProp.Xml?.Attribute);
            Assert.Equal("attribute", idProp.Xml?.NodeType);
            Assert.Equal("i", idProp.Xml?.Prefix);
            Assert.Equal("http://id.xml", idProp.Xml?.Namespace);
            Assert.Equal("Id", idProp.Xml?.Name);
            Assert.Equal("http://pet.docs/id", idProp.ExternalDocs?.Url);
            Assert.Equal("num", idProp.Discriminator?.DefaultMapping);
            Assert.Equal("idType", idProp.Discriminator?.PropertyName);
            Assert.True(idProp.Discriminator?.Mapping?.ContainsKey("str"));
            Assert.Equal("123", idProp.Example);
        }
    }
}