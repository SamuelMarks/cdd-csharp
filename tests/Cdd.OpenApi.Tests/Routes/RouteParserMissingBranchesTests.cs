using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.Models;
using System.Collections.Generic;

namespace Cdd.OpenApi.Tests.Routes
{
    public partial class RouteParserMissingBranchesTests
    {
        [Fact]
        public void Parse_ResponseHeaders_MissingBranches()
        {
            var code = @"
            using Microsoft.AspNetCore.Mvc;
            public class CovApi
            {
                /// <response code=""200"" header=""X-Test1"" header-description=""desc"" header-required=""true"" header-deprecated=""true"" header-example=""123"" header-examples=""a:1,b:2"" header-style=""simple"" header-explode=""true"" header-schema=""integer"" header-content=""app/json:integer"">Ok</response>
                /// <response code=""400"" header=""X-Test2"" header-required=""invalid"" header-deprecated=""invalid"" header-explode=""invalid"" header-examples=""invalid_format"" header-content=""invalid_format"">Bad</response>
                /// <response header=""X-Test3""></response>
                /// <link link=""MyLink"" link-operationRef=""op"" link-description=""desc"" link-requestBody=""req"" link-parameters=""p1:v1,p2:v2"" link-serverUrl=""http://url""></link>
                [HttpGet(""/cov"")]
                public void Get() {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var getOp = paths["/cov"].Get;

            Assert.NotNull(getOp);
            Assert.True(getOp.Responses.ContainsKey("200"));
            Assert.True(getOp.Responses.ContainsKey("400"));
            Assert.True(getOp.Responses.ContainsKey("default"));
        }
    }
}
namespace Cdd.OpenApi.Tests.Routes
{
    public partial class RouteParserMissingBranchesTests
    {
        [Fact]
        public void Parse_MapType_EmptyReturnTypes_MissingBranches()
        {
            var code = @"
            using Microsoft.AspNetCore.Mvc;
            using System.Threading.Tasks;
            public class RetApi
            {
                [HttpGet(""/t1"")] public Task<int> GetT1() { return null; }
                [HttpGet(""/t2"")] public Task GetT2() { return null; }
                [HttpGet(""/t3"")] public Task<IActionResult> GetT3() { return null; }
                [HttpGet(""/t4"")] public System.Threading.Tasks.Task<int> GetT4() { return null; }
                [HttpGet(""/t5"")] public IActionResult GetT5() { return null; }
                [HttpGet(""/t6"")] public void GetT6() { }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);

            Assert.NotNull(paths);
            Assert.Equal("integer", paths["/t1"].Get.Responses["200"].Content["application/json"].Schema.Type);
            Assert.Equal("integer", paths["/t4"].Get.Responses["200"].Content["application/json"].Schema.Type);
            Assert.Null(paths["/t2"].Get.Responses["200"].Content);
            Assert.Null(paths["/t3"].Get.Responses["200"].Content);
            Assert.Null(paths["/t5"].Get.Responses["200"].Content);
            Assert.Null(paths["/t6"].Get.Responses["200"].Content);
        }

        [Fact]
        public void Parse_Encoding_MissingBranches()
        {
            var code = @"
            using Microsoft.AspNetCore.Mvc;
            public class EncApi
            {
                [HttpPost(""/enc"")]
                public void Post(
                    [FromBody]
                    [Encoding(""prop1"", ""app/json"", Style=""form"", Explode=true, AllowReserved=true)]
                    [Encoding(""prop2"", ""app/json"", Style=""form"", Explode=false, AllowReserved=false)]
                    object body
                ) {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            Assert.NotNull(paths);
        }

        [Fact]
        public void Parse_Links_MissingBranches()
        {
            var code = @"
            using Microsoft.AspNetCore.Mvc;
            public class LnkApi
            {
                /// <response code=""200"">OK
                ///   <link link=""MyLink"" link-operationId=""opId"" link-operationRef=""opRef"" link-description=""desc"" link-requestBody=""req"" link-parameters=""p1:v1,p2:v2"" link-serverUrl=""http://url""></link>
                ///   <link link=""MyLink2""></link>
                /// </response>
                [HttpGet(""/lnk"")]
                public void Get() {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            Assert.NotNull(paths);
        }
    }
}
