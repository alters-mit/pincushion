//! FFI-safe functions for pincushion.

use safer_ffi::ffi_export;

use crate::{
    get_areas_in_place, sample_points as sample_points_native, sample_triangles_in_place,
    scale_areas as scale_areas_native,
    set_points_from_sampled_triangles as set_points_from_sampled_triangles_native,
    vecs::{Vector3, Vector3U},
};

/// FFI-safe Vector3.
#[derive(Clone)]
#[safer_ffi::derive_ReprC]
#[repr(C)]
pub struct Vec3 {
    pub x: f32,
    pub y: f32,
    pub z: f32,
}

impl Vector3 for Vec3 {
    fn x(&self) -> f32 {
        self.x
    }

    fn y(&self) -> f32 {
        self.y
    }

    fn z(&self) -> f32 {
        self.z
    }

    fn x_mut(&mut self) -> &mut f32 {
        &mut self.x
    }

    fn y_mut(&mut self) -> &mut f32 {
        &mut self.y
    }

    fn z_mut(&mut self) -> &mut f32 {
        &mut self.z
    }

    fn new(x: f32, y: f32, z: f32) -> Self {
        Self { x, y, z }
    }
}

/// FFI-safe Vector3Uint
#[derive(Copy, Clone)]
#[safer_ffi::derive_ReprC]
#[repr(C)]
pub struct Vec3U {
    pub x: usize,
    pub y: usize,
    pub z: usize,
}

impl Vector3U for Vec3U {
    fn x(&self) -> usize {
        self.x
    }

    fn y(&self) -> usize {
        self.y
    }

    fn z(&self) -> usize {
        self.z
    }

    fn x_mut(&mut self) -> &mut usize {
        &mut self.x
    }

    fn y_mut(&mut self) -> &mut usize {
        &mut self.y
    }

    fn z_mut(&mut self) -> &mut usize {
        &mut self.z
    }

    fn new(x: usize, y: usize, z: usize) -> Self {
        Self { x, y, z }
    }
}

/// - `vertices`: A flat vec of (x, y, z) vertices.
/// - `triangles`: A flat vec of three indices of vertices.
/// - `areas`: A vec that will be filled with the areas of each triangle in `triangles`.
///   This must be the same length as `triangles.len() / 3`.
///
/// Returns: The total area.
#[ffi_export]
pub fn get_areas(
    vertices: &safer_ffi::Vec<Vec3>,
    triangles: &safer_ffi::Vec<Vec3U>,
    areas: &mut safer_ffi::Vec<f32>,
) -> f32 {
    get_areas_in_place(vertices, triangles, areas)
}

/// Scale pre-calculated areas.
///
/// - `areas`: A slice that will be filled with the areas of each triangle
/// - `scale`: The uniform scale of the mesh.
#[ffi_export]
pub fn scale_areas(areas: &mut safer_ffi::Vec<f32>, scale: f32) -> f32 {
    scale_areas_native(areas, scale)
}

/// Sample random points on the mesh.
///
/// - `total_area`: The total surface area of the mesh in square meters.
/// - `vertices`: (x, y, z) vertices.
/// - `triangles`: Indices of vertices comprising a triangle.
/// - `areas`: A slice that will be filled with the areas of each triangle. This must be the same length as `triangles`.
/// - `points`: A pre-defined slice of vertices that will be filled with points. The size can differ from `triangles` and `areas`.
///   This will be filled with the sampled points.
///   This must be defined on the other side of the FFI boundary.
///   To get the expected size of `points`, call `get_areas(vertices, triangles, areas)` followed by `get_num_points(total_area, points_per_m)`
#[ffi_export]
pub fn sample_points(
    total_area: f32,
    vertices: &safer_ffi::Vec<Vec3>,
    triangles: &safer_ffi::Vec<Vec3U>,
    areas: &safer_ffi::Vec<f32>,
    points: &mut safer_ffi::Vec<Vec3>,
) {
    sample_points_native(total_area, vertices, triangles, areas, points);
}

/// Set the triangles at which points can be sampled.
/// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
///
/// - `total_area`: The total surface area of the mesh in square meters.
/// - `triangles`: Indices of vertices comprising a triangle.
/// - `areas`: A slice that will be filled with the areas of each triangle. This must be the same length as `triangles`.
/// - `sampled_triangles`: A pre-defined slice of triangles that will be set in this function. The size can differ from `triangles` and `areas` and must match the number of points that will be sampled.
#[ffi_export]
pub fn sample_triangles(
    total_area: f32,
    triangles: &safer_ffi::Vec<Vec3U>,
    areas: &safer_ffi::Vec<f32>,
    sampled_triangles: &mut safer_ffi::Vec<Vec3U>,
) {
    sample_triangles_in_place(total_area, triangles, areas, sampled_triangles);
}

/// Given pre-sampled triangles, sample vertices.
/// The position of the vertex relative to the spatial area of the triangle is deterministic.
/// In contrast, points sampled via `sample_points` and `sample_points_ppm` will be at a random point on a sampled triangle.
///
/// - `vertices`: (x, y, z) vertices.
/// - `normals`: (x, y, z) normals.
/// - `sampled_triangles`: Presampled triangles.
/// - `points`: A pre-defined slice of vertices that will be filled with points. The size must be the same as `sampled_triangles`.
/// - `sampled_normals`: A pre-defined slice of normal vectors per point in `points`.
#[ffi_export]
pub fn set_points_from_sampled_triangles(
    vertices: &safer_ffi::Vec<Vec3>,
    normals: &safer_ffi::Vec<Vec3>,
    sampled_triangles: &mut safer_ffi::Vec<Vec3U>,
    points: &mut safer_ffi::Vec<Vec3>,
    sampled_normals: &mut safer_ffi::Vec<Vec3>,
) {
    set_points_from_sampled_triangles_native(
        vertices,
        normals,
        sampled_triangles,
        points,
        sampled_normals,
    );
}
