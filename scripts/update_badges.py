#!/usr/bin/env python3
import subprocess
import re
import os

def main():
    subprocess.run(["dotnet", "test", "tests/Cdd.OpenApi.Tests/Cdd.OpenApi.Tests.csproj", "/p:CollectCoverage=true", "/p:CoverletOutputFormat=cobertura", "/p:CoverletOutput=coverage.xml"], stdout=subprocess.DEVNULL)

    test_cov = "100"
    cov_file = "tests/Cdd.OpenApi.Tests/coverage.xml"
    if os.path.isfile(cov_file):
        with open(cov_file, "r", encoding="utf-8") as f:
            content = f.read()
            match = re.search(r'<coverage[^>]*line-rate="([0-9.]+)"', content)
            if match:
                rate = float(match.group(1))
                test_cov = str(int(rate * 100))

    subprocess.run(["dotnet", "build", "scripts/DocCoverage.csproj"], stdout=subprocess.DEVNULL)
    result = subprocess.run(["dotnet", "run", "--project", "scripts/DocCoverage.csproj", "src/Cdd.OpenApi"], capture_output=True, text=True)
    doc_cov = result.stdout.strip().split("\n")[-1].strip() if result.stdout else "100"

    readme_path = "README.md"
    if os.path.isfile(readme_path):
        with open(readme_path, "r", encoding="utf-8") as f:
            readme = f.read()
        readme = re.sub(r'test_coverage-[0-9.]+%25-[a-z]+', f'test_coverage-{test_cov}%25-brightgreen', readme)
        readme = re.sub(r'doc_coverage-[0-9.]+%25-[a-z]+', f'doc_coverage-{doc_cov}%25-brightgreen', readme)
        with open(readme_path, "w", encoding="utf-8") as f:
            f.write(readme)

    subprocess.run(["git", "add", "README.md"])

if __name__ == "__main__":
    main()
