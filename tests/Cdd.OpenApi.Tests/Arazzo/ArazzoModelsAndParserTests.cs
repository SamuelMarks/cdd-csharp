// <copyright file="ArazzoModelsAndParserTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.Json;
using Cdd.OpenApi.Arazzo;
using Cdd.OpenApi.Errors;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Models.Arazzo;
using Xunit;

namespace Cdd.OpenApi.Tests.Arazzo
{
    public class ArazzoModelsAndParserTests
    {
        [Fact]
        public void ParseAndEmit_FullJsonAndYamlRoundTrip_PreservesAllFields()
        {
            var extElement = JsonDocument.Parse(@"{""custom"":true}").RootElement.Clone();
            var doc = new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Self = "https://example.org/arazzo.json",
                Info = new ArazzoInfo
                {
                    Title = "PetstoreWorkflows",
                    Summary = "Petstore workflow suite",
                    Description = "Detailed workflow description",
                    Version = "1.2.3",
                    Extensions = new Dictionary<string, JsonElement> { ["x-info"] = extElement }
                },
                SourceDescriptions = new List<ArazzoSourceDescription>
                {
                    new ArazzoSourceDescription
                    {
                        Name = "petstore",
                        Url = "./petstore.yaml",
                        Type = "openapi",
                        Extensions = new Dictionary<string, JsonElement> { ["x-sd"] = extElement }
                    },
                    new ArazzoSourceDescription
                    {
                        Name = "events",
                        Url = "./events.yaml",
                        Type = "asyncapi"
                    },
                    new ArazzoSourceDescription
                    {
                        Name = "externalWf",
                        Url = "./external.arazzo.yaml",
                        Type = "arazzo"
                    }
                },
                Components = new ArazzoComponents
                {
                    Inputs = new Dictionary<string, OpenApiSchema>
                    {
                        ["userInput"] = new OpenApiSchema { Type = "object", Default = "defaultUser" }
                    },
                    Parameters = new Dictionary<string, ArazzoParameter>
                    {
                        ["pageParam"] = new ArazzoParameter { Name = "page", In = "query", Value = 1 }
                    },
                    SuccessActions = new Dictionary<string, ArazzoSuccessAction>
                    {
                        ["doneAction"] = new ArazzoSuccessAction { Name = "done", Type = "end" }
                    },
                    FailureActions = new Dictionary<string, ArazzoFailureAction>
                    {
                        ["retryAction"] = new ArazzoFailureAction { Name = "retryIt", Type = "retry", RetryAfter = 1.5, RetryLimit = 3 }
                    },
                    Extensions = new Dictionary<string, JsonElement> { ["x-comp"] = extElement }
                },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow
                    {
                        WorkflowId = "setupWorkflow",
                        Steps = new List<ArazzoStep>
                        {
                            new ArazzoStep
                            {
                                StepId = "initStep",
                                OperationPath = "{$sourceDescriptions.petstore.url}#/paths/~1pet/post"
                            }
                        }
                    },
                    new ArazzoWorkflow
                    {
                        WorkflowId = "adoptPet",
                        Summary = "Adopt a pet",
                        Description = "Full pet adoption flow",
                        DependsOn = new List<string> { "setupWorkflow", "$sourceDescriptions.externalWf.init" },
                        Inputs = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["petName"] = new OpenApiSchema { Type = "string", Description = "Name of pet" },
                                ["age"] = new OpenApiSchema { Type = "integer" }
                            }
                        },
                        Parameters = new List<ArazzoParameter>
                        {
                            new ArazzoParameter { Reference = "$components.parameters.pageParam", Value = 2 }
                        },
                        SuccessActions = new List<ArazzoSuccessAction>
                        {
                            new ArazzoSuccessAction { Reference = "$components.successActions.doneAction", Value = "ok" }
                        },
                        FailureActions = new List<ArazzoFailureAction>
                        {
                            new ArazzoFailureAction { Reference = "$components.failureActions.retryAction", Value = "retry" }
                        },
                        Steps = new List<ArazzoStep>
                        {
                            new ArazzoStep
                            {
                                StepId = "createPet",
                                Description = "Create pet record",
                                OperationId = "addPet",
                                Timeout = 5000,
                                CorrelationId = "$request.header.X-Correlation-ID",
                                Action = "send",
                                Parameters = new List<ArazzoParameter>
                                {
                                    new ArazzoParameter
                                    {
                                        Name = "X-Trace",
                                        In = "header",
                                        Value = "$inputs.petName",
                                        Extensions = new Dictionary<string, JsonElement> { ["x-param"] = extElement }
                                    }
                                },
                                RequestBody = new ArazzoRequestBody
                                {
                                    ContentType = "application/json",
                                    Payload = @"{""name"":""placeholder""}",
                                    Replacements = new List<ArazzoPayloadReplacement>
                                    {
                                        new ArazzoPayloadReplacement
                                        {
                                            Target = "/name",
                                            TargetSelectorType = "jsonpointer",
                                            Value = "$inputs.petName",
                                            Extensions = new Dictionary<string, JsonElement> { ["x-rep"] = extElement }
                                        }
                                    },
                                    Extensions = new Dictionary<string, JsonElement> { ["x-rb"] = extElement }
                                },
                                SuccessCriteria = new List<ArazzoCriterion>
                                {
                                    new ArazzoCriterion
                                    {
                                        Context = "$statusCode",
                                        Condition = "$statusCode == 200",
                                        Type = "simple",
                                        Extensions = new Dictionary<string, JsonElement> { ["x-crit"] = extElement }
                                    },
                                    new ArazzoCriterion
                                    {
                                        Context = "$response.body",
                                        Condition = "$[?(@.id != null)]",
                                        Type = new ArazzoExpressionType
                                        {
                                            Type = "jsonpath",
                                            Version = "rfc9535",
                                            Extensions = new Dictionary<string, JsonElement> { ["x-expr"] = extElement }
                                        }
                                    }
                                },
                                OnSuccess = new List<ArazzoSuccessAction>
                                {
                                    new ArazzoSuccessAction
                                    {
                                        Name = "gotoStream",
                                        Type = "goto",
                                        StepId = "streamStep",
                                        WorkflowId = null,
                                        Parameters = new List<ArazzoParameter> { new ArazzoParameter { Name = "p", In = "query", Value = "v" } },
                                        Criteria = new List<ArazzoCriterion> { new ArazzoCriterion { Condition = "$statusCode == 200" } },
                                        Extensions = new Dictionary<string, JsonElement> { ["x-sa"] = extElement }
                                    }
                                },
                                OnFailure = new List<ArazzoFailureAction>
                                {
                                    new ArazzoFailureAction
                                    {
                                        Name = "retryCreate",
                                        Type = "retry",
                                        StepId = "createPet",
                                        WorkflowId = null,
                                        RetryAfter = 0.5,
                                        RetryLimit = 2,
                                        Parameters = new List<ArazzoParameter> { new ArazzoParameter { Name = "p", In = "query", Value = "v" } },
                                        Criteria = new List<ArazzoCriterion> { new ArazzoCriterion { Condition = "$statusCode == 500" } },
                                        Extensions = new Dictionary<string, JsonElement> { ["x-fa"] = extElement }
                                    }
                                },
                                Outputs = new Dictionary<string, ArazzoStepOutput>
                                {
                                    ["petId"] = "$response.body#/id",
                                    ["petTag"] = new ArazzoSelector
                                    {
                                        Context = "$response.body",
                                        Selector = "$.tag.name",
                                        Type = new ArazzoExpressionType { Type = "jsonpath", Version = "rfc9535" },
                                        Extensions = new Dictionary<string, JsonElement> { ["x-sel"] = extElement }
                                    },
                                    ["simpleSel"] = new ArazzoSelector
                                    {
                                        Context = "$response.body",
                                        Selector = "/id",
                                        Type = "jsonpointer"
                                    }
                                },
                                Extensions = new Dictionary<string, JsonElement> { ["x-step"] = extElement }
                            },
                            new ArazzoStep
                            {
                                StepId = "streamStep",
                                ChannelPath = "/pets/AdoptedChannel",
                                Action = "receive",
                                DependsOn = new List<string> { "createPet" }
                            },
                            new ArazzoStep
                            {
                                StepId = "subWorkflowStep",
                                WorkflowId = "setupWorkflow",
                                DependsOn = new List<string> { "streamStep" }
                            }
                        },
                        Outputs = new Dictionary<string, string>
                        {
                            ["adoptedPetId"] = "$steps.createPet.outputs.petId"
                        },
                        Extensions = new Dictionary<string, JsonElement> { ["x-wf"] = extElement }
                    }
                },
                Extensions = new Dictionary<string, JsonElement> { ["x-root"] = extElement }
            };

            Assert.True(doc.Workflows[1].Parameters![0].IsReusable);
            Assert.False(doc.Workflows[1].Steps[0].Parameters![0].IsReusable);
            Assert.True(doc.Workflows[1].SuccessActions![0].IsReusable);
            Assert.False(doc.Workflows[1].Steps[0].OnSuccess![0].IsReusable);
            Assert.True(doc.Workflows[1].FailureActions![0].IsReusable);
            Assert.False(doc.Workflows[1].Steps[0].OnFailure![0].IsReusable);

            var json = ArazzoEmitter.EmitJson(doc);
            var parsedFromJson = ArazzoParser.ParseYamlOrJson(json);
            Assert.Equal("1.1.0", parsedFromJson.Arazzo);
            Assert.Equal("PetstoreWorkflows", parsedFromJson.Info.Title);
            Assert.Equal(2, parsedFromJson.Workflows.Count);
            Assert.Equal(3, parsedFromJson.Workflows[1].Steps.Count);

            var yaml = ArazzoEmitter.EmitYaml(doc);
            var parsedFromYaml = ArazzoParser.ParseYamlOrJson(yaml);
            Assert.Equal("1.1.0", parsedFromYaml.Arazzo);
            Assert.Equal("PetstoreWorkflows", parsedFromYaml.Info.Title);
            Assert.Equal(2, parsedFromYaml.Workflows.Count);
        }

        [Fact]
        public void CriterionTypeAndStepOutput_EqualityAndConverters_WorkCorrectly()
        {
            ArazzoCriterionType ct1 = "jsonpath";
            ArazzoCriterionType ct2 = new ArazzoCriterionType("JSONPATH");
            ArazzoCriterionType ct3 = new ArazzoExpressionType { Type = "jsonpath", Version = "rfc9535" };
            ArazzoCriterionType ctNullInit = new ArazzoCriterionType((string)null!);
            ArazzoCriterionType ctNullObj = new ArazzoCriterionType((ArazzoExpressionType)null!);

            Assert.Equal("simple", ctNullInit.Type);
            Assert.Equal("simple", ctNullObj.Type);
            Assert.True(ct1.Equals(ct2));
            Assert.True(ct1.Equals((object)ct2));
            Assert.False(ct1.Equals((object)ct3));
            Assert.False(ct1.Equals((ArazzoCriterionType?)null));
            Assert.False(ct1.Equals("notACriterionType"));
            Assert.False(ct1.Equals(ct3));
            Assert.False(ct1.Equals(new ArazzoCriterionType("xpath")));
            var ctNoVerObj = new ArazzoCriterionType(new ArazzoExpressionType { Type = "jsonpath", Version = null });
            Assert.Equal(ct1.GetHashCode(), ctNoVerObj.GetHashCode());
            Assert.Equal(ct3.GetHashCode(), new ArazzoCriterionType(new ArazzoExpressionType { Type = "jsonpath", Version = "rfc9535" }).GetHashCode());
            Assert.Equal(ct1.GetHashCode(), ct2.GetHashCode());
            Assert.Equal("jsonpath", ct1.ToString());
            Assert.Equal("jsonpath:rfc9535", ct3.ToString());

            ArazzoStepOutput so1 = "$response.body#/id";
            ArazzoStepOutput so2 = new ArazzoStepOutput("$response.body#/id");
            ArazzoStepOutput soNullStr = new ArazzoStepOutput((string)null!);
            ArazzoStepOutput soNullSel = new ArazzoStepOutput((ArazzoSelector)null!);
            ArazzoStepOutput so3 = new ArazzoSelector { Context = "$response.body", Selector = "$.id", Type = "jsonpath" };
            ArazzoStepOutput so4 = new ArazzoSelector { Context = "$response.body", Selector = "$.id", Type = "jsonpath" };
            ArazzoStepOutput so5 = new ArazzoSelector { Context = null, Selector = "$.other", Type = "xpath" };
            ArazzoStepOutput so6 = new ArazzoSelector { Context = "$response.body", Selector = "$.id", Type = "xpath" };
            ArazzoStepOutput so7 = new ArazzoSelector { Context = "$response.body", Selector = "$.diff", Type = "jsonpath" };

            Assert.Equal(string.Empty, soNullStr.Expression);
            Assert.NotNull(soNullSel.Selector);
            Assert.True(so1.Equals(so2));
            Assert.True(so1.Equals((object)so2));
            Assert.False(so1.Equals((object)so3));
            Assert.True(so3.Equals(so4));
            Assert.False(so3.Equals(so5));
            Assert.False(so3.Equals(so6));
            Assert.False(so3.Equals(so7));
            Assert.False(so1.Equals(so3));
            Assert.False(so3.Equals(so1));
            Assert.False(so1.Equals((ArazzoStepOutput?)null));
            Assert.False(so1.Equals("other"));
            Assert.Equal(so1.GetHashCode(), so2.GetHashCode());
            Assert.Equal(so3.GetHashCode(), so4.GetHashCode());
            Assert.Equal("$response.body#/id", so1.ToString());
            Assert.Equal("$response.body#$.id", so3.ToString());
            Assert.Equal("$response.body#$.other", so5.ToString());

            // Test Converter error and null branches
            var ctConverter = new ArazzoCriterionTypeJsonConverter();
            var nullReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes("null"));
            nullReader.Read();
            Assert.Null(ctConverter.Read(ref nullReader, typeof(ArazzoCriterionType), new JsonSerializerOptions()));

            Assert.Throws<JsonException>(() =>
            {
                var numReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes("123"));
                numReader.Read();
                ctConverter.Read(ref numReader, typeof(ArazzoCriterionType), new JsonSerializerOptions());
            });

            var emptyObjReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes("{}"));
            emptyObjReader.Read();
            var emptyCt = ctConverter.Read(ref emptyObjReader, typeof(ArazzoCriterionType), new JsonSerializerOptions());
            Assert.Equal("simple", emptyCt!.Type);

            var soConverter = new ArazzoStepOutputJsonConverter();
            var nullSoReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes("null"));
            nullSoReader.Read();
            Assert.Null(soConverter.Read(ref nullSoReader, typeof(ArazzoStepOutput), new JsonSerializerOptions()));

            var emptySoObjReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(@"{""type"":{}}"));
            emptySoObjReader.Read();
            var emptySo = soConverter.Read(ref emptySoObjReader, typeof(ArazzoStepOutput), new JsonSerializerOptions());
            Assert.Equal("jsonpath", emptySo!.Selector!.Type!.Type);

            Assert.Throws<JsonException>(() =>
            {
                var numSoReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes("123"));
                numSoReader.Read();
                soConverter.Read(ref numSoReader, typeof(ArazzoStepOutput), new JsonSerializerOptions());
            });
        }

        [Fact]
        public void Validate_AllErrorConditions_ThrowCddException()
        {
            Assert.Throws<ArgumentException>(() => ArazzoParser.ParseYamlOrJson(""));
            Assert.Throws<ArgumentException>(() => ArazzoParser.ParseJson(""));
            Assert.Throws<CddException>(() => ArazzoParser.ParseJson("{invalid json"));
            Assert.Throws<CddException>(() => ArazzoParser.ParseJson("null"));
            Assert.Throws<ArgumentNullException>(() => ArazzoParser.Validate(null!));
            Assert.Throws<ArgumentNullException>(() => ArazzoEmitter.EmitJson(null!));
            Assert.Throws<ArgumentNullException>(() => ArazzoEmitter.EmitYaml(null!));

            // Bad version
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument { Arazzo = "" }));
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument { Arazzo = "2.0.0" }));

            // Missing info
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "", Version = "1.0" }
            }));
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "" }
            }));

            // Empty sourceDescriptions
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription>()
            }));

            // Invalid sourceDescription name
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "bad name!", Url = "http://x" } }
            }));

            // Duplicate sourceDescription name
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription>
                {
                    new ArazzoSourceDescription { Name = "s1", Url = "http://x" },
                    new ArazzoSourceDescription { Name = "s1", Url = "http://y" }
                }
            }));

            // Empty sourceDescription URL
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "" } }
            }));

            // Invalid sourceDescription type
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x", Type = "soap" } }
            }));

            // Empty workflows
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow>()
            }));

            // Empty workflowId
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow> { new ArazzoWorkflow { WorkflowId = "" } }
            }));

            // Duplicate workflowId
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow { WorkflowId = "w1", Steps = new List<ArazzoStep> { new ArazzoStep { StepId = "s1", OperationId = "op1" } } },
                    new ArazzoWorkflow { WorkflowId = "w1", Steps = new List<ArazzoStep> { new ArazzoStep { StepId = "s1", OperationId = "op1" } } }
                }
            }));

            // Empty steps
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow> { new ArazzoWorkflow { WorkflowId = "w1", Steps = new List<ArazzoStep>() } }
            }));

            // Empty stepId
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow { WorkflowId = "w1", Steps = new List<ArazzoStep> { new ArazzoStep { StepId = "", OperationId = "op" } } }
                }
            }));

            // Duplicate stepId
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow
                    {
                        WorkflowId = "w1",
                        Steps = new List<ArazzoStep>
                        {
                            new ArazzoStep { StepId = "s1", OperationId = "op1" },
                            new ArazzoStep { StepId = "s1", OperationId = "op2" }
                        }
                    }
                }
            }));

            // Step with 0 targets or 2 targets
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow { WorkflowId = "w1", Steps = new List<ArazzoStep> { new ArazzoStep { StepId = "s1" } } }
                }
            }));

            // Invalid step action
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow { WorkflowId = "w1", Steps = new List<ArazzoStep> { new ArazzoStep { StepId = "s1", OperationId = "op", Action = "invalid" } } }
                }
            }));

            // Unknown step dependency
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow
                    {
                        WorkflowId = "w1",
                        Steps = new List<ArazzoStep>
                        {
                            new ArazzoStep { StepId = "s1", OperationId = "op", DependsOn = new List<string> { "unknownStep" } }
                        }
                    }
                }
            }));

            // Step dependency cycle
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow
                    {
                        WorkflowId = "w1",
                        Steps = new List<ArazzoStep>
                        {
                            new ArazzoStep { StepId = "s1", OperationId = "op1", DependsOn = new List<string> { "s2" } },
                            new ArazzoStep { StepId = "s2", OperationId = "op2", DependsOn = new List<string> { "s1" } }
                        }
                    }
                }
            }));

            // Unknown workflow dependency
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow
                    {
                        WorkflowId = "w1",
                        DependsOn = new List<string> { "missingWf" },
                        Steps = new List<ArazzoStep> { new ArazzoStep { StepId = "s1", OperationId = "op1" } }
                    }
                }
            }));

            // Workflow dependency cycle
            Assert.Throws<CddException>(() => ArazzoParser.Validate(new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "T", Version = "1.0" },
                SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s1", Url = "http://x" } },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow
                    {
                        WorkflowId = "w1",
                        DependsOn = new List<string> { "w2" },
                        Steps = new List<ArazzoStep> { new ArazzoStep { StepId = "s1", OperationId = "op1" } }
                    },
                    new ArazzoWorkflow
                    {
                        WorkflowId = "w2",
                        DependsOn = new List<string> { "w1" },
                        Steps = new List<ArazzoStep> { new ArazzoStep { StepId = "s1", OperationId = "op2" } }
                    }
                }
            }));
        }

        [Fact]
        public void YamlConverter_EdgeCases_HandleCommentsQuotesMultilineAndArrays()
        {
            var yaml = @"---
# Full YAML comment
arazzo: '1.0.1'
info:
  title: ""Quoted ""Title"""" # trailing comment
  summary: 'Single ''quoted'' summary'
  description: |
    Line one
    Line two
  version: 1.0.0
  x-empty: null
  x-tilde: ~
sourceDescriptions:
  -
    name: api1
    url: https://api.example.com
    x-bool-true: true
    x-bool-false: false
    x-int: 42
    x-float: 3.14
    x-inline-arr: [1, 2]
workflows:
  - workflowId: wf1
    dependsOn:
      - $sourceDescriptions.api1.init
    steps:
      - stepId: s1
        operationId: op1
...
";
            var doc = ArazzoParser.ParseYamlOrJson(yaml);
            Assert.Equal("1.0.1", doc.Arazzo);
            Assert.Contains("Quoted", doc.Info.Title);
            Assert.Equal("Single 'quoted' summary", doc.Info.Summary);
            Assert.Contains("Line one", doc.Info.Description);

            doc.Info.Summary = "";
            doc.Info.Description = "true";
            doc.Self = "null";
            doc.Extensions = new Dictionary<string, JsonElement>
            {
                ["x-null"] = JsonDocument.Parse("null").RootElement.Clone(),
                ["x-newline"] = JsonDocument.Parse(@"""line1\nline2""").RootElement.Clone(),
                ["x-numstr"] = JsonDocument.Parse(@"""123""").RootElement.Clone()
            };
            var emittedYaml = ArazzoEmitter.EmitYaml(doc);
            Assert.Contains("x-null: null", emittedYaml);

            var emptyNode = ArazzoParser.ConvertYamlToJsonNode(@"# only comment
---
...");
            Assert.NotNull(emptyNode);
            var eofMultiline = ArazzoParser.ConvertYamlToJsonNode("desc: |\n  line1\n  line2");
            Assert.NotNull(eofMultiline);
        }
    }
}
