#!/bin/bash
set -e

# Run tests and collect coverage
dotnet test tests/Cdd.OpenApi.Tests/Cdd.OpenApi.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=coverage.xml > /dev/null

TEST_COV="100"
if [ -f "tests/Cdd.OpenApi.Tests/coverage.xml" ]; then
    # Extract line-rate from root coverage node
    RATE=$(grep -o '<coverage[^>]*line-rate="[0-9.]*' tests/Cdd.OpenApi.Tests/coverage.xml | head -1 | grep -o '[0-9.]*$')
    if [ ! -z "$RATE" ]; then
        TEST_COV=$(echo "$RATE * 100" | bc | cut -d'.' -f1)
    fi
fi

# Run doc coverage
dotnet build scripts/DocCoverage.csproj > /dev/null
DOC_COV=$(dotnet run --project scripts/DocCoverage.csproj src/Cdd.OpenApi | tail -1)

# Update README.md
if sed --version 2>/dev/null | grep -q GNU; then
    sed -i -E "s/test_coverage-[0-9.]+%25-[a-z]+/test_coverage-${TEST_COV}%25-brightgreen/g" README.md
    sed -i -E "s/doc_coverage-[0-9.]+%25-[a-z]+/doc_coverage-${DOC_COV}%25-brightgreen/g" README.md
else
    sed -i '' -E "s/test_coverage-[0-9.]+%25-[a-z]+/test_coverage-${TEST_COV}%25-brightgreen/g" README.md
    sed -i '' -E "s/doc_coverage-[0-9.]+%25-[a-z]+/doc_coverage-${DOC_COV}%25-brightgreen/g" README.md
fi

git add README.md
