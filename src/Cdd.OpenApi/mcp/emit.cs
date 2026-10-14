// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Mcp
{
    /// <summary>Generates MCP server and protocol model classes as Roslyn syntax nodes.</summary>
    public static class Emit
    {
        /// <summary>Cached pre-built Roslyn AST declarations for the MCP protocol models.</summary>
        private static readonly IReadOnlyList<MemberDeclarationSyntax> CachedMcpModels = BuildMcpModels();

        /// <summary>
        /// Converts a PascalCase or camelCase identifier to snake_case.
        /// </summary>
        /// <param name="text">The input identifier string.</param>
        /// <returns>The snake_case formatted string.</returns>
        public static string ToSnakeCase(string text)
        {
            return Cdd.OpenApi.Common.OperationHelpers.ToSnakeCase(text);
        }

        /// <summary>Generates a Roslyn ClassDeclarationSyntax for an MCP server wrapping the specified OpenAPI paths.</summary>
        /// <param name="className">The name of the generated MCP server class.</param>
        /// <param name="paths">The OpenAPI paths to expose as MCP tools.</param>
        /// <returns>A ClassDeclarationSyntax representing the MCP server.</returns>
        public static ClassDeclarationSyntax ToMcpServer(string className, Cdd.OpenApi.Models.OpenApiPaths paths)
        {
            var sbTools = new StringBuilder();
            var toolCallStatements = new List<StatementSyntax>();

            if (paths != null)
            {
                foreach (var pathKvp in paths)
                {
                    var path = pathKvp.Key;
                    var pathItem = pathKvp.Value;
                    var ops = new Dictionary<string, Cdd.OpenApi.Models.OpenApiOperation?>
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
                        var description = (op.Summary ?? "No description").Replace(@"""", "");

                        var requiredList = new List<string>();
                        var propsSb = new StringBuilder();

                        if (op.Parameters != null)
                        {
                            foreach (var p in op.Parameters)
                            {
                                var typeStr = p.Schema?.Type ?? "string";
                                if (typeStr == "integer") typeStr = "number";
                                var descStr = (p.Description ?? "").Replace(@"""", "");
                                propsSb.Append(@"{""""" + p.Name + @""""":{""""type"""":""""" + typeStr + @""""",""""description"""":""""" + descStr + @"""""}},");
                                if (p.Required == true)
                                {
                                    requiredList.Add(@"""""" + p.Name + @"""""");
                                }
                            }
                        }

                        string propsJson = propsSb.ToString().TrimEnd(',');
                        string reqJson = string.Join(",", requiredList);

                        sbTools.Append(@"{""""name"""":""""" + toolName + @""""",""""description"""":""""" + description + @""""",""""inputSchema"""":{""""type"""":""""object"""",""""properties"""":{" + propsJson + @"},""""required"""":[" + reqJson + @"]}},");

                        var execSb = new StringBuilder();
                        execSb.Append(@"if (toolName == """ + toolName + @""") { ");
                        execSb.Append(@"var argsList = new System.Collections.Generic.List<string> { """ + ToSnakeCase(toolName) + @""" }; ");

                        if (op.Parameters != null)
                        {
                            foreach (var p in op.Parameters)
                            {
                                execSb.Append(@"if (paramsProp.TryGetProperty(""arguments"", out var argsProp) && argsProp.TryGetProperty(""" + p.Name + @""", out var argVal)) { ");
                                execSb.Append(@"argsList.Add(""--" + p.Name.ToLower() + @"""); argsList.Add(argVal.GetRawText().Trim('""')); ");
                                execSb.Append(@"} ");
                            }
                        }

                        execSb.Append(@"var prevOut = Console.Out; ");
                        execSb.Append(@"var ms = new System.IO.MemoryStream(); ");
                        execSb.Append(@"var sw = new System.IO.StreamWriter(ms); ");
                        execSb.Append(@"Console.SetOut(sw); ");
                        execSb.Append(@"ApiClientCli.Main(argsList.ToArray()); ");
                        execSb.Append(@"sw.Flush(); ");
                        execSb.Append(@"Console.SetOut(prevOut); ");
                        execSb.Append(@"var execOut = System.Text.Encoding.UTF8.GetString(ms.ToArray()).Replace(""\n"", "" "").Replace(""\r"", """").Replace("""""", """"); ");
                        execSb.Append(@"writer.WriteLine(@""{""""jsonrpc"""":""""2.0"""",""""id"""":"" + id + @"",""""result"""":{""""content"""":[{""""type"""":""""text"""",""""text"""":"""""" + execOut + @""""""}]}}""); continue; }");

                        toolCallStatements.Add(SyntaxFactory.ParseStatement(execSb.ToString()));
                    }
                }
            }

            string toolsJson = sbTools.ToString().TrimEnd(',');
            string callsCode = string.Join(" ", toolCallStatements.Select(s => s.ToFullString()));

            var runMethodBody = SyntaxFactory.Block(
                SyntaxFactory.ParseStatement(@"var stdout = Console.OpenStandardOutput();"),
                SyntaxFactory.ParseStatement(@"using var writer = new StreamWriter(stdout, new System.Text.UTF8Encoding(false)) { AutoFlush = true };"),
                SyntaxFactory.ParseStatement(@"var stdin = Console.OpenStandardInput();"),
                SyntaxFactory.ParseStatement(@"using var reader = new StreamReader(stdin, System.Text.Encoding.UTF8);"),
                SyntaxFactory.ParseStatement(
                    @"while (true) { " +
                    @"var line = reader.ReadLine(); " +
                    @"if (line == null) break; " +
                    @"if (string.IsNullOrWhiteSpace(line)) continue; " +
                    @"try { " +
                    @"var document = System.Text.Json.JsonDocument.Parse(line); " +
                    @"var root = document.RootElement; " +
                    @"if (root.TryGetProperty(""method"", out var methodProp)) { " +
                    @"var method = methodProp.GetString(); " +
                    @"var id = root.TryGetProperty(""id"", out var idProp) ? idProp.GetRawText() : ""null""; " +
                    @"if (method == ""initialize"") { " +
                    @"writer.WriteLine(@""{""""jsonrpc"""":""""2.0"""",""""id"""":"" + id + @"" ,""""result"""":{""""protocolVersion"""":""""2024-11-05"""",""""capabilities"""":{""""tools"""":{}},""""serverInfo"""":{""""name"""":""""GeneratedMCP"""",""""version"""":""""1.0.0""""}}}""); " +
                    @"} else if (method == ""notifications/initialized"") { " +
                    @"_initialized = true; " +
                    @"} else if (method == ""ping"") { " +
                    @"writer.WriteLine(@""{""""jsonrpc"""":""""2.0"""",""""id"""":"" + id + @"" ,""""result"""":{}}""); " +
                    @"} else if (method == ""tools/list"") { " +
                    @"string rawJson = @""" + toolsJson + @"""; " +
                    @"writer.WriteLine(@""{""""jsonrpc"""":""""2.0"""",""""id"""":"" + id + @"" ,""""result"""":{""""tools"""":["" + rawJson + @""]}}""); " +
                    @"} else if (method == ""tools/call"") { " +
                    @"if (root.TryGetProperty(""params"", out var paramsProp) && paramsProp.TryGetProperty(""name"", out var nameProp)) { " +
                    @"var toolName = nameProp.GetString(); " +
                    callsCode + @" " +
                    @"writer.WriteLine(@""{""""jsonrpc"""":""""2.0"""",""""id"""":"" + id + @"" ,""""error"""":{""""code"""":-32601,""""message"""":""""Tool not found""""}}""); " +
                    @"} else { " +
                    @"writer.WriteLine(@""{""""jsonrpc"""":""""2.0"""",""""id"""":"" + id + @"" ,""""error"""":{""""code"""":-32602,""""message"""":""""Invalid params""""}}""); " +
                    @"} " +
                    @"} else if (method == ""notifications/cancelled"") { " +
                    @"} else if (method == ""exit"") { break; " +
                    @"} else { " +
                    @"if (id != ""null"") { writer.WriteLine(@""{""""jsonrpc"""":""""2.0"""",""""id"""":"" + id + @"" ,""""error"""":{""""code"""":-32601,""""message"""":""""Method not found""""}}""); } " +
                    @"} " +
                    @"} else { " +
                    @"var id = root.TryGetProperty(""id"", out var idProp) ? idProp.GetRawText() : ""null""; " +
                    @"if (id != ""null"") { writer.WriteLine(@""{""""jsonrpc"""":""""2.0"""",""""id"""":"" + id + @"" ,""""error"""":{""""code"""":-32600,""""message"""":""""Invalid Request""""}}""); } " +
                    @"} " +
                    @"} catch (Exception) { " +
                    @"writer.WriteLine(@""{""""jsonrpc"""":""""2.0"""",""""error"""":{""""code"""":-32700,""""message"""":""""Parse error""""}}""); " +
                    @"} " +
                    @"}")
            );

            var runMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "Run")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(runMethodBody);

            return SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    SyntaxFactory.ParseMemberDeclaration("private bool _initialized = false;")!,
                    runMethod
                );
        }

        /// <summary>Generates the IMcpTransport interface declaration.</summary>
        /// <returns>An InterfaceDeclarationSyntax representing IMcpTransport.</returns>
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

        /// <summary>Returns the pre-built Roslyn AST declarations for the MCP protocol models.</summary>
        /// <returns>An enumerable of MemberDeclarationSyntax nodes for MCP models.</returns>
        public static IEnumerable<MemberDeclarationSyntax> ToMcpModels()
        {
            return CachedMcpModels;
        }

        /// <summary>
        /// Constructs a Roslyn <see cref="ClassDeclarationSyntax"/> for an MCP protocol model with JSON property attributes.
        /// </summary>
        /// <param name="name">The C# class name.</param>
        /// <param name="baseType">The optional base class name.</param>
        /// <param name="properties">The array of JSON property definitions.</param>
        /// <returns>The constructed <see cref="ClassDeclarationSyntax"/>.</returns>
        private static ClassDeclarationSyntax CreateModelClass(
            string name,
            string? baseType,
            params (string JsonName, string Type, string PropName, string? Initializer)[] properties)
        {
            var cls = SyntaxFactory.ClassDeclaration(name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (baseType != null)
            {
                cls = cls.WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(baseType)))));
            }

            var members = new List<MemberDeclarationSyntax>(properties.Length);
            foreach (var p in properties)
            {
                var prop = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(p.Type), p.PropName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("JsonPropertyName"))
                            .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(p.JsonName)))))))))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    );

                if (p.Initializer != null)
                {
                    var expr = p.Initializer.StartsWith("@STR:")
                        ? SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(p.Initializer.Substring(5)))
                        : SyntaxFactory.ParseExpression(p.Initializer);
                    prop = prop.WithInitializer(SyntaxFactory.EqualsValueClause(expr))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                }

                members.Add(prop);
            }

            return cls.AddMembers(members.ToArray());
        }

        /// <summary>
        /// Builds the complete list of MCP protocol model AST declarations.
        /// </summary>
        /// <returns>An immutable list of <see cref="MemberDeclarationSyntax"/> nodes.</returns>
        private static IReadOnlyList<MemberDeclarationSyntax> BuildMcpModels()
        {
            return new MemberDeclarationSyntax[]
            {
                CreateModelClass("Annotations", null,
                    ("audience", "List<string>?", "Audience", null),
                    ("priority", "double?", "Priority", null)),
                CreateModelClass("Annotated", null,
                    ("annotations", "Annotations?", "Annotations", null)),
                CreateModelClass("ResourceContents", null,
                    ("uri", "string", "Uri", "@STR:"),
                    ("mimeType", "string?", "MimeType", null)),
                CreateModelClass("BlobResourceContents", "ResourceContents",
                    ("blob", "string", "Blob", "@STR:")),
                CreateModelClass("TextResourceContents", "ResourceContents",
                    ("text", "string", "Text", "@STR:")),
                CreateModelClass("CallToolRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:tools/call"),
                    ("params", "CallToolRequestParams", "Params", "new()")),
                CreateModelClass("CallToolRequestParams", null,
                    ("name", "string", "Name", "@STR:"),
                    ("arguments", "Dictionary<string, object>?", "Arguments", null)),
                CreateModelClass("CallToolResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("content", "List<object>", "Content", "new()"),
                    ("isError", "bool?", "IsError", null)),
                CreateModelClass("CancelledNotification", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("method", "string", "Method", "@STR:notifications/cancelled"),
                    ("params", "CancelledNotificationParams", "Params", "new()")),
                CreateModelClass("CancelledNotificationParams", null,
                    ("requestId", "string", "RequestId", "@STR:"),
                    ("reason", "string?", "Reason", null)),
                CreateModelClass("ClientCapabilities", null,
                    ("experimental", "Dictionary<string, object>?", "Experimental", null),
                    ("roots", "RootsCapability?", "Roots", null),
                    ("sampling", "SamplingCapability?", "Sampling", null)),
                CreateModelClass("RootsCapability", null,
                    ("listChanged", "bool?", "ListChanged", null)),
                CreateModelClass("SamplingCapability", null),
                CreateModelClass("LoggingCapability", null),
                CreateModelClass("PromptsCapability", null,
                    ("listChanged", "bool?", "ListChanged", null)),
                CreateModelClass("ResourcesCapability", null,
                    ("listChanged", "bool?", "ListChanged", null),
                    ("subscribe", "bool?", "Subscribe", null)),
                CreateModelClass("ToolsCapability", null,
                    ("listChanged", "bool?", "ListChanged", null)),
                CreateModelClass("ClientNotification", null),
                CreateModelClass("ClientRequest", null),
                CreateModelClass("ClientResult", null),
                CreateModelClass("CompleteRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:completion/complete"),
                    ("params", "CompleteRequestParams", "Params", "new()")),
                CreateModelClass("CompleteRequestParams", null,
                    ("ref", "CompleteRequestRef", "Ref", "new()"),
                    ("argument", "CompleteRequestArgument", "Argument", "new()")),
                CreateModelClass("CompleteRequestRef", null,
                    ("type", "string", "Type", "@STR:"),
                    ("name", "string", "Name", "@STR:")),
                CreateModelClass("CompleteRequestArgument", null,
                    ("name", "string", "Name", "@STR:"),
                    ("value", "string", "Value", "@STR:")),
                CreateModelClass("CompleteResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("completion", "Completion", "Completion", "new()")),
                CreateModelClass("Completion", null,
                    ("values", "List<string>", "Values", "new()"),
                    ("total", "int?", "Total", null),
                    ("hasMore", "bool?", "HasMore", null)),
                CreateModelClass("CreateMessageRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:sampling/createMessage"),
                    ("params", "CreateMessageRequestParams", "Params", "new()")),
                CreateModelClass("CreateMessageRequestParams", null,
                    ("messages", "List<SamplingMessage>", "Messages", "new()"),
                    ("modelPreferences", "ModelPreferences?", "ModelPreferences", null),
                    ("systemPrompt", "string?", "SystemPrompt", null),
                    ("includeContext", "string?", "IncludeContext", null),
                    ("temperature", "double?", "Temperature", null),
                    ("maxTokens", "int", "MaxTokens", null),
                    ("stopSequences", "List<string>?", "StopSequences", null),
                    ("metadata", "Dictionary<string, object>?", "Metadata", null)),
                CreateModelClass("CreateMessageResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("role", "string", "Role", "@STR:assistant"),
                    ("content", "object", "Content", "new TextContent()"),
                    ("model", "string", "Model", "@STR:"),
                    ("stopReason", "string?", "StopReason", null)),
                CreateModelClass("Cursor", null),
                CreateModelClass("EmbeddedResource", "Annotated",
                    ("type", "string", "Type", "@STR:resource"),
                    ("resource", "ResourceContents", "Resource", "new()")),
                CreateModelClass("EmptyResult", null),
                CreateModelClass("GetPromptRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:prompts/get"),
                    ("params", "GetPromptRequestParams", "Params", "new()")),
                CreateModelClass("GetPromptRequestParams", null,
                    ("name", "string", "Name", "@STR:"),
                    ("arguments", "Dictionary<string, string>?", "Arguments", null)),
                CreateModelClass("GetPromptResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("description", "string?", "Description", null),
                    ("messages", "List<PromptMessage>", "Messages", "new()")),
                CreateModelClass("ImageContent", "Annotated",
                    ("type", "string", "Type", "@STR:image"),
                    ("data", "string", "Data", "@STR:"),
                    ("mimeType", "string", "MimeType", "@STR:")),
                CreateModelClass("Implementation", null,
                    ("name", "string", "Name", "@STR:"),
                    ("version", "string", "Version", "@STR:")),
                CreateModelClass("InitializeRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:initialize"),
                    ("params", "InitializeParams", "Params", "new()")),
                CreateModelClass("InitializeParams", null,
                    ("protocolVersion", "string", "ProtocolVersion", "@STR:2024-11-05"),
                    ("capabilities", "ClientCapabilities", "Capabilities", "new()"),
                    ("clientInfo", "Implementation", "ClientInfo", "new()")),
                CreateModelClass("InitializeResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("protocolVersion", "string", "ProtocolVersion", "@STR:2024-11-05"),
                    ("capabilities", "ServerCapabilities", "Capabilities", "new()"),
                    ("serverInfo", "Implementation", "ServerInfo", "new()"),
                    ("instructions", "string?", "Instructions", null)),
                CreateModelClass("InitializedNotification", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("method", "string", "Method", "@STR:notifications/initialized"),
                    ("params", "InitializedNotificationParams?", "Params", null)),
                CreateModelClass("InitializedNotificationParams", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null)),
                CreateModelClass("JSONRPCError", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("error", "ErrorDetail", "Error", "new()")),
                CreateModelClass("ErrorDetail", null,
                    ("code", "int", "Code", null),
                    ("message", "string", "Message", "@STR:"),
                    ("data", "object?", "Data", null)),
                CreateModelClass("JSONRPCMessage", null),
                CreateModelClass("JSONRPCNotification", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("method", "string", "Method", "@STR:"),
                    ("params", "NotificationParams?", "Params", null)),
                CreateModelClass("JSONRPCRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:"),
                    ("params", "RequestParams?", "Params", null)),
                CreateModelClass("JSONRPCResponse", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("result", "Dictionary<string, object>?", "Result", null)),
                CreateModelClass("ListPromptsRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:prompts/list"),
                    ("params", "PaginatedRequestParams?", "Params", null)),
                CreateModelClass("ListPromptsResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("nextCursor", "string?", "NextCursor", null),
                    ("prompts", "List<Prompt>", "Prompts", "new()")),
                CreateModelClass("ListResourceTemplatesRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:resources/templates/list"),
                    ("params", "PaginatedRequestParams?", "Params", null)),
                CreateModelClass("ListResourceTemplatesResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("nextCursor", "string?", "NextCursor", null),
                    ("resourceTemplates", "List<ResourceTemplate>", "ResourceTemplates", "new()")),
                CreateModelClass("ListResourcesRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:resources/list"),
                    ("params", "PaginatedRequestParams?", "Params", null)),
                CreateModelClass("ListResourcesResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("nextCursor", "string?", "NextCursor", null),
                    ("resources", "List<Resource>", "Resources", "new()")),
                CreateModelClass("ListRootsRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:roots/list"),
                    ("params", "RequestParams?", "Params", null)),
                CreateModelClass("ListRootsResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("roots", "List<Root>", "Roots", "new()")),
                CreateModelClass("ListToolsRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:tools/list"),
                    ("params", "PaginatedRequestParams?", "Params", null)),
                CreateModelClass("ListToolsResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("nextCursor", "string?", "NextCursor", null),
                    ("tools", "List<Tool>", "Tools", "new()")),
                CreateModelClass("LoggingLevel", null),
                CreateModelClass("LoggingMessageNotification", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("method", "string", "Method", "@STR:notifications/message"),
                    ("params", "LoggingMessageNotificationParams", "Params", "new()")),
                CreateModelClass("LoggingMessageNotificationParams", null,
                    ("level", "string", "Level", "@STR:"),
                    ("logger", "string?", "Logger", null),
                    ("data", "object", "Data", "@STR:")),
                CreateModelClass("ModelHint", null,
                    ("name", "string?", "Name", null)),
                CreateModelClass("ModelPreferences", null,
                    ("hints", "List<ModelHint>?", "Hints", null),
                    ("costPriority", "double?", "CostPriority", null),
                    ("speedPriority", "double?", "SpeedPriority", null),
                    ("intelligencePriority", "double?", "IntelligencePriority", null)),
                CreateModelClass("Notification", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("method", "string", "Method", "@STR:"),
                    ("params", "NotificationParams?", "Params", null)),
                CreateModelClass("NotificationParams", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null)),
                CreateModelClass("PaginatedRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:"),
                    ("params", "PaginatedRequestParams?", "Params", null)),
                CreateModelClass("PaginatedRequestParams", null,
                    ("cursor", "string?", "Cursor", null)),
                CreateModelClass("PaginatedResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("nextCursor", "string?", "NextCursor", null)),
                CreateModelClass("PingRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:ping"),
                    ("params", "RequestParams?", "Params", null)),
                CreateModelClass("ProgressNotification", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("method", "string", "Method", "@STR:notifications/progress"),
                    ("params", "ProgressNotificationParams", "Params", "new()")),
                CreateModelClass("ProgressNotificationParams", null,
                    ("progressToken", "string", "ProgressToken", "@STR:"),
                    ("progress", "double", "Progress", null),
                    ("total", "double?", "Total", null)),
                CreateModelClass("ProgressToken", null),
                CreateModelClass("Prompt", null,
                    ("name", "string", "Name", "@STR:"),
                    ("description", "string?", "Description", null),
                    ("arguments", "List<PromptArgument>?", "Arguments", null)),
                CreateModelClass("PromptArgument", null,
                    ("name", "string", "Name", "@STR:"),
                    ("description", "string?", "Description", null),
                    ("required", "bool?", "Required", null)),
                CreateModelClass("PromptListChangedNotification", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("method", "string", "Method", "@STR:notifications/prompts/list_changed"),
                    ("params", "NotificationParams?", "Params", null)),
                CreateModelClass("PromptMessage", null,
                    ("role", "string", "Role", "@STR:user"),
                    ("content", "object", "Content", "new TextContent()")),
                CreateModelClass("PromptReference", null,
                    ("type", "string", "Type", "@STR:prompt"),
                    ("name", "string", "Name", "@STR:")),
                CreateModelClass("ReadResourceRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:resources/read"),
                    ("params", "ReadResourceRequestParams", "Params", "new()")),
                CreateModelClass("ReadResourceRequestParams", null,
                    ("uri", "string", "Uri", "@STR:")),
                CreateModelClass("ReadResourceResult", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("contents", "List<ResourceContents>", "Contents", "new()")),
                CreateModelClass("Request", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:"),
                    ("params", "RequestParams?", "Params", null)),
                CreateModelClass("RequestParams", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null),
                    ("progressToken", "string?", "ProgressToken", null)),
                CreateModelClass("RequestId", null),
                CreateModelClass("Resource", "Annotated",
                    ("uri", "string", "Uri", "@STR:"),
                    ("name", "string", "Name", "@STR:"),
                    ("description", "string?", "Description", null),
                    ("mimeType", "string?", "MimeType", null),
                    ("size", "int?", "Size", null)),
                CreateModelClass("ResourceListChangedNotification", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("method", "string", "Method", "@STR:notifications/resources/list_changed"),
                    ("params", "NotificationParams?", "Params", null)),
                CreateModelClass("ResourceReference", null,
                    ("type", "string", "Type", "@STR:ref/resource"),
                    ("uri", "string", "Uri", "@STR:")),
                CreateModelClass("ResourceTemplate", "Annotated",
                    ("uriTemplate", "string", "UriTemplate", "@STR:"),
                    ("name", "string", "Name", "@STR:"),
                    ("description", "string?", "Description", null),
                    ("mimeType", "string?", "MimeType", null)),
                CreateModelClass("ResourceUpdatedNotification", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("method", "string", "Method", "@STR:notifications/resources/updated"),
                    ("params", "ResourceUpdatedNotificationParams", "Params", "new()")),
                CreateModelClass("ResourceUpdatedNotificationParams", null,
                    ("uri", "string", "Uri", "@STR:")),
                CreateModelClass("Result", null,
                    ("_meta", "Dictionary<string, object>?", "Meta", null)),
                CreateModelClass("Role", null),
                CreateModelClass("Root", null,
                    ("uri", "string", "Uri", "@STR:"),
                    ("name", "string?", "Name", null)),
                CreateModelClass("RootsListChangedNotification", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("method", "string", "Method", "@STR:notifications/roots/list_changed"),
                    ("params", "NotificationParams?", "Params", null)),
                CreateModelClass("SamplingMessage", null,
                    ("role", "string", "Role", "@STR:user"),
                    ("content", "object", "Content", "new TextContent()")),
                CreateModelClass("ServerCapabilities", null,
                    ("experimental", "Dictionary<string, object>?", "Experimental", null),
                    ("logging", "LoggingCapability?", "Logging", null),
                    ("prompts", "PromptsCapability?", "Prompts", null),
                    ("resources", "ResourcesCapability?", "Resources", null),
                    ("tools", "ToolsCapability?", "Tools", null)),
                CreateModelClass("ServerNotification", null),
                CreateModelClass("ServerRequest", null),
                CreateModelClass("ServerResult", null),
                CreateModelClass("SetLevelRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:logging/setLevel"),
                    ("params", "SetLevelRequestParams", "Params", "new()")),
                CreateModelClass("SetLevelRequestParams", null,
                    ("level", "string", "Level", "@STR:")),
                CreateModelClass("SubscribeRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:resources/subscribe"),
                    ("params", "SubscribeRequestParams", "Params", "new()")),
                CreateModelClass("SubscribeRequestParams", null,
                    ("uri", "string", "Uri", "@STR:")),
                CreateModelClass("TextContent", "Annotated",
                    ("type", "string", "Type", "@STR:text"),
                    ("text", "string", "Text", "@STR:")),
                CreateModelClass("Tool", null,
                    ("name", "string", "Name", "@STR:"),
                    ("description", "string?", "Description", null),
                    ("inputSchema", "ToolInputSchema", "InputSchema", "new()")),
                CreateModelClass("ToolInputSchema", null,
                    ("type", "string", "Type", "@STR:object"),
                    ("properties", "Dictionary<string, object>?", "Properties", null),
                    ("required", "List<string>?", "Required", null)),
                CreateModelClass("ToolListChangedNotification", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("method", "string", "Method", "@STR:notifications/tools/list_changed"),
                    ("params", "NotificationParams?", "Params", null)),
                CreateModelClass("UnsubscribeRequest", null,
                    ("jsonrpc", "string", "JsonRpc", "@STR:2.0"),
                    ("id", "string", "Id", "@STR:"),
                    ("method", "string", "Method", "@STR:resources/unsubscribe"),
                    ("params", "UnsubscribeRequestParams", "Params", "new()")),
                CreateModelClass("UnsubscribeRequestParams", null,
                    ("uri", "string", "Uri", "@STR:"))
            };
        }
    }
}
