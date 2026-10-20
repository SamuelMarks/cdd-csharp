// <copyright file="OpenApiParserExceptionsTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using Xunit;
using Cdd.OpenApi.Parse;

namespace Cdd.OpenApi.Tests.Parse
{
    /// <summary>Verifies or exercises open api parser exceptions tests.</summary>
    public class OpenApiParserExceptionsTests
    {
        /// <summary>Verifies or exercises parse json empty or null input throws argument exception.</summary>
        /// <param name="input">The input JSON string to test.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ParseJson_EmptyOrNullInput_ThrowsArgumentException(string? input)
        {
            var parser = new OpenApiParser();

            var exception = Assert.Throws<ArgumentException>(() => parser.ParseJson(input!));
            Assert.Contains("JSON content cannot be null or empty", exception.Message);
        }

        /// <summary>Verifies or exercises parse json invalid json throws format exception.</summary>
        [Fact]
        public void ParseJson_InvalidJson_ThrowsFormatException()
        {
            var parser = new OpenApiParser();

            var exception = Assert.Throws<FormatException>(() => parser.ParseJson("this is not json"));
            Assert.Contains("Failed to parse OpenAPI JSON", exception.Message);
        }
    }
}
