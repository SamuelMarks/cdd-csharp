// <copyright file="PayloadReplacer.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Errors;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Models.Arazzo;

namespace Cdd.OpenApi.Arazzo.Expressions
{
    /// <summary>
    /// Applies Arazzo 1.1.0 <see cref="ArazzoPayloadReplacement"/> rules and runtime expression interpolations to request payloads.
    /// </summary>
    public static class PayloadReplacer
    {
        /// <summary>
        /// Applies dynamic expression interpolation and targeted <see cref="ArazzoPayloadReplacement"/> rules to a request body payload.
        /// </summary>
        /// <param name="payload">The base request body payload.</param>
        /// <param name="replacements">The optional list of payload replacement rules.</param>
        /// <param name="context">The active workflow execution context.</param>
        /// <returns>The serialized request body payload with all replacements applied.</returns>
        public static string ApplyReplacements(OpenApiAny? payload, IList<ArazzoPayloadReplacement>? replacements, ArazzoExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (payload == null)
            {
                return string.Empty;
            }

            string rawPayload = InterpolateRawPayload(payload.Value, context);
            if (replacements == null || replacements.Count == 0)
            {
                return rawPayload;
            }

            foreach (var rep in replacements)
            {
                var resolvedVal = RuntimeExpression.EvaluateTemplate(rep.Value!.Value, context);
                var dialect = rep.TargetSelectorType != null
                    ? rep.TargetSelectorType.Type.ToLowerInvariant()
                    : (rep.Target.StartsWith("$", StringComparison.Ordinal) ? "jsonpath" : "jsonpointer");

                if (dialect == "xpath")
                {
                    rawPayload = ApplyXPathReplacement(rawPayload, rep.Target, resolvedVal);
                }
                else
                {
                    var rootNode = CriterionEvaluator.TryParseJsonNode(rawPayload)!;
                    var targetPointer = dialect == "jsonpath"
                        ? ConvertSimpleJsonPathToPointer(rep.Target)
                        : rep.Target;
                    ApplyJsonPointerReplacement(rootNode, targetPointer, resolvedVal);
                    rawPayload = rootNode.ToJsonString();
                }
            }

            return rawPayload;
        }

        /// <summary>
        /// Interpolates runtime expressions within a raw payload string.
        /// </summary>
        /// <param name="raw">The raw payload string.</param>
        /// <param name="context">The active workflow execution context.</param>
        /// <returns>The interpolated payload string.</returns>
        private static string InterpolateRawPayload(string raw, ArazzoExecutionContext context)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return string.Empty;
            }

            var node = CriterionEvaluator.TryParseJsonNode(raw);
            if (node != null)
            {
                InterpolateJsonNodeInPlace(node, context);
                return node.ToJsonString();
            }

            return RuntimeExpression.EvaluateTemplate(raw, context);
        }

        /// <summary>
        /// Recursively interpolates runtime expressions in string values of a <see cref="JsonNode"/> hierarchy.
        /// </summary>
        /// <param name="node">The JSON node to walk.</param>
        /// <param name="context">The active workflow execution context.</param>
        private static void InterpolateJsonNodeInPlace(JsonNode? node, ArazzoExecutionContext context)
        {
            if (node is JsonObject obj)
            {
                var keys = new List<string>();
                foreach (var kv in obj) keys.Add(kv.Key);
                foreach (var k in keys)
                {
                    var child = obj[k];
                    if (child is JsonValue jv && jv.TryGetValue<string>(out var s) && s.StartsWith("$", StringComparison.Ordinal))
                    {
                        obj[k] = CreateTypedJsonValue(RuntimeExpression.EvaluateTemplate(s, context));
                    }
                    else
                    {
                        InterpolateJsonNodeInPlace(child, context);
                    }
                }
            }
            else if (node is JsonArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    var item = arr[i];
                    var s = item!.GetValue<string>();
                    arr[i] = CreateTypedJsonValue(RuntimeExpression.EvaluateTemplate(s, context));
                }
            }
        }

        /// <summary>
        /// Applies an in-place replacement at an RFC 6901 JSON Pointer location on <paramref name="root"/>.
        /// </summary>
        /// <param name="root">The mutable root JSON node.</param>
        /// <param name="pointer">The JSON Pointer target (e.g., <c>/petId</c> or <c>/items/0/name</c>).</param>
        /// <param name="newValue">The replacement value string.</param>
        private static void ApplyJsonPointerReplacement(JsonNode root, string pointer, string newValue)
        {
            var normalized = pointer.TrimStart('#').TrimStart('/');
            var segments = normalized.Split('/');
            JsonNode? current = root;
            for (int i = 0; i < segments.Length - 1; i++)
            {
                var seg = segments[i].Replace("~1", "/").Replace("~0", "~");
                current = current![seg];
            }

            var lastSeg = segments[segments.Length - 1].Replace("~1", "/").Replace("~0", "~");
            if (current is JsonObject targetObj)
            {
                targetObj[lastSeg] = CreateTypedJsonValue(newValue);
            }
            else if (current is JsonArray targetArr)
            {
                int lastIdx = int.Parse(lastSeg, CultureInfo.InvariantCulture);
                targetArr[lastIdx] = CreateTypedJsonValue(newValue);
            }
        }

        /// <summary>
        /// Converts a simple JSONPath expression (such as <c>$.user.id</c> or <c>$.items[0].name</c>) into a JSON Pointer string.
        /// </summary>
        /// <param name="jsonPath">The JSONPath target string.</param>
        /// <returns>The equivalent JSON Pointer path.</returns>
        private static string ConvertSimpleJsonPathToPointer(string jsonPath)
        {
            var p = jsonPath.Trim().TrimStart('$').TrimStart('.');
            p = p.Replace("[", "/").Replace("]", string.Empty).Replace("'", string.Empty).Replace(StringEscaper.DoubleQuote, string.Empty).Replace(".", "/");
            return "/" + p.TrimStart('/');
        }

        /// <summary>
        /// Applies an XPath replacement to an XML payload string.
        /// </summary>
        /// <param name="xmlContent">The XML payload string.</param>
        /// <param name="xpath">The target XPath expression.</param>
        /// <param name="newValue">The replacement value.</param>
        /// <returns>The updated XML payload string.</returns>
        private static string ApplyXPathReplacement(string xmlContent, string xpath, string newValue)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xmlContent);
                var node = doc.SelectSingleNode(xpath)!;
                node.InnerText = newValue;
                using var sw = new StringWriter();
                doc.Save(sw);
                return sw.ToString();
            }
            catch (XmlException)
            {
                return xmlContent;
            }
        }

        /// <summary>
        /// Creates a typed <see cref="JsonNode"/> (boolean, integer, number, JSON object/array, or string) from a resolved string value.
        /// </summary>
        /// <param name="val">The resolved string value.</param>
        /// <returns>A <see cref="JsonNode"/> representing the value.</returns>
        private static JsonNode? CreateTypedJsonValue(string val)
        {
            if (val == "true") return JsonValue.Create(true);
            if (val == "false") return JsonValue.Create(false);
            if (long.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l)) return JsonValue.Create(l);
            if (val.StartsWith("[", StringComparison.Ordinal) || val.StartsWith("{", StringComparison.Ordinal))
            {
                return CriterionEvaluator.TryParseJsonNode(val);
            }

            return JsonValue.Create(val);
        }
    }
}
