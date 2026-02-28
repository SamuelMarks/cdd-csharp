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
            Assert.Equal(2, parameters.Count);
            Assert.Equal("id", parameters[0].Name);
            Assert.Equal("path", parameters[0].In);
            Assert.Equal("integer", parameters[0].Schema?.Type);
            
            Assert.Equal("d", parameters[1].Name);
            Assert.Equal("query", parameters[1].In);
            Assert.Equal("number", parameters[1].Schema?.Type);

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
    }
}
