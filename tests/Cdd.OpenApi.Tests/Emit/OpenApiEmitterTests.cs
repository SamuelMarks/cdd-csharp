// <copyright file="OpenApiEmitterTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using Xunit;
using Cdd.OpenApi.Emit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Emit
{
    /// <summary>Verifies or exercises open api emitter tests.</summary>
    public class OpenApiEmitterTests
    {
        /// <summary>Verifies or exercises emit json valid document returns formatted json.</summary>
        [Fact]
        public void EmitJson_ValidDocument_ReturnsFormattedJson()
        {
            // Arrange
            var doc = new OpenApiDocument
            {
                OpenApi = "3.2.0",
                Info = new OpenApiInfo
                {
                    Title = "Test API",
                    Version = "1.0.0"
                },
                Paths = new OpenApiPaths()
            };
            var emitter = new OpenApiEmitter();

            // Act
            var json = emitter.EmitJson(doc);

            // Assert
            Assert.Contains("\"openapi\":\"3.2.0\"", json);
            Assert.Contains("\"title\":\"Test API\"", json);
            Assert.Contains("\"paths\":{}", json);

            var yaml = emitter.EmitYaml(doc);
            Assert.Contains("openapi: 3.2.0", yaml);
            Assert.Contains("title: Test API", yaml);
            Assert.Throws<System.ArgumentNullException>(() => emitter.EmitYaml(null!));
        }
    }
}
