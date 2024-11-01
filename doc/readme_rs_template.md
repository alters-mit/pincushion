### Features

- `obj` adds a `Mesh::from_obj(path)` function to load a mesh from a .obj file.
- `cs` should only be enabled when generating the C# code (see below).

### Create C# Code

1. Create the C# native bindings:

```sh
cargo run --bin cs --features cs
```

The file will be at `../PincushionCs/NativeBindings.cs`

2. Compile the native Rust library:

```sh
cargo build --release
```

The library will be located in `target/release/`

### Example

To run the example: `cargo run --example suzanne --features obj`

### Benchmarks

To run the benchmark: `cargo bench benchmark --features obj`

Results:

@ BENCHMARKS @