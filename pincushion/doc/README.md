### Features

- `ffi` is enabled by default, and adds the FFI-safe functions that allow Pincushion to interface with C#. If you're planning to use Pincushion in a Rust-only project, you should remove this flag, as doing so will let Pincushion use SIMD-capable structs instead of FFI-safe structs. For example, removing the `ffi` features replaces `safer_ffi::Vec<Vertex>` with `Vec<glam::Vec3A>`.
- `obj` adds a `Mesh::from_obj(path)` function to load a mesh from a .obj file.
- `cs` should only be enabled when generating the C# code (see below).

### Create C# Native Bindings

The `com.mit.pincushion` C# package calls the native `pincushion` Rust library via auto-generated native binding methods.

To regenerate the native bindings:

```sh
cargo run --bin cs --features cs
```

The file will be at `../com.mit.pincushion/NativeBindings.cs`

### Example

To run the example:

```sh
cargo run --example suzanne --features obj
```

### Benchmark

To benchmark:

```sh
cargo bench --features obj
```

This will benchmark `pincushion`  with the `ffi` flag enabled. If the `ffi` flag is disabled, `pincushion` runs slightly faster. To benchmark without the `ffi` feature:

```sh
cargo bench --no-default-features --features obj
```
