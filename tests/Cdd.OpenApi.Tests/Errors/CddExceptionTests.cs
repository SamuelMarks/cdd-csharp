using System;
using Cdd.OpenApi.Errors;
using Xunit;

namespace Cdd.OpenApi.Tests.Errors
{
    public class CddExceptionTests
    {
        [Fact]
        public void CddException_Constructors_SetErrorCodeAndMessage()
        {
            var ex1 = new CddException(CddErrorCode.InputNotFound, "Missing file");
            Assert.Equal(CddErrorCode.InputNotFound, ex1.ErrorCode);
            Assert.Equal("Missing file", ex1.Message);

            var inner = new InvalidOperationException("Inner");
            var ex2 = new CddException(CddErrorCode.InvalidSpecification, "Invalid spec", inner);
            Assert.Equal(CddErrorCode.InvalidSpecification, ex2.ErrorCode);
            Assert.Equal("Invalid spec", ex2.Message);
            Assert.Same(inner, ex2.InnerException);
        }
    }
}
