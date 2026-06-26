#!/usr/bin/env python3
import os
import subprocess
import urllib.request
import time
import shutil
import re

def download(url, dest):
    if not os.path.isfile(dest):
        print(f"Downloading {os.path.basename(dest)}...")
        urllib.request.urlretrieve(url, dest)

def wait_for_server(url, timeout=60):
    for i in range(timeout):
        try:
            import urllib.error
            urllib.request.urlopen(url)
            return True
        except urllib.error.HTTPError:
            # Server is up and responding with an HTTP error code (e.g. 401, 404), which is fine
            return True
        except Exception:
            time.sleep(2)
    return False

def run_tests(spec_file, label, port):
    print("=================================================")
    print(f"Running {label} Petstore generated server test...")
    print("=================================================")

    print(f"Generating server for {label}...")
    server_dir = "../cdd-csharp-generated-server"
    if os.path.exists(server_dir):
        shutil.rmtree(server_dir)

    abs_spec_path = os.path.abspath(spec_file)

    subprocess.run(["dotnet", "run", "--project", "src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj", "-f", "net10.0", "--", "from_openapi", "to_server", "-i", abs_spec_path, "-o", server_dir, "--tests"], check=True)

    print(f"Starting generated petstore server on port {port}...")
    log_file = os.path.join(server_dir, "src", "GeneratedProject", f"server_{port}.log")
    with open(log_file, "w") as out:
        server_process = subprocess.Popen(
            ["dotnet", "run", "-f", "net10.0", "--urls", f"http://127.0.0.1:{port}", "--ephemeral"],
            cwd=os.path.join(server_dir, "src", "GeneratedProject"),
            stdout=out,
            stderr=subprocess.STDOUT,
            preexec_fn=os.setsid
        )

    print("Waiting for generated server to be ready...")
    if wait_for_server(f"http://127.0.0.1:{port}/", timeout=30):
        print("Generated server is ready!")
    else:
        print("Server failed to start. Logs:")
        with open(log_file, "r") as f:
            print(f.read())
        try:
            os.killpg(os.getpgid(server_process.pid), 9)
        except ProcessLookupError:
            pass
        raise Exception("Server failed to start")

    print(f"Generating client for {label}...")
    client_dir = "../cdd-csharp-client-gen"
    if os.path.exists(client_dir):
        shutil.rmtree(client_dir)

    subprocess.run(["dotnet", "run", "--project", "src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj", "-f", "net10.0", "--", "from_openapi", "to_sdk", "-i", abs_spec_path, "-o", client_dir, "--tests"], check=True)

    print("Patching client test URL to use generated server port...")
    test_file = os.path.join(client_dir, "tests", "GeneratedProject.Tests", "IntegrationTests.cs")
    with open(test_file, "r", encoding="utf-8") as f:
        content = f.read()
    content = re.sub(r'new Uri\(".*"\)', f'new Uri("http://127.0.0.1:{port}/")', content)
    with open(test_file, "w", encoding="utf-8") as f:
        f.write(content)

    print(f"Running integration tests against generated server for {label}...")
    try:
        subprocess.run(["dotnet", "test", "GeneratedProject.sln"], cwd=client_dir, check=True)
    finally:
        print("Cleaning up generated server...")
        try:
            os.killpg(os.getpgid(server_process.pid), 9)
        except ProcessLookupError:
            pass

def main():
    download("https://raw.githubusercontent.com/OAI/OpenAPI-Specification/main/examples/v2.0/json/petstore.json", "../petstore.json")
    download("https://raw.githubusercontent.com/OAI/OpenAPI-Specification/main/examples/v3.0/petstore.json", "../petstore_oas3.json")

    run_tests("../petstore.json", "Swagger 2.0 (Generated)", "8081")
    run_tests("../petstore_oas3.json", "OpenAPI 3.2.0 (Generated)", "8082")

    print("Generated Petstore tests completed successfully!")

if __name__ == "__main__":
    main()
