// <copyright file="CliHelpText.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

namespace Cdd.OpenApi.Cli
{
    /// <summary>
    /// Centralizes command-line help text constants for the <c>cdd-csharp</c> CLI.
    /// </summary>
    public static class CliHelpText
    {
        /// <summary>Help text for the <c>from_openapi</c> command.</summary>
        public const string FromOpenApiHelp = """
            Generate code from an OpenAPI specification.

            Usage:
              cdd-csharp from_openapi [to_sdk|to_sdk_cli|to_server] -i <input> | -d|--input-dir <dir> [-o <output>]

            Options:
              -i, --input                 Path or URL to the OpenAPI specification.
              -d, --input-dir             Directory containing OpenAPI specifications.
              -o, --output                Output file or directory path.
              --tests                     Generate integration tests and mocks.
              --no-github-actions         Do not generate GitHub Actions scaffolding.
              --no-installable-package    Do not generate installable package scaffolding.
              -m, --mcp                   Generate Model Context Protocol (MCP) server and adapter.
            """;

        /// <summary>Help text for the <c>to_openapi</c> command.</summary>
        public const string ToOpenApiHelp = """
            Usage:
              cdd-csharp to_openapi -i <input> [-o <output>]

            Options:
              -i, --input    Path to source code directory or file.
              -o, --output   Output file or directory path.
            """;

        /// <summary>Help text for the <c>from_arazzo</c> command.</summary>
        public const string FromArazzoHelp = """
            Generate code from an Arazzo 1.1.0 specification.

            Usage:
              cdd-csharp from_arazzo [to_sdk|to_sdk_cli|to_server] -i <input> | -d|--input-dir <dir> [-o <output>]

            Options:
              -i, --input                 Path to the Arazzo specification.
              -d, --input-dir             Directory containing Arazzo specifications.
              -o, --output                Output directory path.
              --tests                     Generate workflow integration tests and mocks.
              --no-github-actions         Do not generate GitHub Actions scaffolding.
              --no-installable-package    Do not generate installable package scaffolding.
            """;

        /// <summary>Help text for the <c>to_arazzo</c> command.</summary>
        public const string ToArazzoHelp = """
            Usage:
              cdd-csharp to_arazzo -i <input> [-o <output>]

            Options:
              -i, --input    Path to C# workflow source code directory or file.
              -o, --output   Output Arazzo specification file path (JSON or YAML).
            """;

        /// <summary>Help text for the <c>to_docs_json</c> command.</summary>
        public const string ToDocsJsonHelp = """
            Usage:
              cdd-csharp to_docs_json -i <input> [-o <output>]

            Options:
              -i, --input    Path or URL to the OpenAPI specification.
              -o, --output   Output file or directory path.
              --no-imports   Omit the imports field.
              --no-wrapping  Omit the wrapper fields.
            """;

        /// <summary>Help text for the <c>sync</c> command.</summary>
        public const string SyncHelp = """
            Usage:
              cdd-csharp sync -t <truth> -i <input> [-o <output>]

            Options:
              -t, --truth    Designate a single source of truth for synchronization.
              -i, --input    Path to the source files.
              -o, --output   Output file or directory path.
            """;

        /// <summary>Help text for the <c>serve_json_rpc</c> command.</summary>
        public const string ServeJsonRpcHelp = """
            Usage:
              cdd-csharp serve_json_rpc [-p <port>] [-l <listen>]

            Options:
              -p, --port     Port to listen on (default: 8080).
              -l, --listen   Address to listen on (default: 127.0.0.1).
            """;

        /// <summary>Help text for the <c>mcp</c> command.</summary>
        public const string McpHelp = """
            Usage:
              cdd-csharp mcp

            Options:
              Run the generator as an MCP server over stdio.
            """;

        /// <summary>Help text for the <c>parse</c> command.</summary>
        public const string ParseHelp = """
            Usage:
              cdd-csharp parse <input>

            Options:
              <input>    Path to the OpenAPI specification to parse and inspect.
            """;

        /// <summary>Help text for the <c>emit</c> command.</summary>
        public const string EmitHelp = """
            Usage:
              cdd-csharp emit <input> <output>

            Options:
              <input>    Path to the input OpenAPI specification.
              <output>   Path to write the re-emitted OpenAPI JSON.
            """;
    }
}
