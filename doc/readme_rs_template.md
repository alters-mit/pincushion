### Features

- `ffi` (default) will compile FFI-safe wrapper functions. This is required when compiling `pincushion` into a library that can be used in Unity.
- `cs` should only be enabled when generating the C# code (see below).

### Create C# Code

1. Create the C# files:

```sh
cargo run --bin cs --features cs
```

The files will be in `../PincushionCs/`

2. Compile the native Rust library:

```sh
cargo build --release
```

The library will be located in `target/release/`

### Example

To run the example: `cargo run --example suzanne --featueres obj`

### Benchmarks

To run the benchmark: `cargo bench benchmark --features obj`

Results:

@ BENCHMARKS @