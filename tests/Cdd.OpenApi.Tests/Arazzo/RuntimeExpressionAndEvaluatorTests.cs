// <copyright file="RuntimeExpressionAndEvaluatorTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using Cdd.OpenApi.Arazzo.Expressions;
using Cdd.OpenApi.Errors;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Models.Arazzo;
using Xunit;

namespace Cdd.OpenApi.Tests.Arazzo
{
    /// <summary>Verifies or exercises runtime expression and evaluator tests.</summary>
    public class RuntimeExpressionAndEvaluatorTests
    {
        /// <summary>Verifies or exercises runtime expression all15 roots and pointers parse and evaluate correctly.</summary>
        [Fact]
        public void RuntimeExpression_All15RootsAndPointers_ParseAndEvaluateCorrectly()
        {
            var ctx = new ArazzoExecutionContext
            {
                Url = "https://api.example.com/v1/pets/10",
                Method = "POST",
                StatusCode = 201,
                Self = "https://api.example.com/arazzo.json",
                RequestBody = @"{""pet"":{""name"":""Fluffy""},""tags"":[""cute"",""fluffy""]}",
                ResponseBody = @"{""id"":10,""status"":""available"",""items"":[{""code"":""A1"",""price"":9.99}]}",
                MessagePayload = @"{""eventId"":""evt-99""}",
                Components = new ArazzoComponents
                {
                    Parameters = new Dictionary<string, ArazzoParameter>
                    {
                        ["limitParam"] = new ArazzoParameter { Name = "limit", In = "query", Value = "25" }
                    },
                    Inputs = new Dictionary<string, OpenApiSchema>
                    {
                        ["envInput"] = new OpenApiSchema { Type = "string", Default = "prod" }
                    },
                    SuccessActions = new Dictionary<string, ArazzoSuccessAction>
                    {
                        ["endSuccess"] = new ArazzoSuccessAction { Name = "end", Type = "end" }
                    },
                    FailureActions = new Dictionary<string, ArazzoFailureAction>
                    {
                        ["retryFail"] = new ArazzoFailureAction { Name = "retry", Type = "retry" }
                    }
                }
            };
            ctx.RequestHeaders["Authorization"] = "Bearer abc";
            ctx.RequestQuery["filter"] = "active";
            ctx.RequestPath["petId"] = "10";
            ctx.ResponseHeaders["X-Rate-Limit"] = "100";
            ctx.MessageHeaders["X-Correlation-ID"] = "corr-1";
            ctx.Inputs["apiKey"] = "key-123";
            ctx.Inputs["nested"] = @"{""user"":{""id"":7}}";
            ctx.Outputs["resultId"] = "10";
            ctx.StepOutputs["login"] = new Dictionary<string, string>
            {
                ["token"] = "jwt-xyz",
                ["profile"] = @"{""role"":""admin""}"
            };
            ctx.SourceDescriptions["petstore"] = "https://petstore.swagger.io/v2";

            Assert.Equal("https://api.example.com/v1/pets/10", RuntimeExpression.Parse("$url").Evaluate(ctx));
            Assert.Equal("POST", RuntimeExpression.Parse("$method").Evaluate(ctx));
            Assert.Equal("201", RuntimeExpression.Parse("$statusCode").Evaluate(ctx));
            Assert.Equal("https://api.example.com/arazzo.json", RuntimeExpression.Parse("$self").Evaluate(ctx));
            Assert.Equal("Bearer abc", RuntimeExpression.Parse("$request.header.authorization").Evaluate(ctx));
            Assert.Equal("active", RuntimeExpression.Parse("$request.query.filter").Evaluate(ctx));
            Assert.Equal("10", RuntimeExpression.Parse("$request.path.petId").Evaluate(ctx));
            Assert.Equal("Fluffy", RuntimeExpression.Parse("$request.body#/pet/name").Evaluate(ctx));
            Assert.Equal("Fluffy", RuntimeExpression.Parse("$request.body.pet.name").Evaluate(ctx));
            Assert.Equal("cute", RuntimeExpression.Parse("$request.body#/tags/0").Evaluate(ctx));
            Assert.Equal("100", RuntimeExpression.Parse("$response.header.x-rate-limit").Evaluate(ctx));
            Assert.Equal("available", RuntimeExpression.Parse("$response.body#/status").Evaluate(ctx));
            Assert.Equal("available", RuntimeExpression.Parse("$response.body.status").Evaluate(ctx));
            Assert.Equal("corr-1", RuntimeExpression.Parse("$message.header.X-Correlation-ID").Evaluate(ctx));
            Assert.Equal("evt-99", RuntimeExpression.Parse("$message.payload#/eventId").Evaluate(ctx));
            Assert.Equal("evt-99", RuntimeExpression.Parse("$message.payload.eventId").Evaluate(ctx));
            Assert.Equal("key-123", RuntimeExpression.Parse("$inputs.apiKey").Evaluate(ctx));
            Assert.Equal("7", RuntimeExpression.Parse("$inputs.nested.user.id").Evaluate(ctx));
            Assert.Equal("10", RuntimeExpression.Parse("$outputs.resultId").Evaluate(ctx));
            Assert.Equal("jwt-xyz", RuntimeExpression.Parse("$steps.login.outputs.token").Evaluate(ctx));
            Assert.Equal("admin", RuntimeExpression.Parse("$steps.login.outputs.profile#/role").Evaluate(ctx));
            Assert.Equal("https://petstore.swagger.io/v2", RuntimeExpression.Parse("$sourceDescriptions.petstore.url").Evaluate(ctx));
            Assert.Equal("https://petstore.swagger.io/v2#getPet", RuntimeExpression.Parse("$sourceDescriptions.petstore.getPet").Evaluate(ctx));
            Assert.Equal("25", RuntimeExpression.Parse("$components.parameters.limitParam").Evaluate(ctx));
            Assert.Equal("prod", RuntimeExpression.Parse("$components.inputs.envInput").Evaluate(ctx));
            Assert.Equal("end", RuntimeExpression.Parse("$components.successActions.endSuccess").Evaluate(ctx));
            Assert.Equal("retry", RuntimeExpression.Parse("$components.failureActions.retryFail").Evaluate(ctx));

            // Missing lookups return null gracefully
            Assert.Null(RuntimeExpression.Parse("$request.header.missing").Evaluate(ctx));
            Assert.Null(RuntimeExpression.Parse("$inputs.missing").Evaluate(ctx));
            Assert.Null(RuntimeExpression.Parse("$steps.missingStep.outputs.val").Evaluate(ctx));
            Assert.Null(RuntimeExpression.Parse("$sourceDescriptions.missing.url").Evaluate(ctx));
            Assert.Null(RuntimeExpression.Parse("$components.unknown.val").Evaluate(ctx));
            ctx.Components = null;
            Assert.Null(RuntimeExpression.Parse("$components.parameters.limitParam").Evaluate(ctx));

            // Template interpolation
            Assert.Equal(string.Empty, RuntimeExpression.EvaluateTemplate("", ctx));
            Assert.Equal("Bearer jwt-xyz!", RuntimeExpression.EvaluateTemplate("Bearer {$steps.login.outputs.token}!", ctx));
            Assert.Equal("Unclosed {$steps.login.outputs.token", RuntimeExpression.EvaluateTemplate("Unclosed {$steps.login.outputs.token", ctx));
            Assert.Equal("jwt-xyz", RuntimeExpression.EvaluateTemplate("$steps.login.outputs.token", ctx));

            // ToCSharpExpression
            Assert.Equal(@"""""", RuntimeExpression.ToCSharpExpression(""));
            Assert.Equal("input.ApiKey", RuntimeExpression.ToCSharpExpression("$inputs.api-key"));
            Assert.Equal(@"stepOutputs[""login""][""token""]", RuntimeExpression.ToCSharpExpression("$steps.login.outputs.token"));
            Assert.Equal("lastStatusCode.ToString()", RuntimeExpression.ToCSharpExpression("$statusCode"));
            Assert.Equal("lastUrl", RuntimeExpression.ToCSharpExpression("$url"));
            Assert.Equal("lastMethod", RuntimeExpression.ToCSharpExpression("$method"));
            Assert.Equal("SelfUri", RuntimeExpression.ToCSharpExpression("$self"));
            Assert.Equal("lastResponseBody", RuntimeExpression.ToCSharpExpression("$response.body"));
            Assert.Equal(@"ExtractJsonPointer(lastResponseBody, ""/id"")", RuntimeExpression.ToCSharpExpression("$response.body#/id"));
            Assert.Contains("EvaluateRuntimeExpression", RuntimeExpression.ToCSharpExpression("$request.header.Auth"));
            Assert.Equal(@"""literal""", RuntimeExpression.ToCSharpExpression("literal"));

            // Error branches
            Assert.Throws<ArgumentNullException>(() => RuntimeExpression.Parse("$url").Evaluate(null!));
            Assert.Throws<CddException>(() => RuntimeExpression.Parse(""));
            Assert.Throws<CddException>(() => RuntimeExpression.Parse("noDollar"));
            Assert.Throws<CddException>(() => RuntimeExpression.Parse("$request.header."));
            Assert.Throws<CddException>(() => RuntimeExpression.Parse("$steps.bad"));
            Assert.Throws<CddException>(() => RuntimeExpression.Parse("$sourceDescriptions.bad"));
            Assert.Throws<CddException>(() => RuntimeExpression.Parse("$components.bad"));
            Assert.Throws<CddException>(() => RuntimeExpression.Parse("$unknownRoot"));
        }

