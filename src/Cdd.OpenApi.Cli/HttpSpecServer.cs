// <copyright file="HttpSpecServer.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading;

namespace Cdd.OpenApi.Cli
{
    /// <summary>
    /// Implements the HTTP JSON-RPC endpoint server for the <c>serve_json_rpc</c> CLI command.
    /// </summary>
    public static class HttpSpecServer
    {
        /// <summary>
        /// Exposes the CLI commands via an HTTP JSON-RPC 2.0 listener.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="cancellationToken">Optional cancellation token to stop the server loop.</param>
        /// <returns>Exit code (<c>0</c> for success).</returns>
        public static int ServeJsonRpc(string[] args, CancellationToken cancellationToken = default)
        {
            string portStr = Environment.GetEnvironmentVariable("CDD_PORT") ?? Environment.GetEnvironmentVariable("PORT") ?? "8080";
            string listen = Environment.GetEnvironmentVariable("CDD_LISTEN") ?? Environment.GetEnvironmentVariable("LISTEN") ?? "127.0.0.1";

            for (int i = 1; i < args.Length; i++)
            {
                if ((args[i] == "--port" || args[i] == "-p") && i + 1 < args.Length)
                {
                    portStr = args[++i];
                }
                else if ((args[i] == "--listen" || args[i] == "-l") && i + 1 < args.Length)
                {
                    listen = args[++i];
                }
            }

            string url = $"http://{(listen == "0.0.0.0" ? "*" : listen)}:{portStr}/";
            if (cancellationToken.IsCancellationRequested)
            {
                return 0;
            }

            using var listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            using var reg = cancellationToken.Register(() => listener.Stop());
            Console.WriteLine($"JSON-RPC Server listening on {listen}:{portStr}");

            while (listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = listener.GetContext();
                }
                catch
                {
                    break;
                }

                var request = context.Request;
                var response = context.Response;
                response.ContentType = "application/json; charset=utf-8";

                try
                {
                    using var reader = new StreamReader(request.InputStream);
                    string json = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(json)) continue;

                    var jRpcReq = JsonNode.Parse(json);
                    string method = jRpcReq?["method"]?.ToString() ?? string.Empty;
                    var parameters = jRpcReq?["params"] as JsonArray;
                    var idNode = jRpcReq?["id"];

                    if (method == "version")
                    {
                        var res = new JsonObject { ["jsonrpc"] = "2.0", ["id"] = idNode?.DeepClone(), ["result"] = CliArgumentParser.Version };
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes(res.ToJsonString());
                        response.OutputStream.Write(buf, 0, buf.Length);
                    }
                    else
                    {
                        var invokeArgs = new List<string> { method };
                        if (parameters != null)
                        {
                            foreach (var p in parameters)
                            {
                                invokeArgs.Add(p?.ToString() ?? string.Empty);
                            }
                        }

                        int retCode = 1;
                        if (method == "from_openapi") { var parsed = CddCli.ParseFromOpenApiArgs(invokeArgs.ToArray()); retCode = CddCli.FromOpenApi(parsed.type, parsed.config); }
                        else if (method == "to_openapi") { var parsed = CddCli.ParseToOpenApiArgs(invokeArgs.ToArray()); retCode = CddCli.ToOpenApi(parsed.inputPath, parsed.outputPath); }
                        else if (method == "from_arazzo") { var parsed = CddCli.ParseFromArazzoArgs(invokeArgs.ToArray()); retCode = CddCli.FromArazzo(parsed.type, parsed.config); }
                        else if (method == "to_arazzo") { var parsed = CddCli.ParseToArazzoArgs(invokeArgs.ToArray()); retCode = CddCli.ToArazzo(parsed.inputPath, parsed.outputPath); }
                        else if (method == "to_docs_json") { var parsed = CddCli.ParseDocsJsonArgs(invokeArgs.ToArray()); retCode = CddCli.ToDocsJson(parsed.inputPath, parsed.outputPath, parsed.noImports, parsed.noWrapping); }

                        var res = new JsonObject { ["jsonrpc"] = "2.0", ["id"] = idNode?.DeepClone(), ["result"] = retCode };
                        byte[] buf = System.Text.Encoding.UTF8.GetBytes(res.ToJsonString());
                        response.OutputStream.Write(buf, 0, buf.Length);
                    }
                }
                catch (Exception ex)
                {
                    var res = new JsonObject { ["jsonrpc"] = "2.0", ["error"] = new JsonObject { ["code"] = -32603, ["message"] = ex.Message } };
                    byte[] buf = System.Text.Encoding.UTF8.GetBytes(res.ToJsonString());
                    response.OutputStream.Write(buf, 0, buf.Length);
                }
                finally
                {
                    response.Close();
                }
            }

            return 0;
        }
    }
}
