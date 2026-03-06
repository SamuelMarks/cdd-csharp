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
            Assert.Equal(1, parameters.Count);
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
                /// <response code=""204"" header=""X-Limit"" link=""OtherOp"">No Content</response>
                public async Task<string> GetUserAsync()
                {
                    await _client.GetAsync(""/user"");
                    return """";
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
                
                public async Task GetItAsync(
                    [Examples(""ex1"", ""one"", ""ex2"", ""two"")] string id)
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
    }
}