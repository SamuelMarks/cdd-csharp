#!/usr/bin/env python3
import os
import subprocess
import urllib.request
import time
import shutil

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
            time.sleep(1)
    return False

def run_tests(spec_file, base_path, label):
    print("=================================================")
    print(f"Running {label} Petstore test...")
    print("=================================================")

    print(f"Starting petstore server (Base Path: {base_path})...")
    use_docker = False
    server_process = None

    if shutil.which("npm"):
        print("Using npx @stoplight/prism-cli to mock the petstore natively...")
        with open("prism.log", "w") as out:
            server_process = subprocess.Popen(["npx", "-y", "@stoplight/prism-cli", "mock", "-p", "8085", spec_file], stdout=out, stderr=subprocess.STDOUT)
        time.sleep(5)
    elif shutil.which("python3"):
        print("Using python3 to mock the petstore natively (empty 200/404 responses are sufficient for these tests)...")
        with open("python_mock.log", "w") as out:
            server_process = subprocess.Popen(["python3", "-m", "http.server", "8085"], stdout=out, stderr=subprocess.STDOUT)
        time.sleep(2)
    elif shutil.which("docker") and subprocess.run(["docker", "info"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL).returncode == 0:
        print("Falling back to docker for petstore server...")
        use_docker = True
        subprocess.run(["docker", "rm", "-f", "petstore_server"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        if subprocess.run(["docker", "run", "-d", "-p", "8085:8085", "-e", "SWAGGER_HOST=http://localhost:8085", "-e", f"SWAGGER_BASE_PATH={base_path}", "--name", "petstore_server", "swaggerapi/petstore"], stdout=subprocess.DEVNULL).returncode != 0:
            print("Docker run failed, tests may fail")
        print("Waiting for petstore server to be ready...")
        if wait_for_server("http://localhost:8085/", timeout=60):
            print("Petstore server is ready! Waiting a few more seconds for endpoints to map...")
            time.sleep(3)
    else:
        print("Warning: no suitable runtime (npm, python3, docker) is available. Tests relying on localhost:8085 will likely fail.")

    print(f"Generating client for {label}...")
    client_dir = "../cdd-csharp-client"
    if os.path.exists(client_dir):
        shutil.rmtree(client_dir)

    abs_spec_path = os.path.abspath(spec_file)

    subprocess.run(["dotnet", "run", "--project", "src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj", "-f", "net10.0", "--", "from_openapi", "to_sdk", "-i", abs_spec_path, "-o", client_dir, "--tests"], check=True)

    import re
    test_file = os.path.join(client_dir, "tests", "GeneratedProject.Tests", "IntegrationTests.cs")
    with open(test_file, "r", encoding="utf-8") as f:
        content = f.read()
    content = content.replace("8080", "8085")
    with open(test_file, "w", encoding="utf-8") as f:
        f.write(content)

    print(f"Running integration tests for {label}...")
    subprocess.run(["dotnet", "test", "GeneratedProject.sln"], cwd=client_dir, check=True)

    print("Cleaning up petstore server...")
    if server_process:
        server_process.kill()
    if use_docker:
        subprocess.run(["docker", "rm", "-f", "petstore_server"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

def main():
    download("https://raw.githubusercontent.com/OAI/OpenAPI-Specification/main/examples/v2.0/json/petstore.json", "../petstore.json")
    download("https://raw.githubusercontent.com/OAI/OpenAPI-Specification/main/examples/v3.0/petstore.json", "../petstore_oas3.json")

    run_tests("../petstore.json", "/v2", "Swagger 2.0")
    run_tests("../petstore_oas3.json", "/api/v3", "OpenAPI 3.2.0")

    print("Petstore tests completed successfully!")

if __name__ == "__main__":
    main()
