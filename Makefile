.PHONY: install_base install_deps build_docs build build_wasm test run help all

ifeq (build_docs,$(firstword $(MAKECMDGOALS)))
  DOCS_DIR := $(word 2,$(MAKECMDGOALS))
  ifeq ($(DOCS_DIR),)
    DOCS_DIR := docs
  else
    $(eval .PHONY: $(DOCS_DIR))
    $(eval $(DOCS_DIR):;@:)
  endif
else
  DOCS_DIR := docs
endif

ifeq (build,$(firstword $(MAKECMDGOALS)))
  BIN_DIR := $(word 2,$(MAKECMDGOALS))
  ifeq ($(BIN_DIR),)
    BIN_DIR := bin
  else
    $(eval .PHONY: $(BIN_DIR))
    $(eval $(BIN_DIR):;@:)
  endif
else
  BIN_DIR := bin
endif

ifeq (run,$(firstword $(MAKECMDGOALS)))
  RUN_ARGS := $(wordlist 2,$(words $(MAKECMDGOALS)),$(MAKECMDGOALS))
  ifneq ($(RUN_ARGS),)
    $(foreach arg,$(RUN_ARGS),$(eval .PHONY: $(arg)))
    $(foreach arg,$(RUN_ARGS),$(eval $(arg):;@:))
  endif
endif

# Default goal
.DEFAULT_GOAL := help

install_base:
	@echo "Installing .NET SDK..."
	@if [ "$$(uname)" = "Darwin" ]; then \
		brew install --cask dotnet-sdk || brew install dotnet; \
	elif [ -f /etc/debian_version ]; then \
		sudo apt-get update && (sudo apt-get install -y dotnet-sdk-10.0 || sudo apt-get install -y dotnet-sdk-9.0 || (curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0)); \
	elif [ -f /etc/redhat-release ]; then \
		(sudo dnf install -y dotnet-sdk-10.0 || sudo dnf install -y dotnet-sdk-9.0 || (curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0)); \
	elif [ "$$(uname)" = "FreeBSD" ]; then \
		sudo pkg install -y dotnet; \
	else \
		curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0; \
	fi

install_deps:
	dotnet restore

build_docs:
	dotnet build src/Cdd.OpenApi/Cdd.OpenApi.csproj -c Release
	mkdir -p $(DOCS_DIR)
	@cp -v src/Cdd.OpenApi/bin/Release/*/Cdd.OpenApi.xml $(DOCS_DIR)/ 2>/dev/null || echo "No XML docs generated"

build:
	dotnet publish src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -c Release -o $(BIN_DIR)

test:
	dotnet test

run: build
	./$(BIN_DIR)/cdd-csharp $(RUN_ARGS)

help:
	@echo "Available tasks:"
	@echo "  install_base   - install language runtime and anything else relevant (e.g., .NET SDK)"
	@echo "  install_deps   - install local dependencies (dotnet restore)"
	@echo "  build_docs     - build the API docs and put them in the 'docs' directory. Usage: make build_docs [dir]"
	@echo "  build          - build the CLI binary. Usage: make build [dir]"
	@echo "  test           - run tests locally"
	@echo "  run            - the CLI. Dependencies are built first. Usage: make run [args...] (Note: for args like --version, use: make run -- --version)"
	@echo "  help           - show what options are available"
	@echo "  all            - show help text"

all: help

build_wasm:
	@echo "Installing WASM workloads..."
	dotnet workload install wasm-tools || true
	dotnet workload install wasi-experimental || true
	@echo "Building for WASI/WASM..."
	dotnet publish src/Cdd.OpenApi.Cli/Cdd.OpenApi.Cli.csproj -c Release -r browser-wasm -o $(BIN_DIR)/wasm
