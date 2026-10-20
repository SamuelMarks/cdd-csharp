// <copyright file="OperationHelpers.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Common
{
    /// <summary>
    /// Enumeration of standard and extended HTTP verbs supported in OpenAPI 3.2.0.
    /// </summary>
    public enum HttpVerb
    {
        /// <summary>HTTP GET method.</summary>
        Get,

        /// <summary>HTTP PUT method.</summary>
        Put,

        /// <summary>HTTP POST method.</summary>
        Post,

        /// <summary>HTTP DELETE method.</summary>
        Delete,

        /// <summary>HTTP OPTIONS method.</summary>
        Options,

        /// <summary>HTTP HEAD method.</summary>
        Head,

        /// <summary>HTTP PATCH method.</summary>
        Patch,

        /// <summary>HTTP TRACE method.</summary>
        Trace,

        /// <summary>HTTP QUERY method (OpenAPI 3.2.0).</summary>
        Query,

        /// <summary>Custom or extension HTTP method defined in <c>additionalOperations</c>.</summary>
        Custom
    }

    /// <summary>
    /// Enumeration of OpenAPI and Swagger parameter locations (<c>in</c>).
    /// </summary>
    public enum ParameterLocation
    {
        /// <summary>Query string parameter (<c>query</c>).</summary>
        Query,

        /// <summary>HTTP request header parameter (<c>header</c>).</summary>
        Header,

        /// <summary>URL path template parameter (<c>path</c>).</summary>
        Path,

        /// <summary>HTTP cookie parameter (<c>cookie</c>).</summary>
        Cookie,

        /// <summary>Swagger 2.0 body parameter (<c>body</c>).</summary>
        Body,

        /// <summary>Swagger 2.0 form data parameter (<c>formData</c>).</summary>
        FormData
    }

    /// <summary>
    /// Provides shared helper methods for iterating and normalizing OpenAPI path operations.
    /// </summary>
    public static class OperationHelpers
    {
        /// <summary>
        /// Parses an HTTP method string into a strongly-typed <see cref="HttpVerb"/> value.
        /// </summary>
        /// <param name="method">The HTTP method string (case-insensitive).</param>
        /// <returns>The corresponding <see cref="HttpVerb"/> enum value.</returns>
        public static HttpVerb ParseHttpVerb(string? method)
        {
            return (method?.Trim().ToUpperInvariant()) switch
            {
                "GET" => HttpVerb.Get,
                "PUT" => HttpVerb.Put,
                "POST" => HttpVerb.Post,
                "DELETE" => HttpVerb.Delete,
                "OPTIONS" => HttpVerb.Options,
                "HEAD" => HttpVerb.Head,
                "PATCH" => HttpVerb.Patch,
                "TRACE" => HttpVerb.Trace,
                "QUERY" => HttpVerb.Query,
                _ => HttpVerb.Custom
            };
        }

        /// <summary>
        /// Converts an <see cref="HttpVerb"/> enum value (and optional custom verb string) to its canonical PascalCase name.
        /// </summary>
        /// <param name="verb">The HTTP verb enum value.</param>
        /// <param name="customVerb">Optional custom verb string when <paramref name="verb"/> is <see cref="HttpVerb.Custom"/>.</param>
        /// <returns>The PascalCase verb name (e.g., <c>"Get"</c>, <c>"Post"</c>).</returns>
        public static string ToPascalVerb(HttpVerb verb, string? customVerb = null)
        {
            return verb switch
            {
                HttpVerb.Get => "Get",
                HttpVerb.Put => "Put",
                HttpVerb.Post => "Post",
                HttpVerb.Delete => "Delete",
                HttpVerb.Options => "Options",
                HttpVerb.Head => "Head",
                HttpVerb.Patch => "Patch",
                HttpVerb.Trace => "Trace",
                HttpVerb.Query => "Query",
                _ => !string.IsNullOrEmpty(customVerb)
                    ? customVerb.Substring(0, 1).ToUpperInvariant() + customVerb.Substring(1).ToLowerInvariant()
                    : "Get"
            };
        }

        /// <summary>
        /// Assigns an <see cref="OpenApiOperation"/> onto the appropriate property of an <see cref="OpenApiPathItem"/> for the given HTTP method string.
        /// </summary>
        /// <param name="pathItem">The target <see cref="OpenApiPathItem"/>.</param>
        /// <param name="httpMethod">The HTTP method name (case-insensitive).</param>
        /// <param name="operation">The operation to assign.</param>
        public static void SetOperation(OpenApiPathItem pathItem, string httpMethod, OpenApiOperation operation)
        {
            var verb = ParseHttpVerb(httpMethod);
            switch (verb)
            {
                case HttpVerb.Get: pathItem.Get = operation; break;
                case HttpVerb.Put: pathItem.Put = operation; break;
                case HttpVerb.Post: pathItem.Post = operation; break;
                case HttpVerb.Delete: pathItem.Delete = operation; break;
                case HttpVerb.Options: pathItem.Options = operation; break;
                case HttpVerb.Head: pathItem.Head = operation; break;
                case HttpVerb.Patch: pathItem.Patch = operation; break;
                case HttpVerb.Trace: pathItem.Trace = operation; break;
                case HttpVerb.Query: pathItem.Query = operation; break;
                default:
                    pathItem.AdditionalOperations ??= new Dictionary<string, OpenApiOperation>();
                    pathItem.AdditionalOperations[httpMethod.ToUpperInvariant()] = operation;
                    break;
            }
        }

        /// <summary>
        /// Parses an OpenAPI parameter location string (<c>in</c>) into a <see cref="ParameterLocation"/> value.
        /// </summary>
        /// <param name="location">The parameter location string (e.g., <c>"query"</c>, <c>"path"</c>).</param>
        /// <returns>The parsed <see cref="ParameterLocation"/> value.</returns>
        public static ParameterLocation ParseParameterLocation(string? location)
        {
            return (location?.Trim().ToLowerInvariant()) switch
            {
                "header" => ParameterLocation.Header,
                "path" => ParameterLocation.Path,
                "cookie" => ParameterLocation.Cookie,
                "body" => ParameterLocation.Body,
                "formdata" => ParameterLocation.FormData,
                _ => ParameterLocation.Query
            };
        }

        /// <summary>
        /// Converts a <see cref="ParameterLocation"/> enum value to its canonical OpenAPI string representation.
        /// </summary>
        /// <param name="location">The parameter location enum value.</param>
        /// <returns>The canonical lowercase OpenAPI location string.</returns>
        public static string ToLocationString(ParameterLocation location)
        {
            return location switch
            {
                ParameterLocation.Header => "header",
                ParameterLocation.Path => "path",
                ParameterLocation.Cookie => "cookie",
                ParameterLocation.Body => "body",
                ParameterLocation.FormData => "formData",
                _ => "query"
            };
        }

        /// <summary>
        /// Extracts all HTTP operations defined on an <see cref="OpenApiPathItem"/> keyed by PascalCase HTTP verb name.
        /// </summary>
        /// <param name="pathItem">The OpenAPI path item to inspect.</param>
        /// <returns>A dictionary of HTTP verb names (e.g., "Get", "Post") to their <see cref="OpenApiOperation"/> instances.</returns>
        public static Dictionary<string, OpenApiOperation?> GetOperationsByPascalVerb(OpenApiPathItem pathItem)
        {
            var operations = new Dictionary<string, OpenApiOperation?>
            {
                { "Get", pathItem.Get },
                { "Put", pathItem.Put },
                { "Post", pathItem.Post },
                { "Delete", pathItem.Delete },
                { "Options", pathItem.Options },
                { "Head", pathItem.Head },
                { "Patch", pathItem.Patch },
                { "Trace", pathItem.Trace },
                { "Query", pathItem.Query }
            };

            if (pathItem.AdditionalOperations != null)
            {
                foreach (var addOp in pathItem.AdditionalOperations)
                {
                    if (!string.IsNullOrEmpty(addOp.Key))
                    {
                        var normVerb = addOp.Key.Substring(0, 1).ToUpperInvariant() + addOp.Key.Substring(1).ToLowerInvariant();
                        operations[normVerb] = addOp.Value;
                    }
                }
            }

            return operations;
        }

        /// <summary>
        /// Converts a collection of <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> instances and optional path metadata into an <see cref="OpenApiPaths"/> dictionary.
        /// </summary>
        /// <param name="endpoints">The IR endpoints to convert.</param>
        /// <param name="pathMetadata">Optional path-level metadata keyed by path template.</param>
        /// <returns>The constructed <see cref="OpenApiPaths"/> dictionary.</returns>
        public static OpenApiPaths ConvertEndpointsToPaths(
            IEnumerable<Cdd.OpenApi.Ir.IrEndpoint> endpoints,
            IReadOnlyDictionary<string, Cdd.OpenApi.Ir.IrPathMetadata>? pathMetadata = null)
        {
            var paths = new OpenApiPaths();
            if (pathMetadata != null)
            {
                foreach (var metaKvp in pathMetadata)
                {
                    paths[metaKvp.Key] = new OpenApiPathItem
                    {
                        Ref = metaKvp.Value.Ref,
                        Summary = metaKvp.Value.Summary,
                        Description = metaKvp.Value.Description,
                        Servers = metaKvp.Value.Servers,
                        Parameters = metaKvp.Value.Parameters
                    };
                }
            }

            foreach (var ep in endpoints)
            {
                if (!paths.TryGetValue(ep.Path, out var pathItem))
                {
                    pathItem = new OpenApiPathItem();
                    paths[ep.Path] = pathItem;
                }

                SetOperation(pathItem, ep.HttpMethod, ep.ToOpenApiOperation());
            }

            return paths;
        }

        /// <summary>
        /// Extracts a list of <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> instances from an <see cref="IDictionary{TKey, TValue}"/> of route paths to <see cref="OpenApiPathItem"/>.
        /// </summary>
        /// <param name="paths">The OpenAPI paths dictionary to inspect.</param>
        /// <returns>A list of extracted <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> instances.</returns>
        public static List<Cdd.OpenApi.Ir.IrEndpoint> ExtractEndpointsFromPaths(IDictionary<string, OpenApiPathItem> paths)
        {
            var endpoints = new List<Cdd.OpenApi.Ir.IrEndpoint>();
            foreach (var pathKvp in paths)
            {
                var ops = GetOperationsByPascalVerb(pathKvp.Value);
                foreach (var opKvp in ops)
                {
                    if (opKvp.Value != null)
                    {
                        endpoints.Add(Cdd.OpenApi.Ir.IrEndpoint.FromOpenApiOperation(pathKvp.Key, opKvp.Key, opKvp.Value));
                    }
                }
            }

            return endpoints;
        }

        /// <summary>
        /// Infers an <see cref="OpenApiPathItem"/> dictionary from method names on a C# class (used for mocks and unit test suites).
        /// </summary>
        /// <param name="classNode">The C# class declaration syntax node to inspect.</param>
        /// <param name="stripTestSuffix">Whether to strip a trailing <c>Test</c> suffix from method names before inferring the verb and operationId.</param>
        /// <returns>A dictionary mapping inferred route paths to <see cref="OpenApiPathItem"/> instances.</returns>
        public static IDictionary<string, OpenApiPathItem> ParseHeuristicPaths(
            Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classNode,
            bool stripTestSuffix)
        {
            var paths = new Dictionary<string, OpenApiPathItem>();
            foreach (var method in classNode.Members.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>())
            {
                var name = method.Identifier.Text;
                if (stripTestSuffix && name.EndsWith("Test", System.StringComparison.Ordinal))
                {
                    name = name.Substring(0, name.Length - 4);
                }

                var route = "/" + name.ToLowerInvariant();
                var op = new OpenApiOperation { OperationId = name };
                var pathItem = new OpenApiPathItem();

                if (name.StartsWith("Post", System.StringComparison.Ordinal) && name.Length > 4)
                {
                    pathItem.Post = op;
                }
                else if (name.StartsWith("Put", System.StringComparison.Ordinal) && name.Length > 3)
                {
                    pathItem.Put = op;
                }
                else if (name.StartsWith("Delete", System.StringComparison.Ordinal) && name.Length > 6)
                {
                    pathItem.Delete = op;
                }
                else if (name.StartsWith("Patch", System.StringComparison.Ordinal) && name.Length > 5)
                {
                    pathItem.Patch = op;
                }
                else if (name.StartsWith("Options", System.StringComparison.Ordinal) && name.Length > 7)
                {
                    pathItem.Options = op;
                }
                else if (name.StartsWith("Head", System.StringComparison.Ordinal) && name.Length > 4)
                {
                    pathItem.Head = op;
                }
                else if (name.StartsWith("Trace", System.StringComparison.Ordinal) && name.Length > 5)
                {
                    pathItem.Trace = op;
                }
                else if (name.StartsWith("Query", System.StringComparison.Ordinal) && name.Length > 5)
                {
                    pathItem.Query = op;
                }
                else
                {
                    pathItem.Get = op;
                }

                paths[route] = pathItem;
            }

            return paths;
        }

        /// <summary>
        /// Converts a PascalCase or camelCase identifier to snake_case.
        /// </summary>
        /// <param name="text">The input identifier string.</param>
        /// <returns>The snake_case formatted string.</returns>
        public static string ToSnakeCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var sb = new System.Text.StringBuilder();
            sb.Append(char.ToLowerInvariant(text[0]));
            for (int i = 1; i < text.Length; ++i)
            {
                char c = text[i];
                if (char.IsUpper(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
