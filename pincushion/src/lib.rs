//! Uniformly sample points on a mesh.
//!
//! Includes FFI-safe functions for Unity/C# bindings.
//!
//! ### Usage
//!
//! ```
#![doc = include_str!("../examples/readme.rs")]
//! ```
//!
//! Documentation for Unity/C# can be found [here](https://github.com/alters-mit/pincushion).
//!
#![doc = include_str!("../../doc/readme_rs.md")]

use safer_ffi::ffi_export;

pub use area::Area;
pub use mesh::Mesh;
pub use vecs::*;

#[cfg(feature = "cs")]
pub mod cs;

pub mod ffi;

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

#[cfg(test)]
mod tests {
    #[cfg(feature = "obj")]
    #[test]
    fn test_sample_points() {
        let mesh = super::Mesh::from_obj("tests/suzanne.obj");
        let (points, _) = mesh.sample_points(80., 1.);
        assert_eq!(points.len(), 997);
    }
}
