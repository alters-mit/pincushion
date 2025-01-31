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

use fastrand::Rng;
use safer_ffi::ffi_export;

pub use area::Area;
pub use mesh::Mesh;
pub use vecs::*;

#[cfg(feature = "cs")]
pub mod cs;
pub mod ffi;
#[cfg(feature = "mask")]
pub mod mask;

mod area;
mod mesh;
mod sampler;
mod vecs;

/// - `total_area`: The total area of the triangles in square meters. See: `get_areas(vertices, triangles)` and `get_areas_in_place(vertices, triangles, areas)`
/// - `points_per_m`: The number of points per square meter. The mesh's unit of measurement is assumed to be meters.
///
/// Returns: The number of points to be sampled.
#[ffi_export]
pub fn get_num_points(total_area: f32, points_per_m: f32) -> usize {
    (total_area * points_per_m) as usize
}

pub(crate) fn get_rng(seed: Option<u64>) -> Rng {
    match seed {
        Some(seed) => Rng::with_seed(seed),
        None => Rng::new(),
    }
}
