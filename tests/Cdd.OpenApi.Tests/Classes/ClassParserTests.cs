using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.Classes;
using System.Linq;

namespace Cdd.OpenApi.Tests.Classes
{
    public class ClassParserTests
    {
        [Fact]
        public void ToSchema_ParsesClassWithProperties_ToOpenApiSchema()
        {
            var code = @"
            /// <summary>
            /// A representation of a pet.
            /// </summary>
            public class Pet 
            {
                /// <summary>The pet's ID</summary>
                public int Id { get; set; }

                public string? Name { get; set; }

                public bool IsVaccinated { get; set; }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var schema = Cdd.OpenApi.Classes.Parse.ToSchema(classNode);

            Assert.Equal("object", schema.Type);
            Assert.Equal("A representation of a pet.", schema.Description);
            
            Assert.NotNull(schema.Properties);
            Assert.Equal(3, schema.Properties.Count);
            
            Assert.True(schema.Properties.ContainsKey("Id"));
            Assert.Equal("integer", schema.Properties["Id"].Type);
            Assert.Equal("The pet's ID", schema.Properties["Id"].Description);

            Assert.True(schema.Properties.ContainsKey("Name"));
            Assert.Equal("string", schema.Properties["Name"].Type);
            
            Assert.True(schema.Properties.ContainsKey("IsVaccinated"));
            Assert.Equal("boolean", schema.Properties["IsVaccinated"].Type);

            Assert.NotNull(schema.Required);
            Assert.Equal(2, schema.Required.Count);
            Assert.Contains("Id", schema.Required);
            Assert.Contains("IsVaccinated", schema.Required);
            Assert.DoesNotContain("Name", schema.Required);
        }

        [Fact]
        public void ToSchema_NoRequiredProperties_RequiredIsNull()
        {
            var code = @"
            public class Pet 
            {
                public int? Id { get; set; }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var schema = Cdd.OpenApi.Classes.Parse.ToSchema(classNode);
            Assert.Null(schema.Required);
        }

        [Fact]
        public void MapType_UnknownType_MapsToObject()
        {
            var code = @"
            public class Container 
            {
                public CustomType Data { get; set; }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var schema = Cdd.OpenApi.Classes.Parse.ToSchema(classNode);
            Assert.Equal("object", schema.Properties!["Data"].Type);
        }
    }
}
