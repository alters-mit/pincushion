#![doc = include_str!("../doc/header.md")]
//!
#![cfg_attr(all(), doc = embed_doc_image::embed_image!("pincushion", "doc/pincushion.png"))]
//!
//! ![pincushion]
//!
#![doc = include_str!("../doc/overview.md")]
//!
#![doc = include_str!("../doc/unity.md")]
//!
//! ```
#![doc = include_str!("../examples/readme.rs")]
//! ```
//!
#![doc = include_str!("../doc/README.md")]

mod area;
#[cfg(feature = "cs")]
pub mod cs;
#[cfg(feature = "ffi")]
mod ffi;
mod mesh;
mod sampler;
mod triangle;
#[cfg(feature = "ffi")]
mod vec3;

#[cfg(not(feature = "ffi"))]
pub use glam;
#[cfg(not(feature = "ffi"))]
use glam::{Mat4, Vec3A};
#[cfg(feature = "ffi")]
use safer_ffi::ffi_export;

pub use area::Area;
#[cfg(feature = "ffi")]
pub use ffi::*;
pub use mesh::Mesh;
pub use triangle::Triangle;
#[cfg(feature = "ffi")]
pub use vec3::Vec3;

#[cfg(feature = "ffi")]
type Vector3 = Vec3;
#[cfg(not(feature = "ffi"))]
type Vector3 = Vec3A;
#[cfg(feature = "ffi")]
type Vek<T> = safer_ffi::Vec<T>;
#[cfg(not(feature = "ffi"))]
type Vek<T> = Vec<T>;

/// Returns the number of points to be sampled.
///
/// - `total_area`: The total area of the triangles in square meters. See: [`set_area`]
/// - `points_per_m`: The number of points per square meter. The mesh's unit of measurement is assumed to be meters.
#[cfg_attr(feature = "ffi", ffi_export)]
pub fn get_num_points(total_area: f32, points_per_m: f32) -> usize {
    (total_area * points_per_m) as usize
}

#[cfg(not(feature = "ffi"))]
/// Apply a transform matrix to transform sampled points (modify its position and rotation).
///
/// - `matrix`: A 4x4 transform matrix.
/// - `points`: The points that will be transformed.
pub fn transform_points(matrix: &Mat4, points: &mut [Vec3A]) {
    points
        .iter_mut()
        .for_each(|p| *p = matrix.transform_point3a(*p));
}
