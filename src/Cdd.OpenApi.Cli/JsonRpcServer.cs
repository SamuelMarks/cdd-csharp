// <copyright file="JsonRpcServer.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cdd.OpenApi.Emit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Parse;

namespace Cdd.OpenApi.Cli
{
    /// <summary>
    /// Implements the stdio Model Context Protocol (MCP) JSON-RPC 2.0 server for the CDD CLI.
    /// </summary>
    public static class JsonRpcServer
    {
        /// <summary>
        /// Runs the CDD toolchain as a Model Context Protocol (MCP) server over standard input and output.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>Exit code (<c>0</c> for success).</returns>
        public static int ServeMcp(string[] args)
        {
            var writer = Console.Out;
            var reader = Console.In;
            var parser = new OpenApiParser();
            var emitter = new OpenApiEmitter();
            var activeDoc = new OpenApiDocument { OpenApi = "3.2.0", Info = new OpenApiInfo { Title = "Active API", Version = "1.0.0" }, Paths = new OpenApiPaths() };

            static void WriteJsonRpc(TextWriter w, JsonNode? idNode, JsonObject? resultObj, JsonObject? errorObj = null)
            {
                var envelope = new JsonObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = idNode!.DeepClone()
                };
                if (errorObj != null)
                {
                    envelope["error"] = errorObj;
                }
                else
                {
                    envelope["result"] = resultObj;
                }

                w.WriteLine(JsonSerializer.Serialize(envelope, OpenApiJsonContext.Default.JsonObject));
            }

            while (true)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;

                JsonDocument document;
                try
                {
                    document = JsonDocument.Parse(line);
                }
                catch (JsonException)
                {
                    var parseErr = new JsonObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["error"] = new JsonObject { ["code"] = -32700, ["message"] = "Parse error" }
                    };
                    writer.WriteLine(JsonSerializer.Serialize(parseErr, OpenApiJsonContext.Default.JsonObject));
                    continue;
                }

