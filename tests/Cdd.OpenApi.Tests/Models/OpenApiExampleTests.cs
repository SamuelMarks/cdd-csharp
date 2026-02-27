using Xunit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Models
{
    public class OpenApiExampleTests
    {
        [Fact]
        public void Example_Properties_CanBeSetAndGot()
        {
            var example = new OpenApiExample
            {
                Summary = "A summary",
                Description = "A description",
                Value = 42,
                ExternalValue = "http://example.com/value.txt"
            };

            Assert.Equal("A summary", example.Summary);
            Assert.Equal("A description", example.Description);
            Assert.Equal(42, example.Value);
            Assert.Equal("http://example.com/value.txt", example.ExternalValue);
        }
    }
}
