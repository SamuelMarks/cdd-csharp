// <copyright file="OpenApiEmitterExceptionsTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using Xunit;
using Cdd.OpenApi.Emit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Emit
{
    /// <summary>Verifies or exercises open api emitter exceptions tests.</summary>
    public class OpenApiEmitterExceptionsTests
    {
        /// <summary>Verifies or exercises emit json null document throws argument null exception.</summary>
        [Fact]
        public void EmitJson_NullDocument_ThrowsArgumentNullException()
        {
            var emitter = new OpenApiEmitter();

            Assert.Throws<ArgumentNullException>(() => emitter.EmitJson(null!));
        }
    }
}
