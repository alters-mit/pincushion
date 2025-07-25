//! Uniform mesh sampler for Rust and Unity.
//!
#![doc = include_str!("../../doc/overview.md")]
//!
//! Pincushion can be used as a typical Rust crate or as a native library in Unity.
//!
//! This documentation is for the Rust crate.
//! Documentation for Unity/C# can be found [here](https://github.com/alters-mit/pincushion).
//!
//! ### Usage
//!
//! ```
#![doc = include_str!("../examples/readme.rs")]
//! ```
//!
#![doc = include_str!("../../doc/readme_rs.md")]

mod area;
#[cfg(feature = "cs")]
pub mod cs;
#[cfg(feature = "ffi")]
mod ffi;
mod mesh;
mod sampler;
mod triangle;
#[cfg(feature = "ffi")]
mod vertex;

#[cfg(not(feature = "ffi"))]
pub use glam;
#[cfg(feature = "ffi")]
use safer_ffi::ffi_export;

pub use area::Area;
#[cfg(feature = "ffi")]
pub use ffi::*;
pub use mesh::Mesh;
pub use triangle::Triangle;
#[cfg(feature = "ffi")]
pub use vertex::Vertex;

/// - `total_area`: The total area of the triangles in square meters. See: `set_area(mesh, scale, area)`.
/// - `points_per_m`: The number of points per square meter. The mesh's unit of measurement is assumed to be meters.
///
/// Returns: The number of points to be sampled.
#[cfg_attr(feature = "ffi", ffi_export)]
pub fn get_num_points(total_area: f32, points_per_m: f32) -> usize {
    (total_area * points_per_m) as usize
}
