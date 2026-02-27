using System;
using Xunit;
using Cdd.OpenApi.Emit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Emit
{
    public class OpenApiEmitterExceptionsTests
    {
        [Fact]
        public void EmitJson_NullDocument_ThrowsArgumentNullException()
        {
            var emitter = new OpenApiEmitter();

            Assert.Throws<ArgumentNullException>(() => emitter.EmitJson(null!));
        }
    }
}
