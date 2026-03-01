# cdd-csharp WASM Support

The `cdd-csharp` CLI tool is compatible with WebAssembly (WASM).

By using the `.NET` SDK's built-in `browser-wasm` runtime support, this CLI can be fully compiled and distributed as a WASM package.

## Building for WASM

Ensure you have the `wasm-tools` workload installed:
```bash
dotnet workload install wasm-tools
```

Then run the provided Make task:
```bash
make build_wasm
```

The output will be placed in `bin/wasm/`. This output includes `.wasm` files and necessary JavaScript glue code allowing it to run entirely in the browser.
This enables `cdd-csharp` to be used in a unified CLI of all `cdd-*` projects without requiring `dotnet` to be installed on the host, or integrated into a web-based IDE entirely client-side.
