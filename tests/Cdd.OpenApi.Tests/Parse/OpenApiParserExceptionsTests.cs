using System;
using Xunit;
using Cdd.OpenApi.Parse;

namespace Cdd.OpenApi.Tests.Parse
{
    public class OpenApiParserExceptionsTests
    {
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

        [Fact]
        public void ParseJson_InvalidJson_ThrowsFormatException()
        {
            var parser = new OpenApiParser();

            var exception = Assert.Throws<FormatException>(() => parser.ParseJson("this is not json"));
            Assert.Contains("Failed to parse OpenAPI JSON", exception.Message);
        }
    }
}
