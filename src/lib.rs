//! Given a target density, generate randomly sampled points on a mesh.
//!
//! This crate was built with Unity/C# bindings in mind.
//!
//! Imagine, if you will, an animated human that is visually rendered with uniformly sampled points.
//! This mesh needs to move dynamically, so that number and positions of the points need to change.
//! The best way to do that is in a shader.
//! This crate *doesn't* cover this use-case.
//!
//! *However,* suppose you want the animated human to walk in a room that is also rendered in uniformly sampled points.
//! In that case, the room geometry is static.
//! The sampled points never need to change.
//! So, you could sample the points exactly once and use them later.
//! That's what this crate is for.
//!
//! # Usage (native Rust)
//!
//! ```
//! use tobj::{load_obj, GPU_LOAD_OPTIONS};
//!
//! use pincushion::{sample_points_from_ppm, Vertex, Triangle};
//!
//! fn get_obj(path: &str) -> (Vec<Vertex>, Vec<Triangle>) {
//!     let obj = &load_obj(path, &GPU_LOAD_OPTIONS).unwrap().0[0].mesh;
//!     let vertices = obj.positions.chunks_exact(3).map(|v| [v[0], v[1], v[2]]).collect::<Vec<Vertex>>();
//!     let triangles = obj.indices.chunks_exact(3).map(|triangle|
//!         [triangle[0] as usize, triangle[1] as usize, triangle[2] as usize]
//!     ).collect::<Vec<Triangle>>();
//!     (vertices, triangles)
//! }
//!
//! fn main() {
//!     let (vertices, triangles) = get_obj("tests/suzanne.obj");
//!
//!     // This, plus the volume, controls the number of points per square meter.
//!     // The volume of the mesh is also assumed to be in square meters.
//!     let points_per_m = 0.015;
//!
//!     // Sample the points.
//!     let points = sample_points_from_ppm(&vertices, &triangles, points_per_m);
//! }
//! ```
//!
//! # Usage (Unity)
//!
//! To add the codebase to your Unity project:
//!
//! 1. Run: `cargo build --release --features ffi`
//! 2. Copy into your project all .cs scripts in `cs/` plus the native library: `target/release/pincushion.so` (In Windows, it's `pincushion.dll`)
//! 3. In your code, add: `using Pincushion;`
//!
//! To generate points, call `Vector3[] points = mesh.GetSampledPoints(pointsPerM);`
//!
//! Whenever the Rust codebase changes, the C# bindings must change as well. To do this: `cargo run --bin cs --features cs`
//!
//! # Example
//!
//! To run the example: `cargo run --example suzanne`

#[cfg(feature = "cs")]
pub mod cs;

#[cfg(feature = "ffi")]
pub mod ffi;

use rand::{thread_rng, Rng};

pub type Vertex = [f32; 3];
pub type Triangle = [usize; 3];

/// - `vertices`: A slice of (x, y, z) vertices.
/// - `triangles`: A slice of three indices of vertices.
///
/// Returns: The area of each triangle and the total area.
pub fn get_areas(vertices: &[Vertex], triangles: &[Triangle]) -> (Vec<f32>, f32) {
    let mut areas = vec![0.0; triangles.len()];
    let total_area = get_areas_in_place(vertices, triangles, &mut areas);
    (areas, total_area)
}

/// - `vertices`: A slice of (x, y, z) vertices.
/// - `triangles`: A slice of three indices of vertices.
/// - `areas`: A slice that will be filled with the areas of each triangle in `triangles`.
///   This must be the same length as `triangles`.
///
/// Returns: The total area.
pub fn get_areas_in_place(vertices: &[Vertex], triangles: &[Triangle], areas: &mut [f32]) -> f32 {
    let mut total_area = 0.;
    triangles
        .iter()
        .zip(areas.iter_mut())
        .for_each(|(triangle, area)| {
            // Get this triangle's area.
            *area = get_triangle_area(
                &vertices[triangle[0]],
                &vertices[triangle[1]],
                &vertices[triangle[2]],
            );
            // Add to the total.
            total_area += *area;
        });
    total_area
}

/// - `total_area`: The total area of the triangles. See: `get_areas(vertices, triangles)` and `get_areas_in_place(vertices, triangles, areas)`
/// - `points_per_m`: The number of points per square meter. This function assumed that `total_area` is in square meters.
///
/// Returns: The volume of the mesh.
#[cfg_attr(feature = "ffi", safer_ffi::ffi_export)]
pub fn get_num_points(total_area: f32, points_per_m: f32) -> usize {
    (total_area / points_per_m) as usize
}

/// Sample points on a mesh, given a density of points.
///
/// - `vertices`: A slice of (x, y, z) vertices.
/// - `triangles`: A slice of three indices of vertices.
/// - `points_per_m`: The number of points per square meter.
///
/// Returns: An vec of sampled points.
pub fn sample_points_from_ppm(
    vertices: &[Vertex],
    triangles: &[Triangle],
    points_per_m: f32,
) -> Vec<Vertex> {
    let (areas, total_area) = get_areas(vertices, triangles);
    let num_points = get_num_points(total_area, points_per_m);
    let mut points = vec![[0.0; 3]; num_points];
    sample_points(vertices, triangles, &areas, total_area, &mut points);
    points
}

