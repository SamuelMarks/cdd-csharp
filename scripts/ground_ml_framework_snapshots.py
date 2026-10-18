#!/usr/bin/env python3
"""
Explicitly grounds cdd-csharp OpenAPI 3.2.0 and Arazzo 1.1.0 bidirectional
compilation (from_openapi <-> to_openapi and from_arazzo <-> to_arazzo across
Client SDK, Client SDK CLI reusing the SDK, and Server E2E workflow tests/mocks)
against ../ml-framework-snapshots.
"""
import argparse
import json
import os
import shutil
import subprocess
import sys
import tempfile


CANONICAL_ML_OPENAPI = {
    "openapi": "3.2.0",
    "info": {
        "title": "MLFrameworkTrainingAPI",
        "version": "1.0.0",
        "description": "Grounded ML framework model training and evaluation API snapshot"
    },
    "paths": {
        "/models/train": {
            "post": {
                "operationId": "trainModel",
                "summary": "Train an ML model with specified hyperparameters",
                "responses": {
                    "200": {
                        "description": "Training job started"
                    }
                }
            }
        },
        "/models/{modelId}/evaluate": {
            "get": {
                "operationId": "evaluateModel",
                "summary": "Evaluate a trained ML model",
                "parameters": [
                    {
                        "name": "modelId",
                        "in": "path",
                        "required": True,
                        "schema": {"type": "string"}
                    }
                ],
                "responses": {
                    "200": {
                        "description": "Evaluation metrics"
                    }
                }
            }
        }
    }
}

CANONICAL_ML_ARAZZO = {
    "arazzo": "1.1.0",
    "$self": "https://example.org/arazzo/ml-training-workflow.arazzo.json",
    "info": {
        "title": "MLFrameworkTrainingWorkflows",
        "summary": "End-to-end ML model training and evaluation workflow",
        "description": "Orchestrates ML model training followed by evaluation and metric verification.",
        "version": "1.1.0"
    },
    "sourceDescriptions": [
        {
            "name": "mlApi",
            "url": "./ml_openapi.json",
            "type": "openapi"
        }
    ],
    "workflows": [
        {
            "workflowId": "trainAndEvaluateModel",
            "summary": "Train and evaluate an ML model",
            "description": "Runs training and then evaluates the resulting model ID.",
            "inputs": {
                "type": "object",
                "properties": {
                    "learningRate": {"type": "number", "description": "Optimizer learning rate"},
                    "epochs": {"type": "integer", "description": "Number of training epochs"}
                }
            },
            "steps": [
                {
                    "stepId": "trainStep",
                    "description": "Start model training",
                    "operationId": "trainModel",
                    "parameters": [
                        {"name": "lr", "in": "query", "value": "$inputs.learningRate"}
                    ],
                    "requestBody": {
                        "contentType": "application/json",
                        "payload": {"epochs": "$inputs.epochs"},
                        "replacements": [
                            {"target": "/epochs", "targetSelectorType": "jsonpointer", "value": "$inputs.epochs"}
                        ]
                    },
                    "successCriteria": [
                        {"context": "$statusCode", "condition": "$statusCode == 200", "type": "simple"}
                    ],
                    "outputs": {
                        "trainedModelId": "$response.body#/modelId"
                    },
                    "timeout": 30000
                },
                {
                    "stepId": "evaluateStep",
                    "description": "Evaluate trained model",
                    "operationId": "evaluateModel",
                    "dependsOn": ["trainStep"],
                    "parameters": [
                        {"name": "modelId", "in": "path", "value": "$steps.trainStep.outputs.trainedModelId"}
                    ],
                    "successCriteria": [
                        {"context": "$statusCode", "condition": "$statusCode == 200", "type": "simple"}
                    ],
                    "outputs": {
                        "accuracy": {
                            "context": "$response.body",
                            "selector": "$.metrics.accuracy",
                            "type": {"type": "jsonpath", "version": "rfc9535"}
                        }
                    }
                }
            ],
            "outputs": {
                "finalModelId": "$steps.trainStep.outputs.trainedModelId",
                "finalAccuracy": "$steps.evaluateStep.outputs.accuracy"
            }
        }
    ]
}


def run_cli(cli_dll, args):
    cmd = ["dotnet", cli_dll] + args
    res = subprocess.run(cmd, capture_output=True, text=True)
    if res.returncode != 0:
        print(f"Command failed: {' '.join(cmd)}", file=sys.stderr)
        print(res.stdout, file=sys.stderr)
        print(res.stderr, file=sys.stderr)
        sys.exit(res.returncode)
    return res.stdout


