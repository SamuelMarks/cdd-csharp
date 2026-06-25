#!/bin/bash
set -e

# Download petstore.json and petstore_oas3.json if they don't exist
if [ ! -f "../petstore.json" ]; then
    echo "Downloading petstore.json..."
    curl -sL "https://raw.githubusercontent.com/OAI/OpenAPI-Specification/main/examples/v2.0/json/petstore.json" -o "../petstore.json"
fi
if [ ! -f "../petstore_oas3.json" ]; then
    echo "Downloading petstore_oas3.json..."
    curl -sL "https://raw.githubusercontent.com/OAI/OpenAPI-Specification/main/examples/v3.0/petstore.json" -o "../petstore_oas3.json"
fi

# Function to run petstore integration tests
run_tests() {
    local spec_file=$1
    local base_path=$2
    local label=$3

    echo "================================================="
    echo "Running $label Petstore test..."
    echo "================================================="

    echo "Starting petstore server via docker (Base Path: $base_path)..."
    docker rm -f petstore_server >/dev/null 2>&1 || true
    if command -v docker >/dev/null 2>&1 && docker info >/dev/null 2>&1; then
        docker run -d -p 8080:8080 -e SWAGGER_HOST="http://localhost:8080" -e SWAGGER_BASE_PATH="$base_path" --name petstore_server swaggerapi/petstore >/dev/null || echo "Docker run failed, tests may fail"
        # Wait for the server to be ready
        echo "Waiting for petstore server to be ready..."
        for i in {1..30}; do
            if curl -s "http://localhost:8080/" >/dev/null 2>&1; then
                echo "Petstore server is ready! Waiting a few more seconds for endpoints to map..."
                sleep 3
                break
            fi
            sleep 2
        done
    else
        echo "Warning: docker is not installed or daemon is not running. Tests relying on localhost:8080 may fail."
    fi

    echo "Generating client for $label..."
    rm -rf ../cdd-csharp-client
    # Make sure we use a robust path to the spec file
    local abs_spec_path
    abs_spec_path=$(cd "$(dirname "$spec_file")" && pwd)/$(basename "$spec_file")

    dotnet run --project src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -f net10.0 -- from_openapi to_sdk -i "$abs_spec_path" -o ../cdd-csharp-client --tests

    echo "Running integration tests for $label..."
    (cd ../cdd-csharp-client && dotnet test GeneratedProject.sln)

    echo "Cleaning up docker server..."
    docker rm -f petstore_server >/dev/null 2>&1 || true
}

run_tests "../petstore.json" "/v2" "Swagger 2.0"
run_tests "../petstore_oas3.json" "/api/v3" "OpenAPI 3.2.0"

echo "Petstore tests completed successfully!"
