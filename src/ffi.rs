use core::slice;

use safer_ffi::ffi_export;

use crate::{get_num_points, get_volume, sample_points};

/// - `vertices`: The vertices as a flat slice of coordinates.
/// - `triangles`: The triangles as a flat slice of indices.
///
/// Returns: The volume of the mesh.
#[ffi_export]
pub fn get_volume_ffi(vertices: &safer_ffi::Vec<f32>, triangles: &safer_ffi::Vec<usize>) -> f32 {
    unsafe { get_volume(ffi_vertices(vertices), ffi_triangles(triangles)) }
}

/// - `volume`: The volume of the mesh, assumed to be in meters squared.
/// - `points_per_cm`: The number of points per centimeter.
///
/// Returns: The number of points that should be sampled.
#[ffi_export]
pub fn get_num_points_ffi(volume: f32, points_per_cm: f32) -> usize {
    get_num_points(volume, points_per_cm)
}

/// Sample random points on the mesh.
///
/// - `vertices`: The vertices as a flat slice of coordinates.
/// - `triangles`: The triangles as a flat slice of indices.
/// - `points`: A pre-defined flat slice of coordinates.
///   This will be filled with the sampled points.
///   This must be defined on the other side of the FFI boundary.
///   To get the expected size of `points`, call `get_volume_ffi(vertices, triangles)` followed by `get_num_points_ffi(volume, points_per_cm)`
#[ffi_export]
pub fn sample_points_ffi(
    vertices: &safer_ffi::Vec<f32>,
    triangles: &safer_ffi::Vec<usize>,
    points: &mut safer_ffi::Vec<f32>,
) {
    unsafe {
        sample_points(
            ffi_vertices(vertices),
            ffi_triangles(triangles),
            ffi_vertices_mut(points),
        );
    }
}

/// Sample points on a mesh, given a density of points and a pre-defined slice to fill.
///
/// - `vertices`: The vertices as a flat slice of coordinates.
/// - `triangles`: The triangles as a flat slice of indices.
/// - `points_per_cm`: The number of points per centimeter.
/// - `points` A predefined flat slice of coordinates.
///   This will be filled with the sampled points.
///   The number of points, as derived from `points_per_cm`, can be more than `points.len()`.
///   If this happens, the entirety of `points` will be filled, resulting in no error.
///   The number of points can also be less than `points.len()`.
///   If this happens, a slice of `points` is filled.
///
/// Returns: The number of sampled points (the size of the `points` sub-slice).
#[ffi_export]
pub fn sample_points_ffi_from_ppcm(
    vertices: &safer_ffi::Vec<f32>,
    triangles: &safer_ffi::Vec<usize>,
    points_per_cm: f32,
    points: &mut safer_ffi::Vec<f32>,
) -> usize {
    unsafe {
        let vertices = ffi_vertices(vertices);
        let triangles = ffi_triangles(triangles);
        let points = ffi_vertices_mut(points);
        // Get the volume and the number of points. Clamp to the max number of points.
        let num_points =
            get_num_points(get_volume(vertices, triangles), points_per_cm).min(points.len());
        // Sample the points.
        sample_points(vertices, triangles, &mut points[0..num_points]);
        num_points
    }
}

/// Converts a flat array of vertex coordinates from a safer-ffi vec into a shaped slice of vertices.
/// e.g.: `[x0, y0, z0, x1, y1, z1, ...]` into `[[x0, y0, z0], [x1, y1, z1], ...]`
unsafe fn ffi_vertices(vertices: &safer_ffi::Vec<f32>) -> &[[f32; 3]] {
    slice::from_raw_parts(vertices.as_ptr() as *const [f32; 3], vertices.len() / 3)
}

/// Converts a flat array of triangle indices from a safer-ffi vec into a shaped slice of triangles.
unsafe fn ffi_triangles(triangles: &safer_ffi::Vec<usize>) -> &[[usize; 3]] {
    slice::from_raw_parts(triangles.as_ptr() as *const [usize; 3], triangles.len() / 3)
}

/// Converts a flat array of vertex coordinates from a safer-ffi vec into a shaped slice of vertices.
/// e.g.: `[x0, y0, z0, x1, y1, z1, ...]` into `[[x0, y0, z0], [x1, y1, z1], ...]`
unsafe fn ffi_vertices_mut(vertices: &mut safer_ffi::Vec<f32>) -> &mut [[f32; 3]] {
    slice::from_raw_parts_mut(vertices.as_mut_ptr() as *mut [f32; 3], vertices.len() / 3)
}
