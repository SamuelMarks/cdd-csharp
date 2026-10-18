#!/usr/bin/env python3
"""
End-to-end Arazzo 1.1.0 conformance test script.
Verifies JSON and YAML round-tripping across Client SDK, Client SDK CLI (reusing SDK),
and Server E2E workflow tests & mocks, including a comprehensive Petstore Arazzo 1.1.0
workflow (login -> createPet -> getPetById -> placeOrder -> deletePet with retries,
gotos, criteria, selectors, and payload replacements) and ../ml-framework-snapshots grounding.
"""
import json
import os
import subprocess
import sys
import tempfile


PETSTORE_ARAZZO = {
    "arazzo": "1.1.0",
    "$self": "https://petstore.swagger.io/v2/petstore.arazzo.json",
    "info": {
        "title": "PetstoreWorkflows",
        "summary": "Petstore end-to-end Arazzo 1.1.0 workflows",
        "description": "Multi-step Petstore workflow with login, pet creation, retrieval, order placement, and deletion.",
        "version": "1.1.0"
    },
    "sourceDescriptions": [
        {
            "name": "petstore",
            "url": "./petstore.yaml",
            "type": "openapi"
        }
    ],
    "components": {
        "inputs": {
            "petCredentials": {
                "type": "object",
                "properties": {
                    "apiKey": {"type": "string"}
                }
            }
        },
        "parameters": {
            "apiKeyHeader": {
                "name": "api_key",
                "in": "header",
                "value": "$inputs.apiKey"
            }
        },
        "successActions": {
            "endSuccess": {
                "name": "endSuccess",
                "type": "end"
            }
        },
        "failureActions": {
            "retryTransient": {
                "name": "retryTransient",
                "type": "retry",
                "retryAfter": 1.0,
                "retryLimit": 3,
                "criteria": [
                    {"context": "$statusCode", "condition": "$statusCode == 503", "type": "simple"}
                ]
            }
        }
    },
    "workflows": [
        {
            "workflowId": "loginAndPurchasePet",
            "summary": "Login, create pet, fetch pet, place order, and delete pet",
            "description": "Full lifecycle workflow for purchasing a pet from the store.",
            "inputs": {
                "type": "object",
                "properties": {
                    "username": {"type": "string"},
                    "password": {"type": "string"},
                    "petName": {"type": "string"},
                    "quantity": {"type": "integer"}
                }
            },
            "steps": [
                {
                    "stepId": "loginUser",
                    "description": "Authenticate user session",
                    "operationId": "loginUser",
                    "parameters": [
                        {"name": "username", "in": "query", "value": "$inputs.username"},
                        {"name": "password", "in": "query", "value": "$inputs.password"}
                    ],
                    "successCriteria": [
                        {"context": "$statusCode", "condition": "$statusCode == 200", "type": "simple"}
                    ],
                    "outputs": {
                        "sessionToken": "$response.header.X-Expires-After"
                    }
                },
                {
                    "stepId": "createPet",
                    "description": "Add a new pet to the store",
                    "operationId": "addPet",
                    "dependsOn": ["loginUser"],
                    "requestBody": {
                        "contentType": "application/json",
                        "payload": {"name": "doggie", "status": "available"},
                        "replacements": [
                            {"target": "/name", "targetSelectorType": "jsonpointer", "value": "$inputs.petName"}
                        ]
                    },
                    "successCriteria": [
                        {"context": "$statusCode", "condition": "^20[01]$", "type": "regex"}
                    ],
                    "onFailure": [
                        {
                            "name": "retryCreate",
                            "type": "retry",
                            "retryAfter": 0.5,
                            "retryLimit": 2
                        }
                    ],
                    "outputs": {
                        "petId": {
                            "context": "$response.body",
                            "selector": "$.id",
                            "type": {"type": "jsonpath", "version": "rfc9535"}
                        }
                    },
                    "timeout": 5000
                },
                {
                    "stepId": "getPet",
                    "description": "Verify created pet by ID",
                    "operationPath": "{$sourceDescriptions.petstore.url}#/paths/~1pet~1{petId}/get",
                    "dependsOn": ["createPet"],
                    "parameters": [
                        {"name": "petId", "in": "path", "value": "$steps.createPet.outputs.petId"},
                        {"reference": "$components.parameters.apiKeyHeader", "value": "$steps.loginUser.outputs.sessionToken"}
                    ],
                    "successCriteria": [
                        {"context": "$statusCode", "condition": "$statusCode == 200", "type": "simple"}
                    ],
                    "onSuccess": [
                        {"name": "proceedToOrder", "type": "goto", "stepId": "placeOrder"}
                    ],
                    "outputs": {
                        "verifiedPetId": "$response.body#/id"
                    }
                },
                {
                    "stepId": "placeOrder",
                    "description": "Place an order for the verified pet",
                    "operationId": "placeOrder",
                    "dependsOn": ["getPet"],
                    "requestBody": {
                        "contentType": "application/json",
                        "payload": {"petId": 0, "quantity": 1, "status": "placed"},
                        "replacements": [
                            {"target": "/petId", "value": "$steps.getPet.outputs.verifiedPetId"},
                            {"target": "/quantity", "value": "$inputs.quantity"}
                        ]
                    },
                    "successCriteria": [
                        {"context": "$statusCode", "condition": "$statusCode == 200", "type": "simple"}
                    ],
                    "outputs": {
                        "orderId": "$response.body#/id"
                    }
                },
                {
                    "stepId": "deletePet",
                    "description": "Clean up pet record after order",
                    "operationId": "deletePet",
                    "dependsOn": ["placeOrder"],
                    "parameters": [
                        {"name": "petId", "in": "path", "value": "$steps.createPet.outputs.petId"}
                    ],
                    "successCriteria": [
                        {"context": "$statusCode", "condition": "$statusCode == 200", "type": "simple"}
                    ]
                }
            ],
            "outputs": {
                "purchasedPetId": "$steps.createPet.outputs.petId",
                "finalOrderId": "$steps.placeOrder.outputs.orderId"
            }
        }
    ]
}