        /// <summary>Verifies or exercises criterion evaluator simple regex json path json pointer and xpath evaluate correctly.</summary>
        [Fact]
        public void CriterionEvaluator_SimpleRegexJsonPathJsonPointerAndXPath_EvaluateCorrectly()
        {
            var ctx = new ArazzoExecutionContext
            {
                StatusCode = 200,
                ResponseBody = @"{""status"":""available"",""count"":5,""items"":[{""id"":1,""active"":true},{""id"":2,""active"":false}]}"
            };

            Assert.Throws<ArgumentNullException>(() => CriterionEvaluator.EvaluateCriterion(null!, ctx));
            Assert.Throws<ArgumentNullException>(() => CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion(), null!));
            Assert.Throws<ArgumentNullException>(() => CriterionEvaluator.EvaluateSelector(null!, ctx));
            Assert.Throws<ArgumentNullException>(() => CriterionEvaluator.EvaluateSelector(new ArazzoStepOutput("$url"), null!));

            // Simple conditions
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Condition = "" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Condition = "$statusCode == 200 && $.count >= 5" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Condition = "$statusCode != 500" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Condition = "$statusCode == 404 || $statusCode == 200" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Condition = "$.count > 3" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Condition = "$.count <= 5" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Condition = "$.count < 10" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Condition = "$.status != 'sold'" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Condition = "'a' == 'a' && 'b' > 'a' && 'a' < 'b' && 'a' <= 'a' && 'b' >= 'b'" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Condition = "$.status" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$statusCode", Condition = "== 200" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Condition = "$inputs.missing == null" }, ctx));

