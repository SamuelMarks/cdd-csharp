# WebAssembly (WASM) Support

`cdd-csharp` supports being compiled to WebAssembly (WASM/WASI) using the experimental .NET 10 WASI workload.

This enables you to:
- Run the CLI in constrained environments without .NET installed.
- Integrate the CDD toolchain into web browsers or unified cdd-* CLI binaries via tools like Wasmtime or Wasmer.

## Building for WASM
Run the provided make target:
```bash
make build_wasm
# or
make.bat build_wasm
```

This will output WASM binaries in `bin/wasm/`.
