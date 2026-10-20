// <copyright file="FunctionsTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Cdd.OpenApi.Tests.Functions
{
    /// <summary>Verifies or exercises functions tests.</summary>
    public class FunctionsTests
    {
        /// <summary>Verifies or exercises parse and emit function round trip and branches.</summary>
        [Fact]
        public void ParseAndEmitFunction_RoundTripAndBranches()
        {
            var code = @"
            public class TestFunc
            {
                /// <summary>
                /// Does a task
                /// </summary>
                public void DoTask() {}

                public string ComputeValue(int count, string label = ""default"") => label;
                public Task AsyncTask() => Task.CompletedTask;
                public System.Threading.Tasks.Task FullAsyncTask() => System.Threading.Tasks.Task.CompletedTask;
            }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var methods = tree.GetRoot().GetDescendantNodesSafe().OfType<MethodDeclarationSyntax>().ToList();

            var op = global::Cdd.OpenApi.Functions.Parse.ParseFunction(methods[0]);
            Assert.Equal("DoTask", op.OperationId);
            Assert.Equal("Does a task", op.Summary);
            Assert.Null(op.Parameters);
            Assert.Null(op.Responses);

            var ep = global::Cdd.OpenApi.Functions.Parse.ParseEndpoint(methods[0]);
            Assert.Equal("DoTask", ep.OperationId);
            var emittedFromEp = global::Cdd.OpenApi.Functions.Emit.EmitFunction(ep);
            AssertHelper.ContainsNoWhitespace("public static void DoTask()", emittedFromEp.ToFormattedString());

            var emitted = global::Cdd.OpenApi.Functions.Emit.EmitFunction(op);
            var formatted = emitted.ToFormattedString();
            AssertHelper.ContainsNoWhitespace("public static void DoTask()", formatted);
            Assert.Contains("Does a task", formatted);

            var op2 = global::Cdd.OpenApi.Functions.Parse.ParseFunction(methods[1]);
            Assert.Equal("ComputeValue", op2.OperationId);
            Assert.NotNull(op2.Parameters);
            Assert.Equal(2, op2.Parameters.Count);
            Assert.Equal("count", op2.Parameters[0].Name);
            Assert.True(op2.Parameters[0].Required);
            Assert.Equal("integer", op2.Parameters[0].Schema?.Type);
            Assert.Equal("label", op2.Parameters[1].Name);
            Assert.False(op2.Parameters[1].Required);
            Assert.NotNull(op2.Responses);
            Assert.Equal("string", op2.Responses["200"].Content?["application/json"].Schema?.Type);

            var emitted2 = global::Cdd.OpenApi.Functions.Emit.EmitFunction(op2);
            AssertHelper.ContainsNoWhitespace("public static string ComputeValue(int count, string label)", emitted2.ToFormattedString());

            var op3 = global::Cdd.OpenApi.Functions.Parse.ParseFunction(methods[2]);
            Assert.Null(op3.Responses);

            var op4 = global::Cdd.OpenApi.Functions.Parse.ParseFunction(methods[3]);
            Assert.Null(op4.Responses);

            var typelessParamMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "Untyped")
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("arg")));
            var opUntyped = global::Cdd.OpenApi.Functions.Parse.ParseFunction(typelessParamMethod);
            Assert.Equal("string", opUntyped.Parameters?[0].Schema?.Type);

            var opEdgeCases = new OpenApiOperation
            {
                OperationId = null,
                Summary = null,
                Responses = new OpenApiResponses
                {
                    ["404"] = new OpenApiResponse { Description = "Not Found" }
                }
            };
            var emittedWithoutId = global::Cdd.OpenApi.Functions.Emit.EmitFunction(opEdgeCases);
            AssertHelper.ContainsNoWhitespace("public static void DefaultFunction()", emittedWithoutId.ToFormattedString());

            var opNullContent = new OpenApiOperation
            {
                OperationId = "NullContentOp",
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse { Content = null }
                }
            };
            AssertHelper.ContainsNoWhitespace("public static void NullContentOp()", global::Cdd.OpenApi.Functions.Emit.EmitFunction(opNullContent).ToFormattedString());

            var opTextPlain = new OpenApiOperation
            {
                OperationId = "TextPlainOp",
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Content = new Dictionary<string, OpenApiMediaType> { ["text/plain"] = new OpenApiMediaType() }
                    }
                }
            };
            AssertHelper.ContainsNoWhitespace("public static void TextPlainOp()", global::Cdd.OpenApi.Functions.Emit.EmitFunction(opTextPlain).ToFormattedString());

            var opNullSchema = new OpenApiOperation
            {
                OperationId = "NullSchemaOp",
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Content = new Dictionary<string, OpenApiMediaType> { ["application/json"] = new OpenApiMediaType { Schema = null } }
                    }
                }
            };
            AssertHelper.ContainsNoWhitespace("public static void NullSchemaOp()", global::Cdd.OpenApi.Functions.Emit.EmitFunction(opNullSchema).ToFormattedString());
        }
    }
}
