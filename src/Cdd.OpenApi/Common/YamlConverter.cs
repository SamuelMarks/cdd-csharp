// <copyright file="YamlConverter.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;

namespace Cdd.OpenApi.Common
{
    /// <summary>
    /// Provides zero-dependency bidirectional conversion between YAML strings and <see cref="JsonNode"/> trees.
    /// </summary>
    public static class YamlConverter
    {
        /// <summary>
        /// Converts a YAML string into a <see cref="JsonNode"/> hierarchy.
        /// </summary>
        /// <param name="yaml">The YAML document string.</param>
        /// <returns>The root <see cref="JsonNode"/>.</returns>
        public static JsonNode ConvertYamlToJsonNode(string yaml)
        {
            var rawLines = yaml.Replace(StringEscaper.CrLf, StringEscaper.LineFeed).Split(StringEscaper.LineFeedChar);
            var lines = new List<(int Indent, string Content)>();
            foreach (var raw in rawLines)
            {
                var stripped = StripYamlComment(raw);
                if (string.IsNullOrWhiteSpace(stripped) || stripped.Trim() == "---" || stripped.Trim() == "...")
                {
                    continue;
                }

                int indent = 0;
                while (stripped[indent] == ' ')
                {
                    indent++;
                }

                lines.Add((indent, stripped.Substring(indent).TrimEnd()));
            }

            if (lines.Count == 0)
            {
                return new JsonObject();
            }

            int index = 0;
            return ParseYamlBlock(lines, ref index, lines[0].Indent);
        }

        /// <summary>
        /// Serializes a <see cref="JsonNode"/> hierarchy into a canonical YAML string.
        /// </summary>
        /// <param name="rootNode">The root <see cref="JsonNode"/> to format as YAML.</param>
        /// <returns>The formatted YAML string.</returns>
        public static string ConvertJsonNodeToYaml(JsonNode? rootNode)
        {
            var sb = new StringBuilder();
            WriteYamlNode(rootNode, sb, 0, isArrayItem: false);
            return sb.ToString();
        }

        /// <summary>
        /// Strips unquoted YAML comments (<c># ...</c>) from a single line.
        /// </summary>
        /// <param name="line">The raw YAML line.</param>
        /// <returns>The line without trailing comment.</returns>
        private static string StripYamlComment(string line)
        {
            int hashIdx = line.IndexOf(" #", StringComparison.Ordinal);
            if (hashIdx >= 0)
            {
                return line.Substring(0, hashIdx);
            }

            if (line.TrimStart().StartsWith("#", StringComparison.Ordinal))
            {
                return string.Empty;
            }

            return line;
        }

        /// <summary>
        /// Parses an indented YAML block (mapping or sequence) starting at <paramref name="index"/>.
        /// </summary>
        /// <param name="lines">The preprocessed list of indented YAML lines.</param>
        /// <param name="index">The current line index (updated as lines are consumed).</param>
        /// <param name="baseIndent">The indentation level of the current block.</param>
        /// <returns>A <see cref="JsonNode"/> representing the block.</returns>
        private static JsonNode ParseYamlBlock(List<(int Indent, string Content)> lines, ref int index, int baseIndent)
        {
            if (lines[index].Content.StartsWith("-", StringComparison.Ordinal))
            {
                var arr = new JsonArray();
                while (index < lines.Count && lines[index].Indent == baseIndent &&
                       lines[index].Content.StartsWith("-", StringComparison.Ordinal))
                {
                    var currentLine = lines[index];
                    var afterDash = currentLine.Content.Substring(1).Trim();
                    index++;

                    if (string.IsNullOrEmpty(afterDash))
                    {
                        arr.Add(ParseYamlBlock(lines, ref index, lines[index].Indent));
                    }
                    else if (TrySplitYamlKeyValue(afterDash, out var k, out var v))
                    {
                        var itemObj = new JsonObject();
                        itemObj[k] = ParseYamlScalar(v);

                        var childIndent = lines[index].Indent;
                        var restObj = ParseYamlObjectEntries(lines, ref index, childIndent);
                        foreach (var kvp in restObj)
                        {
                            itemObj[kvp.Key] = kvp.Value!.DeepClone();
                        }

                        arr.Add((JsonNode)itemObj);
                    }
                    else
                    {
                        arr.Add(ParseYamlScalar(afterDash));
                    }
                }

                return arr;
            }

            return ParseYamlObjectEntries(lines, ref index, baseIndent);
        }

