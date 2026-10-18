// <copyright file="CriterionEvaluator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using Cdd.OpenApi.Models.Arazzo;

namespace Cdd.OpenApi.Arazzo.Expressions
{
    /// <summary>
    /// Holds the runtime state for executing an Arazzo 1.1.0 workflow and evaluating runtime expressions, selectors, and criteria.
    /// </summary>
    public sealed class ArazzoExecutionContext
    {
        /// <summary>Gets or sets the current step's resolved request URL (<c>$url</c>).</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>Gets or sets the current step's HTTP method (<c>$method</c>).</summary>
        public string Method { get; set; } = "GET";

        /// <summary>Gets or sets the current step's HTTP status code (<c>$statusCode</c>).</summary>
        public int StatusCode { get; set; } = 200;

        /// <summary>Gets or sets the canonical document URI (<c>$self</c>).</summary>
        public string? Self { get; set; }

        /// <summary>Gets the outgoing request headers (<c>$request.header.*</c>).</summary>
        public IDictionary<string, string> RequestHeaders { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Gets the outgoing query parameters (<c>$request.query.*</c>).</summary>
        public IDictionary<string, string> RequestQuery { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Gets the outgoing path parameters (<c>$request.path.*</c>).</summary>
        public IDictionary<string, string> RequestPath { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Gets or sets the outgoing request body (<c>$request.body</c>).</summary>
        public string? RequestBody { get; set; }

        /// <summary>Gets the incoming response headers (<c>$response.header.*</c>).</summary>
        public IDictionary<string, string> ResponseHeaders { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Gets or sets the incoming response body (<c>$response.body</c>).</summary>
        public string? ResponseBody { get; set; }

        /// <summary>Gets the AsyncAPI message headers (<c>$message.header.*</c>).</summary>
        public IDictionary<string, string> MessageHeaders { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Gets or sets the AsyncAPI message payload (<c>$message.payload</c>).</summary>
        public string? MessagePayload { get; set; }

        /// <summary>Gets the workflow input parameters (<c>$inputs.*</c>).</summary>
        public IDictionary<string, string> Inputs { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>Gets the workflow output values (<c>$outputs.*</c>).</summary>
        public IDictionary<string, string> Outputs { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>Gets the captured outputs for each executed step (<c>$steps.&lt;stepId&gt;.outputs.&lt;name&gt;</c>).</summary>
        public IDictionary<string, IDictionary<string, string>> StepOutputs { get; } = new Dictionary<string, IDictionary<string, string>>(StringComparer.Ordinal);

        /// <summary>Gets the map of source description names to base URLs (<c>$sourceDescriptions.&lt;name&gt;.url</c>).</summary>
        public IDictionary<string, string> SourceDescriptions { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>Gets or sets the reusable components registry (<c>$components.*</c>).</summary>
        public ArazzoComponents? Components { get; set; }
    }

    /// <summary>
    /// Evaluates Arazzo 1.1.0 <see cref="ArazzoCriterion"/> conditions and <see cref="ArazzoSelector"/> / <see cref="ArazzoStepOutput"/> queries across Simple, Regex, JSON Pointer (RFC 6901), JSONPath (RFC 9535), and XPath dialects.
    /// </summary>
    public static class CriterionEvaluator
    {
        /// <summary>
        /// Evaluates an <see cref="ArazzoCriterion"/> against the given <see cref="ArazzoExecutionContext"/>.
        /// </summary>
        /// <param name="criterion">The criterion assertion to evaluate.</param>
        /// <param name="context">The active workflow execution context.</param>
        /// <returns><c>true</c> if the criterion passes; otherwise <c>false</c>.</returns>
        public static bool EvaluateCriterion(ArazzoCriterion criterion, ArazzoExecutionContext context)
        {
            if (criterion == null)
            {
                throw new ArgumentNullException(nameof(criterion));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string dialect = (criterion.Type != null ? criterion.Type.Type : "simple").ToLowerInvariant();
            string contextVal = !string.IsNullOrWhiteSpace(criterion.Context)
                ? RuntimeExpression.EvaluateTemplate(criterion.Context!, context)
                : context.ResponseBody!;

            switch (dialect)
            {
                case "regex":
                    return Regex.IsMatch(contextVal, criterion.Condition);

                case "jsonpath":
                    return EvaluateJsonPathCondition(contextVal, criterion.Condition, context);

                case "xpath":
                    return EvaluateXPathCondition(contextVal, criterion.Condition);

                case "jsonpointer":
                    var node = TryParseJsonNode(contextVal);
                    return EvaluateJsonPointer(node, criterion.Condition) != null;

                default:
                    return EvaluateSimpleCondition(criterion.Condition, contextVal, context);
            }
        }

        /// <summary>
        /// Evaluates an <see cref="ArazzoStepOutput"/> (either a string runtime expression or a structured <see cref="ArazzoSelector"/>) against <paramref name="context"/>.
        /// </summary>
        /// <param name="stepOutput">The step output definition.</param>
        /// <param name="context">The active workflow execution context.</param>
        /// <returns>The extracted value string, or <c>null</c> if not found.</returns>
        public static string? EvaluateSelector(ArazzoStepOutput stepOutput, ArazzoExecutionContext context)
        {
            if (stepOutput == null)
            {
                throw new ArgumentNullException(nameof(stepOutput));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (stepOutput.Expression != null)
            {
                return RuntimeExpression.EvaluateTemplate(stepOutput.Expression, context);
            }

            var sel = stepOutput.Selector!;
            var rawContext = !string.IsNullOrWhiteSpace(sel.Context)
                ? RuntimeExpression.EvaluateTemplate(sel.Context!, context)
                : context.ResponseBody!;

            var selectorQuery = sel.Selector!;
            var dialect = (sel.Type != null ? sel.Type.Type : "jsonpath").ToLowerInvariant();

            switch (dialect)
            {
                case "jsonpointer":
                    return JsonNodeToScalarString(EvaluateJsonPointer(TryParseJsonNode(rawContext), selectorQuery));

                case "xpath":
                    return EvaluateXPath(rawContext, selectorQuery);

                default:
                    var matches = EvaluateJsonPath(TryParseJsonNode(rawContext), selectorQuery);
                    return JsonNodeToScalarString(matches[0]);
            }
        }

        /// <summary>
        /// Evaluates an RFC 6901 JSON Pointer against a <see cref="JsonNode"/> root.
        /// </summary>
        /// <param name="root">The root JSON node.</param>
        /// <param name="pointer">The JSON Pointer (e.g., <c>/items/0/id</c> or <c>#/items/0/id</c>).</param>
        /// <returns>The resolved <see cref="JsonNode"/>, or <c>null</c> if the path does not exist.</returns>
        public static JsonNode? EvaluateJsonPointer(JsonNode? root, string? pointer)
        {
            var normalized = pointer!.TrimStart('#').TrimStart('/');
            var segments = normalized.Split('/');
            JsonNode? current = root;
            foreach (var rawSeg in segments)
            {
                var seg = rawSeg.Replace("~1", "/").Replace("~0", "~");
                if (current is JsonObject obj && obj.TryGetPropertyValue(seg, out var nextObjNode))
                {
                    current = nextObjNode;
                }
                else if (current is JsonArray arr && int.TryParse(seg, NumberStyles.Integer, CultureInfo.InvariantCulture, out int idx) && idx < arr.Count)
                {
                    current = arr[idx];
                }
                else
                {
                    return null;
                }
            }

            return current;
        }

        /// <summary>
        /// Evaluates an RFC 9535 / Goessner JSONPath expression against a <see cref="JsonNode"/> root.
        /// </summary>
        /// <param name="root">The root JSON node.</param>
        /// <param name="jsonPath">The JSONPath query string (e.g., <c>$.store.book[0].title</c> or <c>$[?(@.id == 1)]</c>).</param>
        /// <returns>The list of matching <see cref="JsonNode"/> elements.</returns>
        public static IList<JsonNode?> EvaluateJsonPath(JsonNode? root, string jsonPath)
        {
            string path = jsonPath.Trim().TrimStart('$');
            var currentNodes = new List<JsonNode?> { root };
            int i = 0;
            while (i < path.Length)
            {
                if (path[i] == '.')
                {
                    if (path[i + 1] == '.')
                    {
                        i += 2;
                        int end = FindNextJsonPathDelimiter(path, i);
                        var propName = path.Substring(i, end - i);
                        i = end;
                        var nextNodes = new List<JsonNode?>();
                        foreach (var n in currentNodes)
                        {
                            CollectRecursiveMatches(n, propName, nextNodes);
                        }

                        currentNodes = nextNodes;
                    }
                    else
                    {
                        i++;
                        int end = FindNextJsonPathDelimiter(path, i);
                        var propName = path.Substring(i, end - i);
                        i = end;
                        currentNodes = StepProperty(currentNodes, propName);
                    }
                }
                else
                {
                    int close = path.IndexOf(']', i);
                    var inside = path.Substring(i + 1, close - i - 1).Trim();
                    i = close + 1;
                    currentNodes = StepBracket(currentNodes, inside);
                }
            }

            return currentNodes;
        }

        /// <summary>
        /// Evaluates an XPath expression against an XML document string.
        /// </summary>
        /// <param name="xmlContent">The XML payload string.</param>
        /// <param name="xpath">The XPath query expression.</param>
        /// <returns>The evaluated string result, or <c>null</c> if evaluation fails or yields no match.</returns>
        public static string? EvaluateXPath(string xmlContent, string xpath)
        {
            if (string.IsNullOrWhiteSpace(xmlContent))
            {
                return null;
            }

            try
            {
                using var sr = new StringReader(xmlContent);
                var doc = new XPathDocument(sr);
                var nav = doc.CreateNavigator();
                var evalResult = nav.Evaluate(xpath);
                if (evalResult is XPathNodeIterator iter)
                {
                    iter.MoveNext();
                    return iter.Current!.Value;
                }

                return Convert.ToString(evalResult, CultureInfo.InvariantCulture);
            }
            catch (XmlException)
            {
                return null;
            }
            catch (XPathException)
            {
                return null;
            }
        }

        /// <summary>
        /// Safely parses a JSON string into a <see cref="JsonNode"/>, returning <c>null</c> if the input is not valid JSON.
        /// </summary>
        /// <param name="json">The raw JSON string.</param>
        /// <returns>The parsed <see cref="JsonNode"/> or <c>null</c>.</returns>
        internal static JsonNode? TryParseJsonNode(string? json)
        {
            try
            {
                return JsonNode.Parse(json!);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a <see cref="JsonNode"/> into a scalar string or raw JSON representation.
        /// </summary>
        /// <param name="node">The JSON node to format.</param>
        /// <returns>The string value or <c>null</c>.</returns>
        internal static string? JsonNodeToScalarString(JsonNode? node)
        {
            if (node == null) return null;
            var raw = node.ToJsonString();
            if (raw.StartsWith(@"""", StringComparison.Ordinal))
            {
                return node.GetValue<string>();
            }

            return raw;
        }

        /// <summary>
        /// Evaluates a JSONPath condition expression (supporting filter queries and comparison trails like <c>$.count == 1</c>).
        /// </summary>
        /// <param name="jsonText">The JSON context payload.</param>
        /// <param name="condition">The JSONPath condition string.</param>
        /// <param name="context">The execution context.</param>
        /// <returns><c>true</c> if the JSONPath condition holds; otherwise <c>false</c>.</returns>
        private static bool EvaluateJsonPathCondition(string jsonText, string condition, ArazzoExecutionContext context)
        {
            var root = TryParseJsonNode(jsonText);
            if (root == null) return false;

            if (!condition.Contains("[?(", StringComparison.Ordinal))
            {
                foreach (var op in new[] { "==", "!=", ">=", "<=", ">", "<" })
                {
                    if (condition.Contains(op, StringComparison.Ordinal))
                    {
                        var parts = condition.Split(new[] { op }, 2, StringSplitOptions.None);
                        var leftMatches = EvaluateJsonPath(root, parts[0].Trim());
                        var leftVal = JsonNodeToScalarString(leftMatches[0]);
                        var rightVal = ResolveOperand(parts[1].Trim(), null, context);
                        return CompareOperands(leftVal, rightVal, op);
                    }
                }
            }

            var matches = EvaluateJsonPath(root, condition);
            return matches.Count > 0;
        }

        /// <summary>
        /// Evaluates an XPath condition against an XML string.
        /// </summary>
        /// <param name="xmlText">The XML payload string.</param>
        /// <param name="condition">The XPath condition expression.</param>
        /// <returns><c>true</c> if the XPath expression matches at least one node or evaluates to true; otherwise <c>false</c>.</returns>
        private static bool EvaluateXPathCondition(string xmlText, string condition)
        {
            var val = EvaluateXPath(xmlText, condition);
            return !string.IsNullOrEmpty(val);
        }

        /// <summary>
        /// Evaluates a <c>simple</c> Arazzo criterion condition supporting logical <c>&amp;&amp;</c>, <c>||</c>, and relational operators (<c>==</c>, <c>!=</c>, <c>&gt;=</c>, <c>&lt;=</c>, <c>&gt;</c>, <c>&lt;</c>).
        /// </summary>
        /// <param name="condition">The simple condition string.</param>
        /// <param name="contextVal">The pre-resolved criterion context value, if any.</param>
        /// <param name="context">The active workflow execution context.</param>
        /// <returns><c>true</c> if the condition evaluates to true; otherwise <c>false</c>.</returns>
        private static bool EvaluateSimpleCondition(string condition, string? contextVal, ArazzoExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(condition))
            {
                return true;
            }

            if (condition.Contains("||", StringComparison.Ordinal))
            {
                var orParts = condition.Split(new[] { "||" }, StringSplitOptions.None);
                return orParts.Any(p => EvaluateSimpleCondition(p.Trim(), contextVal, context));
            }

            if (condition.Contains("&&", StringComparison.Ordinal))
            {
                var andParts = condition.Split(new[] { "&&" }, StringSplitOptions.None);
                return andParts.All(p => EvaluateSimpleCondition(p.Trim(), contextVal, context));
            }

            foreach (var op in new[] { "==", "!=", ">=", "<=", ">", "<" })
            {
                int idx = condition.IndexOf(op, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    var leftRaw = condition.Substring(0, idx).Trim();
                    var rightRaw = condition.Substring(idx + op.Length).Trim();
                    var leftVal = ResolveOperand(leftRaw, contextVal, context);
                    var rightVal = ResolveOperand(rightRaw, contextVal, context);
                    return CompareOperands(leftVal, rightVal, op);
                }
            }

            var single = ResolveOperand(condition.Trim(), contextVal, context);
            return !string.IsNullOrEmpty(single);
        }

        /// <summary>
        /// Resolves a single operand in a simple condition (handling <c>$...</c> runtime expressions, <c>$.prop</c> context paths, string literals, and primitives).
        /// </summary>
        /// <param name="raw">The raw operand text.</param>
        /// <param name="contextVal">The resolved context value if present.</param>
        /// <param name="context">The active workflow execution context.</param>
        /// <returns>The resolved operand string.</returns>
        private static string? ResolveOperand(string raw, string? contextVal, ArazzoExecutionContext context)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return contextVal;
            }

            if (raw.StartsWith("'", StringComparison.Ordinal))
            {
                return raw.Substring(1, raw.Length - 2);
            }

            if (raw.StartsWith("$.", StringComparison.Ordinal))
            {
                var node = TryParseJsonNode(contextVal);
                var matches = EvaluateJsonPath(node, raw);
                return JsonNodeToScalarString(matches[0]);
            }

            if (raw.StartsWith("$", StringComparison.Ordinal))
            {
                return RuntimeExpression.Parse(raw).Evaluate(context);
            }

            if (string.Equals(raw, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return raw;
        }

        /// <summary>
        /// Compares two resolved operand strings using numeric or ordinal comparison for the specified relational operator.
        /// </summary>
        /// <param name="left">The left operand string.</param>
        /// <param name="right">The right operand string.</param>
        /// <param name="op">The relational operator (<c>==</c>, <c>!=</c>, <c>&gt;=</c>, <c>&lt;=</c>, <c>&gt;</c>, <c>&lt;</c>).</param>
        /// <returns><c>true</c> if the comparison holds; otherwise <c>false</c>.</returns>
        private static bool CompareOperands(string? left, string? right, string op)
        {
            if (double.TryParse(left, NumberStyles.Float, CultureInfo.InvariantCulture, out double ld) &&
                double.TryParse(right, NumberStyles.Float, CultureInfo.InvariantCulture, out double rd))
            {
                return op switch
                {
                    "==" => Math.Abs(ld - rd) < 1e-9,
                    "!=" => Math.Abs(ld - rd) >= 1e-9,
                    ">=" => ld >= rd,
                    "<=" => ld <= rd,
                    ">" => ld > rd,
                    _ => ld < rd
                };
            }

            int cmp = string.Compare(left, right, StringComparison.Ordinal);
            return op switch
            {
                "==" => string.Equals(left, right, StringComparison.Ordinal),
                "!=" => !string.Equals(left, right, StringComparison.Ordinal),
                ">=" => cmp >= 0,
                "<=" => cmp <= 0,
                ">" => cmp > 0,
                _ => cmp < 0
            };
        }

        /// <summary>
        /// Finds the next delimiter (<c>.</c> or <c>[</c>) in a JSONPath string starting from <paramref name="start"/>.
        /// </summary>
        /// <param name="path">The JSONPath string.</param>
        /// <param name="start">The start index.</param>
        /// <returns>The index of the next delimiter or the end of the string.</returns>
        private static int FindNextJsonPathDelimiter(string path, int start)
        {
            for (int i = start; i < path.Length; i++)
            {
                if (path[i] == '.' || path[i] == '[') return i;
            }

            return path.Length;
        }

        /// <summary>
        /// Steps into a named property across a list of JSON nodes.
        /// </summary>
        /// <param name="nodes">The current candidate nodes.</param>
        /// <param name="propName">The property name.</param>
        /// <returns>The stepped child nodes.</returns>
        private static List<JsonNode?> StepProperty(List<JsonNode?> nodes, string propName)
        {
            var next = new List<JsonNode?>();
            foreach (var obj in nodes.OfType<JsonObject>())
            {
                obj.TryGetPropertyValue(propName, out var child);
                next.Add(child);
            }

            return next;
        }

        /// <summary>
        /// Evaluates a bracket segment (<c>[0]</c>, <c>[*]</c>, <c>['prop']</c>, or filter <c>[?(@.prop == val)]</c>) across candidate JSON nodes.
        /// </summary>
        /// <param name="nodes">The current candidate nodes.</param>
        /// <param name="inside">The trimmed expression inside brackets.</param>
        /// <returns>The filtered or indexed JSON nodes.</returns>
        private static List<JsonNode?> StepBracket(List<JsonNode?> nodes, string inside)
        {
            var next = new List<JsonNode?>();
            if (inside == "*")
            {
                foreach (var n in nodes.OfType<JsonArray>())
                {
                    foreach (var item in n) next.Add(item);
                }

                return next;
            }

            if (inside.StartsWith("'", StringComparison.Ordinal))
            {
                var key = inside.Substring(1, inside.Length - 2);
                return StepProperty(nodes, key);
            }

            var filterExpr = inside.Substring(2, inside.Length - 3).Trim();
            foreach (var arr in nodes.OfType<JsonArray>())
            {
                foreach (var item in arr)
                {
                    if (MatchesFilter(item, filterExpr)) next.Add(item);
                }
            }

            return next;
        }

        /// <summary>
        /// Evaluates a JSONPath filter expression (e.g., <c>@.status == 'available'</c> or <c>@.id != null</c>) against a single JSON item.
        /// </summary>
        /// <param name="item">The candidate JSON item.</param>
        /// <param name="filterExpr">The filter expression inside <c>?()</c>.</param>
        /// <returns><c>true</c> if the item matches the filter; otherwise <c>false</c>.</returns>
        private static bool MatchesFilter(JsonNode? item, string filterExpr)
        {
            var obj = (JsonObject)item!;
            foreach (var op in new[] { "==", "!=", ">=", "<=", ">", "<" })
            {
                int idx = filterExpr.IndexOf(op, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    var left = filterExpr.Substring(0, idx).Trim();
                    var right = filterExpr.Substring(idx + op.Length).Trim();
                    var prop = left.Substring(2);
                    obj.TryGetPropertyValue(prop, out var propNode);
                    var leftStr = JsonNodeToScalarString(propNode);
                    return CompareOperands(leftStr, right, op);
                }
            }

            var checkProp = filterExpr.Substring(2);
            obj.TryGetPropertyValue(checkProp, out var checkNode);
            return JsonNodeToScalarString(checkNode) == "true";
        }

        /// <summary>
        /// Recursively collects descendant properties matching <paramref name="propName"/> for the JSONPath <c>..</c> operator.
        /// </summary>
        /// <param name="node">The current JSON node.</param>
        /// <param name="propName">The target property name.</param>
        /// <param name="results">The output list of matching nodes.</param>
        private static void CollectRecursiveMatches(JsonNode? node, string propName, List<JsonNode?> results)
        {
            if (node is JsonObject obj)
            {
                if (obj.TryGetPropertyValue(propName, out var match))
                {
                    results.Add(match);
                }

                foreach (var kv in obj)
                {
                    CollectRecursiveMatches(kv.Value, propName, results);
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    CollectRecursiveMatches(item, propName, results);
                }
            }
        }
    }
}
