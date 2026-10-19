// <copyright file="StringEscaper.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;

namespace Cdd.OpenApi.Common
{
    /// <summary>
    /// Provides shared string and XML escaping helpers for C# code generation and YAML/JSON processing.
    /// </summary>
    public static class StringEscaper
    {
        /// <summary>The double-quote character.</summary>
        public const char DoubleQuoteChar = '\u0022';

        /// <summary>The single-quote character.</summary>
        public const char SingleQuoteChar = '\u0027';

        /// <summary>The backslash character.</summary>
        public const char BackslashChar = '\u005C';

        /// <summary>The line-feed character.</summary>
        public const char LineFeedChar = '\u000A';

        /// <summary>The carriage-return character.</summary>
        public const char CarriageReturnChar = '\u000D';

        /// <summary>The double-quote string.</summary>
        public const string DoubleQuote = "\u0022";

        /// <summary>The backslash string.</summary>
        public const string Backslash = "\u005C";

        /// <summary>The line-feed string.</summary>
        public const string LineFeed = "\u000A";

        /// <summary>The carriage-return string.</summary>
        public const string CarriageReturn = "\u000D";

        /// <summary>The carriage-return and line-feed string.</summary>
        public const string CrLf = "\u000D\u000A";

        /// <summary>
        /// Escapes backslashes, double quotes, and newlines for embedding inside a C# string literal.
        /// </summary>
        /// <param name="text">The raw string to escape.</param>
        /// <returns>The escaped C# string literal content.</returns>
        public static string EscapeCSharpLiteral(string text)
        {
            return text
                .Replace(Backslash, Backslash + Backslash, StringComparison.Ordinal)
                .Replace(DoubleQuote, Backslash + DoubleQuote, StringComparison.Ordinal)
                .Replace(CarriageReturn, string.Empty, StringComparison.Ordinal)
                .Replace(LineFeed, " ", StringComparison.Ordinal);
        }

        /// <summary>
        /// Escapes special XML characters (<c>&amp;</c>, <c>&lt;</c>, <c>&gt;</c>, <c>"</c>, <c>'</c>) and normalizes line breaks for single-line XML doc comments.
        /// </summary>
        /// <param name="text">The raw text to escape.</param>
        /// <returns>The XML-safe text.</returns>
        public static string EscapeXmlComment(string text)
        {
            return text
                .Replace(CarriageReturn, string.Empty, StringComparison.Ordinal)
                .Replace(LineFeed, " ", StringComparison.Ordinal)
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace(DoubleQuote, "&quot;", StringComparison.Ordinal)
                .Replace("'", "&apos;", StringComparison.Ordinal);
        }
    }
}