            // Regex
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$statusCode", Condition = "^20[0-9]$", Type = "regex" }, ctx));
            Assert.False(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$statusCode", Condition = "^50[0-9]$", Type = "regex" }, ctx));

            // JSONPointer
            Assert.Null(CriterionEvaluator.EvaluateJsonPointer(null, "/id"));
            Assert.Null(CriterionEvaluator.JsonNodeToScalarString(null));
            Assert.Contains("active", CriterionEvaluator.EvaluateSelector(new ArazzoSelector { Selector = "/items/0", Type = "jsonpointer" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$response.body", Condition = "/items/0/id", Type = "jsonpointer" }, ctx));
            Assert.False(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$response.body", Condition = "/items/99/id", Type = "jsonpointer" }, ctx));
            Assert.False(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$response.body", Condition = "/status/notAnObject", Type = "jsonpointer" }, ctx));

            // JSONPath
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = @"{""a"":{""b"":1}}", Condition = "$.a.b == 1", Type = "jsonpath" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$response.body", Condition = "$.count == 5", Type = "jsonpath" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$response.body", Condition = "$.items[?(@.id == 1)].id", Type = "jsonpath" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$response.body", Condition = "$.items[?(@.active)]", Type = "jsonpath" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$response.body", Condition = "$..id", Type = "jsonpath" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$response.body", Condition = "$.items[*].id", Type = "jsonpath" }, ctx));
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "$response.body", Condition = "$['status']", Type = "jsonpath" }, ctx));
            Assert.False(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = "not json", Condition = "$.id", Type = "jsonpath" }, ctx));

            // XPath
            var xml = "<store><book id='10'><title>Dune</title></book></store>";
            Assert.True(CriterionEvaluator.EvaluateCriterion(new ArazzoCriterion { Context = xml, Condition = "//book/title", Type = "xpath" }, ctx));
            Assert.Equal("Dune", CriterionEvaluator.EvaluateSelector(new ArazzoSelector { Context = xml, Selector = "//book/title", Type = "xpath" }, ctx));
            Assert.Equal("1", CriterionEvaluator.EvaluateSelector(new ArazzoSelector { Context = xml, Selector = "count(//book)", Type = "xpath" }, ctx));
            Assert.Null(CriterionEvaluator.EvaluateXPath("<bad xml", "//book"));
            Assert.Null(CriterionEvaluator.EvaluateXPath(xml, "///invalid[xpath"));
            Assert.Null(CriterionEvaluator.EvaluateXPath("", "//book"));

            // Selectors
            Assert.Equal("$statusCode", CriterionEvaluator.EvaluateSelector(new ArazzoStepOutput("200"), ctx) == "200" ? "$statusCode" : "");
            Assert.Equal("1", CriterionEvaluator.EvaluateSelector(new ArazzoSelector { Selector = "/items/0/id", Type = "jsonpointer" }, ctx));
            Assert.Equal("available", CriterionEvaluator.EvaluateSelector(new ArazzoSelector { Selector = "$.status", Type = "jsonpath" }, ctx));
            Assert.Equal("available", CriterionEvaluator.EvaluateSelector(new ArazzoSelector { Selector = "$.status", Type = null }, ctx));
        }

