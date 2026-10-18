// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cdd.OpenApi.Models.Arazzo;

namespace Cdd.OpenApi.Arazzo
{
    /// <summary>
    /// Serializes <see cref="ArazzoDocument"/> instances into formatted JSON or YAML strings.
    /// </summary>
    public static class ArazzoEmitter
    {
        /// <summary>
        /// Serializes an <see cref="ArazzoDocument"/> into an indented JSON string.
        /// </summary>
        /// <param name="document">The <see cref="ArazzoDocument"/> to serialize.</param>
        /// <returns>The serialized Arazzo 1.1.0 JSON string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is null.</exception>
        public static string EmitJson(ArazzoDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(document, ArazzoJsonContext.Default.ArazzoDocument);
        }

        /// <summary>
        /// Serializes an <see cref="ArazzoDocument"/> into a canonical YAML string.
        /// </summary>
        /// <param name="document">The <see cref="ArazzoDocument"/> to serialize.</param>
        /// <returns>The serialized Arazzo 1.1.0 YAML string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is null.</exception>
        public static string EmitYaml(ArazzoDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var json = EmitJson(document);
            var rootNode = JsonNode.Parse(json);
            var sb = new StringBuilder();
            WriteYamlNode(rootNode, sb, 0, isArrayItem: false);
            return sb.ToString();
        }

        /// <summary>
        /// Recursively writes a <see cref="JsonNode"/> as indented YAML into <paramref name="sb"/>.
        /// </summary>
        /// <param name="node">The JSON node to format as YAML.</param>
        /// <param name="sb">The destination string builder.</param>
        /// <param name="indent">The current space indentation level.</param>
        /// <param name="isArrayItem">Whether this node is the direct child of a YAML sequence entry.</param>
        private static void WriteYamlNode(JsonNode? node, StringBuilder sb, int indent, bool isArrayItem)
        {
            var pad = new string(' ', indent);
            if (node is JsonObject obj)
            {
                bool first = true;
                foreach (var kvp in obj)
                {
                    var keyStr = NeedsYamlQuotes(kvp.Key) ? "\"" + kvp.Key + "\"" : kvp.Key;
                    var prefix = (isArrayItem && first) ? string.Empty : pad;
                    first = false;

                    if (kvp.Value is JsonObject childObj && childObj.Count > 0)
                    {
                        sb.AppendLine($"{prefix}{keyStr}:");
                        WriteYamlNode(childObj, sb, indent + 2, isArrayItem: false);
                    }
                    else if (kvp.Value is JsonArray childArr && childArr.Count > 0)
                    {
                        sb.AppendLine($"{prefix}{keyStr}:");
                        WriteYamlNode(childArr, sb, indent + 2, isArrayItem: false);
                    }
                    else
                    {
                        sb.AppendLine($"{prefix}{keyStr}: {FormatYamlScalar(kvp.Value)}");
                    }
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    if (item is JsonObject itemObj && itemObj.Count > 0)
                    {
                        sb.Append($"{pad}- ");
                        WriteYamlNode(itemObj, sb, indent + 2, isArrayItem: true);
                    }
                    else
                    {
                        sb.AppendLine($"{pad}- {FormatYamlScalar(item)}");
                    }
                }
            }
        }

        /// <summary>
        /// Formats a primitive or empty <see cref="JsonNode"/> as a YAML scalar value.
        /// </summary>
        /// <param name="node">The scalar node to format.</param>
        /// <returns>The formatted YAML scalar text.</returns>
        private static string FormatYamlScalar(JsonNode? node)
        {
            if (node == null) return "null";

            var raw = node.ToJsonString();
            if (raw.StartsWith(@"""", StringComparison.Ordinal))
            {
                var unquoted = node.GetValue<string>();
                if (NeedsYamlQuotes(unquoted))
                {
                    return raw;
                }

                return unquoted;
            }

            return raw;
        }

        /// <summary>
        /// Determines whether a string requires quotes in YAML to preserve its type and syntax.
        /// </summary>
        /// <param name="val">The string value to check.</param>
        /// <returns><c>true</c> if quotes are required; otherwise <c>false</c>.</returns>
        private static bool NeedsYamlQuotes(string val)
        {
            if (string.IsNullOrEmpty(val)) return true;
            if (val == "true" || val == "false" || val == "null" || val == "~") return true;
            if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _)) return true;
            const string specialChars = ":#{}[]$*&!|>'\"^";
            foreach (char c in val)
            {
                if (specialChars.IndexOf(c) >= 0 || c == (char)10)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
