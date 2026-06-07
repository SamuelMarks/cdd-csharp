using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Cdd.OpenApi.Mcp
{
    /// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
        /// <summary>Auto-generated documentation for ToMcpServer.</summary>
        private static string ToSnakeCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return string.Concat(text.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        /// <summary>Auto-generated documentation for ToMcpServer.</summary>
        public static ClassDeclarationSyntax ToMcpServer(string className, Cdd.OpenApi.Models.OpenApiPaths paths)
        {
            var sbTools = new System.Text.StringBuilder();
            var sbCalls = new System.Text.StringBuilder();
            if (paths != null)
            {
                foreach (var pathKvp in paths)
                {
                    var path = pathKvp.Key;
                    var pathItem = pathKvp.Value;
                    var ops = new System.Collections.Generic.Dictionary<string, Cdd.OpenApi.Models.OpenApiOperation?>
                    {
                        { "get", pathItem.Get },
                        { "post", pathItem.Post },
                        { "put", pathItem.Put },
                        { "delete", pathItem.Delete },
                        { "patch", pathItem.Patch }
                    };

                    foreach (var opKvp in ops)
                    {
                        var op = opKvp.Value;
                        if (op == null) continue;

                        var operationId = op.OperationId ?? (opKvp.Key + path.Replace("/", "").Replace("{", "").Replace("}", ""));
                        var toolName = operationId;
                        var description = op.Summary ?? "No description";
                        description = description.Replace("\"", "\\\"");

                        var requiredList = new System.Collections.Generic.List<string>();
                        var propsSb = new System.Text.StringBuilder();

                        if (op.Parameters != null)
                        {
                            foreach (var p in op.Parameters)
                            {
                                var typeStr = p.Schema?.Type ?? "string";
                                if (typeStr == "integer") typeStr = "number";
                                var descStr = p.Description ?? "";
                                descStr = descStr.Replace("\"", "\\\"");
                                propsSb.Append("{\"\"" + p.Name + "\"\":{\"\"type\"\":\"\"" + typeStr + "\"\".\"\"description\"\":\"\"" + descStr + "\"\"}},");
                                if (p.Required == true)
                                {
                                    requiredList.Add("\"\"" + p.Name + "\"\"");
                                }
                            }
                        }

                        string propsJson = propsSb.ToString().TrimEnd(',');
                        propsJson = propsJson.Replace(".", ",");
                        string reqJson = string.Join(",", requiredList);

                        sbTools.Append("{\"\"name\"\":\"\"" + toolName + "\"\".\"\"description\"\":\"\"" + description + "\"\".\"\"inputSchema\"\":{\"\"type\"\":\"\"object\"\".\"\"properties\"\":{" + propsJson + "}.\"\"required\"\":[" + reqJson + "]}},");

                        var execSb = new System.Text.StringBuilder();
                        execSb.Append("if (toolName == \"" + toolName + "\") { ");
                        execSb.Append("var argsList = new System.Collections.Generic.List<string> { \"" + ToSnakeCase(toolName) + "\" }; ");

                        if (op.Parameters != null)
                        {
                            foreach (var p in op.Parameters)
                            {
                                execSb.Append("if (paramsProp.TryGetProperty(\"arguments\", out var argsProp) && argsProp.TryGetProperty(\"" + p.Name + "\", out var argVal)) { ");
                                execSb.Append("argsList.Add(\"--" + p.Name.ToLower() + "\"); argsList.Add(argVal.GetRawText().Trim('\"')); ");
                                execSb.Append("} ");
                            }
                        }

                        execSb.Append("var prevOut = Console.Out; ");
                        execSb.Append("var ms = new System.IO.MemoryStream(); ");
                        execSb.Append("var sw = new System.IO.StreamWriter(ms); ");
                        execSb.Append("Console.SetOut(sw); ");
                        execSb.Append("ApiClientCli.Main(argsList.ToArray()); ");
                        execSb.Append("sw.Flush(); ");
                        execSb.Append("Console.SetOut(prevOut); ");
                        execSb.Append("var execOut = System.Text.Encoding.UTF8.GetString(ms.ToArray()).Replace(\"\\n\", \"\\\\n\").Replace(\"\\r\", \"\\\\r\").Replace(\"\\\"\", \"\\\\\\\"\"); ");

                        execSb.Append("writer.WriteLine(\"{\\\"jsonrpc\\\":\\\"2.0\\\",\\\"id\\\":\" + id + \",\\\"result\\\":{\\\"content\\\":[{\\\"type\\\":\\\"text\\\",\\\"text\\\":\\\"\" + execOut + \"\\\"}]}}\"); continue; }");

                        sbCalls.Append(execSb.ToString());
                    }
                }
            }
            string toolsJson = sbTools.ToString().TrimEnd(',').Replace(".", ",");
            string callsCode = sbCalls.ToString();

            var codeB = new System.Text.StringBuilder();
            codeB.AppendLine("public class " + className);
            codeB.AppendLine("{");
            codeB.AppendLine("    private bool _initialized = false;");
            codeB.AppendLine("    public void Run()");
            codeB.AppendLine("    {");
            codeB.AppendLine("        var stdout = Console.OpenStandardOutput();");
            codeB.AppendLine("        using var writer = new StreamWriter(stdout, new System.Text.UTF8Encoding(false)) { AutoFlush = true };");
            codeB.AppendLine("        var stdin = Console.OpenStandardInput();");
            codeB.AppendLine("        using var reader = new StreamReader(stdin, System.Text.Encoding.UTF8);");
            codeB.AppendLine("        while (true)");
            codeB.AppendLine("        {");
            codeB.AppendLine("            var line = reader.ReadLine();");
            codeB.AppendLine("            if (line == null) break;");
            codeB.AppendLine("            if (string.IsNullOrWhiteSpace(line)) continue;");
            codeB.AppendLine("            try");
            codeB.AppendLine("            {");
            codeB.AppendLine("                var document = System.Text.Json.JsonDocument.Parse(line);");
            codeB.AppendLine("                var root = document.RootElement;");
            codeB.AppendLine("                if (root.TryGetProperty(\"method\", out var methodProp))");
            codeB.AppendLine("                {");
            codeB.AppendLine("                    var method = methodProp.GetString();");
            codeB.AppendLine("                    var id = root.TryGetProperty(\"id\", out var idProp) ? idProp.GetRawText() : \"null\";");
            codeB.AppendLine("                    if (method == \"initialize\")");
            codeB.AppendLine("                    {");
            codeB.AppendLine("                        writer.WriteLine(\"{\\\"jsonrpc\\\":\\\"2.0\\\",\\\"id\\\":\" + id + \",\\\"result\\\":{\\\"protocolVersion\\\":\\\"2024-11-05\\\",\\\"capabilities\\\":{\\\"tools\\\":{}},\\\"serverInfo\\\":{\\\"name\\\":\\\"GeneratedMCP\\\",\\\"version\\\":\\\"1.0.0\\\"}}}\");");
            codeB.AppendLine("                    }");
            codeB.AppendLine("                    else if (method == \"notifications/initialized\")");
            codeB.AppendLine("                    {");
            codeB.AppendLine("                        _initialized = true;");
            codeB.AppendLine("                    }");
            codeB.AppendLine("                    else if (method == \"ping\")");
            codeB.AppendLine("                    {");
            codeB.AppendLine("                        writer.WriteLine(\"{\\\"jsonrpc\\\":\\\"2.0\\\",\\\"id\\\":\" + id + \",\\\"result\\\":{}}\");");
            codeB.AppendLine("                    }");
            codeB.AppendLine("                    else if (method == \"tools/list\")");
            codeB.AppendLine("                    {");
            codeB.AppendLine("                        string rawJson = @\"" + toolsJson.Replace("\"", "\"\"") + "\";");
            codeB.AppendLine("                        rawJson = rawJson.Replace(\"\\\"\\\"\", \"\\\"\");");
            codeB.AppendLine("                        writer.WriteLine(\"{\\\"jsonrpc\\\":\\\"2.0\\\",\\\"id\\\":\" + id + \",\\\"result\\\":{\\\"tools\\\":[\" + rawJson + \"]}}\");");
            codeB.AppendLine("                    }");
            codeB.AppendLine("                    else if (method == \"tools/call\")");
            codeB.AppendLine("                    {");
            codeB.AppendLine("                        if (root.TryGetProperty(\"params\", out var paramsProp) && paramsProp.TryGetProperty(\"name\", out var nameProp))");
            codeB.AppendLine("                        {");
            codeB.AppendLine("                            var toolName = nameProp.GetString();");
            codeB.AppendLine("                            " + callsCode);
            codeB.AppendLine("                            writer.WriteLine(\"{\\\"jsonrpc\\\":\\\"2.0\\\",\\\"id\\\":\" + id + \",\\\"error\\\":{\\\"code\\\":-32601,\\\"message\\\":\\\"Tool not found\\\"}}\");");
            codeB.AppendLine("                        }");
            codeB.AppendLine("                        else");
            codeB.AppendLine("                        {");
            codeB.AppendLine("                            writer.WriteLine(\"{\\\"jsonrpc\\\":\\\"2.0\\\",\\\"id\\\":\" + id + \",\\\"error\\\":{\\\"code\\\":-32602,\\\"message\\\":\\\"Invalid params\\\"}}\");");
            codeB.AppendLine("                        }");
            codeB.AppendLine("                    }");
            codeB.AppendLine("                    else if (method == \"notifications/cancelled\") { }");
            codeB.AppendLine("                    else if (method == \"exit\") { break; }");
            codeB.AppendLine("                    else");
            codeB.AppendLine("                    {");
            codeB.AppendLine("                        if (id != \"null\")");
            codeB.AppendLine("                        {");
            codeB.AppendLine("                            writer.WriteLine(\"{\\\"jsonrpc\\\":\\\"2.0\\\",\\\"id\\\":\" + id + \",\\\"error\\\":{\\\"code\\\":-32601,\\\"message\\\":\\\"Method not found\\\"}}\");");
            codeB.AppendLine("                        }");
            codeB.AppendLine("                    }");
            codeB.AppendLine("                }");
            codeB.AppendLine("                else");
            codeB.AppendLine("                {");
            codeB.AppendLine("                    var id = root.TryGetProperty(\"id\", out var idProp) ? idProp.GetRawText() : \"null\";");
            codeB.AppendLine("                    if (id != \"null\")");
            codeB.AppendLine("                    {");
            codeB.AppendLine("                        writer.WriteLine(\"{\\\"jsonrpc\\\":\\\"2.0\\\",\\\"id\\\":\" + id + \",\\\"error\\\":{\\\"code\\\":-32600,\\\"message\\\":\\\"Invalid Request\\\"}}\");");
            codeB.AppendLine("                    }");
            codeB.AppendLine("                }");
            codeB.AppendLine("            }");
            codeB.AppendLine("            catch (Exception)");
            codeB.AppendLine("            {");
            codeB.AppendLine("                writer.WriteLine(\"{\\\"jsonrpc\\\":\\\"2.0\\\",\\\"error\\\":{\\\"code\\\":-32700,\\\"message\\\":\\\"Parse error\\\"}}\");");
            codeB.AppendLine("            }");
            codeB.AppendLine("        }");
            codeB.AppendLine("    }");
            codeB.AppendLine("}");

            var syntaxTree = CSharpSyntaxTree.ParseText(codeB.ToString());
            var rootNode = syntaxTree.GetRoot();
            return WasmSafeRoslyn.GetDescendantNodesSafe(rootNode).OfType<ClassDeclarationSyntax>().First();
        }
        /// <summary>Auto-generated documentation for ToMcpModels.</summary>
        public static InterfaceDeclarationSyntax ToMcpTransport()
        {
            return SyntaxFactory.InterfaceDeclaration("IMcpTransport")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("System.Threading.Tasks.Task"), "SendAsync")
                        .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("message")).WithType(SyntaxFactory.ParseTypeName("string")))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("System.Threading.Tasks.Task<string>"), "ReceiveAsync")
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                );
        }

        /// <summary>Auto-generated documentation for ToMcpModels.</summary>
        public static System.Collections.Generic.IEnumerable<MemberDeclarationSyntax> ToMcpModels()
        {
            var code = @"
    public class Annotations
    {
        [JsonPropertyName(""audience"")] public List<string>? Audience { get; set; }
        [JsonPropertyName(""priority"")] public double? Priority { get; set; }
    }

    public class Annotated
    {
        [JsonPropertyName(""annotations"")] public Annotations? Annotations { get; set; }
    }

    public class ResourceContents
    {
        [JsonPropertyName(""uri"")] public string Uri { get; set; } = """";
        [JsonPropertyName(""mimeType"")] public string? MimeType { get; set; }
    }

    public class BlobResourceContents : ResourceContents
    {
        [JsonPropertyName(""blob"")] public string Blob { get; set; } = """";
    }

    public class TextResourceContents : ResourceContents
    {
        [JsonPropertyName(""text"")] public string Text { get; set; } = """";
    }

    public class CallToolRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""tools/call"";
        [JsonPropertyName(""params"")] public CallToolRequestParams Params { get; set; } = new();
    }

    public class CallToolRequestParams
    {
        [JsonPropertyName(""name"")] public string Name { get; set; } = """";
        [JsonPropertyName(""arguments"")] public Dictionary<string, object>? Arguments { get; set; }
    }

    public class CallToolResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""content"")] public List<object> Content { get; set; } = new();
        [JsonPropertyName(""isError"")] public bool? IsError { get; set; }
    }

    public class CancelledNotification
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""notifications/cancelled"";
        [JsonPropertyName(""params"")] public CancelledNotificationParams Params { get; set; } = new();
    }

    public class CancelledNotificationParams
    {
        [JsonPropertyName(""requestId"")] public string RequestId { get; set; } = """";
        [JsonPropertyName(""reason"")] public string? Reason { get; set; }
    }

    public class ClientCapabilities
    {
        [JsonPropertyName(""experimental"")] public Dictionary<string, object>? Experimental { get; set; }
        [JsonPropertyName(""roots"")] public RootsCapability? Roots { get; set; }
        [JsonPropertyName(""sampling"")] public SamplingCapability? Sampling { get; set; }
    }

    public class RootsCapability { [JsonPropertyName(""listChanged"")] public bool? ListChanged { get; set; } }
    public class SamplingCapability { }
    public class LoggingCapability { }
    public class PromptsCapability { [JsonPropertyName(""listChanged"")] public bool? ListChanged { get; set; } }
    public class ResourcesCapability { [JsonPropertyName(""listChanged"")] public bool? ListChanged { get; set; } [JsonPropertyName(""subscribe"")] public bool? Subscribe { get; set; } }
    public class ToolsCapability { [JsonPropertyName(""listChanged"")] public bool? ListChanged { get; set; } }

    public class ClientNotification { }
    public class ClientRequest { }
    public class ClientResult { }

    public class CompleteRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""completion/complete"";
        [JsonPropertyName(""params"")] public CompleteRequestParams Params { get; set; } = new();
    }

    public class CompleteRequestParams
    {
        [JsonPropertyName(""ref"")] public CompleteRequestRef Ref { get; set; } = new();
        [JsonPropertyName(""argument"")] public CompleteRequestArgument Argument { get; set; } = new();
    }

    public class CompleteRequestRef
    {
        [JsonPropertyName(""type"")] public string Type { get; set; } = """";
        [JsonPropertyName(""name"")] public string Name { get; set; } = """";
    }

    public class CompleteRequestArgument
    {
        [JsonPropertyName(""name"")] public string Name { get; set; } = """";
        [JsonPropertyName(""value"")] public string Value { get; set; } = """";
    }

    public class CompleteResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""completion"")] public Completion Completion { get; set; } = new();
    }

    public class Completion
    {
        [JsonPropertyName(""values"")] public List<string> Values { get; set; } = new();
        [JsonPropertyName(""total"")] public int? Total { get; set; }
        [JsonPropertyName(""hasMore"")] public bool? HasMore { get; set; }
    }

    public class CreateMessageRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""sampling/createMessage"";
        [JsonPropertyName(""params"")] public CreateMessageRequestParams Params { get; set; } = new();
    }

    public class CreateMessageRequestParams
    {
        [JsonPropertyName(""messages"")] public List<SamplingMessage> Messages { get; set; } = new();
        [JsonPropertyName(""modelPreferences"")] public ModelPreferences? ModelPreferences { get; set; }
        [JsonPropertyName(""systemPrompt"")] public string? SystemPrompt { get; set; }
        [JsonPropertyName(""includeContext"")] public string? IncludeContext { get; set; }
        [JsonPropertyName(""temperature"")] public double? Temperature { get; set; }
        [JsonPropertyName(""maxTokens"")] public int MaxTokens { get; set; }
        [JsonPropertyName(""stopSequences"")] public List<string>? StopSequences { get; set; }
        [JsonPropertyName(""metadata"")] public Dictionary<string, object>? Metadata { get; set; }
    }

    public class CreateMessageResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""role"")] public string Role { get; set; } = ""assistant"";
        [JsonPropertyName(""content"")] public object Content { get; set; } = new TextContent();
        [JsonPropertyName(""model"")] public string Model { get; set; } = """";
        [JsonPropertyName(""stopReason"")] public string? StopReason { get; set; }
    }

    public class Cursor { }

    public class EmbeddedResource : Annotated
    {
        [JsonPropertyName(""type"")] public string Type { get; set; } = ""resource"";
        [JsonPropertyName(""resource"")] public ResourceContents Resource { get; set; } = new();
    }

    public class EmptyResult { }

    public class GetPromptRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""prompts/get"";
        [JsonPropertyName(""params"")] public GetPromptRequestParams Params { get; set; } = new();
    }

    public class GetPromptRequestParams
    {
        [JsonPropertyName(""name"")] public string Name { get; set; } = """";
        [JsonPropertyName(""arguments"")] public Dictionary<string, string>? Arguments { get; set; }
    }

    public class GetPromptResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""description"")] public string? Description { get; set; }
        [JsonPropertyName(""messages"")] public List<PromptMessage> Messages { get; set; } = new();
    }

    public class ImageContent : Annotated
    {
        [JsonPropertyName(""type"")] public string Type { get; set; } = ""image"";
        [JsonPropertyName(""data"")] public string Data { get; set; } = """";
        [JsonPropertyName(""mimeType"")] public string MimeType { get; set; } = """";
    }

    public class Implementation
    {
        [JsonPropertyName(""name"")] public string Name { get; set; } = """";
        [JsonPropertyName(""version"")] public string Version { get; set; } = """";
    }

    public class InitializeRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""initialize"";
        [JsonPropertyName(""params"")] public InitializeParams Params { get; set; } = new();
    }

    public class InitializeParams
    {
        [JsonPropertyName(""protocolVersion"")] public string ProtocolVersion { get; set; } = ""2024-11-05"";
        [JsonPropertyName(""capabilities"")] public ClientCapabilities Capabilities { get; set; } = new();
        [JsonPropertyName(""clientInfo"")] public Implementation ClientInfo { get; set; } = new();
    }

    public class InitializeResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""protocolVersion"")] public string ProtocolVersion { get; set; } = ""2024-11-05"";
        [JsonPropertyName(""capabilities"")] public ServerCapabilities Capabilities { get; set; } = new();
        [JsonPropertyName(""serverInfo"")] public Implementation ServerInfo { get; set; } = new();
        [JsonPropertyName(""instructions"")] public string? Instructions { get; set; }
    }

    public class InitializedNotification
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""notifications/initialized"";
        [JsonPropertyName(""params"")] public InitializedNotificationParams? Params { get; set; }
    }

    public class InitializedNotificationParams
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
    }

    public class JSONRPCError
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""error"")] public ErrorDetail Error { get; set; } = new();
    }

    public class ErrorDetail
    {
        [JsonPropertyName(""code"")] public int Code { get; set; }
        [JsonPropertyName(""message"")] public string Message { get; set; } = """";
        [JsonPropertyName(""data"")] public object? Data { get; set; }
    }

    public class JSONRPCMessage { }

    public class JSONRPCNotification
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""method"")] public string Method { get; set; } = """";
        [JsonPropertyName(""params"")] public NotificationParams? Params { get; set; }
    }

    public class JSONRPCRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = """";
        [JsonPropertyName(""params"")] public RequestParams? Params { get; set; }
    }

    public class JSONRPCResponse
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""result"")] public Dictionary<string, object>? Result { get; set; }
    }

    public class ListPromptsRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""prompts/list"";
        [JsonPropertyName(""params"")] public PaginatedRequestParams? Params { get; set; }
    }

    public class ListPromptsResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""nextCursor"")] public string? NextCursor { get; set; }
        [JsonPropertyName(""prompts"")] public List<Prompt> Prompts { get; set; } = new();
    }

    public class ListResourceTemplatesRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""resources/templates/list"";
        [JsonPropertyName(""params"")] public PaginatedRequestParams? Params { get; set; }
    }

    public class ListResourceTemplatesResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""nextCursor"")] public string? NextCursor { get; set; }
        [JsonPropertyName(""resourceTemplates"")] public List<ResourceTemplate> ResourceTemplates { get; set; } = new();
    }

    public class ListResourcesRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""resources/list"";
        [JsonPropertyName(""params"")] public PaginatedRequestParams? Params { get; set; }
    }

    public class ListResourcesResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""nextCursor"")] public string? NextCursor { get; set; }
        [JsonPropertyName(""resources"")] public List<Resource> Resources { get; set; } = new();
    }

    public class ListRootsRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""roots/list"";
        [JsonPropertyName(""params"")] public RequestParams? Params { get; set; }
    }

    public class ListRootsResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""roots"")] public List<Root> Roots { get; set; } = new();
    }

    public class ListToolsRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""tools/list"";
        [JsonPropertyName(""params"")] public PaginatedRequestParams? Params { get; set; }
    }

    public class ListToolsResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""nextCursor"")] public string? NextCursor { get; set; }
        [JsonPropertyName(""tools"")] public List<Tool> Tools { get; set; } = new();
    }

    public class LoggingLevel { }

    public class LoggingMessageNotification
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""notifications/message"";
        [JsonPropertyName(""params"")] public LoggingMessageNotificationParams Params { get; set; } = new();
    }

    public class LoggingMessageNotificationParams
    {
        [JsonPropertyName(""level"")] public string Level { get; set; } = """";
        [JsonPropertyName(""logger"")] public string? Logger { get; set; }
        [JsonPropertyName(""data"")] public object Data { get; set; } = """";
    }

    public class ModelHint
    {
        [JsonPropertyName(""name"")] public string? Name { get; set; }
    }

    public class ModelPreferences
    {
        [JsonPropertyName(""hints"")] public List<ModelHint>? Hints { get; set; }
        [JsonPropertyName(""costPriority"")] public double? CostPriority { get; set; }
        [JsonPropertyName(""speedPriority"")] public double? SpeedPriority { get; set; }
        [JsonPropertyName(""intelligencePriority"")] public double? IntelligencePriority { get; set; }
    }

    public class Notification
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""method"")] public string Method { get; set; } = """";
        [JsonPropertyName(""params"")] public NotificationParams? Params { get; set; }
    }

    public class NotificationParams
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
    }

    public class PaginatedRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = """";
        [JsonPropertyName(""params"")] public PaginatedRequestParams? Params { get; set; }
    }

    public class PaginatedRequestParams
    {
        [JsonPropertyName(""cursor"")] public string? Cursor { get; set; }
    }

    public class PaginatedResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""nextCursor"")] public string? NextCursor { get; set; }
    }

    public class PingRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""ping"";
        [JsonPropertyName(""params"")] public RequestParams? Params { get; set; }
    }

    public class ProgressNotification
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""notifications/progress"";
        [JsonPropertyName(""params"")] public ProgressNotificationParams Params { get; set; } = new();
    }

    public class ProgressNotificationParams
    {
        [JsonPropertyName(""progressToken"")] public string ProgressToken { get; set; } = """";
        [JsonPropertyName(""progress"")] public double Progress { get; set; }
        [JsonPropertyName(""total"")] public double? Total { get; set; }
    }

    public class ProgressToken { }

    public class Prompt
    {
        [JsonPropertyName(""name"")] public string Name { get; set; } = """";
        [JsonPropertyName(""description"")] public string? Description { get; set; }
        [JsonPropertyName(""arguments"")] public List<PromptArgument>? Arguments { get; set; }
    }

    public class PromptArgument
    {
        [JsonPropertyName(""name"")] public string Name { get; set; } = """";
        [JsonPropertyName(""description"")] public string? Description { get; set; }
        [JsonPropertyName(""required"")] public bool? Required { get; set; }
    }

    public class PromptListChangedNotification
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""notifications/prompts/list_changed"";
        [JsonPropertyName(""params"")] public NotificationParams? Params { get; set; }
    }

    public class PromptMessage
    {
        [JsonPropertyName(""role"")] public string Role { get; set; } = ""user"";
        [JsonPropertyName(""content"")] public object Content { get; set; } = new TextContent();
    }

    public class PromptReference
    {
        [JsonPropertyName(""type"")] public string Type { get; set; } = ""prompt"";
        [JsonPropertyName(""name"")] public string Name { get; set; } = """";
    }

    public class ReadResourceRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""resources/read"";
        [JsonPropertyName(""params"")] public ReadResourceRequestParams Params { get; set; } = new();
    }

    public class ReadResourceRequestParams
    {
        [JsonPropertyName(""uri"")] public string Uri { get; set; } = """";
    }

    public class ReadResourceResult
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""contents"")] public List<ResourceContents> Contents { get; set; } = new();
    }

    public class Request
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = """";
        [JsonPropertyName(""params"")] public RequestParams? Params { get; set; }
    }

    public class RequestParams
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
        [JsonPropertyName(""progressToken"")] public string? ProgressToken { get; set; }
    }

    public class RequestId { }

    public class Resource : Annotated
    {
        [JsonPropertyName(""uri"")] public string Uri { get; set; } = """";
        [JsonPropertyName(""name"")] public string Name { get; set; } = """";
        [JsonPropertyName(""description"")] public string? Description { get; set; }
        [JsonPropertyName(""mimeType"")] public string? MimeType { get; set; }
        [JsonPropertyName(""size"")] public int? Size { get; set; }
    }

    public class ResourceListChangedNotification
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""notifications/resources/list_changed"";
        [JsonPropertyName(""params"")] public NotificationParams? Params { get; set; }
    }

    public class ResourceReference
    {
        [JsonPropertyName(""type"")] public string Type { get; set; } = ""ref/resource"";
        [JsonPropertyName(""uri"")] public string Uri { get; set; } = """";
    }

    public class ResourceTemplate : Annotated
    {
        [JsonPropertyName(""uriTemplate"")] public string UriTemplate { get; set; } = """";
        [JsonPropertyName(""name"")] public string Name { get; set; } = """";
        [JsonPropertyName(""description"")] public string? Description { get; set; }
        [JsonPropertyName(""mimeType"")] public string? MimeType { get; set; }
    }

    public class ResourceUpdatedNotification
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""notifications/resources/updated"";
        [JsonPropertyName(""params"")] public ResourceUpdatedNotificationParams Params { get; set; } = new();
    }

    public class ResourceUpdatedNotificationParams
    {
        [JsonPropertyName(""uri"")] public string Uri { get; set; } = """";
    }

    public class Result
    {
        [JsonPropertyName(""_meta"")] public Dictionary<string, object>? Meta { get; set; }
    }

    public class Role { }

    public class Root
    {
        [JsonPropertyName(""uri"")] public string Uri { get; set; } = """";
        [JsonPropertyName(""name"")] public string? Name { get; set; }
    }

    public class RootsListChangedNotification
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""notifications/roots/list_changed"";
        [JsonPropertyName(""params"")] public NotificationParams? Params { get; set; }
    }

    public class SamplingMessage
    {
        [JsonPropertyName(""role"")] public string Role { get; set; } = ""user"";
        [JsonPropertyName(""content"")] public object Content { get; set; } = new TextContent();
    }

    public class ServerCapabilities
    {
        [JsonPropertyName(""experimental"")] public Dictionary<string, object>? Experimental { get; set; }
        [JsonPropertyName(""logging"")] public LoggingCapability? Logging { get; set; }
        [JsonPropertyName(""prompts"")] public PromptsCapability? Prompts { get; set; }
        [JsonPropertyName(""resources"")] public ResourcesCapability? Resources { get; set; }
        [JsonPropertyName(""tools"")] public ToolsCapability? Tools { get; set; }
    }

    public class ServerNotification { }
    public class ServerRequest { }
    public class ServerResult { }

    public class SetLevelRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""logging/setLevel"";
        [JsonPropertyName(""params"")] public SetLevelRequestParams Params { get; set; } = new();
    }

    public class SetLevelRequestParams
    {
        [JsonPropertyName(""level"")] public string Level { get; set; } = """";
    }

    public class SubscribeRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""resources/subscribe"";
        [JsonPropertyName(""params"")] public SubscribeRequestParams Params { get; set; } = new();
    }

    public class SubscribeRequestParams
    {
        [JsonPropertyName(""uri"")] public string Uri { get; set; } = """";
    }

    public class TextContent : Annotated
    {
        [JsonPropertyName(""type"")] public string Type { get; set; } = ""text"";
        [JsonPropertyName(""text"")] public string Text { get; set; } = """";
    }

    public class Tool
    {
        [JsonPropertyName(""name"")] public string Name { get; set; } = """";
        [JsonPropertyName(""description"")] public string? Description { get; set; }
        [JsonPropertyName(""inputSchema"")] public ToolInputSchema InputSchema { get; set; } = new();
    }

    public class ToolInputSchema
    {
        [JsonPropertyName(""type"")] public string Type { get; set; } = ""object"";
        [JsonPropertyName(""properties"")] public Dictionary<string, object>? Properties { get; set; }
        [JsonPropertyName(""required"")] public List<string>? Required { get; set; }
    }

    public class ToolListChangedNotification
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""notifications/tools/list_changed"";
        [JsonPropertyName(""params"")] public NotificationParams? Params { get; set; }
    }

    public class UnsubscribeRequest
    {
        [JsonPropertyName(""jsonrpc"")] public string JsonRpc { get; set; } = ""2.0"";
        [JsonPropertyName(""id"")] public string Id { get; set; } = """";
        [JsonPropertyName(""method"")] public string Method { get; set; } = ""resources/unsubscribe"";
        [JsonPropertyName(""params"")] public UnsubscribeRequestParams Params { get; set; } = new();
    }

    public class UnsubscribeRequestParams
    {
        [JsonPropertyName(""uri"")] public string Uri { get; set; } = """";
    }
            ";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();
            return WasmSafeRoslyn.GetDescendantNodesSafe(root).OfType<ClassDeclarationSyntax>();
        }
    }
}
