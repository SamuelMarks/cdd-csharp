# Makefile for cdd-csharp

.PHONY: help all install_base install_deps build_docs build test run build_wasm build_docker run_docker

all: help

help:
	@echo "Available tasks:"
	@echo "  install_base  - Install .NET runtime (if needed, usually managed via dotnet-install)"
	@echo "  install_deps  - Restore dependencies"
	@echo "  build_docs    - Build API docs (default docs output or specify DOCS_DIR=path)"
	@echo "  build         - Build CLI binary (default bin output or specify BIN_DIR=path)"
	@echo "  test          - Run tests"
	@echo "  run           - Run the CLI, args passed after 'run' (e.g. make run ARGS=\"--version\")"
	@echo "  build_wasm    - Build WASM version"
	@echo "  build_docker  - Build Docker images"
	@echo "  run_docker    - Run Docker image and ping JSON-RPC"

install_base:
	@echo "Assuming dotnet SDK is installed or managed by system"

install_deps:
	dotnet restore CddOpenApi.slnx

DOCS_DIR ?= docs
build_docs:
	dotnet tool restore || true
	dotnet docfx build docs/docfx.json -o $(DOCS_DIR) || echo "DocFX not configured completely yet, but script runs"

BIN_DIR ?= bin/Release/net10.0/linux-x64/publish
build:
	dotnet publish src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -c Release -f net10.0 -r linux-x64 --self-contained -o $(BIN_DIR)

test:
	dotnet test CddOpenApi.slnx

ifeq (run,$(firstword $(MAKECMDGOALS)))
  RUN_ARGS := $(wordlist 2,$(words $(MAKECMDGOALS)),$(MAKECMDGOALS))
  ifneq ($(RUN_ARGS),)
    $(eval $(RUN_ARGS):;@:)
  endif
endif

run: build
	$(BIN_DIR)/cdd-csharp $(RUN_ARGS)

build_wasm:
	dotnet build src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -c Release -f net8.0 -p:UseWasiSdk=true
	mkdir -p bin
	cp src/Cdd.OpenApi.Cli/bin/Release/net8.0/cdd-csharp.wasm bin/cdd-csharp.wasm
build_docker:
	docker build -t cdd-csharp-alpine -f alpine.Dockerfile .
	docker build -t cdd-csharp-debian -f debian.Dockerfile .

run_docker:
	docker run -d --name cdd-csharp-test -p 8085:8082 cdd-csharp-alpine
	sleep 2
	curl -X POST -H "Content-Type: application/json" -d '{"jsonrpc": "2.0", "method": "version", "id": 1}' http://127.0.0.1:8085/ || true
	docker stop cdd-csharp-test
	docker rm cdd-csharp-test
