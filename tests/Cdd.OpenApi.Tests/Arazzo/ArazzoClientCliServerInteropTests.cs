// <copyright file="ArazzoClientCliServerInteropTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cdd.OpenApi.Arazzo;
using Cdd.OpenApi.Cli;
using Cdd.OpenApi.Ir;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Models.Arazzo;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

#pragma warning disable CA1416 // Validate platform compatibility

namespace Cdd.OpenApi.Tests.Arazzo
{
    public class ArazzoClientCliServerInteropTests
    {
        private static ArazzoDocument CreateSampleArazzo()
        {
            return new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Self = "https://example.com/arazzo.json",
                Info = new ArazzoInfo
                {
                    Title = "OrderWorkflows",
                    Summary = "Order orchestration",
                    Description = "Order processing steps",
                    Version = "2.0.0"
                },
                SourceDescriptions = new List<ArazzoSourceDescription>
                {
                    new ArazzoSourceDescription { Name = "ordersApi", Url = "./orders.json", Type = "openapi" }
                },
                Components = new ArazzoComponents
                {
                    Inputs = new Dictionary<string, OpenApiSchema>
                    {
                        ["sharedInput"] = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["tenantId"] = new OpenApiSchema { Type = "string" }
                            }
                        }
                    }
                },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow
                    {
                        WorkflowId = "prepareCart",
                        Steps = new List<ArazzoStep>
                        {
                            new ArazzoStep { StepId = "initCart", OperationId = "createCart" }
                        }
                    },
                    new ArazzoWorkflow
                    {
                        WorkflowId = "placeOrder",
                        Summary = "Place an order",
                        Description = "Places an order after preparing cart",
                        DependsOn = new List<string> { "prepareCart" },
                        Inputs = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["itemCount"] = new OpenApiSchema { Type = "integer" },
                                ["orderId"] = new OpenApiSchema { Type = "integer", Format = "int64" },
                                ["express"] = new OpenApiSchema { Type = "boolean" },
                                ["discount"] = new OpenApiSchema { Type = "number", Format = "double" },
                                ["coupon"] = new OpenApiSchema { Type = "string" }
                            }
                        },
                        Steps = new List<ArazzoStep>
                        {
                            new ArazzoStep
                            {
                                StepId = "callPrepare",
                                WorkflowId = "prepareCart",
                                Timeout = 1000
                            },
                            new ArazzoStep
                            {
                                StepId = "submitOrder",
                                OperationId = "submitOrderOp",
                                Outputs = new Dictionary<string, ArazzoStepOutput>
                                {
                                    ["receiptId"] = "$response.body#/receiptId"
                                }
                            }
                        },
                        Outputs = new Dictionary<string, string>
                        {
                            ["finalReceipt"] = "$steps.submitOrder.outputs.receiptId"
                        }
                    }
                }
            };
        }

        [Fact]
        public void IrService_FromArazzoAndToArazzo_RoundTripsCleanly()
        {
            var doc = CreateSampleArazzo();
            var ir = IrService.FromArazzo(doc);
            Assert.Equal("OrderWorkflows", ir.Title);
            Assert.Equal("2.0.0", ir.Version);
            Assert.Single(ir.SourceDescriptions);
            Assert.Equal(2, ir.Workflows.Count);

            var back = ir.ToArazzo();
            Assert.Equal("OrderWorkflows", back.Info.Title);
            Assert.Equal(2, back.Workflows.Count);

            var emptyIr = new IrService { Title = "Empty", Version = "1.0" };
            var emptyArazzo = emptyIr.ToArazzo();
            Assert.Single(emptyArazzo.SourceDescriptions);
        }

        [Fact]
        public void ClientsCliAndServers_EmitAndParse_RoundTripAndHandleHandwrittenCode()
        {
            Assert.Throws<ArgumentNullException>(() => Cdd.OpenApi.Arazzo.Clients.Emit.ToWorkflowModels(null!));
            Assert.Throws<ArgumentNullException>(() => Cdd.OpenApi.Arazzo.Clients.Emit.ToWorkflowOrchestrator("Wf", null!));
            Assert.Throws<ArgumentNullException>(() => Cdd.OpenApi.Arazzo.Clients.Parse.ToArazzoDocument(null!));
            Assert.Throws<ArgumentNullException>(() => Cdd.OpenApi.Arazzo.Cli.Emit.ToWorkflowCli("Cli", "Wf", null!));
            Assert.Throws<ArgumentNullException>(() => Cdd.OpenApi.Arazzo.Cli.Parse.ToArazzoDocument(null!));
            Assert.Throws<ArgumentNullException>(() => Cdd.OpenApi.Arazzo.Servers.Emit.ToWorkflowTests("Tests", "Wf", null!));
            Assert.Throws<ArgumentNullException>(() => Cdd.OpenApi.Arazzo.Servers.Emit.ToWorkflowMock("Mock", null!));
            Assert.Throws<ArgumentNullException>(() => Cdd.OpenApi.Arazzo.Servers.Parse.ToArazzoDocument(null!));
            Assert.Throws<ArgumentNullException>(() => ArazzoCodeGenerator.Generate(null!));
            Assert.Throws<ArgumentNullException>(() => ArazzoSpecGenerator.Generate((string)null!));
            Assert.Throws<ArgumentNullException>(() => ArazzoSpecGenerator.Generate((IEnumerable<string>)null!));

            var doc = CreateSampleArazzo();
            var linkedOas = new Dictionary<string, OpenApiDocument>
            {
                ["ordersApi"] = new OpenApiDocument
                {
                    Info = new OpenApiInfo { Title = "Orders", Version = "1.0" },
                    Paths = new OpenApiPaths()
                }
            };

            var generated = ArazzoCodeGenerator.Generate(doc, "Generated", GenerateType.All, tests: true, linkedOas);
            Assert.Contains(generated, g => g.FileName.EndsWith("WorkflowModels.cs"));
            Assert.Contains(generated, g => g.FileName.EndsWith("Workflows.cs"));
            Assert.Contains(generated, g => g.FileName.EndsWith("WorkflowCli.cs"));
            Assert.Contains(generated, g => g.FileName.EndsWith("WorkflowMock.cs"));
            Assert.Contains(generated, g => g.FileName.EndsWith("WorkflowTests.cs"));

            // Round-trip from generated SDK code
            var sdkCode = generated.First(g => g.FileName.EndsWith("Workflows.cs")).Code;
            var sdkRoundTrip = ArazzoSpecGenerator.Generate(sdkCode);
            Assert.Equal(2, sdkRoundTrip.Workflows.Count);

            // Round-trip from generated CLI code
            var cliCode = generated.First(g => g.FileName.EndsWith("WorkflowCli.cs")).Code;
            var cliRoundTrip = ArazzoSpecGenerator.Generate(cliCode);
            Assert.Equal(2, cliRoundTrip.Workflows.Count);

            // Round-trip from generated Server Test code
            var serverTestCode = generated.First(g => g.FileName.EndsWith("WorkflowTests.cs")).Code;
            var serverTree = CSharpSyntaxTree.ParseText(serverTestCode);
            var serverClasses = serverTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            var serverRoundTrip = Cdd.OpenApi.Arazzo.Servers.Parse.ToArazzoDocument(serverClasses);
            Assert.Equal(2, serverRoundTrip.Workflows.Count);

            // Hand-written C# workflow class without <workflow-spec> tag
            var handwritten = @"
public class CheckoutInput
{
    /// <summary>Customer ID.</summary>
    public string? CustomerId { get; set; }
    public System.Collections.Generic.Dictionary<string, string> Values { get; set; } = new();
}
/// <summary>Custom handwritten workflows summary.</summary>
public class CustomWorkflows
{
    public const string ArazzoVersion = ""1.1.0"";
    public const string SelfUri = ""https://handwritten.example/arazzo.json"";

    /// <summary>Handwritten checkout flow.</summary>
    public async System.Threading.Tasks.Task CheckoutAsync(CheckoutInput input)
    {
        await _client.LoginUserAsync();
        await _client.CreateOrderAsync();
    }

    public async System.Threading.Tasks.Task EmptyFlowWorkflowTest()
    {
    }
}
public class CustomWorkflowCli
{
    public static int Main(string[] args)
    {
        switch (args[0])
        {
            case ""manualCliFlow"":
                return 0;
        }
        return 0;
    }
}
";
            var hwDoc = ArazzoSpecGenerator.Generate(handwritten);
            Assert.Equal("https://handwritten.example/arazzo.json", hwDoc.Self);
            Assert.Equal(2, hwDoc.Workflows.Count);
            Assert.Equal("checkout", hwDoc.Workflows[0].WorkflowId);
            Assert.Equal(2, hwDoc.Workflows[0].Steps.Count);
            Assert.Equal("loginUser", hwDoc.Workflows[0].Steps[0].OperationId);

            // CLI-only handwritten fallback
            var cliOnly = @"
public class OnlyWorkflowCli
{
    public static int Main(string[] args)
    {
        switch (args[0])
        {
            case ""cliOnlyFlow"":
                return 0;
        }
        return 0;
    }
}";
            var cliOnlyDoc = ArazzoSpecGenerator.Generate(cliOnly);
            Assert.Single(cliOnlyDoc.Workflows);
            Assert.Equal("cliOnlyFlow", cliOnlyDoc.Workflows[0].WorkflowId);

            // Empty source code fallback
            var emptyDoc = ArazzoSpecGenerator.Generate("   ");
            Assert.Single(emptyDoc.Workflows);
            Assert.Equal("defaultWorkflow", emptyDoc.Workflows[0].WorkflowId);
        }

        [Fact]
        public void CliProgramJsonRpcMcpAndBrowserInterop_ArazzoCommands_ExecuteCleanly()
        {
            var doc = CreateSampleArazzo();
            var arazzoJson = ArazzoEmitter.EmitJson(doc);

            // BrowserInterop
            var sdkJson = BrowserInterop.FromArazzo(arazzoJson, "to_sdk", false);
            Assert.Contains("OrderWorkflowsWorkflows.cs", sdkJson);
            var cliJson = BrowserInterop.FromArazzo(arazzoJson, "to_sdk_cli", false);
            Assert.Contains("OrderWorkflowsWorkflowCli.cs", cliJson);
            var srvJson = BrowserInterop.FromArazzo(arazzoJson, "to_server", true);
            Assert.Contains("OrderWorkflowsWorkflowTests.cs", srvJson);
            var errJson = BrowserInterop.FromArazzo("{bad json");
            Assert.Contains("error", errJson);

            var filesDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(sdkJson)!;
            var wfCode = filesDict.Values.First(v => v.Contains("class OrderWorkflowsWorkflows"));
            var extractedArazzo = BrowserInterop.ToArazzo(wfCode);
            Assert.Contains("1.1.0", extractedArazzo);
            var toArazzoErr = BrowserInterop.ToArazzo(null!);
            Assert.Contains("error", toArazzoErr);

            // CLI commands & CddGenerator.GenerateFromArazzo
            var tempDir = Path.Combine(Path.GetTempPath(), "cdd_arazzo_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            try
            {
                var specFile = Path.Combine(tempDir, "spec.arazzo.json");
                File.WriteAllText(specFile, arazzoJson);

                var outDir = Path.Combine(tempDir, "generated");
                Assert.Equal(0, Program.Main(new[] { "from_arazzo", "--help" }));
                Assert.Equal(0, Program.Main(new[] { "to_arazzo", "--help" }));
                Assert.Equal(1, CddCli.FromArazzo(GenerateType.All, new CddConfig()));
                Assert.Equal(1, CddCli.FromArazzo(GenerateType.All, new CddConfig { InputPath = Path.Combine(tempDir, "missing.json") }));
                Assert.Equal(1, CddCli.ToArazzo("", "out.json"));
                Assert.Equal(1, CddCli.ToArazzo(Path.Combine(tempDir, "missing_dir"), "out.json"));
                Assert.Throws<ArgumentException>(() => CddGenerator.GenerateFromArazzo(new CddConfig()));

                Assert.Equal(0, Program.Main(new[] { "from_arazzo", "to_sdk_cli", "-i", specFile, "-o", outDir, "--no-installable-package", "--no-github-actions", "--tests" }));

                // Directory input mode
                var inDir = Path.Combine(tempDir, "specs_dir");
                Directory.CreateDirectory(inDir);
                File.WriteAllText(Path.Combine(inDir, "spec.yaml"), ArazzoEmitter.EmitYaml(doc));
                CddGenerator.GenerateFromArazzo(new CddConfig { InputDir = inDir, OutputDir = outDir, NoInstallablePackage = true, NoGithubActions = true }, GenerateType.Server);

                var outJsonFile = Path.Combine(tempDir, "roundtrip.arazzo.json");
                var outYamlFile = Path.Combine(tempDir, "roundtrip.arazzo.yaml");
                var singleCsFile = Directory.GetFiles(outDir, "*.cs", SearchOption.AllDirectories).First();
                Assert.Equal(0, Program.Main(new[] { "to_arazzo", "-i", singleCsFile, "-o", outJsonFile }));
                Assert.Equal(0, Program.Main(new[] { "to_arazzo", "-i", outDir, "-o", outYamlFile }));
                Assert.True(File.Exists(outJsonFile));
                Assert.True(File.Exists(outYamlFile));

                // Minimal ArazzoDocument (null optional fields) emission & round-trip
                var minDoc = new ArazzoDocument
                {
                    Arazzo = "1.1.0",
                    Info = new ArazzoInfo { Title = "Min", Version = "1.0" },
                    SourceDescriptions = new List<ArazzoSourceDescription> { new ArazzoSourceDescription { Name = "s", Url = "./s.json" } },
                    Workflows = new List<ArazzoWorkflow>
                    {
                        new ArazzoWorkflow
                        {
                            WorkflowId = "minWf",
                            DependsOn = new List<string> { "$sourceDescriptions.s.init" },
                            Steps = new List<ArazzoStep> { new ArazzoStep { StepId = "s1", WorkflowId = "$sourceDescriptions.s.wf" } }
                        }
                    }
                };
                var minFiles = ArazzoCodeGenerator.Generate(minDoc, "Gen", GenerateType.All, true);
                Assert.NotEmpty(minFiles);

                // Sync --truth arazzo and --truth workflow
                var syncOutDir = Path.Combine(tempDir, "sync_out");
                var syncOutSpec = Path.Combine(tempDir, "sync_arazzo.json");
                Assert.Equal(0, CddCli.Sync("arazzo", specFile, syncOutDir));
                Assert.Equal(0, CddCli.Sync("workflow", syncOutDir, syncOutSpec));

                // MCP cdd_from_arazzo and cdd_to_arazzo
                var mcpInput = string.Join(((char)10).ToString(), new[]
                {
                    @"{""jsonrpc"":""2.0"",""id"":1,""method"":""tools/call"",""params"":{""name"":""cdd_from_arazzo"",""arguments"":{""arazzo"":" + System.Text.Json.JsonSerializer.Serialize(arazzoJson) + "}}}",
                    @"{""jsonrpc"":""2.0"",""id"":2,""method"":""tools/call"",""params"":{""name"":""cdd_to_arazzo"",""arguments"":{""code"":" + System.Text.Json.JsonSerializer.Serialize(wfCode) + "}}}",
                    @"{""jsonrpc"":""2.0"",""id"":3,""method"":""exit""}"
                });
                var origIn = Console.In;
                var origOut = Console.Out;
                try
                {
                    using var sr = new StringReader(mcpInput);
                    using var sw = new StringWriter();
                    Console.SetIn(sr);
                    Console.SetOut(sw);
                    Assert.Equal(0, CddCli.ServeMcp(Array.Empty<string>()));
                    var mcpOut = sw.ToString();
                    Assert.Contains("Generated Arazzo workflow code successfully", mcpOut);
                    Assert.Contains("OrderWorkflows", mcpOut);
                }
                finally
                {
                    Console.SetIn(origIn);
                    Console.SetOut(origOut);
                }
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
