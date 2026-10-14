// <copyright file="NullableAttributes.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

#pragma warning disable CS0436

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Compiler-synthesized attribute specifying the default nullability context for contained declarations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
    internal sealed class NullableContextAttribute : Attribute
    {
        /// <summary>The default nullability state flag.</summary>
        public readonly byte Flag;

        /// <summary>Initializes a new instance of the <see cref="NullableContextAttribute"/> class.</summary>
        /// <param name="P_0">The default nullability state byte flag.</param>
        public NullableContextAttribute(byte P_0) { Flag = P_0; }
    }

    /// <summary>
    /// Compiler-synthesized attribute specifying nullability annotations on a declaration or type reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, Inherited = false)]
    internal sealed class NullableAttribute : Attribute
    {
        /// <summary>The array of nullability state flags.</summary>
        public readonly byte[] NullableFlags;

        /// <summary>Initializes a new instance of the <see cref="NullableAttribute"/> class with a single flag.</summary>
        /// <param name="P_0">The nullability state byte flag.</param>
        public NullableAttribute(byte P_0) { NullableFlags = new byte[] { P_0 }; }

        /// <summary>Initializes a new instance of the <see cref="NullableAttribute"/> class with an array of flags.</summary>
        /// <param name="P_0">The array of nullability state byte flags.</param>
        public NullableAttribute(byte[] P_0) { NullableFlags = P_0; }
    }
}
