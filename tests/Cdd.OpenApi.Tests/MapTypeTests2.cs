using Xunit;
using Cdd.OpenApi.Routes;
using Cdd.OpenApi.Classes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Cdd.OpenApi.Tests
{
    public class MapTypeTests2
    {
        [Fact]
        public void RouteParseMapType_HandlesDecimal_Double_Float_Bool()
        {
            var code = @"
            public class C {
                [HttpGet] public void M(decimal d, double db, float f, bool b, object o) {}
            }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            
            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var p = paths["/"].Get?.Parameters;
            
            Assert.Equal("number", p?[0].Schema?.Type); // decimal
            Assert.Equal("number", p?[1].Schema?.Type); // double
            Assert.Equal("number", p?[2].Schema?.Type); // float
            Assert.Equal("boolean", p?[3].Schema?.Type); // bool
            Assert.Equal("string", p?[4].Schema?.Type); // object fallback
        }
        
        [Fact]
        public void ClassParseMapType_HandlesDecimal_Double_Float_Bool()
        {
            var code = @"
            public class C {
                public decimal D {get;set;}
                public double Db {get;set;}
                public float F {get;set;}
                public bool B {get;set;}
            }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            
            var schema = Cdd.OpenApi.Classes.Parse.ToSchema(classNode);
            var p = schema.Properties;
            
            Assert.Equal("number", p?["D"].Type); // decimal
            Assert.Equal("number", p?["Db"].Type); // double
            Assert.Equal("number", p?["F"].Type); // float
            Assert.Equal("boolean", p?["B"].Type); // bool
        }
    }
}
