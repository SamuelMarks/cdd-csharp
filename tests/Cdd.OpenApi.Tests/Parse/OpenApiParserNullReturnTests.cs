using System;
using Xunit;
using Cdd.OpenApi.Parse;
using System.Text.Json;

namespace Cdd.OpenApi.Tests.Parse
{
    public class OpenApiParserNullReturnTests
    {
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
