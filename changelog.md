# 0.2.x

## 0.2.1

- Added random seed parameters to each of the sampling functions in both the C# and Rust code. These random seeds can be used to deterministically recreate sampled points.
  - Added `autoSeed` parameter to PincushionManager. Set this to false to manually seed each sampled mesh, thereby allowing for deterministic outcomes.
  - The Rust code now uses `fastrand` to generate random numbers instead of `rand`. This is, as implied, somewhat faster, and is portable (results should be the same regardless of OS or hardware).
- Moved all code in `ffi.rs` into `lib.rs`
- Fixed: The binary used to generate C# native bindings has an invalid target file path.

## 0.2.0

- Converted `PincushionCs` into a Unity package: `com.mit.pincushion`. This *greatly* simplifies the install/update process.
- `UnityExample` now references `com.mit.pincushion` instead of containing duplicate code
- Added a GitHub workflow that will rebuild the native `pincushion` library for Windows, MacOS, and Linux whenever changes are made to the Rust source code.
- Added `bump_version.py` to bump the Rust and C# version strings
- Added `copy_into_package.py` to build the native Pincushion library and copy it into the Unity package.
- Small performance improvement to point sampling 