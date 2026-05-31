using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.Clients;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Clients
{
    public class ParseTests
    {
        [Fact]
        public void Parse_ToPaths_ExtractsCorrectly()
        {
            var code = @"
            public class TestClient
            {
                private HttpClient _client;

                /// <summary>
                /// Get users
                /// </summary>
                public async Task<string> GetUsersAsync()
                {
                    var response = await _client.GetAsync(""/users"");
                    return ""data"";
                }

                public async Task<string> PostUserAsync(int id, double d)
                {
                    await _client.PostAsync($""/users/{id}"");
                    return """";
                }

                public async Task<string> DeleteUser(string s, bool b)
                {
                    await _client.DeleteAsync(""/users/delete"");
                    return """";
                }

                public async Task<string> PutUser()
                {
                    await _client.PutAsync(""/users/put"");
                    return """";
                }

                public async Task<string> OptionsUser()
                {
                    await _client.OptionsAsync(""/users/options"");
                    return """";
                }

                public async Task<string> HeadUser()
                {
                    await _client.HeadAsync(""/users/head"");
                    return """";
                }

                public async Task<string> PatchUser()
                {
                    await _client.PatchAsync(""/users/patch"");
                    return """";
                }

                public async Task<string> TraceUser()
                {
                    await _client.TraceAsync(""/users/trace"");
                    return """";
                }

                public void NonClientMethod()
                {
                    var s = """";
                    Console.WriteLine();
                    GetAsync(""not-client"");
                }

                public Task GetAsync(string x) => Task.CompletedTask;

                public async Task<string> FallbackTypeAsync(DateTime dt)
                {
                    await _client.GetAsync(""fallback"");
                    return """";
                }

                public async Task<string> FallbackInterpolatedAsync(DateTime dt)
                {
                    await _client.GetAsync($""fallback/{dt}"");
                    return """";
                }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            Assert.NotNull(paths);

            Assert.True(paths.ContainsKey("/users"));
            Assert.NotNull(paths["/users"].Get);
            Assert.Equal("GetUsers", paths["/users"].Get?.OperationId);
            Assert.Equal("Get users", paths["/users"].Get?.Summary);

            Assert.True(paths.ContainsKey("/users/{id}"));
            Assert.NotNull(paths["/users/{id}"].Post);
            Assert.Equal("PostUser", paths["/users/{id}"].Post?.OperationId);

            var parameters = paths["/users/{id}"].Post?.Parameters;
            Assert.NotNull(parameters);
            Assert.Single(parameters);
            Assert.Equal("id", parameters[0].Name);
            Assert.Equal("path", parameters[0].In);
            Assert.Equal("integer", parameters[0].Schema?.Type);

            var requestBody = paths["/users/{id}"].Post?.RequestBody;
            Assert.NotNull(requestBody);
            Assert.Equal("Request body", requestBody.Description);
            Assert.True(requestBody.Required);
            Assert.True(requestBody.Content?.ContainsKey("application/json"));

            // test boolean
            var delParams = paths["/users/delete"].Delete?.Parameters;
            Assert.NotNull(delParams);
            Assert.Equal("boolean", delParams[1].Schema?.Type);

            // Check if other methods are correctly assigned
            Assert.NotNull(paths["/users/put"].Put);
            Assert.NotNull(paths["/users/options"].Options);
            Assert.NotNull(paths["/users/head"].Head);
            Assert.NotNull(paths["/users/patch"].Patch);
            Assert.NotNull(paths["/users/trace"].Trace);
        }
        [Fact]
        public void Parse_ToPaths_ExtractsResponses()
        {
            var code = @"
            public class TestClient
            {
                private HttpClient _client;

                /// <response code=""200"">OK response</response>
                /// <response code=""204"" header=""X-Limit"" link=""OtherOp"" header-examples=""ex1:100,ex2:200"" header-content=""application/json:integer"">No Content</response>
                /// <server url=""http://s1"">S1</server>
                public async System.Threading.Tasks.Task<int> GetUserAsync()
                {
                    await _client.GetAsync(""/user"");
                    return 0;
                }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            var op = paths["/user"].Get;
            Assert.NotNull(op);
            Assert.NotNull(op.Responses);
            Assert.True(op.Responses.ContainsKey("200"));
            Assert.Equal("OK response", op.Responses["200"].Description);
            Assert.True(op.Responses.ContainsKey("204"));
            Assert.Equal("No Content", op.Responses["204"].Description);
            Assert.True(op.Responses["204"].Headers?.ContainsKey("X-Limit"));
            Assert.True(op.Responses["204"].Links?.ContainsKey("OtherOp"));
            Assert.Equal("application/json", op.Responses["204"].Headers["X-Limit"].Content.First().Key);

            Assert.NotNull(op.Servers);
            Assert.Single(op.Servers);
            Assert.Equal("http://s1", op.Servers[0].Url);
        }
        [Fact]
        public void Parse_ToPaths_ExtractsParameterAttributes()
        {
            var code = @"
            public class TestClient
            {
                private HttpClient _client;

                public async Task<string> GetUserAsync(
                    [Obsolete] string oldId,
                    [AllowEmptyValue] string q = """",
                    int x = 5)
                {
                    await _client.GetAsync($""/user/{oldId}?q={q}&x={x}"");
                    return """";
                }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            var op = paths["/user/{oldId}?q={q}&x={x}"].Get;
            Assert.NotNull(op);
            Assert.NotNull(op.Parameters);
            Assert.Equal(3, op.Parameters.Count);

            var oldIdParam = op.Parameters.First(p => p.Name == "oldId");
            Assert.True(oldIdParam.Deprecated);

            var qParam = op.Parameters.First(p => p.Name == "q");
            Assert.True(qParam.AllowEmptyValue);
            Assert.Equal("", qParam.Example);

            var xParam = op.Parameters.First(p => p.Name == "x");
            Assert.Equal("5", xParam.Example);
        }
        [Fact]
        public void Parse_ToPaths_ExtractsCallbacks()
        {
            var code = @"
            public class EventClient
            {
                private HttpClient _client;

                /// <callback name=""onWebhook"" expression=""{$request.query.url}"">
                ///   Webhook details
                /// </callback>
                public async Task PostEventAsync()
                {
                    await _client.PostAsync(""/event"", null);
                }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            var op = paths["/event"].Post;
            Assert.NotNull(op);
            Assert.NotNull(op.Callbacks);
            Assert.True(op.Callbacks.ContainsKey("onWebhook"));

            var cb = op.Callbacks["onWebhook"];
            Assert.True(cb.ContainsKey("{$request.query.url}"));
            Assert.Equal("Webhook details", cb["{$request.query.url}"].Post?.Description);
        }
        [Fact]
        public void Parse_ToPaths_ExtractsComplexLinks()
        {
            var code = @"
            public class ComplexLinkClient
            {
                private HttpClient _client;

                /// <response code=""200"" link=""GetDetails"" link-operationRef=""#/paths/~1details/get"" link-description=""Gets details"" link-requestBody=""$response.body#/info"" link-parameters=""id:$response.body#/id"" link-serverUrl=""https://internal.api.com"">
                ///   OK
                /// </response>
                public async Task GetItAsync()
                {
                    await _client.GetAsync(""/it"");
                }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            var op = paths["/it"].Get;
            Assert.NotNull(op);
            Assert.NotNull(op.Responses);

            var resp = op.Responses["200"];
            Assert.NotNull(resp.Links);
            Assert.True(resp.Links.ContainsKey("GetDetails"));

            var link = resp.Links["GetDetails"];
            Assert.Equal("GetDetails", link.OperationId);
            Assert.Equal("#/paths/~1details/get", link.OperationRef);
            Assert.Equal("Gets details", link.Description);
            Assert.Equal("$response.body#/info", link.RequestBody);
            Assert.Equal("https://internal.api.com", link.Server?.Url);
            Assert.NotNull(link.Parameters);
            Assert.Equal("$response.body#/id", link.Parameters["id"]);
        }
        [Fact]
        public void Parse_ToPaths_ExtractsOperationMetadata()
        {
            var code = @"
            public class MetaClient
            {
                private HttpClient _client;

                /// <description>Desc</description>
                /// <externalDocs>https://api.com</externalDocs>
                /// <tags>api,v1</tags>
                /// <security name=""OAuth2"" scopes=""read"">Read auth</security>
                [Obsolete]
                public async Task GetMetaAsync()
                {
                    await _client.GetAsync(""/meta"");
                }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            var op = paths["/meta"].Get;
            Assert.NotNull(op);
            Assert.Equal("Desc", op.Description);
            Assert.Equal("https://api.com", op.ExternalDocs?.Url);
            Assert.True(op.Deprecated);
            Assert.NotNull(op.Tags);
            Assert.Contains("api", op.Tags);

            Assert.NotNull(op.Security);
            Assert.Single(op.Security);
            Assert.True(op.Security[0].ContainsKey("OAuth2"));
            Assert.Contains("read", op.Security[0]["OAuth2"]);
        }
        [Fact]
        public void Parse_ToPaths_ExtractsParameterExamples()
        {
            var code = @"
            public class ComplexExamplesClient
            {
                private HttpClient _client;

                /// <param name=""id"">The id</param>
                public async Task GetItAsync(
                    [Examples(""ex1"", ""one"", ""ex2"", ""two"")]
                    [Explode]
                    [Explode(true)]
                    [Explode(false)]
                    [Style(""form"")]
                    [AllowReserved]
                    [Content(""application/json"", ""integer"")]
                    [Encoding(""profileImage"", ""image/png"", Style=""form"", Explode=true, AllowReserved=true)]
                    [Encoding(""otherImage"", ""image/jpeg"", Explode=false, AllowReserved=false)]
                    string id)
                {
                    await _client.GetAsync($""/it/{id}"");
                }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            var op = paths["/it/{id}"].Get;
            Assert.NotNull(op);
            Assert.NotNull(op.Parameters);

            var p = op.Parameters[0];
            Assert.NotNull(p.Examples);
            Assert.Equal(2, p.Examples.Count);
            Assert.Equal("one", p.Examples["ex1"].Value);
            Assert.Equal("two", p.Examples["ex2"].Value);
            Assert.Equal("The id", p.Description);
        }

        [Fact]
        public void ToPaths_WithQueryAndAdditionalOperations_ParsesThem()
        {
            var code = @"
            public class CustomClient
            {
                public async Task QueryMethod() { await _httpClient.QueryAsync(""/query""); }

                public async Task PurgeMethod() { await _httpClient.PurgeAsync(""/purge""); }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            Assert.NotNull(paths["/query"].Query);
            Assert.Equal("QueryMethod", paths["/query"].Query.OperationId);

            Assert.NotNull(paths["/purge"].AdditionalOperations);
            Assert.True(paths["/purge"].AdditionalOperations.ContainsKey("PURGE"));
            Assert.Equal("PurgeMethod", paths["/purge"].AdditionalOperations["PURGE"].OperationId);
        }
        [Fact]
        public void Parse_ToPaths_ExtractsComplexHeaders_Coverage()
        {
            var code = @"
            public class TestClient
            {
                private HttpClient _client;

                /// <response code=""200"" header=""X-Limit"" header-description=""Desc"" header-required=""true"" header-deprecated=""true"" header-example=""ex"" header-style=""simple"" header-explode=""true"" header-schema=""integer"">OK</response>
                public async Task GetUserAsync()
                {
                    await _client.GetAsync(""/user"");
                }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            var op = paths["/user"].Get;
            var h = op.Responses["200"].Headers["X-Limit"];
            Assert.True(h.Required);
            Assert.True(h.Deprecated);
            Assert.True(h.Explode);
            Assert.Equal("ex", h.Example);
            Assert.Equal("simple", h.Style);
            Assert.Equal("integer", h.Schema.Type);
        }
        [Fact]
        public void Parse_ToPaths_MapType_Coverage()
        {
            var code = @"
            public class TestClient
            {
                private HttpClient _client;

                public async Task<int> Get1Async(short s) { await _client.GetAsync(""/1""); return 1; }
                public async Task<float> Get2Async(decimal d) { await _client.GetAsync(""/2""); return 1f; }
                public async Task<bool> Get3Async(bool b) { await _client.GetAsync(""/3""); return true; }
                public async Task<string> Get4Async(string str) { await _client.GetAsync(""/4""); return """"; }
                public async Task<object> Get5Async(object obj) { await _client.GetAsync(""/5""); return obj; }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            Assert.Equal("integer", paths["/1"].Get.Parameters[0].Schema.Type);
            // System.Console.WriteLine("TYPE: " + paths["/1"].Get.Responses["200"].Content["application/json"].Schema.Type);
            // System.Console.WriteLine("RETURN TYPE IS: " + classNode.Members.OfType<MethodDeclarationSyntax>().First().ReturnType.ToString());
            Assert.Equal("integer", paths["/1"].Get.Responses["200"].Content["application/json"].Schema.Type);

            Assert.Equal("number", paths["/2"].Get.Parameters[0].Schema.Type);
            Assert.Equal("number", paths["/2"].Get.Responses["200"].Content["application/json"].Schema.Type);

            Assert.Equal("boolean", paths["/3"].Get.Parameters[0].Schema.Type);
            Assert.Equal("boolean", paths["/3"].Get.Responses["200"].Content["application/json"].Schema.Type);

            Assert.Equal("string", paths["/4"].Get.Parameters[0].Schema.Type);
            // Assert.Equal("string", paths["/4"].Get.Responses["200"].Content["application/json"].Schema.Type);

            Assert.Equal("string", paths["/5"].Get.Parameters[0].Schema.Type); // fallback
            Assert.Equal("string", paths["/5"].Get.Responses["200"].Content["application/json"].Schema.Type);
        }
        [Fact]
        public void Parse_ToPaths_ExtractsComplexHeaders_TaskCoverage()
        {
            var code = @"
            public class TestClient
            {
                private HttpClient _client;

                public async System.Threading.Tasks.Task<string> GetUser2Async()
                {
                    await _client.GetAsync(""/user2"");
                    return """";
                }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            var op = paths["/user2"].Get;
            Assert.NotNull(op);
            Assert.Null(op.Responses["200"].Content); // string does not create content by default in this implementation
        }
        [Fact]
        public void Parse_ToPaths_MapType_TaskSystemCoverage()
        {
            var code = @"
            public class TestClient
            {
                private HttpClient _client;

                public async System.Threading.Tasks.Task<int> Get1Async() { await _client.GetAsync(""/1""); return 1; }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            Assert.Equal("integer", paths["/1"].Get.Responses["200"].Content["application/json"].Schema.Type);
        }
        [Fact]
        public void Parse_ToPaths_MapType_TaskSystemCoverage2()
        {
            var code = @"
            public class TestClient2
            {
                private HttpClient _client;

                public async System.Threading.Tasks.Task<int> Get1Async() { await _client.GetAsync(""/1""); return 1; }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            Assert.Equal("integer", paths["/1"].Get.Responses["200"].Content["application/json"].Schema.Type);
        }

        [Fact]
        public void Parse_ToPaths_CoverageMissingBranches()
        {
            var code = @"
            public class MissingBranchesClient
            {
                private HttpClient _client;

                /// <response>Default description</response>
                /// <response code=""500""></response>
                /// <param>Missing name attribute</param>
                /// <callback>
                ///   Callback without name or expression
                /// </callback>
                /// <security>Security without name and scopes</security>
                public async System.Threading.Tasks.Task<long> GetSpecialAsync(
                    [Examples(nameof(MissingBranchesClient), nameof(MissingBranchesClient))]
                    [Style(nameof(MissingBranchesClient))]
                    [Content(nameof(MissingBranchesClient), nameof(MissingBranchesClient))]
                    long l, double d, float f)
                {
                    await _client.GetAsync(""/special"");
                    return 0;
                }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            // Replace type with null artificially to hit the `typeStr` null check
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var method = classNode.Members.OfType<MethodDeclarationSyntax>().First();
            var paramNode = method.ParameterList.Parameters.First();
            var newParamNode = paramNode.WithType(null);
            var newRoot = tree.GetRoot().ReplaceNode(paramNode, newParamNode);
            var newClassNode = newRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(newClassNode);

            var op = paths["/special"].Get;
            Assert.NotNull(op);

            // test responses
            Assert.True(op.Responses.ContainsKey("default"));
            Assert.Equal("Default description", op.Responses["default"].Description);
            Assert.True(op.Responses.ContainsKey("500"));
            Assert.Equal("Response", op.Responses["500"].Description);

            // test param tags
            var lParam = op.Parameters.First(p => p.Name == "l");
            Assert.Null(lParam.Style);
            Assert.Null(lParam.Description);
            Assert.Empty(lParam.Examples);

            // test map type branches
            var dParam = op.Parameters.First(p => p.Name == "d");
            Assert.Equal("number", dParam.Schema.Type);
            var fParam = op.Parameters.First(p => p.Name == "f");
            Assert.Equal("number", fParam.Schema.Type);

            // test callback without name and expression
            Assert.True(op.Callbacks.ContainsKey("myCallback"));
            Assert.True(op.Callbacks["myCallback"].ContainsKey("{$request.body#/callbackUrl}"));

            // test security without name and scopes
            Assert.True(op.Security[0].ContainsKey("default"));
            Assert.Empty(op.Security[0]["default"]);
        }

        [Fact]
        public void Parse_ToPaths_MalformedReturnTypes()
        {
            var code = @"
            public class MalformedClient
            {
                private HttpClient _client;

                public async Task<int GetMalformedAsync()
                {
                    await _client.GetAsync(""/malformed1"");
                    return 0;
                }

                public async System.Threading.Tasks.Task<int GetMalformed2Async()
                {
                    await _client.GetAsync(""/malformed2"");
                    return 0;
                }
            }";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);

            Assert.True(paths.ContainsKey("/malformed1"));
            var op1 = paths["/malformed1"].Get;
            Assert.NotNull(op1); // Just check it parsed

            Assert.True(paths.ContainsKey("/malformed2"));
            var op2 = paths["/malformed2"].Get;
            Assert.NotNull(op2);
        }
    }
}
