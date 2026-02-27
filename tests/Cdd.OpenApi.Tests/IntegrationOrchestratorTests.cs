using System.Collections.Generic;
using Xunit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests
{
    public class IntegrationOrchestratorTests
    {
        [Fact]
        public void SpecGenerator_CodeGenerator_RoundTrip()
        {
            var code = @"
            using System;
            
            public class User 
            {
                /// <summary>
                /// User identifier
                /// </summary>
                public int Id { get; set; }
                public string Name { get; set; }
            }
            
            public class UsersController 
            {
                /// <summary>
                /// Get a user by ID
                /// </summary>
                [HttpGet(""/users/{id}"")]
                public void GetUser(int id) {}
            }";

            // AST -> Spec
            var doc = SpecGenerator.Generate(new[] { code });
            
            Assert.NotNull(doc.Paths);
            Assert.True(doc.Paths.ContainsKey("/users/{id}"));
            Assert.NotNull(doc.Components?.Schemas);
            Assert.True(doc.Components.Schemas.ContainsKey("User"));

            // Spec -> AST
            var generatedFiles = CodeGenerator.Generate(doc);

            Assert.Equal(2, generatedFiles.Count);
            
            var userClass = generatedFiles.Find(f => f.FileName == "Models/User.cs");
            Assert.NotNull(userClass);
            Assert.Contains("public class User", userClass.Code);
            Assert.Contains("int Id", userClass.Code);
            
            var interfaceFile = generatedFiles.Find(f => f.FileName == "IApi.cs");
            Assert.NotNull(interfaceFile);
            Assert.Contains("interface IApi", interfaceFile.Code);
            Assert.Contains("GetUser", interfaceFile.Code);
        }
    }
}
