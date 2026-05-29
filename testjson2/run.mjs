import fs from 'fs';
import { WASI } from 'wasi';
const wasi = new WASI({ version: 'preview1', env: process.env });
const wasmBuffer = fs.readFileSync('bin/Release/net8.0/wasi-wasm/testjson2.wasm');
const wasmModule = await WebAssembly.compile(wasmBuffer);
const instance = await WebAssembly.instantiate(wasmModule, { wasi_snapshot_preview1: wasi.wasiImport });
wasi.start(instance);