                using (document)
                {
                    var root = document.RootElement;
                    if (root.TryGetProperty("method", out var methodProp))
                    {
                        var method = methodProp.GetString();
                        JsonNode? idNode = root.TryGetProperty("id", out var idProp) ? JsonNode.Parse(idProp.GetRawText()) : null;

                        if (method == "initialize")
                        {
                            WriteJsonRpc(writer, idNode, new JsonObject
                            {
                                ["protocolVersion"] = "2024-11-05",
                                ["capabilities"] = new JsonObject { ["tools"] = new JsonObject(), ["resources"] = new JsonObject() },
                                ["serverInfo"] = new JsonObject { ["name"] = "cdd-meta-mcp", ["version"] = "1.0.0" }
                            });
                        }
                        else if (method == "notifications/initialized" || method == "notifications/cancelled" || method == "notifications/progress")
                        {
                            // Notification messages require no response
                        }
                        else if (method == "ping" || method == "logging/setLevel")
                        {
                            WriteJsonRpc(writer, idNode, new JsonObject());
                        }
                        else if (method == "prompts/list")
                        {
                            JsonNode? nextCursor = "page2";
                            if (root.TryGetProperty("params", out var p) && p.TryGetProperty("cursor", out var c) && c.GetString() == "page2")
                            {
                                nextCursor = null;
                            }

                            WriteJsonRpc(writer, idNode, new JsonObject
                            {
                                ["nextCursor"] = nextCursor,
                                ["prompts"] = new JsonArray(
                                    new JsonObject
                                    {
                                        ["name"] = "cdd_prompt",
                                        ["description"] = "A default prompt",
                                        ["arguments"] = new JsonArray(
                                            new JsonObject { ["name"] = "arg1", ["description"] = "An argument", ["required"] = true })
                                    })
                            });
                        }
                        else if (method == "prompts/get")
                        {
                            if (root.TryGetProperty("params", out var p) && p.TryGetProperty("name", out var n) && n.GetString() == "cdd_prompt")
                            {
                                WriteJsonRpc(writer, idNode, new JsonObject
                                {
                                    ["description"] = "A default prompt",
                                    ["messages"] = new JsonArray(
                                        new JsonObject
                                        {
                                            ["role"] = "user",
                                            ["content"] = new JsonObject { ["type"] = "text", ["text"] = "Prompt content" }
                                        })
                                });
                            }
                            else
                            {
                                WriteJsonRpc(writer, idNode, null, new JsonObject { ["code"] = -32602, ["message"] = "Prompt not found" });
                            }
                        }
                        else if (method == "tools/list")
                        {
                            JsonNode? nextCursor = "page2";
                            if (root.TryGetProperty("params", out var p) && p.TryGetProperty("cursor", out var c) && c.GetString() == "page2")
                            {
                                nextCursor = null;
                            }

                            WriteJsonRpc(writer, idNode, new JsonObject
                            {
                                ["nextCursor"] = nextCursor,
                                ["tools"] = new JsonArray(
                                    new JsonObject
                                    {
                                        ["name"] = "cdd_generate",
                                        ["description"] = "Generate C# code from OpenAPI",
                                        ["inputSchema"] = new JsonObject
                                        {
                                            ["type"] = "object",
                                            ["properties"] = new JsonObject { ["input"] = new JsonObject { ["type"] = "string" } },
                                            ["required"] = new JsonArray("input")
                                        }
                                    },
                                    new JsonObject
                                    {
                                        ["name"] = "cdd_inspect",
                                        ["description"] = "Inspect the active schema",
                                        ["inputSchema"] = new JsonObject { ["type"] = "object", ["properties"] = new JsonObject(), ["required"] = new JsonArray() }
                                    },
                                    new JsonObject
                                    {
                                        ["name"] = "cdd_extract",
                                        ["description"] = "Extract OpenAPI schema from C# code",
                                        ["inputSchema"] = new JsonObject
                                        {
                                            ["type"] = "object",
                                            ["properties"] = new JsonObject { ["input"] = new JsonObject { ["type"] = "string" }, ["output"] = new JsonObject { ["type"] = "string" } },
                                            ["required"] = new JsonArray("input")
                                        }
                                    },
                                    new JsonObject
                                    {
                                        ["name"] = "cdd_generate_in_memory",
                                        ["description"] = "Run generation core directly in memory",
                                        ["inputSchema"] = new JsonObject
                                        {
                                            ["type"] = "object",
                                            ["properties"] = new JsonObject { ["schema"] = new JsonObject { ["type"] = "string" } },
                                            ["required"] = new JsonArray("schema")
                                        }
                                    },
                                    new JsonObject
                                    {
                                        ["name"] = "cdd_from_arazzo",
                                        ["description"] = "Generate C# workflow code from an Arazzo 1.1.0 specification",
                                        ["inputSchema"] = new JsonObject
                                        {
                                            ["type"] = "object",
                                            ["properties"] = new JsonObject { ["arazzo"] = new JsonObject { ["type"] = "string" } },
                                            ["required"] = new JsonArray("arazzo")
                                        }
                                    },
                                    new JsonObject
                                    {
                                        ["name"] = "cdd_to_arazzo",
                                        ["description"] = "Extract Arazzo 1.1.0 specification from C# workflow code",
                                        ["inputSchema"] = new JsonObject
                                        {
                                            ["type"] = "object",
                                            ["properties"] = new JsonObject { ["code"] = new JsonObject { ["type"] = "string" } },
                                            ["required"] = new JsonArray("code")
                                        }
                                    })
                            });
                        }
                        else if (method == "tools/call")
                        {
                            if (root.TryGetProperty("params", out var paramsProp) && paramsProp.TryGetProperty("name", out var nameProp))
                            {
                                var toolName = nameProp.GetString();
                                bool hasArgs = paramsProp.TryGetProperty("arguments", out var argsProp);
                                try
                                {
                                    if (toolName == "cdd_generate")
                                    {
                                        if (hasArgs && argsProp.TryGetProperty("input", out var inputProp))
                                        {
                                            var outDir = argsProp.TryGetProperty("output", out var outProp) ? outProp.GetString()! : Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                                            CddGenerator.GenerateAll(new CddConfig { InputPath = inputProp.GetString()!, OutputDir = outDir, NoGithubActions = true, NoInstallablePackage = true });
                                        }
                                        else
                                        {
                                            CodeGenerator.Generate(activeDoc);
                                        }

                                        WriteJsonRpc(writer, idNode, new JsonObject
                                        {
                                            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = "Generated code successfully" })
                                        });
                                    }
                                    else if (toolName == "cdd_inspect")
                                    {
                                        if (hasArgs && argsProp.TryGetProperty("input", out var inputProp))
                                        {
                                            activeDoc = parser.ParseJson(File.ReadAllText(inputProp.GetString()!));
                                        }

                                        var ir = Cdd.OpenApi.Ir.IrService.FromOpenApi(activeDoc);
                                        WriteJsonRpc(writer, idNode, new JsonObject
                                        {
                                            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = $"Schema inspected successfully ({ir.Endpoints.Count} endpoints)" })
                                        });
                                    }
                                    else if (toolName == "cdd_extract")
                                    {
                                        var sources = hasArgs && argsProp.TryGetProperty("input", out var inputProp)
                                            ? new[] { File.ReadAllText(inputProp.GetString()!) }
                                            : Array.Empty<string>();
                                        activeDoc = SpecGenerator.Generate(sources);
                                        var emittedJson = emitter.EmitJson(activeDoc);
                                        WriteJsonRpc(writer, idNode, new JsonObject
                                        {
                                            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = $"Code-to-schema extracted successfully: {emittedJson}" })
                                        });
                                    }
                                    else if (toolName == "cdd_generate_in_memory")
                                    {
                                        var docToGen = hasArgs && argsProp.TryGetProperty("schema", out var schemaProp)
                                            ? parser.ParseJson(schemaProp.GetString()!)
                                            : activeDoc;
                                        var generatedFiles = CodeGenerator.Generate(docToGen);
                                        WriteJsonRpc(writer, idNode, new JsonObject
                                        {
                                            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = $"In-memory generation successful ({generatedFiles.Count} files)" })
                                        });
                                    }
                                    else if (toolName == "cdd_from_arazzo")
                                    {
                                        var arazzoText = argsProp.GetProperty("arazzo").GetString()!;
                                        var arazzoDoc = Arazzo.ArazzoParser.ParseYamlOrJson(arazzoText);
                                        var generatedFiles = ArazzoCodeGenerator.Generate(arazzoDoc);
                                        WriteJsonRpc(writer, idNode, new JsonObject
                                        {
                                            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = $"Generated Arazzo workflow code successfully ({generatedFiles.Count} files)" })
                                        });
                                    }
                                    else if (toolName == "cdd_to_arazzo")
                                    {
                                        var csText = argsProp.GetProperty("code").GetString()!;
                                        var arazzoDoc = ArazzoSpecGenerator.Generate(csText);
                                        var emittedArazzo = Arazzo.ArazzoEmitter.EmitJson(arazzoDoc);
                                        WriteJsonRpc(writer, idNode, new JsonObject
                                        {
                                            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = emittedArazzo })
                                        });
                                    }
                                    else
                                    {
                                        WriteJsonRpc(writer, idNode, null, new JsonObject { ["code"] = -32601, ["message"] = "Tool not found" });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    WriteJsonRpc(writer, idNode, null, new JsonObject { ["code"] = -32603, ["message"] = ex.Message });
                                }
                            }
                            else
                            {
                                WriteJsonRpc(writer, idNode, null, new JsonObject { ["code"] = -32602, ["message"] = "Invalid params" });
                            }
                        }
                        else if (method == "resources/list")
                        {
                            WriteJsonRpc(writer, idNode, new JsonObject
                            {
                                ["resources"] = new JsonArray(
                                    new JsonObject { ["uri"] = "cdd://schema", ["name"] = "Loaded API Schema", ["mimeType"] = "application/json" },
                                    new JsonObject { ["uri"] = "cdd://ast", ["name"] = "Internal AST Structures", ["mimeType"] = "application/json" })
                            });
                        }
                        else if (method == "resources/read")
                        {
                            if (root.TryGetProperty("params", out var pProp) && pProp.TryGetProperty("uri", out var uriProp) && uriProp.GetString() == "cdd://ast")
                            {
                                var ir = Cdd.OpenApi.Ir.IrService.FromOpenApi(activeDoc);
                                var astObj = new JsonObject
                                {
                                    ["ast"] = ir.Title,
                                    ["modelsCount"] = ir.Models.Count,
                                    ["endpointsCount"] = ir.Endpoints.Count
                                };
                                WriteJsonRpc(writer, idNode, new JsonObject
                                {
                                    ["contents"] = new JsonArray(new JsonObject
                                    {
                                        ["uri"] = "cdd://ast",
                                        ["mimeType"] = "application/json",
                                        ["text"] = JsonSerializer.Serialize(astObj, OpenApiJsonContext.Default.JsonObject)
                                    })
                                });
                            }
                            else
                            {
                                WriteJsonRpc(writer, idNode, new JsonObject
                                {
                                    ["contents"] = new JsonArray(new JsonObject
                                    {
                                        ["uri"] = "cdd://schema",
                                        ["mimeType"] = "application/json",
                                        ["text"] = emitter.EmitJson(activeDoc)
                                    })
                                });
                            }
                        }
                        else if (method == "resources/templates/list")
                        {
                            WriteJsonRpc(writer, idNode, new JsonObject
                            {
                                ["resourceTemplates"] = new JsonArray(
                                    new JsonObject { ["uriTemplate"] = "cdd://{path}", ["name"] = "A template", ["mimeType"] = "application/json" })
                            });
                        }
                        else if (method == "sampling/createMessage")
                        {
                            WriteJsonRpc(writer, idNode, new JsonObject
                            {
                                ["role"] = "assistant",
                                ["model"] = "dummy",
                                ["content"] = new JsonObject { ["type"] = "text", ["text"] = "Sampled message" }
                            });
                        }
                        else if (method == "exit")
                        {
                            break;
                        }
                        else
                        {
                            if (idNode != null)
                            {
                                WriteJsonRpc(writer, idNode, null, new JsonObject { ["code"] = -32601, ["message"] = "Method not found" });
                            }
                        }
                    }
                }
            }

            return 0;
        }
    }
}
