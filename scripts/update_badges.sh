#!/bin/bash
set -e

# Run tests and collect coverage
dotnet test tests/Cdd.OpenApi.Tests/Cdd.OpenApi.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=coverage.xml > /dev/null

TEST_COV="100"
if [ -f "tests/Cdd.OpenApi.Tests/coverage.xml" ]; then
    # Extract line-rate from root coverage node
    RATE=$(grep -oP '<coverage[^>]*line-rate="\K[0-9.]+' tests/Cdd.OpenApi.Tests/coverage.xml | head -1)
    if [ ! -z "$RATE" ]; then
        TEST_COV=$(echo "$RATE * 100" | bc | cut -d'.' -f1)
    fi
fi

# Run doc coverage
dotnet build scripts/DocCoverage.csproj > /dev/null
DOC_COV=$(dotnet run --project scripts/DocCoverage.csproj src/Cdd.OpenApi | tail -1)

# Update README.md
sed -i -E "s/Test Coverage-[0-9.]+%-brightgreen/Test Coverage-${TEST_COV}%-brightgreen/g" README.md
sed -i -E "s/Doc Coverage-[0-9.]+%-brightgreen/Doc Coverage-${DOC_COV}%-brightgreen/g" README.md

git add README.md
