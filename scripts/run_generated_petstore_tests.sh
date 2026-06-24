#!/bin/bash
set -e

run_tests() {
    local spec_file=$1
    local label=$2
    local port=$3

    echo "================================================="
    echo "Running $label Petstore generated server test..."
    echo "================================================="

    echo "Generating server for $label..."
    rm -rf ../cdd-csharp-generated-server
    local abs_spec_path
    abs_spec_path=$(cd "$(dirname "$spec_file")" && pwd)/$(basename "$spec_file")

    dotnet run --project src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -f net10.0 -- from_openapi to_server -i "$abs_spec_path" -o ../cdd-csharp-generated-server --tests

    echo "Starting generated petstore server on port $port..."
    (cd ../cdd-csharp-generated-server/src/GeneratedProject && dotnet run -f net10.0 --urls "http://127.0.0.1:$port" --ephemeral) > server_$port.log 2>&1 &
    SERVER_PID=$!

    echo "Waiting for generated server to be ready..."
    local ready=false
    for i in {1..30}; do
        if curl -s "http://127.0.0.1:$port/" >/dev/null 2>&1; then
            echo "Generated server is ready!"
            ready=true
            break
        fi
        sleep 2
    done

    if [ "$ready" = false ]; then
        echo "Server failed to start. Logs:"
        cat ../cdd-csharp-generated-server/src/GeneratedProject/server_$port.log
        kill $SERVER_PID || true
        exit 1
    fi

    echo "Generating client for $label..."
    rm -rf ../cdd-csharp-client-gen
    dotnet run --project src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -f net10.0 -- from_openapi to_sdk -i "$abs_spec_path" -o ../cdd-csharp-client-gen --tests

    echo "Patching client test URL to use generated server port..."
    if [ "$(uname)" = "Darwin" ]; then
        sed -i '' "s|new Uri(\".*\")|new Uri(\"http://127.0.0.1:$port/\")|g" ../cdd-csharp-client-gen/tests/GeneratedProject.Tests/IntegrationTests.cs
    else
        sed -i "s|new Uri(\".*\")|new Uri(\"http://127.0.0.1:$port/\")|g" ../cdd-csharp-client-gen/tests/GeneratedProject.Tests/IntegrationTests.cs
    fi

    echo "Running integration tests against generated server for $label..."
    (cd ../cdd-csharp-client-gen && dotnet test GeneratedProject.sln) || (kill $SERVER_PID && exit 1)

    echo "Cleaning up generated server..."
    kill $SERVER_PID || true
}

run_tests "../petstore.json" "Swagger 2.0 (Generated)" "8081"
run_tests "../petstore_oas3.json" "OpenAPI 3.2.0 (Generated)" "8082"

echo "Generated Petstore tests completed successfully!"