        /// <summary>Verifies or exercises payload replacer json pointer json path and xpath modify payload correctly.</summary>
        [Fact]
        public void PayloadReplacer_JsonPointerJsonPathAndXPath_ModifyPayloadCorrectly()
        {
            var ctx = new ArazzoExecutionContext();
            ctx.Inputs["petName"] = "Rex";
            ctx.Inputs["isVaccinated"] = "true";
            ctx.Inputs["isAdopted"] = "false";
            ctx.Inputs["age"] = "4";
            ctx.Inputs["tagsJson"] = @"[""guard""]";
            ctx.Inputs["objJson"] = @"{""k"":1}";

            Assert.Throws<ArgumentNullException>(() => PayloadReplacer.ApplyReplacements("{}", null, null!));
            Assert.Equal(string.Empty, PayloadReplacer.ApplyReplacements(null, null, ctx));
            Assert.Equal(string.Empty, PayloadReplacer.ApplyReplacements("", null, ctx));
            Assert.Equal("Hello Rex", PayloadReplacer.ApplyReplacements("Hello {$inputs.petName}", null, ctx));

            var basePayload = new OpenApiAny(@"{""name"":""$inputs.petName"",""count"":1,""nested"":{},""arr"":[""$inputs.age""]}", System.Text.Json.JsonValueKind.Object);
            var replacements = new List<ArazzoPayloadReplacement>
            {
                new ArazzoPayloadReplacement { Target = "/nested/vaccinated", TargetSelectorType = "jsonpointer", Value = "$inputs.isVaccinated" },
                new ArazzoPayloadReplacement { Target = "/nested/adopted", Value = "$inputs.isAdopted" },
                new ArazzoPayloadReplacement { Target = "/nested/meta", Value = "$inputs.objJson" },
                new ArazzoPayloadReplacement { Target = "$.arr[0]", TargetSelectorType = "jsonpath", Value = "$inputs.age" },
                new ArazzoPayloadReplacement { Target = "$.tags", Value = "$inputs.tagsJson" }
            };

            var resultJson = PayloadReplacer.ApplyReplacements(basePayload, replacements, ctx);
            Assert.Contains("Rex", resultJson);
            Assert.Contains("vaccinated", resultJson);
            Assert.Contains("[4]", resultJson);
            Assert.Contains("guard", resultJson);

            // XML XPath replacement
            var xmlPayload = new OpenApiAny("<pet><name>Old</name></pet>", System.Text.Json.JsonValueKind.String);
            var xmlRep = new List<ArazzoPayloadReplacement>
            {
                new ArazzoPayloadReplacement { Target = "//pet/name", TargetSelectorType = "xpath", Value = "$inputs.petName" }
            };
            var updatedXml = PayloadReplacer.ApplyReplacements(xmlPayload, xmlRep, ctx);
            Assert.Contains("Rex", updatedXml);

            // Invalid XML fallback
            var badXml = new OpenApiAny("<unclosed", System.Text.Json.JsonValueKind.String);
            Assert.Equal("<unclosed", PayloadReplacer.ApplyReplacements(badXml, xmlRep, ctx));
        }
    }
}