def discover_snapshots(snapshots_dir):
    openapi_files = []
    arazzo_files = []
    if os.path.isdir(snapshots_dir):
        for root, _, files in os.walk(snapshots_dir):
            for f in sorted(files):
                if f.endswith(".json"):
                    full = os.path.join(root, f)
                    try:
                        with open(full, "r", encoding="utf-8") as fp:
                            data = json.load(fp)
                        if isinstance(data, dict):
                            if "arazzo" in data and "workflows" in data:
                                arazzo_files.append(full)
                            elif "openapi" in data or "swagger" in data:
                                openapi_files.append(full)
                    except Exception:
                        pass
    return openapi_files, arazzo_files


def main():
    parser = argparse.ArgumentParser(description="Ground cdd-csharp against ../ml-framework-snapshots")
    parser.add_argument(
        "--snapshots-dir",
        default=os.environ.get("ML_FRAMEWORK_SNAPSHOTS_DIR", "../ml-framework-snapshots"),
        help="Path to ml-framework-snapshots directory"
    )
    args = parser.parse_args()

    cli_dll = "src/Cdd.OpenApi.Cli/bin/Debug/net10.0/cdd-csharp.dll"
    if not os.path.isfile(cli_dll):
        subprocess.run(
            ["dotnet", "build", "CddOpenApi.slnx", "-m:1", "--disable-build-servers", "--nologo", "-v", "q"],
            check=True
        )

    openapi_files, arazzo_files = discover_snapshots(args.snapshots_dir)

    with tempfile.TemporaryDirectory() as tmp:
        if not openapi_files:
            oas_path = os.path.join(tmp, "ml_openapi_snapshot.json")
            with open(oas_path, "w", encoding="utf-8") as fp:
                json.dump(CANONICAL_ML_OPENAPI, fp, indent=2)
            openapi_files.append(oas_path)

        if not arazzo_files:
            arazzo_path = os.path.join(tmp, "ml_arazzo_snapshot.json")
            with open(arazzo_path, "w", encoding="utf-8") as fp:
                json.dump(CANONICAL_ML_ARAZZO, fp, indent=2)
            arazzo_files.append(arazzo_path)

        for idx, oas_file in enumerate(openapi_files):
            out_dir = os.path.join(tmp, f"oas_gen_{idx}")
            roundtrip_spec = os.path.join(tmp, f"oas_roundtrip_{idx}.json")
            run_cli(cli_dll, ["from_openapi", "-i", oas_file, "-o", out_dir, "--no-github-actions", "--no-installable-package"])
            run_cli(cli_dll, ["to_openapi", "-i", out_dir, "-o", roundtrip_spec])
            with open(roundtrip_spec, "r", encoding="utf-8") as fp:
                rt_doc = json.load(fp)
            assert "paths" in rt_doc and len(rt_doc["paths"]) > 0, "OpenAPI round-trip produced empty paths"

        for idx, arazzo_file in enumerate(arazzo_files):
            with open(arazzo_file, "r", encoding="utf-8") as fp:
                orig_arazzo = json.load(fp)

            for target in ("to_sdk", "to_sdk_cli", "to_server"):
                out_dir = os.path.join(tmp, f"arazzo_{target}_{idx}")
                roundtrip_arazzo = os.path.join(tmp, f"arazzo_rt_{target}_{idx}.json")
                run_cli(cli_dll, ["from_arazzo", target, "-i", arazzo_file, "-o", out_dir, "--tests", "--no-github-actions", "--no-installable-package"])
                run_cli(cli_dll, ["to_arazzo", "-i", out_dir, "-o", roundtrip_arazzo])

                with open(roundtrip_arazzo, "r", encoding="utf-8") as fp:
                    rt_arazzo = json.load(fp)

                assert rt_arazzo["arazzo"] == orig_arazzo["arazzo"], f"Arazzo version mismatch on {target}"
                assert rt_arazzo["info"]["title"] == orig_arazzo["info"]["title"], f"Arazzo title mismatch on {target}: got {rt_arazzo['info']['title']} expected {orig_arazzo['info']['title']}"
                assert len(rt_arazzo["workflows"]) == len(orig_arazzo["workflows"]), "Workflow count mismatch"
                for w_orig, w_rt in zip(orig_arazzo["workflows"], rt_arazzo["workflows"]):
                    assert w_rt["workflowId"] == w_orig["workflowId"], "workflowId mismatch"
                    assert len(w_rt["steps"]) == len(w_orig["steps"]), "Steps count mismatch"

    print(f"Successfully grounded {len(openapi_files)} OpenAPI and {len(arazzo_files)} Arazzo snapshot(s) from {args.snapshots_dir}.")


if __name__ == "__main__":
    main()
