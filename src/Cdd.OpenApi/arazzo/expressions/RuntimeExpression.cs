// <copyright file="RuntimeExpression.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using Cdd.OpenApi.Errors;
using Cdd.OpenApi.Models.Arazzo;

namespace Cdd.OpenApi.Arazzo.Expressions
{
    /// <summary>
    /// Enumerates the 15 Arazzo 1.1.0 runtime expression roots.
    /// </summary>
    public enum RuntimeExpressionKind
    {
        /// <summary>Resolved operation request URL (<c>$url</c>).</summary>
        Url,

        /// <summary>Operation HTTP method (<c>$method</c>).</summary>
        Method,

        /// <summary>Response HTTP status code (<c>$statusCode</c>).</summary>
        StatusCode,

        /// <summary>Outgoing HTTP request header (<c>$request.header.&lt;name&gt;</c>).</summary>
        RequestHeader,

        /// <summary>Outgoing HTTP query parameter (<c>$request.query.&lt;name&gt;</c>).</summary>
        RequestQuery,

        /// <summary>Outgoing HTTP path parameter (<c>$request.path.&lt;name&gt;</c>).</summary>
        RequestPath,

        /// <summary>Outgoing HTTP request body (<c>$request.body</c> or <c>$request.body#/... </c>).</summary>
        RequestBody,

        /// <summary>Incoming HTTP response header (<c>$response.header.&lt;name&gt;</c>).</summary>
        ResponseHeader,

        /// <summary>Incoming HTTP response body (<c>$response.body</c> or <c>$response.body#/... </c>).</summary>
        ResponseBody,

        /// <summary>AsyncAPI message header (<c>$message.header.&lt;name&gt;</c>).</summary>
        MessageHeader,

        /// <summary>AsyncAPI message payload (<c>$message.payload</c> or <c>$message.payload#/... </c>).</summary>
        MessagePayload,

        /// <summary>Workflow input parameter (<c>$inputs.&lt;name&gt;</c>).</summary>
        Inputs,

        /// <summary>Workflow output parameter (<c>$outputs.&lt;name&gt;</c>).</summary>
        Outputs,

        /// <summary>Prior step output variable (<c>$steps.&lt;stepId&gt;.outputs.&lt;name&gt;</c>).</summary>
        Steps,

        /// <summary>Source description property (<c>$sourceDescriptions.&lt;name&gt;.url</c> or operation reference).</summary>
        SourceDescriptions,

        /// <summary>Reusable component reference (<c>$components.&lt;section&gt;.&lt;name&gt;</c>).</summary>
        Components,

        /// <summary>Self-referential document base URI (<c>$self</c>).</summary>
        Self
    }

    /// <summary>
    /// Represents a parsed Arazzo 1.1.0 ABNF runtime expression and provides evaluation and C# code generation helpers.
    /// </summary>
    public sealed class RuntimeExpression
    {
        /// <summary>
        /// Gets the parsed expression kind.
        /// </summary>
        public RuntimeExpressionKind Kind { get; }

        /// <summary>
        /// Gets the primary identifier segment (e.g., header/query/path/input/output name, stepId, sourceDescription name, or component section).
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the secondary identifier segment (e.g., output key in <c>$steps.&lt;stepId&gt;.outputs.&lt;key&gt;</c>, property in <c>$sourceDescriptions.&lt;name&gt;.&lt;prop&gt;</c>, or component name in <c>$components.&lt;section&gt;.&lt;name&gt;</c>).
        /// </summary>
        public string? SubName { get; }

        /// <summary>
        /// Gets the optional RFC 6901 JSON Pointer suffix (e.g., <c>/data/id</c> when the expression contains <c>#/data/id</c>).
        /// </summary>
        public string? JsonPointer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeExpression"/> class.
        /// </summary>
        /// <param name="raw">The raw expression string.</param>
        /// <param name="kind">The parsed expression kind.</param>
        /// <param name="name">The primary identifier token.</param>
        /// <param name="subName">The secondary identifier token.</param>
        /// <param name="jsonPointer">The optional JSON Pointer suffix.</param>
        public RuntimeExpression(string raw, RuntimeExpressionKind kind, string? name = null, string? subName = null, string? jsonPointer = null)
        {
            _ = raw;
            Kind = kind;
            Name = name;
            SubName = subName;
            JsonPointer = jsonPointer;
        }

