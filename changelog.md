# 0.2.x

## 0.2.0

- Converted `PincushionCs` into a Unity package: `com.mit.pincushion`. This *greatly* simplifies the install/update process.
- `UnityExample` now references `com.mit.pincushion` instead of containing duplicate code
- Added `bump_version.py` to bump the Rust and C# version strings
- Added `copy_into_package.py` to build the native Pincushion library and copy it into the Unity package.
- Small point sampling performance improvement