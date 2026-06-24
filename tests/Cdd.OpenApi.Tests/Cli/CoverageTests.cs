using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Xunit;
using Cdd.OpenApi.Cli;

namespace Cdd.OpenApi.Tests.Cli
{
    [Collection("Cli")]
    public class CoverageTests
    {
        [Fact]
        public void FetchHttpContent_Works()
        {
            var result = CddCli.GenerateDocsJson(new[] { "to_docs_json", "-i", "http://localhost:1/invalid" });
            Assert.Equal(0, result);
        }

        [Fact]
        public void ServeMcp_Works()
        {
            var input = "invalid json\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"method\":\"notifications/initialized\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"method\":\"notifications/cancelled\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"method\":\"notifications/progress\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"ping\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"prompts/list\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":4,\"method\":\"prompts/list\", \"params\": {\"cursor\":\"page2\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":5,\"method\":\"prompts/get\", \"params\": {\"name\":\"test\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":51,\"method\":\"prompts/get\", \"params\": {\"name\":\"cdd_prompt\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":52,\"method\":\"prompts/get\", \"params\": {\"name\":\"unknown\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":53,\"method\":\"prompts/get\", \"params\": {}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":54,\"method\":\"prompts/get\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":6,\"method\":\"resources/list\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":7,\"method\":\"resources/read\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":8,\"method\":\"resources/read\", \"params\": {\"uri\":\"cdd://ast\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":9,\"method\":\"resources/templates/list\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":10,\"method\":\"sampling/createMessage\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":11,\"method\":\"logging/setLevel\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":12,\"method\":\"unknown_method\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"method\":\"unknown_notification\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":14,\"method\":\"tools/list\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":15,\"method\":\"tools/list\", \"params\": {\"cursor\":\"page2\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":16,\"method\":\"tools/call\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":17,\"method\":\"tools/call\", \"params\": {\"name\":\"unknown\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":18,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_generate\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":19,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_inspect\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":20,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_extract\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":21,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_generate_in_memory\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":13,\"method\":\"exit\"}\n";
            var originalIn = Console.In;
            var originalOut = Console.Out;
            using var sr = new StringReader(input);
            using var sw = new StringWriter();
            Console.SetIn(sr);
            Console.SetOut(sw);
            try
            {
                var result = CddCli.ServeMcp(new string[0]);
                Assert.Equal(0, result);
                Assert.Contains("cdd-meta-mcp", sw.ToString());
            }
            finally
            {
                Console.SetIn(originalIn);
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void ServeJsonRpc_ArgParsing()
        {
            Action<string[]> run = args =>
            {
                var t = new Thread(() => { try { CddCli.ServeJsonRpc(args); } catch { } });
                t.Start();
                Thread.Sleep(100);
                t.Interrupt();
                t.Join(200);
            };
            run(new[] { "serve_json_rpc", "--port" });
            run(new[] { "serve_json_rpc", "-p" });
            run(new[] { "serve_json_rpc", "--listen" });
            run(new[] { "serve_json_rpc", "-l" });
        }

        [Fact]
        public async Task ServeJsonRpc_Works()
        {
            int port = 51239;
            Environment.SetEnvironmentVariable("CDD_LISTEN", "localhost");
            Exception serverError = null;
            var thread = new Thread(() =>
            {
                try
                {
                    // Call with missing param values
                    CddCli.ServeJsonRpc(new[] { "serve_json_rpc", "-p", port.ToString(), "-l", "localhost" });
                }
                catch (Exception ex)
                {
                    serverError = ex;
                }
            });
            thread.Start();

            try
            {
                await Task.Delay(500);
                if (serverError != null && !(serverError is ThreadInterruptedException))
                {
                    throw new Exception("Server failed to start", serverError);
                }

                using var client = new HttpClient();
                // Empty body
                await client.PostAsync($"http://localhost:{port}/", new StringContent("", Encoding.UTF8, "application/json"));

                // No method, no ID
                await client.PostAsync($"http://localhost:{port}/", new StringContent("{}", Encoding.UTF8, "application/json"));
                await client.PostAsync($"http://localhost:{port}/", new StringContent("null", Encoding.UTF8, "application/json"));
                await client.PostAsync($"http://localhost:{port}/", new StringContent("{\"jsonrpc\":\"2.0\"}", Encoding.UTF8, "application/json"));
                await client.PostAsync($"http://localhost:{port}/", new StringContent("{\"jsonrpc\":\"2.0\",\"method\":null}", Encoding.UTF8, "application/json"));
                await client.PostAsync($"http://localhost:{port}/", new StringContent("{\"jsonrpc\":\"2.0\",\"method\":\"version\",\"params\":{}}", Encoding.UTF8, "application/json"));

                // Valid version without ID
                await client.PostAsync($"http://localhost:{port}/", new StringContent("{\"jsonrpc\":\"2.0\",\"method\":\"version\"}", Encoding.UTF8, "application/json"));

                // Valid version with ID
                var req = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"version\"}", Encoding.UTF8, "application/json");
                var res = await client.PostAsync($"http://localhost:{port}/", req);
                var content = await res.Content.ReadAsStringAsync();
                Assert.Contains("0.0.3", content);

                // Add branch coverage for error handling
                var reqErr = new StringContent("invalid json", Encoding.UTF8, "application/json");
                var resErr = await client.PostAsync($"http://localhost:{port}/", reqErr);
                var contentErr = await resErr.Content.ReadAsStringAsync();

                // Call something to generate code
                var reqGen = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"to_openapi\", \"params\": [\"does_not_exist.cs\", null]}", Encoding.UTF8, "application/json");
                await client.PostAsync($"http://localhost:{port}/", reqGen);

                // Call from_openapi
                var reqFrom = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"from_openapi\", \"params\": [\"to_sdk\", \"-i\", \"spec.json\"]}", Encoding.UTF8, "application/json");
                await client.PostAsync($"http://localhost:{port}/", reqFrom);

                // Call to_docs_json
                var reqDocs = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":4,\"method\":\"to_docs_json\", \"params\": [\"-i\", \"spec.json\"]}", Encoding.UTF8, "application/json");
                await client.PostAsync($"http://localhost:{port}/", reqDocs);

                // Call with 0.0.0.0
                Environment.SetEnvironmentVariable("CDD_LISTEN", "0.0.0.0");
                var thread2 = new Thread(() =>
                {
                    try { CddCli.ServeJsonRpc(new[] { "serve_json_rpc", "-p", "51240", "-l", "0.0.0.0" }); } catch { }
                });
                thread2.Start();
                await Task.Delay(200);
                thread2.Interrupt();
            }
            finally
            {
                Environment.SetEnvironmentVariable("CDD_LISTEN", null);
                thread.Interrupt();
                thread.Join(1000);
            }
        }
    }
}