        /// <summary>
        /// Parses a single Arazzo 1.1.0 runtime expression string into a <see cref="RuntimeExpression"/> AST node.
        /// </summary>
        /// <param name="expression">The runtime expression starting with <c>$</c>.</param>
        /// <returns>The parsed <see cref="RuntimeExpression"/>.</returns>
        /// <exception cref="CddException">Thrown when the expression syntax violates the Arazzo 1.1.0 ABNF grammar.</exception>
        public static RuntimeExpression Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression) || !expression.StartsWith("$", StringComparison.Ordinal))
            {
                throw new CddException(
                    CddErrorCode.InvalidSpecification,
                    $"Invalid Arazzo runtime expression '{expression}'. Runtime expressions must start with '$'.");
            }

            string core = expression.Trim();
            string? pointer = null;
            int hashIdx = core.IndexOf('#');
            if (hashIdx >= 0)
            {
                pointer = core.Substring(hashIdx + 1);
                core = core.Substring(0, hashIdx);
            }

            if (core == "$url")
            {
                return new RuntimeExpression(expression, RuntimeExpressionKind.Url, jsonPointer: pointer);
            }

            if (core == "$method")
            {
                return new RuntimeExpression(expression, RuntimeExpressionKind.Method, jsonPointer: pointer);
            }

            if (core == "$statusCode")
            {
                return new RuntimeExpression(expression, RuntimeExpressionKind.StatusCode, jsonPointer: pointer);
            }

            if (core == "$self")
            {
                return new RuntimeExpression(expression, RuntimeExpressionKind.Self, jsonPointer: pointer);
            }

            if (core.StartsWith("$request.header.", StringComparison.Ordinal))
            {
                var name = core.Substring("$request.header.".Length);
                EnsureNonEmptyToken(name, expression);
                return new RuntimeExpression(expression, RuntimeExpressionKind.RequestHeader, name, jsonPointer: pointer);
            }

            if (core.StartsWith("$request.query.", StringComparison.Ordinal))
            {
                var name = core.Substring("$request.query.".Length);
                EnsureNonEmptyToken(name, expression);
                return new RuntimeExpression(expression, RuntimeExpressionKind.RequestQuery, name, jsonPointer: pointer);
            }

            if (core.StartsWith("$request.path.", StringComparison.Ordinal))
            {
                var name = core.Substring("$request.path.".Length);
                EnsureNonEmptyToken(name, expression);
                return new RuntimeExpression(expression, RuntimeExpressionKind.RequestPath, name, jsonPointer: pointer);
            }

            if (core == "$request.body" || core.StartsWith("$request.body.", StringComparison.Ordinal))
            {
                var propSuffix = core.Length > "$request.body".Length ? "/" + core.Substring("$request.body.".Length).Replace('.', '/') : null;
                return new RuntimeExpression(expression, RuntimeExpressionKind.RequestBody, jsonPointer: pointer ?? propSuffix);
            }

            if (core.StartsWith("$response.header.", StringComparison.Ordinal))
            {
                var name = core.Substring("$response.header.".Length);
                EnsureNonEmptyToken(name, expression);
                return new RuntimeExpression(expression, RuntimeExpressionKind.ResponseHeader, name, jsonPointer: pointer);
            }

            if (core == "$response.body" || core.StartsWith("$response.body.", StringComparison.Ordinal))
            {
                var propSuffix = core.Length > "$response.body".Length ? "/" + core.Substring("$response.body.".Length).Replace('.', '/') : null;
                return new RuntimeExpression(expression, RuntimeExpressionKind.ResponseBody, jsonPointer: pointer ?? propSuffix);
            }

            if (core.StartsWith("$message.header.", StringComparison.Ordinal))
            {
                var name = core.Substring("$message.header.".Length);
                EnsureNonEmptyToken(name, expression);
                return new RuntimeExpression(expression, RuntimeExpressionKind.MessageHeader, name, jsonPointer: pointer);
            }

            if (core == "$message.payload" || core.StartsWith("$message.payload.", StringComparison.Ordinal))
            {
                var propSuffix = core.Length > "$message.payload".Length ? "/" + core.Substring("$message.payload.".Length).Replace('.', '/') : null;
                return new RuntimeExpression(expression, RuntimeExpressionKind.MessagePayload, jsonPointer: pointer ?? propSuffix);
            }

            if (core.StartsWith("$inputs.", StringComparison.Ordinal))
            {
                var name = core.Substring("$inputs.".Length);
                EnsureNonEmptyToken(name, expression);
                return new RuntimeExpression(expression, RuntimeExpressionKind.Inputs, name, jsonPointer: pointer);
            }

            if (core.StartsWith("$outputs.", StringComparison.Ordinal))
            {
                var name = core.Substring("$outputs.".Length);
                EnsureNonEmptyToken(name, expression);
                return new RuntimeExpression(expression, RuntimeExpressionKind.Outputs, name, jsonPointer: pointer);
            }

            if (core.StartsWith("$steps.", StringComparison.Ordinal))
            {
                var rest = core.Substring("$steps.".Length);
                var parts = rest.Split(new[] { '.' }, 3);
                if (parts.Length < 3 || parts[1] != "outputs" || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[2]))
                {
                    throw new CddException(
                        CddErrorCode.InvalidSpecification,
                        $"Invalid $steps runtime expression '{expression}'. Expected '$steps.<stepId>.outputs.<name>'.");
                }

                return new RuntimeExpression(expression, RuntimeExpressionKind.Steps, parts[0], parts[2], pointer);
            }

            if (core.StartsWith("$sourceDescriptions.", StringComparison.Ordinal))
            {
                var rest = core.Substring("$sourceDescriptions.".Length);
                var parts = rest.Split(new[] { '.' }, 2);
                if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                {
                    throw new CddException(
                        CddErrorCode.InvalidSpecification,
                        $"Invalid $sourceDescriptions runtime expression '{expression}'. Expected '$sourceDescriptions.<name>.<property>'.");
                }

                return new RuntimeExpression(expression, RuntimeExpressionKind.SourceDescriptions, parts[0], parts[1], pointer);
            }

            if (core.StartsWith("$components.", StringComparison.Ordinal))
            {
                var rest = core.Substring("$components.".Length);
                var parts = rest.Split(new[] { '.' }, 2);
                if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                {
                    throw new CddException(
                        CddErrorCode.InvalidSpecification,
                        $"Invalid $components runtime expression '{expression}'. Expected '$components.<section>.<name>'.");
                }

                return new RuntimeExpression(expression, RuntimeExpressionKind.Components, parts[0], parts[1], pointer);
            }

            throw new CddException(
                CddErrorCode.InvalidSpecification,
                $"Unrecognized Arazzo 1.1.0 runtime expression '{expression}'.");
        }

        /// <summary>
        /// Evaluates this <see cref="RuntimeExpression"/> against the provided <see cref="ArazzoExecutionContext"/>.
        /// </summary>
        /// <param name="context">The active workflow execution context.</param>
        /// <returns>The resolved string or JSON value, or <c>null</c> if not found.</returns>
        public string? Evaluate(ArazzoExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string? baseVal = Kind switch
            {
                RuntimeExpressionKind.Url => context.Url,
                RuntimeExpressionKind.Method => context.Method,
                RuntimeExpressionKind.StatusCode => context.StatusCode.ToString(System.Globalization.CultureInfo.InvariantCulture),
                RuntimeExpressionKind.Self => context.Self,
                RuntimeExpressionKind.RequestHeader => TryGetCaseInsensitive(context.RequestHeaders, Name!),
                RuntimeExpressionKind.RequestQuery => TryGetCaseInsensitive(context.RequestQuery, Name!),
                RuntimeExpressionKind.RequestPath => TryGetCaseInsensitive(context.RequestPath, Name!),
                RuntimeExpressionKind.RequestBody => EvaluateBodyWithPointer(context.RequestBody, JsonPointer),
                RuntimeExpressionKind.ResponseHeader => TryGetCaseInsensitive(context.ResponseHeaders, Name!),
                RuntimeExpressionKind.ResponseBody => EvaluateBodyWithPointer(context.ResponseBody, JsonPointer),
                RuntimeExpressionKind.MessageHeader => TryGetCaseInsensitive(context.MessageHeaders, Name!),
                RuntimeExpressionKind.MessagePayload => EvaluateBodyWithPointer(context.MessagePayload, JsonPointer),
                RuntimeExpressionKind.Inputs => EvaluateDictWithPointer(context.Inputs, Name!, JsonPointer),
                RuntimeExpressionKind.Outputs => EvaluateDictWithPointer(context.Outputs, Name!, JsonPointer),
                RuntimeExpressionKind.Steps => EvaluateStepOutput(context, Name!, SubName!, JsonPointer),
                RuntimeExpressionKind.SourceDescriptions => EvaluateSourceDescription(context, Name!, SubName!),
                _ => EvaluateComponent(context, Name!, SubName!)
            };

            return baseVal;
        }

        /// <summary>
        /// Evaluates a string that may either be a direct runtime expression (<c>$...</c>) or contain embedded <c>{$...}</c> template expressions.
        /// </summary>
        /// <param name="template">The string template or expression to evaluate.</param>
        /// <param name="context">The active workflow execution context.</param>
        /// <returns>The interpolated string result.</returns>
        public static string EvaluateTemplate(string template, ArazzoExecutionContext context)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            if (template.StartsWith("$", StringComparison.Ordinal) && !template.Contains("{", StringComparison.Ordinal))
            {
                return Parse(template).Evaluate(context)!;
            }

            var sb = new StringBuilder();
            int i = 0;
            while (i < template.Length)
            {
                int open = template.IndexOf("{$", i, StringComparison.Ordinal);
                if (open < 0)
                {
                    sb.Append(template.Substring(i));
                    break;
                }

                sb.Append(template.Substring(i, open - i));
                int close = template.IndexOf('}', open + 2);
                if (close < 0)
                {
                    sb.Append(template.Substring(open));
                    break;
                }

                var exprStr = template.Substring(open + 1, close - open - 1);
                var resolved = Parse(exprStr).Evaluate(context)!;
                sb.Append(resolved);
                i = close + 1;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Compiles a runtime expression or literal into a C# source expression string suitable for generated workflow code.
        /// </summary>
        /// <param name="expressionOrLiteral">The runtime expression or literal string.</param>
        /// <returns>A valid C# expression string.</returns>
        public static string ToCSharpExpression(string expressionOrLiteral)
        {
            var dq = ((char)34).ToString();
            if (string.IsNullOrEmpty(expressionOrLiteral))
            {
                return dq + dq;
            }

            if (expressionOrLiteral.StartsWith("$", StringComparison.Ordinal) && !expressionOrLiteral.Contains("{", StringComparison.Ordinal))
            {
                var parsed = Parse(expressionOrLiteral);
                return parsed.Kind switch
                {
                    RuntimeExpressionKind.Inputs => $"input.{ToPascalIdentifier(parsed.Name!)}",
                    RuntimeExpressionKind.Steps => $"stepOutputs[{dq}{parsed.Name}{dq}][{dq}{parsed.SubName}{dq}]",
                    RuntimeExpressionKind.StatusCode => "lastStatusCode.ToString()",
                    RuntimeExpressionKind.Url => "lastUrl",
                    RuntimeExpressionKind.Method => "lastMethod",
                    RuntimeExpressionKind.Self => "SelfUri",
                    RuntimeExpressionKind.ResponseBody => string.IsNullOrEmpty(parsed.JsonPointer)
                        ? "lastResponseBody"
                        : $"ExtractJsonPointer(lastResponseBody, {dq}{parsed.JsonPointer}{dq})",
                    _ => $"EvaluateRuntimeExpression({dq}{EscapeCSharpString(expressionOrLiteral)}{dq})"
                };
            }

            return dq + EscapeCSharpString(expressionOrLiteral) + dq;
        }

        /// <summary>
        /// Ensures a parsed identifier token is non-empty.
        /// </summary>
        /// <param name="token">The identifier token.</param>
        /// <param name="fullExpression">The full expression for error messages.</param>
        private static void EnsureNonEmptyToken(string token, string fullExpression)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new CddException(
                    CddErrorCode.InvalidSpecification,
                    $"Missing identifier token in runtime expression '{fullExpression}'.");
            }
        }

        /// <summary>
        /// Looks up a dictionary value using case-insensitive key matching.
        /// </summary>
        /// <param name="dict">The dictionary to search.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The value if found; otherwise <c>null</c>.</returns>
        private static string? TryGetCaseInsensitive(IDictionary<string, string> dict, string key)
        {
            foreach (var kvp in dict)
            {
                if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Evaluates a JSON body string with an optional RFC 6901 JSON Pointer.
        /// </summary>
        /// <param name="body">The raw JSON or text body.</param>
        /// <param name="pointer">The optional JSON Pointer.</param>
        /// <returns>The resolved string value.</returns>
        private static string? EvaluateBodyWithPointer(string? body, string? pointer)
        {
            if (string.IsNullOrEmpty(pointer)) return body;
            var node = CriterionEvaluator.TryParseJsonNode(body);
            var target = CriterionEvaluator.EvaluateJsonPointer(node, pointer);
            return CriterionEvaluator.JsonNodeToScalarString(target);
        }

        /// <summary>
        /// Evaluates a dictionary entry with an optional RFC 6901 JSON Pointer or dotted sub-path.
        /// </summary>
        /// <param name="dict">The dictionary to query.</param>
        /// <param name="name">The key name (potentially containing dotted path segments).</param>
        /// <param name="pointer">The optional JSON Pointer suffix.</param>
        /// <returns>The resolved value or <c>null</c>.</returns>
        private static string? EvaluateDictWithPointer(IDictionary<string, string> dict, string name, string? pointer)
        {
            string lookupKey = name;
            string? derivedPointer = pointer;
            if (name.Contains(".", StringComparison.Ordinal))
            {
                int dotIdx = name.IndexOf('.');
                lookupKey = name.Substring(0, dotIdx);
                derivedPointer = "/" + name.Substring(dotIdx + 1).Replace('.', '/');
            }

            if (!dict.TryGetValue(lookupKey, out var raw))
            {
                return null;
            }

            return EvaluateBodyWithPointer(raw, derivedPointer);
        }

        /// <summary>
        /// Evaluates a <c>$steps.&lt;stepId&gt;.outputs.&lt;name&gt;</c> reference against <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="stepId">The target step identifier.</param>
        /// <param name="outputName">The output variable name.</param>
        /// <param name="pointer">The optional JSON Pointer suffix.</param>
        /// <returns>The resolved step output value.</returns>
        private static string? EvaluateStepOutput(ArazzoExecutionContext context, string stepId, string outputName, string? pointer)
        {
            if (!context.StepOutputs.TryGetValue(stepId, out var stepDict))
            {
                return null;
            }

            return EvaluateDictWithPointer(stepDict, outputName, pointer);
        }

        /// <summary>
        /// Evaluates a <c>$sourceDescriptions.&lt;name&gt;.&lt;prop&gt;</c> reference against <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="sourceName">The source description name.</param>
        /// <param name="prop">The property or operation name.</param>
        /// <returns>The resolved source description URL or operation identifier.</returns>
        private static string? EvaluateSourceDescription(ArazzoExecutionContext context, string sourceName, string prop)
        {
            if (context.SourceDescriptions.TryGetValue(sourceName, out var url))
            {
                if (string.Equals(prop, "url", StringComparison.OrdinalIgnoreCase))
                {
                    return url;
                }

                return $"{url}#{prop}";
            }

            return null;
        }

        /// <summary>
        /// Evaluates a <c>$components.&lt;section&gt;.&lt;name&gt;</c> reference against <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="section">The component section (<c>parameters</c>, <c>inputs</c>, <c>successActions</c>, <c>failureActions</c>).</param>
        /// <param name="name">The component key name.</param>
        /// <returns>The resolved component value string.</returns>
        private static string? EvaluateComponent(ArazzoExecutionContext context, string section, string name)
        {
            if (context.Components == null)
            {
                return null;
            }

            if (section == "parameters" && context.Components.Parameters != null &&
                context.Components.Parameters.TryGetValue(name, out var param))
            {
                return param.Value!.Value;
            }

            if (section == "inputs" && context.Components.Inputs != null &&
                context.Components.Inputs.TryGetValue(name, out var schema))
            {
                return schema.Default!.Value;
            }

            if (section == "successActions" && context.Components.SuccessActions != null &&
                context.Components.SuccessActions.TryGetValue(name, out var sa))
            {
                return sa.Type;
            }

            if (section == "failureActions" && context.Components.FailureActions != null &&
                context.Components.FailureActions.TryGetValue(name, out var fa))
            {
                return fa.Type;
            }

            return null;
        }

        /// <summary>
        /// Converts a kebab/snake/camel/spaced identifier into a valid C# PascalCase identifier.
        /// </summary>
        /// <param name="raw">The raw identifier.</param>
        /// <returns>The PascalCase identifier.</returns>
        internal static string ToPascalIdentifier(string raw)
        {
            var sb = new StringBuilder();
            bool upperNext = true;
            foreach (char c in raw)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(upperNext ? char.ToUpperInvariant(c) : c);
                    upperNext = false;
                }
                else
                {
                    upperNext = true;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Escapes double quotes and backslashes for embedding in C# string literals.
        /// </summary>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        private static string EscapeCSharpString(string str)
        {
            var bs = ((char)92).ToString();
            var dq = ((char)34).ToString();
            return str.Replace(bs, bs + bs).Replace(dq, bs + dq);
        }
    }
}
