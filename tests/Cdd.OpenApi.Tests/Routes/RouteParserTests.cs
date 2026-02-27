using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.Routes;
using System.Linq;

namespace Cdd.OpenApi.Tests.Routes
{
    public class RouteParserTests
    {
        [Fact]
        public void ToPaths_ParsesControllerMethods_ToOpenApiPaths()
        {
            var code = @"
            public class PetsController 
            {
                /// <summary>
                /// Gets all pets.
                /// </summary>
                [HttpGet(""/pets"")]
                public void GetPets(int limit, string type) {}

                [HttpPost(""/pets"")]
                public void CreatePet() {}

                [HttpGet(""/pets/{id}"")]
                public void GetPetById(int id) {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);

            Assert.NotNull(paths);
            Assert.Equal(2, paths.Count);

            Assert.True(paths.ContainsKey("/pets"));
            var petsPath = paths["/pets"];
            
            // GET /pets
            Assert.NotNull(petsPath.Get);
            Assert.Equal("GetPets", petsPath.Get.OperationId);
            Assert.Equal("Gets all pets.", petsPath.Get.Summary);
            Assert.Equal(2, petsPath.Get.Parameters?.Count);
            Assert.Equal("limit", petsPath.Get.Parameters?[0].Name);
            Assert.Equal("query", petsPath.Get.Parameters?[0].In);
            Assert.Equal("integer", petsPath.Get.Parameters?[0].Schema?.Type);

            // POST /pets
            Assert.NotNull(petsPath.Post);
            Assert.Equal("CreatePet", petsPath.Post.OperationId);

            // GET /pets/{id}
            Assert.True(paths.ContainsKey("/pets/{id}"));
            var petIdPath = paths["/pets/{id}"];
            Assert.NotNull(petIdPath.Get);
            Assert.Equal("GetPetById", petIdPath.Get.OperationId);
            Assert.Single(petIdPath.Get.Parameters!);
            Assert.Equal("id", petIdPath.Get.Parameters?[0].Name);
            Assert.Equal("path", petIdPath.Get.Parameters?[0].In);
        }

        [Fact]
        public void ToPaths_MethodWithNoAttribute_IsIgnored()
        {
            var code = @"
            public class PetsController 
            {
                public void HelperMethod() {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);

            Assert.Empty(paths);
        }

        [Fact]
        public void SetOperation_SetsAllVerbs()
        {
            var code = @"
            public class PetsController 
            {
                [HttpPut] public void Put() {}
                [HttpDelete] public void Delete() {}
                [HttpOptions] public void Options() {}
                [HttpHead] public void Head() {}
                [HttpPatch] public void Patch() {}
                [HttpTrace] public void Trace() {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var rootPath = paths["/"];

            Assert.NotNull(rootPath.Put);
            Assert.NotNull(rootPath.Delete);
            Assert.NotNull(rootPath.Options);
            Assert.NotNull(rootPath.Head);
            Assert.NotNull(rootPath.Patch);
            Assert.NotNull(rootPath.Trace);
        }
    }
}
