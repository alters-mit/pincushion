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

use std::slice::from_raw_parts_mut;

use fastrand::Rng;
use glam::{Mat4, Vec3};
use safer_ffi::ffi_export;

pub use area::Area;
pub use mesh::Mesh;
pub use vecs::*;

#[cfg(feature = "cs")]
pub mod cs;

mod area;
mod mesh;
mod sampler;
mod vecs;

/// - `mesh` The source mesh.
/// - `scale` The uniform scale of the mesh.
/// - `area`: The `Area` of the mesh.
#[ffi_export]
pub fn set_area(mesh: &Mesh, scale: f32, area: &mut Area) {
    mesh.set_area(scale, area)
}

/// - `total_area`: The total area of the triangles in square meters. See: `set_area(mesh, scale, area)`.
/// - `points_per_m`: The number of points per square meter. The mesh's unit of measurement is assumed to be meters.
///
/// Returns: The number of points to be sampled.
#[ffi_export]
pub fn get_num_points(total_area: f32, points_per_m: f32) -> usize {
    (total_area * points_per_m) as usize
}

/// Sample random points on the mesh.
///
/// - `mesh` The source mesh.
/// - `area`: The `Area` of the mesh.
/// - `sampled_points`: (x, y, z) sampled points.
/// - `sampled_normals`: Normal directional vectors, one per sampled point. This must be the same size as `sampled_points`.
/// - `seed`: A random seed used for sampling.
#[ffi_export]
pub fn sample_points(
    mesh: &Mesh,
    area: &Area,
    sampled_points: &mut safer_ffi::Vec<Vertex>,
    sampled_normals: &mut safer_ffi::Vec<Vertex>,
    seed: u64,
) {
    mesh.set_sampled_points(area, sampled_points, sampled_normals, Some(seed));
}

/// Set the triangles at which points can be sampled.
/// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
///
/// - `mesh` The source mesh.
/// - `area`: The `Area` of the mesh.
/// - `sampled_triangles`: A pre-defined slice of triangles that will be set in this function. The size must match the number of points that will be sampled.
/// - `seed`: A random seed used for sampling.
#[ffi_export]
pub fn sample_triangles(
    mesh: &Mesh,
    area: &Area,
    sampled_triangles: &mut safer_ffi::Vec<Triangle>,
    seed: u64,
) {
    mesh.set_sampled_triangles(area, sampled_triangles, Some(seed));
}

/// In Pincushion, a "mask" can be used to filter out some pre-sampled points.
/// To do this in a way that is visually appealing, `set_mask_indices` is called to:
/// 
/// - Get the indices of each point (i.e. `[0, 1, 2, ...]`)
/// - Shuffle those indices. `seed` is the random seed.
/// - Copy the shuffled indices into `mask_indices`.
#[cfg(feature = "mask")]
#[ffi_export]
pub fn set_mask_indices(mask_indices: &mut safer_ffi::Vec<usize>, seed: u64) {
    let mut vec = Vec::from_iter(0..mask_indices.len());
    Rng::with_seed(seed).shuffle(&mut vec);
    mask_indices.copy_from_slice(&vec[0..]);
}

/// Set a samnpled points mask.
///
/// - `factor`: A value between 0 and 1. The number of "true" values will be `mask.len() * factor`.
/// - `mask_indices`: A precalculated array from `set_mask_indices`.
/// - `mask` The mask array. Values will be set to 0 or 1.
///   This is a vec of u32s because on the Unity side, Pincushion will send this data to the GPU, and the GPU wants 32bit types.
#[cfg(feature = "mask")]
#[ffi_export]
pub fn set_mask(factor: f32, mask_indices: &safer_ffi::Vec<usize>, mask: &mut safer_ffi::Vec<u32>) {
    // Reset the mask.
    mask.fill(0);
    // Get the number of true values as a fraction of the size of `mask`.
    let num_true = (mask.len() as f32 * factor.clamp(0., 1.)) as usize;
    mask_indices[0..num_true].iter().for_each(|i| mask[*i] = 1);
}


/// Apply a transform matrix to transform sampled points.
/// This is meant to be used in a Unity context to transform a mesh by a position and rotation.
/// 
/// Unity *does* have two ways to do this: `Transform.TransformPoint(Vector3)` and `Transform.TransformPoints(Span<Vector3>)`.
/// `TransformPoint` is relatively slow. `TransformPoints` isn't available in the version of Unity I'm using for the project that Pincushion was originally intended for.
/// That said: Unity's `TransformPoints` is *probably* slower than Pincushion's `transform_points`.
/// This is because Unity seems to cast each float (f32) to a double (f64) to avoid imprecision problems.
/// In Pincushion, only f32s are used, the assumption being that `points` isn't going to be used repeatedly so there won't be accumulated error.
///
/// - `matrix`: A 4x4 transform matrix. The length is assumed to always be 16.
/// - `points`: The points that will be transformed.
#[cfg(feature = "transform")]
#[ffi_export]
pub fn transform_points(matrix: &safer_ffi::Vec<f32>, points: &mut safer_ffi::Vec<Vertex>) {
    let matrix = Mat4::from_cols_slice(matrix);

    // Cast `points` as `glam::Vec3`.
    // This is very, very safe because both structs are repr(C).
    let ptr = points.as_mut_ptr().cast::<Vec3>();
    let len = points.len();
    unsafe {
        // Cast, and then transform.
        from_raw_parts_mut(ptr, len)
            .iter_mut()
            .for_each(|p| *p = matrix.transform_point3(*p));
    }
}

#[cfg(test)]
mod tests {
    use super::{set_mask, set_mask_indices};

    #[test]
    fn test_nths() {
        let num_points = 997;
        let mut mask_indices = vec![0; num_points].into();
        set_mask_indices(&mut mask_indices, 0);
        let mut mask = vec![0; num_points].into();
        for factor in [0., 0.1, 0.5, 1.] {
            set_mask(factor, &mask_indices, &mut mask);

            let num_true: usize = mask.iter().filter(|m| **m == 1).count();
            assert_eq!((factor * num_points as f32) as usize, num_true);
        }
    }
}