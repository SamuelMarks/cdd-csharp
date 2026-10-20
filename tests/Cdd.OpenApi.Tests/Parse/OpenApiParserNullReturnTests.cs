// <copyright file="OpenApiParserNullReturnTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using Xunit;
using Cdd.OpenApi.Parse;
using System.Text.Json;

namespace Cdd.OpenApi.Tests.Parse
{
    /// <summary>Verifies or exercises open api parser null return tests.</summary>
    public class OpenApiParserNullReturnTests
    {
        /// <summary>Verifies or exercises parse json null literal json returns empty document.</summary>
        [Fact]
        public void ParseJson_NullLiteralJson_ReturnsEmptyDocument()
        {
            var parser = new OpenApiParser();

            var doc = parser.ParseJson("null");

            Assert.NotNull(doc);
            Assert.Equal("3.2.0", doc.OpenApi);
        }
    }
}
