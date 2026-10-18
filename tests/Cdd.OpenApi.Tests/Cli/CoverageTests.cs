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
        public async Task FetchHttpContent_Works()
        {
            var argsInvalid = new[] { "to_docs_json", "-i", "http://localhost:1/invalid" };
            var parsedInvalid = CddCli.ParseDocsJsonArgs(argsInvalid);
            var ex = Assert.Throws<Cdd.OpenApi.Errors.CddException>(() =>
                CddCli.ToDocsJson(parsedInvalid.inputPath, parsedInvalid.outputPath, parsedInvalid.noImports, parsedInvalid.noWrapping));
            Assert.Equal(Cdd.OpenApi.Errors.CddErrorCode.HttpFetchError, ex.ErrorCode);

            using var listener = new System.Net.HttpListener();
            int port = 51249;
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            var serverTask = Task.Run(async () =>
            {
                var ctx = await listener.GetContextAsync();
                byte[] buf = Encoding.UTF8.GetBytes("{\"openapi\":\"3.2.0\",\"paths\":{}}");
                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                ctx.Response.Close();
            });

            try
            {
                var argsValid = new[] { "to_docs_json", "-i", $"http://localhost:{port}/spec.json" };
                var parsedValid = CddCli.ParseDocsJsonArgs(argsValid);
                var result = CddCli.ToDocsJson(parsedValid.inputPath, parsedValid.outputPath, parsedValid.noImports, parsedValid.noWrapping);
                Assert.Equal(0, result);
                await serverTask;
            }
            finally
            {
                listener.Stop();
            }
        }

        [Fact]
        public void ServeMcp_Works()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            var tempSpec = Path.Combine(tempDir, "spec.json");
            var tempCode = Path.Combine(tempDir, "Model.cs");
            var tempOutDir = Path.Combine(tempDir, "out");
            File.WriteAllText(tempSpec, "{\"openapi\":\"3.2.0\",\"info\":{\"title\":\"T\",\"version\":\"1.0\"},\"paths\":{\"/items\":{\"get\":{\"operationId\":\"GetItems\"}}}}");
            File.WriteAllText(tempCode, "namespace Models { public class Item { public int Id { get; set; } } }");

            var escapedSpec = tempSpec.Replace("\\", "\\\\");
            var escapedCode = tempCode.Replace("\\", "\\\\");
            var escapedOut = tempOutDir.Replace("\\", "\\\\");

            var input = "invalid json\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"method\":\"notifications/initialized\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"method\":\"notifications/cancelled\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"method\":\"notifications/progress\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"ping\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"prompts/list\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":4,\"method\":\"prompts/list\", \"params\": {\"cursor\":\"page2\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":41,\"method\":\"prompts/list\", \"params\": {\"cursor\":\"page1\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":42,\"method\":\"prompts/list\", \"params\": {}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":5,\"method\":\"prompts/get\", \"params\": {\"name\":\"test\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":51,\"method\":\"prompts/get\", \"params\": {\"name\":\"cdd_prompt\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":52,\"method\":\"prompts/get\", \"params\": {\"name\":\"unknown\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":53,\"method\":\"prompts/get\", \"params\": {}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":54,\"method\":\"prompts/get\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":6,\"method\":\"resources/list\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":7,\"method\":\"resources/read\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":71,\"method\":\"resources/read\", \"params\": {}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":72,\"method\":\"resources/read\", \"params\": {\"uri\":\"cdd://schema\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":8,\"method\":\"resources/read\", \"params\": {\"uri\":\"cdd://ast\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":9,\"method\":\"resources/templates/list\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":10,\"method\":\"sampling/createMessage\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":11,\"method\":\"logging/setLevel\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":12,\"method\":\"unknown_method\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"method\":\"unknown_notification\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":14,\"method\":\"tools/list\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":15,\"method\":\"tools/list\", \"params\": {\"cursor\":\"page2\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":151,\"method\":\"tools/list\", \"params\": {\"cursor\":\"page1\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":152,\"method\":\"tools/list\", \"params\": {}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":16,\"method\":\"tools/call\"}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":161,\"method\":\"tools/call\", \"params\": {}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":17,\"method\":\"tools/call\", \"params\": {\"name\":\"unknown\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":18,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_generate\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":181,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_generate\", \"arguments\": {}}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":182,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_generate\", \"arguments\": {\"input\":\"" + escapedSpec + "\"}}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":183,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_generate\", \"arguments\": {\"input\":\"" + escapedSpec + "\", \"output\":\"" + escapedOut + "\"}}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":184,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_generate\", \"arguments\": {\"input\":\"does_not_exist.json\"}}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":19,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_inspect\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":191,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_inspect\", \"arguments\": {}}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":192,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_inspect\", \"arguments\": {\"input\":\"" + escapedSpec + "\"}}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":20,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_extract\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":201,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_extract\", \"arguments\": {}}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":202,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_extract\", \"arguments\": {\"input\":\"" + escapedCode + "\"}}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":21,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_generate_in_memory\"}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":211,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_generate_in_memory\", \"arguments\": {}}}\n" +
                        "{\"jsonrpc\":\"2.0\",\"id\":212,\"method\":\"tools/call\", \"params\": {\"name\":\"cdd_generate_in_memory\", \"arguments\": {\"schema\":\"{\\\"openapi\\\":\\\"3.2.0\\\",\\\"paths\\\":{}}\"}}}\n" +
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
                var output = sw.ToString();
                Assert.Contains("cdd-meta-mcp", output);
                Assert.Contains("Generated code successfully", output);
                Assert.Contains("Schema inspected successfully", output);
                Assert.Contains("Code-to-schema extracted successfully", output);
                Assert.Contains("In-memory generation successful", output);
            }
            finally
            {
                Console.SetIn(originalIn);
                Console.SetOut(originalOut);
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void ServeJsonRpc_ArgParsing()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.Equal(0, CddCli.ServeJsonRpc(new[] { "serve_json_rpc", "--port" }, cts.Token));
            Assert.Equal(0, CddCli.ServeJsonRpc(new[] { "serve_json_rpc", "-p" }, cts.Token));
            Assert.Equal(0, CddCli.ServeJsonRpc(new[] { "serve_json_rpc", "--listen" }, cts.Token));
            Assert.Equal(0, CddCli.ServeJsonRpc(new[] { "serve_json_rpc", "-l" }, cts.Token));
        }

        [Fact]
        public async Task ServeJsonRpc_Works()
        {
            int port = 51239;
            Environment.SetEnvironmentVariable("CDD_LISTEN", "localhost");
            using var cts = new CancellationTokenSource();
            var serverTask = Task.Run(() => CddCli.ServeJsonRpc(new[] { "serve_json_rpc", "-p", port.ToString(), "-l", "localhost" }, cts.Token));

            try
            {
                await Task.Delay(150);

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
                Assert.Contains("0.0.4", content);

                // Add branch coverage for error handling
                var reqErr = new StringContent("invalid json", Encoding.UTF8, "application/json");
                var resErr = await client.PostAsync($"http://localhost:{port}/", reqErr);
                var contentErr = await resErr.Content.ReadAsStringAsync();
                Assert.Contains("error", contentErr);

                // Call something to generate code
                var reqGen = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"to_openapi\", \"params\": [\"does_not_exist.cs\", null]}", Encoding.UTF8, "application/json");
                await client.PostAsync($"http://localhost:{port}/", reqGen);

                // Call from_openapi
                var reqFrom = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"from_openapi\", \"params\": [\"to_sdk\", \"-i\", \"spec.json\"]}", Encoding.UTF8, "application/json");
                await client.PostAsync($"http://localhost:{port}/", reqFrom);

                // Call from_arazzo and to_arazzo
                var reqFromArazzo = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":31,\"method\":\"from_arazzo\", \"params\": [\"to_sdk\", \"-i\", \"spec.arazzo.json\"]}", Encoding.UTF8, "application/json");
                await client.PostAsync($"http://localhost:{port}/", reqFromArazzo);
                var reqToArazzo = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":32,\"method\":\"to_arazzo\", \"params\": [\"-i\", \"does_not_exist.cs\"]}", Encoding.UTF8, "application/json");
                await client.PostAsync($"http://localhost:{port}/", reqToArazzo);

                // Call to_docs_json
                var reqDocs = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":4,\"method\":\"to_docs_json\", \"params\": [\"-i\", \"spec.json\"]}", Encoding.UTF8, "application/json");
                await client.PostAsync($"http://localhost:{port}/", reqDocs);

                // Call with 0.0.0.0
                Environment.SetEnvironmentVariable("CDD_LISTEN", "0.0.0.0");
                using var cts2 = new CancellationTokenSource(50);
                try { CddCli.ServeJsonRpc(new[] { "serve_json_rpc", "-p", "51240", "-l", "0.0.0.0" }, cts2.Token); } catch { }
            }
            finally
            {
                Environment.SetEnvironmentVariable("CDD_LISTEN", null);
                cts.Cancel();
                await serverTask;
            }
        }
    }
}
