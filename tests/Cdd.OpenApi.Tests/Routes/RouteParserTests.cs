using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.Routes;
using System.Linq;
using Cdd.OpenApi.Models;
using System.Collections.Generic;

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
        [Fact]
        public void ToPaths_WithFromBody_SetsRequestBody()
        {
            var code = @"
            public class UserApi
            {
                [HttpPost(""/users"")]
                public Task<User> CreateUser([FromBody] User user) { return Task.FromResult(user); }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);

            var postOp = paths["/users"].Post;
            Assert.NotNull(postOp);
            Assert.NotNull(postOp.RequestBody);
            Assert.Equal("Request body for user", postOp.RequestBody.Description);
            Assert.True(postOp.RequestBody.Required);
            Assert.NotNull(postOp.RequestBody.Content);
            Assert.True(postOp.RequestBody.Content.ContainsKey("application/json"));
            Assert.Equal("string", postOp.RequestBody.Content["application/json"].Schema?.Type); // "User" falls back to string in current stub MapType
        }
        [Fact]
        public void ToPaths_WithDocstringResponses_ParsesHeadersAndLinks()
        {
            var code = @"
            public class UserApi
            {
                /// <summary>Delete</summary>
                /// <response code=""204"" header=""X-RateLimit"" header-examples=""ex1:100,ex2:200"" header-content=""application/json:integer"" link=""GetUsers"">Success with header</response>
                /// <response code=""404"">Not Found</response>
                [HttpDelete(""/users/{id}"")]
                public System.Threading.Tasks.Task<string> DeleteUser(int id) { return null; }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var op = paths["/users/{id}"].Delete;

            Assert.NotNull(op);
            Assert.NotNull(op.Responses);
            Assert.True(op.Responses.ContainsKey("204"));
            Assert.Equal("Success with header", op.Responses["204"].Description);
            Assert.NotNull(op.Responses["204"].Headers);
            Assert.True(op.Responses["204"].Headers.ContainsKey("X-RateLimit"));
            Assert.NotNull(op.Responses["204"].Links);
            Assert.True(op.Responses["204"].Links.ContainsKey("GetUsers"));
            Assert.True(op.Responses.ContainsKey("404"));
        }
        [Fact]
        public void ToPaths_WithParameterAttributes_ParsesThem()
        {
            var code = @"
            public class UserApi
            {
                [HttpGet(""/users"")]
                public void GetUsers([Obsolete] string oldId, [AllowEmptyValue] string q = """", int limit = 10) {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var op = paths["/users"].Get;

            Assert.NotNull(op);
            Assert.NotNull(op.Parameters);
            Assert.Equal(3, op.Parameters.Count);

            var oldId = op.Parameters[0];
            Assert.True(oldId.Deprecated);

            var q = op.Parameters[1];
            Assert.True(q.AllowEmptyValue);
            Assert.Equal("", q.Example);

            var limit = op.Parameters[2];
            Assert.Equal("10", limit.Example);
        }
        [Fact]
        public void ToInterface_WithParameterAttributes_GeneratesThem()
        {
            var paths = new OpenApiPaths
            {
                ["/users"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetUsers",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "oldId", Schema = new OpenApiSchema { Type = "integer" }, Deprecated = true, Example = 42 },
                            new OpenApiParameter { Name = "q", Schema = new OpenApiSchema { Type = "string" }, AllowEmptyValue = true, Example = "" }
                        }
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IUserApi", paths);
            var code = interfaceNode.ToFormattedString();

            AssertHelper.ContainsNoWhitespace("[System.Obsolete] int oldId = 42", code);
            AssertHelper.ContainsNoWhitespace("[AllowEmptyValue] string q = \"\"", code);
        }
        [Fact]
        public void ToPaths_WithComplexParameterAttributes_ParsesThem()
        {
            var code = @"
            public class ComplexApi
            {
                [HttpGet(""/complex"")]
                public void GetComplex([Style(""matrix"")] [Explode] [AllowReserved] string ids) {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var op = paths["/complex"].Get;

            Assert.NotNull(op);
            Assert.NotNull(op.Parameters);
            Assert.Single(op.Parameters);

            var ids = op.Parameters[0];
            Assert.Equal("matrix", ids.Style);
            Assert.True(ids.Explode);
            Assert.True(ids.AllowReserved);
        }
        [Fact]
        public void ToPaths_WithContentParameter_ParsesIt()
        {
            var code = @"
            public class ContentApi
            {
                [HttpGet(""/content"")]
                public void GetContent([Content(""application/json"", ""object"")] string jsonParam) {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var op = paths["/content"].Get;

            Assert.NotNull(op);
            Assert.NotNull(op.Parameters);

            var p = op.Parameters[0];
            Assert.Null(p.Schema);
            Assert.NotNull(p.Content);
            Assert.True(p.Content.ContainsKey("application/json"));
            Assert.Equal("object", p.Content["application/json"].Schema?.Type);
        }
        [Fact]
        public void ToInterface_WithComplexParameterAttributes_GeneratesThem()
        {
            var paths = new OpenApiPaths
            {
                ["/complex"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetComplex",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter
                            {
                                Name = "ids",
                                Schema = new OpenApiSchema { Type = "string" },
                                Style = "matrix",
                                Explode = true,
                                AllowReserved = true
                            },
                            new OpenApiParameter
                            {
                                Name = "payload",
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = "object" } } }
                                }
                            }
                        }
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IComplexApi", paths);
            var code = interfaceNode.ToFormattedString();

            AssertHelper.ContainsNoWhitespace("[Style(\"matrix\")]", code);
            AssertHelper.ContainsNoWhitespace("[Explode]", code);
            AssertHelper.ContainsNoWhitespace("[AllowReserved]", code);
            AssertHelper.ContainsNoWhitespace("[Content(\"application/json\", \"object\")] string payload", code);
        }
        [Fact]
        public void ToPaths_WithServerDocstrings_ParsesThem()
        {
            var code = @"
            public class ServerApi
            {
                /// <server url=""https://{region}.api.com"" name=""prod"">
                ///   Prod server
                ///   <variable name=""region"" default=""us-east"">
                ///     The region
                ///     <enum>us-east</enum>
                ///     <enum>eu-west</enum>
                ///   </variable>
                /// </server>
                [HttpGet(""/srv"")]
                public void Get() {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var op = paths["/srv"].Get;

            Assert.NotNull(op);
            Assert.NotNull(op.Servers);
            Assert.Single(op.Servers);

            var srv = op.Servers[0];
            Assert.Equal("https://{region}.api.com", srv.Url);
            Assert.Equal("Prod server", srv.Description);
            Assert.Equal("prod", srv.Name);

            Assert.NotNull(srv.Variables);
            Assert.True(srv.Variables.ContainsKey("region"));

            var v = srv.Variables["region"];
            Assert.Equal("us-east", v.Default);
            Assert.Equal("The region", v.Description);
            Assert.NotNull(v.Enum);
            Assert.Equal(2, v.Enum.Count);
            Assert.Equal("us-east", v.Enum[0]);
            Assert.Equal("eu-west", v.Enum[1]);
        }
        [Fact]
        public void ToPaths_WithCallback_ParsesThem()
        {
            var code = @"
            public class CallbackApi
            {
                /// <callback name=""onEvent"" expression=""{$request.query.url}"">
                ///   Callback info
                /// </callback>
                [HttpPost(""/subscribe"")]
                public void Post() {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var op = paths["/subscribe"].Post;

            Assert.NotNull(op);
            Assert.NotNull(op.Callbacks);
            Assert.True(op.Callbacks.ContainsKey("onEvent"));

            var cb = op.Callbacks["onEvent"];
            Assert.True(cb.ContainsKey("{$request.query.url}"));
            Assert.Equal("Callback info", cb["{$request.query.url}"].Post?.Description);
        }
        [Fact]
        public void ToPaths_WithComplexLinks_ParsesThem()
        {
            var code = @"
            public class LinkApi
            {
                /// <response code=""200"" link=""GetUser"" link-operationRef=""#/paths/~1users/get"" link-description=""Gets the user"" link-requestBody=""$response.body#/id"" link-parameters=""userId:$response.body#/id"" link-serverUrl=""https://alt.api.com"">
                ///   OK
                /// </response>
                [HttpGet(""/links"")]
                public void Get() {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var op = paths["/links"].Get;

            Assert.NotNull(op);
            Assert.NotNull(op.Responses);

            var resp = op.Responses["200"];
            Assert.NotNull(resp.Links);
            Assert.True(resp.Links.ContainsKey("GetUser"));

            var link = resp.Links["GetUser"];
            Assert.Equal("GetUser", link.OperationId);
            Assert.Equal("#/paths/~1users/get", link.OperationRef);
            Assert.Equal("Gets the user", link.Description);
            Assert.Equal("$response.body#/id", link.RequestBody);
            Assert.Equal("https://alt.api.com", link.Server?.Url);
            Assert.NotNull(link.Parameters);
            Assert.Equal("$response.body#/id", link.Parameters["userId"]);
        }
        [Fact]
        public void ToPaths_WithOperationMetadata_ParsesThem()
        {
            var code = @"
            public class MetadataApi
            {
                /// <description>The desc</description>
                /// <externalDocs>https://docs.api.com</externalDocs>
                /// <tags>users,admin</tags>
                /// <security name=""OAuth2"" scopes=""read:users"">Auth</security>
                /// <param name=""q1"">The q1 desc</param>
                [Obsolete]
                [HttpGet(""/meta"")]
                public float Get(
                    [FromQuery, Explode] string q1,
                    [FromQuery, Explode(true)] string q2,
                    [FromQuery, Explode(false)] string q3,
                    [FromQuery, Style(""form"")] string q4,
                    [FromQuery, AllowReserved] string q5,
                    [FromQuery, AllowEmptyValue] string q6,
                    [FromQuery, Example(""123"")] string q7,
                    [FromQuery, Content(""application/json"", ""integer"")] string q8,
                    [FromQuery] bool b
                ) { return 0f; }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var op = paths["/meta"].Get;

            Assert.NotNull(op);
            Assert.Equal("The desc", op.Description);
            Assert.Equal("https://docs.api.com", op.ExternalDocs?.Url);
            Assert.True(op.Deprecated);
            Assert.NotNull(op.Tags);
            Assert.Contains("users", op.Tags);
            Assert.Contains("admin", op.Tags);

            Assert.NotNull(op.Security);
            Assert.Single(op.Security);
            Assert.True(op.Security[0].ContainsKey("OAuth2"));
            Assert.Contains("read:users", op.Security[0]["OAuth2"]);

            Assert.Equal(9, op.Parameters.Count);
        }
        [Fact]
        public void ToPaths_WithParameterExamples_ParsesThem()
        {
            var code = @"
            public class ExamplesApi
            {
                [HttpGet(""/ex"")]
                public void Get([Examples(""ex1"", ""one"", ""ex2"", ""two"")] string id) {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var op = paths["/ex"].Get;

            Assert.NotNull(op);
            Assert.NotNull(op.Parameters);

            var p = op.Parameters[0];
            Assert.NotNull(p.Examples);
            Assert.Equal(2, p.Examples.Count);
            Assert.Equal("one", p.Examples["ex1"].Value);
            Assert.Equal("two", p.Examples["ex2"].Value);
        }
        [Fact]
        public void ToPaths_WithEncodingAttributes_ParsesThem()
        {
            var code = @"
            using Cdd.OpenApi.Models;
            public class EncodeApi
            {
                [HttpPost(""/enc"")]
                public void Post([FromBody]
                    [Encoding(""profileImage"", ""image/png"", Style=""form"", Explode=false, AllowReserved=false)]
                    object body) {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var op = paths["/enc"].Post;

            Assert.NotNull(op);
            Assert.NotNull(op.RequestBody);
            Assert.True(op.RequestBody.Content.ContainsKey("application/x-www-form-urlencoded"));

            var encs = op.RequestBody.Content["application/x-www-form-urlencoded"].Encoding;
            Assert.NotNull(encs);
            Assert.True(encs.ContainsKey("profileImage"));

            var e = encs["profileImage"];
            Assert.Equal("image/png", e.ContentType);
            Assert.Equal("form", e.Style);
            Assert.False(e.Explode);
            Assert.False(e.AllowReserved);
        }

        [Fact]
        public void MapType_MissingBranches_Coverage()
        {
            var code = @"
            using Microsoft.AspNetCore.Mvc;
            public class TypeApi
            {
                [HttpPost(""/t"")]
                public void Post(long p1, short p2, double p3, decimal p4, bool p5, string p6, object p7) {}
            }";

            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First();
            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var op = paths["/t"].Post;

            Assert.Equal("integer", op.Parameters.First(p => p.Name == "p1").Schema.Type);
            Assert.Equal("integer", op.Parameters.First(p => p.Name == "p2").Schema.Type);
            Assert.Equal("number", op.Parameters.First(p => p.Name == "p3").Schema.Type);
            Assert.Equal("number", op.Parameters.First(p => p.Name == "p4").Schema.Type);
            Assert.Equal("boolean", op.Parameters.First(p => p.Name == "p5").Schema.Type);
            Assert.Equal("string", op.Parameters.First(p => p.Name == "p6").Schema.Type);
            Assert.Equal("string", op.Parameters.First(p => p.Name == "p7").Schema.Type);
        }

        [Fact]
        public void ToPaths_WithQueryAndAdditionalOperations_ParsesThem()
        {
            var code = @"
            public class CustomApi
            {
                [HttpQuery(""/query"")]
                public void QueryMethod() {}

                [HttpPurge(""/purge"")]
                public void PurgeMethod() {}
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);

            Assert.NotNull(paths["/query"].Query);
            Assert.Equal("QueryMethod", paths["/query"].Query.OperationId);

            Assert.NotNull(paths["/purge"].AdditionalOperations);
            Assert.True(paths["/purge"].AdditionalOperations.ContainsKey("PURGE"));
            Assert.Equal("PurgeMethod", paths["/purge"].AdditionalOperations["PURGE"].OperationId);
        }
    }
}
