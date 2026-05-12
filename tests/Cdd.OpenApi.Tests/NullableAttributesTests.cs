using Xunit;
using System;
using System.Reflection;
using System.Linq;

namespace Cdd.OpenApi.Tests
{
    public class NullableAttributesTests
    {
        [Fact]
        public void CanInstantiateAttributes()
        {
            var assembly = typeof(Cdd.OpenApi.CodeGenerator).Assembly;
            var ctxType = assembly.GetType("System.Runtime.CompilerServices.NullableContextAttribute");
            var ctx = Activator.CreateInstance(ctxType, new object[] { (byte)1 });
            Assert.Equal((byte)1, ctxType.GetField("Flag").GetValue(ctx));
            
            var attrType = assembly.GetType("System.Runtime.CompilerServices.NullableAttribute");
            var attr1 = Activator.CreateInstance(attrType, new object[] { (byte)1 });
            Assert.Equal(new byte[] { 1 }, attrType.GetField("NullableFlags").GetValue(attr1));
            
            var attr2 = Activator.CreateInstance(attrType, new object[] { new byte[] { 1, 2 } });
            Assert.Equal(new byte[] { 1, 2 }, attrType.GetField("NullableFlags").GetValue(attr2));
        }
    }
}
