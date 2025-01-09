# 0.2.x

## 0.2.0

- Converted `PincushionCs` into a Unity package: `com.mit.pincushion`. This *greatly* simplifies the install/update process.
- `UnityExample` now references `com.mit.pincushion` instead of containing duplicate code
- Added a GitHub workflow that will rebuild the native `pincushion` library for Windows, MacOS, and Linux whenever changes are made to the Rust source code.
- Added `bump_version.py` to bump the Rust and C# version strings
- Added `copy_into_package.py` to build the native Pincushion library and copy it into the Unity package.
- Small performance improvement to point sampling 