        /// <summary>
        /// Parses YAML mapping key-value entries at <paramref name="baseIndent"/> into a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="lines">The preprocessed list of indented YAML lines.</param>
        /// <param name="index">The current line index.</param>
        /// <param name="baseIndent">The indentation level of the mapping.</param>
        /// <returns>The populated <see cref="JsonObject"/>.</returns>
        private static JsonObject ParseYamlObjectEntries(List<(int Indent, string Content)> lines, ref int index, int baseIndent)
        {
            var obj = new JsonObject();
            while (index < lines.Count && lines[index].Indent == baseIndent &&
                   !lines[index].Content.StartsWith("- ", StringComparison.Ordinal))
            {
                var line = lines[index].Content;
                index++;
                TrySplitYamlKeyValue(line, out var key, out var val);
                if (string.IsNullOrEmpty(val) || val == "|")
                {
                    if (val == "|")
                    {
                        var childIndent = lines[index].Indent;
                        var sb = new List<string>();
                        while (index < lines.Count && lines[index].Indent >= childIndent)
                        {
                            sb.Add(lines[index].Content);
                            index++;
                        }

                        obj[key] = JsonValue.Create(string.Join(StringEscaper.LineFeed, sb));
                    }
                    else
                    {
                        obj[key] = ParseYamlBlock(lines, ref index, lines[index].Indent);
                    }
                }
                else
                {
                    obj[key] = ParseYamlScalar(val);
                }
            }

            return obj;
        }

        /// <summary>
        /// Attempts to split a YAML <c>key: value</c> line into unquoted key and raw value parts.
        /// </summary>
        /// <param name="text">The YAML line text.</param>
        /// <param name="key">When successful, receives the unquoted property key.</param>
        /// <param name="value">When successful, receives the trimmed value part.</param>
        /// <returns><c>true</c> if the line represents a YAML key-value mapping; otherwise <c>false</c>.</returns>
        private static bool TrySplitYamlKeyValue(string text, out string key, out string value)
        {
            bool inQuotes = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == StringEscaper.DoubleQuoteChar) inQuotes = !inQuotes;
                else if (c == ':' && !inQuotes && (i + 1 == text.Length || text[i + 1] == ' '))
                {
                    key = UnquoteYaml(text.Substring(0, i).Trim());
                    value = text.Substring(i + 1).Trim();
                    return true;
                }
            }

            key = string.Empty;
            value = string.Empty;
            return false;
        }

        /// <summary>
        /// Parses an inline YAML scalar or inline JSON array/object into a <see cref="JsonNode"/>.
        /// </summary>
        /// <param name="raw">The raw scalar value text.</param>
        /// <returns>The parsed <see cref="JsonNode"/> or <c>null</c>.</returns>
        private static JsonNode? ParseYamlScalar(string raw)
        {
            if (raw == "null" || raw == "~")
            {
                return null;
            }

            if (raw.StartsWith("[", StringComparison.Ordinal) || raw.StartsWith("{", StringComparison.Ordinal))
            {
                return JsonNode.Parse(raw);
            }

            if (raw.StartsWith(StringEscaper.DoubleQuote, StringComparison.Ordinal) || raw.StartsWith("'", StringComparison.Ordinal))
            {
                return JsonValue.Create(UnquoteYaml(raw));
            }

            if (string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase))
            {
                return JsonValue.Create(true);
            }

            if (string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase))
            {
                return JsonValue.Create(false);
            }

            if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
            {
                return JsonValue.Create(l);
            }

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            {
                return JsonValue.Create(d);
            }

            return JsonValue.Create(raw);
        }

        /// <summary>
        /// Removes surrounding single or double quotes from a YAML scalar string.
        /// </summary>
        /// <param name="val">The potentially quoted YAML string.</param>
        /// <returns>The unquoted string.</returns>
        private static string UnquoteYaml(string val)
        {
            if (val.Length >= 2)
            {
                var dq = StringEscaper.DoubleQuote;
                var bs = StringEscaper.Backslash;
                if (val[0] == StringEscaper.DoubleQuoteChar && val[val.Length - 1] == StringEscaper.DoubleQuoteChar)
                {
                    return val.Substring(1, val.Length - 2).Replace(bs + dq, dq).Replace(bs + bs, bs);
                }

                if (val[0] == StringEscaper.SingleQuoteChar && val[val.Length - 1] == StringEscaper.SingleQuoteChar)
                {
                    return val.Substring(1, val.Length - 2).Replace("''", "'");
                }
            }

            return val;
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
                    var keyStr = NeedsYamlQuotes(kvp.Key) ? StringEscaper.DoubleQuote + kvp.Key + StringEscaper.DoubleQuote : kvp.Key;
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
            if (raw.StartsWith(StringEscaper.DoubleQuote, StringComparison.Ordinal))
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
            if (double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out _)) return true;
            const string specialChars = ":#{}[]$*&!|>'\u0022^";
            foreach (char c in val)
            {
                if (specialChars.IndexOf(c) >= 0 || c == StringEscaper.LineFeedChar)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
