#pragma warning disable CS0436

namespace System.Runtime.CompilerServices {
    [AttributeUsage(AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
    internal sealed class NullableContextAttribute : Attribute {
        public readonly byte Flag;
        /// <summary>Internal shim constructor</summary>
        public NullableContextAttribute(byte P_0) { Flag = P_0; }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, Inherited = false)]
    internal sealed class NullableAttribute : Attribute {
        public readonly byte[] NullableFlags;
        /// <summary>Internal shim constructor</summary>
        public NullableAttribute(byte P_0) { NullableFlags = new byte[] { P_0 }; }
        /// <summary>Internal shim constructor</summary>
        public NullableAttribute(byte[] P_0) { NullableFlags = P_0; }
    }
}