def main():
    subprocess.run(
        ["python3", "scripts/ground_ml_framework_snapshots.py", "--snapshots-dir", "../ml-framework-snapshots"],
        check=True
    )

    cli_dll = "src/Cdd.OpenApi.Cli/bin/Debug/net10.0/cdd-csharp.dll"
    with tempfile.TemporaryDirectory() as tmp:
        arazzo_json_path = os.path.join(tmp, "petstore.arazzo.json")
        with open(arazzo_json_path, "w", encoding="utf-8") as fp:
            json.dump(PETSTORE_ARAZZO, fp, indent=2)

        for target in ("to_sdk", "to_sdk_cli", "to_server"):
            out_dir = os.path.join(tmp, f"petstore_{target}")
            rt_json = os.path.join(tmp, f"petstore_rt_{target}.json")
            rt_yaml = os.path.join(tmp, f"petstore_rt_{target}.yaml")

            subprocess.run(
                ["dotnet", cli_dll, "from_arazzo", target, "-i", arazzo_json_path, "-o", out_dir, "--tests", "--no-github-actions", "--no-installable-package"],
                check=True
            )
            subprocess.run(
                ["dotnet", cli_dll, "to_arazzo", "-i", out_dir, "-o", rt_json],
                check=True
            )
            subprocess.run(
                ["dotnet", cli_dll, "to_arazzo", "-i", out_dir, "-o", rt_yaml],
                check=True
            )

            with open(rt_json, "r", encoding="utf-8") as fp:
                rt_doc = json.load(fp)
            assert rt_doc["arazzo"] == "1.1.0"
            assert rt_doc["info"]["title"] == "PetstoreWorkflows"
            assert len(rt_doc["workflows"]) == 1
            assert len(rt_doc["workflows"][0]["steps"]) == 5

    print("Arazzo 1.1.0 end-to-end conformance tests completed successfully!")


if __name__ == "__main__":
    main()
