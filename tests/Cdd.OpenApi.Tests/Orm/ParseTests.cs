using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.Orm;

namespace Cdd.OpenApi.Tests.Orm
{
    public class ParseTests
    {
        [Fact]
        public void ToSchemas_ParsesDbContext_ReturnsSchemas()
        {
            var code = @"
            public class AppDbContext : DbContext
            {
                /// <summary>Represents a User</summary>
                public DbSet<User> Users { get; set; }
                public DbSet<Order> Orders { get; set; }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var schemas = Cdd.OpenApi.Orm.Parse.ToSchemas(classNode);

            Assert.Equal(2, schemas.Count);
            Assert.True(schemas.ContainsKey("User"));
            Assert.Equal("Represents a User", schemas["User"].Description);
            Assert.True(schemas.ContainsKey("Order"));
            Assert.Equal("Entity for Order", schemas["Order"].Description);
        }
    }
}