/// Sample random points on the mesh.triangle_end_index
///
/// - `vertices`: A slice of (x, y, z) vertices.
/// - `triangles`: A slice of three indices of vertices.
/// - `areas`: The area of each triangle. See: [`get_areas(vertices, triangles)`] and [`get_areas_in_place(vertices, triangles, areas)`].
/// - `total_area`: The total area.
/// - `points`: A pre-defined slice of vertices that will be filled with points. The size can differ from `triangles` and `areas`.
pub fn sample_points(
    vertices: &[Vertex],
    triangles: &[Triangle],
    areas: &[f32],
    total_area: f32,
    points: &mut [Vertex],
) {
    // The area per point is used to uniformly sample the points.
    let area_per_point = total_area / points.len() as f32;
    let mut rng = thread_rng();
    // When sampling points, start at this index.
    let mut start_index_point = 0;
    // When choosing trandom triangles, start at this index.
    let mut start_index_triangle = 0;
    // The accumulated triangle area. This is used to set the end indices.
    let mut total_accumulated_area = 0.0;
    for (index, area) in areas.iter().enumerate() {
        // Add area.
        total_accumulated_area += *area;
        // We have enough area.
        if total_accumulated_area >= area_per_point {
            // Derive how many points we can fit in the accumulated area.
            let num_points = (total_accumulated_area / area_per_point) as usize;
            // Sample some points.
            for i in 0..num_points {
                // Get a random triangle, bounded by the start index and the current index in `areas`.
                let triangle = triangles[rng.gen_range(start_index_triangle..=index)];
                // Get a random point on that triangle.
                // Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/master/Assets/Script/VFXMeshBakingHelper.cs
                let mut u = rng.gen_range(0.0..1.0);
                let mut v = rng.gen_range(0.0..1.0);
                let t = f32::sqrt(v);
                v = u * t;
                u = (1.0 - u) * t;
                let w = 1.0 - u - v;
                // Set the point at `start_index_pooint` offset by 0..num_points.
                points[start_index_point + i] = add(
                    &add(
                        &mul(&vertices[triangle[0]], u),
                        &mul(&vertices[triangle[1]], v),
                    ),
                    &mul(&vertices[triangle[2]], w),
                );
            }
            // Start adding points at the offset.
            start_index_point += num_points;
            // Reset the accumulated area.
            total_accumulated_area = 0.0;
            // Increment to the next starting triangle.
            start_index_triangle = index + 1;
        }
    }
}

/// Returns the area of a triangle.
/// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/90714a3b61dbc731d9e8dc4c4ca93c2ba1da5156/Assets/Script/VFXMeshBakingHelper.cs#L202
fn get_triangle_area(p0: &Vertex, p1: &Vertex, p2: &Vertex) -> f32 {
    0.5 * magnitude(&cross(&sub(p1, p0), &sub(p2, p0)))
}

// For add, sub, etc. see: glam::Vec3

fn add(a: &Vertex, b: &Vertex) -> Vertex {
    [a[0] + b[0], a[1] + b[1], a[2] + b[2]]
}

fn sub(a: &Vertex, b: &Vertex) -> Vertex {
    [a[0] - b[0], a[1] - b[1], a[2] - b[2]]
}

fn mul(v: &Vertex, m: f32) -> Vertex {
    [v[0] * m, v[1] * m, v[2] * m]
}

fn cross(a: &Vertex, b: &Vertex) -> Vertex {
    [
        a[1] * b[2] - b[1] * a[2],
        a[2] * b[0] - b[2] * a[0],
        a[0] * b[1] - b[0] * a[1],
    ]
}

fn dot(a: &Vertex, b: &Vertex) -> f32 {
    (a[0] * b[0]) + (a[1] * b[1]) + (a[2] * b[2])
}

fn magnitude(v: &Vertex) -> f32 {
    f32::sqrt(dot(v, v))
}

#[cfg(test)]
mod tests {
    use tobj::{load_obj, GPU_LOAD_OPTIONS};

    use crate::{sample_points_from_ppm, Triangle, Vertex};

    #[test]
    fn test_sample_points() {
        let (vertices, triangles) = get_obj();
        let points = sample_points_from_ppm(&vertices, &triangles, 0.015);
        assert_eq!(points.len(), 831);
    }

    fn get_obj() -> (Vec<Vertex>, Vec<Triangle>) {
        let obj = &load_obj("tests/suzanne.obj", &GPU_LOAD_OPTIONS).unwrap().0[0].mesh;
        let vertices = obj
            .positions
            .chunks_exact(3)
            .map(|v| [v[0], v[1], v[2]])
            .collect::<Vec<Vertex>>();
        let triangles = obj
            .indices
            .chunks_exact(3)
            .map(|triangle| {
                [
                    triangle[0] as usize,
                    triangle[1] as usize,
                    triangle[2] as usize,
                ]
            })
            .collect::<Vec<Triangle>>();
        (vertices, triangles)
    }
}
