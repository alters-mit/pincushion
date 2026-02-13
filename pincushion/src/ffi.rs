use crate::{Area, Mesh, Triangle, Vertex};
use fastrand::Rng;
use glam::{Mat4, Vec3};
use safer_ffi::ffi_export;
use std::slice::from_raw_parts_mut;

/// Set the values of an existing [`Area`].
/// Requires the `ffi` feature.
///
/// - `mesh` The source mesh.
/// - `scale` The uniform scale of the mesh.
/// - `area`: The [`Area`] of the mesh.
#[ffi_export]
pub fn set_area(mesh: &Mesh, scale: f32, area: &mut Area) {
    mesh.set_area(scale, area)
}

/// Sample random points on the mesh.
/// Requires the `ffi` feature.
///
/// - `mesh` The source [`Mesh`].
/// - `area`: The [`Area`] of the mesh.
/// - `sampled_points`: (x, y, z) sampled points.
/// - `sampled_normals`: Normal directional vectors, one per sampled point. This must be the same length as `sampled_points`.
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
/// This is useful for deformable meshes in situations where the positions will change but
/// not the triangles we want to derive positions from.
/// Requires the `ffi` feature.
///
/// - `mesh` The source [`Mesh`].
/// - `area`: The [`Area`] of the mesh.
/// - `sampled_triangles`: A pre-defined slice of triangles that will be set in this function.
///    The length must equal number of points that will be sampled.
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

/// Apply a "mask" to filter out some sampled points.
/// Requires the `ffi` feature.
///
/// `mask_indices` must be the same length as the number of sampled points.
/// To do this in a way that is visually appealing, `set_mask_indices` is called to:
///
/// - Get the indices of each point (i.e. `[0, 1, 2, ...]`)
/// - Shuffle those indices. `seed` is the random seed.
/// - Copy the shuffled indices into `mask_indices`.
#[ffi_export]
pub fn set_mask_indices(mask_indices: &mut safer_ffi::Vec<usize>, seed: u64) {
    let mut vec = Vec::from_iter(0..mask_indices.len());
    Rng::with_seed(seed).shuffle(&mut vec);
    mask_indices.copy_from_slice(&vec[0..]);
}

/// Convert `mask_indices` (see [`set_mask_indices`]) to `mask`, a vec of 0s and 1s.
/// Requires the `ffi` feature.
///
/// - `mask_indices` is a  randomly shuffled vec of indices of vertices.
/// - `factor` is a value between 0. and 1. The number of "true" values will be `mask.len() * factor`.
/// - `mask` is a vec of u32s because that's what the GPU wants. It must be the same length as `mask_indices`.
#[ffi_export]
pub fn set_mask(factor: f32, mask_indices: &safer_ffi::Vec<usize>, mask: &mut safer_ffi::Vec<u32>) {
    // Reset the mask.
    mask.fill(0);
    // Get the number of true values as a fraction of the size of `mask`.
    let num_true = (mask.len() as f32 * factor.clamp(0., 1.)) as usize;
    mask_indices[0..num_true].iter().for_each(|i| mask[*i] = 1);
}

/// Apply a transform matrix to transform sampled points (modify its position and rotation).
///
/// Unity has two built-in ways to do the same operation, but they are slower than Pincushion:
///
/// - `Transform.TransformPoint(Vector3)` is definitely slower because it's not a bulk operation.
/// - `Transform.TransformPoints(Span<Vector3>)` is *probably* slower because it casts each float (f32) to a double (f64) and back again.
///   This function doesn't perform that cast because it assumes that you won't continuously do math on its output,
///   and therefore there's no need to worry about precision problems.
///
/// - `matrix`: A 4x4 transform matrix. The length is assumed to always be 16.
/// - `points`: The points that will be transformed.
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
