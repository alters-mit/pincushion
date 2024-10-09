//! Given a target density, generate randomly sampled points on a mesh.
//!
//! This crate was built with Unity/C# bindings in mind. It's useful for sampling points for *static* meshes (i.e. not SkinnedMeshRenders).
//!
//! # Usage (native Rust)
//!
//! ```
//! use tobj::{load_obj, GPU_LOAD_OPTIONS};
//!
//! use pincushion::sample_points_from_ppcm;
//!
//! fn get_obj(path: &str) -> (Vec<[f32; 3]>, Vec<[usize; 3]>) {
//!     let obj = &load_obj(path, &GPU_LOAD_OPTIONS).unwrap().0[0].mesh;
//!     let vertices = obj.positions.chunks_exact(3).map(|v| [v[0], v[1], v[2]]).collect::<Vec<[f32; 3]>>();
//!     let triangles = obj.indices.chunks_exact(3).map(|triangle|
//!         [triangle[0] as usize, triangle[1] as usize, triangle[2] as usize]
//!     ).collect::<Vec<[usize; 3]>>();
//!     (vertices, triangles)
//! }
//!
//! fn main() {
//!     let (vertices, triangles) = get_obj("tests/suzanne.obj");
//!
//!     // This, plus the volume, controls the number of points per centimeter.
//!     // The volume of the mesh is assumed to be in meters squared.
//!     let points_per_cm = 1.5;
//!
//!     // Sample the points.
//!     let points = sample_points_from_ppcm(&vertices, &triangles, points_per_cm);
//! }
//! ```
//!
//! # Usage (Unity)
//!
//! Add the C# scripts in this repo into your project. Then, add: `using Pincushion;`
//!
//! To generate points, call `Vector3[] points = mesh.GetSampledPoints(pointsPerCm);` or `int size = mesh.GetSampledPoints(pointsPerCm, ref points);`.
//!
//! The expected number of points is the product of the mesh volume and `pointsPerCm`.
//! If you don't include `numPoints`, the output will match the expected number.
//! If you do include `numPoints`, then `points.Length == numPoints`. This is the (slightly) faster option.
//!
//! To build a library that can be used in Unity/C#: `cargo build --release --features ffi`
//!
//! To generate C# native bindings: `cargo run --bin cs --features cs`
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

pub fn get_triangle_areas(vertices: &[Vertex], triangles: &[Triangle]) -> (Vec<f32>, f32) {
    let mut areas = vec![0.0; triangles.len()];
    let total_area = get_triangle_areas_in_place(vertices, triangles, &mut areas);
    (areas, total_area)
}

pub fn get_triangle_areas_in_place(
    vertices: &[Vertex],
    triangles: &[Triangle],
    areas: &mut [f32],
) -> f32 {
    let mut total_area = 0.;
    triangles
        .iter()
        .zip(areas.iter_mut())
        .for_each(|(triangle, area)| {
            *area = get_triangle_area(
                &vertices[triangle[0]],
                &vertices[triangle[1]],
                &vertices[triangle[2]],
            );
            total_area += *area;
        });
    total_area
}

/// - `vertices`: The vertices as a flat slice of coordinates.
/// - `triangles`: The triangles as a flat slice of indices.
///
/// Returns: The volume of the mesh.
#[cfg_attr(feature = "ffi", safer_ffi::ffi_export)]
pub fn get_num_points(total_area: f32, points_per_cm: f32) -> usize {
    (total_area * 100.0 * points_per_cm) as usize
}

/// Sample points on a mesh, given a density of points.
///
/// - `vertices`: The vertices.
/// - `triangles`: The triangle indices.
/// - `points_per_cm`: The number of points per centimeter.
///
/// Returns: An vec of sampled points.
pub fn sample_points_from_ppcm(
    vertices: &[Vertex],
    triangles: &[Triangle],
    points_per_cm: f32,
) -> Vec<Vertex> {
    let (areas, total_area) = get_triangle_areas(vertices, triangles);
    let num_points = get_num_points(total_area, points_per_cm);
    let mut points = vec![[0.0; 3]; num_points];
    sample_points(
        vertices,
        triangles,
        &areas,
        total_area,
        points_per_cm,
        &mut points,
    );
    points
}

/// Sample random points on the mesh.
///
/// - `vertices`: The vertices.
/// - `triangles`: The triangle indices.
/// - `points`: A pre-defined slice of vertices that will be filled with points.
pub fn sample_points(
    vertices: &[Vertex],
    triangles: &[Triangle],
    areas: &[f32],
    total_area: f32,
    points_per_cm: f32,
    points: &mut [Vertex],
) {
    // The area per point is used to uniformly sample the points.
    let area_per_point = points_per_cm / (total_area * 100.0);
    println!("{} {}", area_per_point, total_area / points.len() as f32);
    let mut rng = thread_rng();
    let mut accumulated_area = 0.0;
    let mut triangle_indices = vec![0; points.len()];
    let mut num_indices = 0;
    let mut point_index = 0;
    for (i, area) in areas.iter().enumerate() {
        accumulated_area += *area;
        triangle_indices[num_indices] = i;
        num_indices += 1;
        if accumulated_area >= area_per_point {
            // Set the points.
            for point in points[point_index..point_index + num_indices].iter_mut() {
                // Having found enough area, pick a random triangle.
                let triangle = triangles[triangle_indices[rng.gen_range(0..num_indices)]];
                // Get a random point on that triangle.
                // Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/master/Assets/Script/VFXMeshBakingHelper.cs
                let mut u = rng.gen_range(0.0..1.0);
                let mut v = rng.gen_range(0.0..1.0);
                let t = f32::sqrt(v);
                v = u * t;
                u = (1.0 - u) * t;
                let w = 1.0 - u - v;
                *point = add(
                    &add(
                        &mul(&vertices[triangle[0]], u),
                        &mul(&vertices[triangle[1]], v),
                    ),
                    &mul(&vertices[triangle[2]], w),
                );
            }
            // Reset.
            point_index += num_indices;
            accumulated_area = 0.0;
            num_indices = 0;
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

    use crate::sample_points_from_ppcm;

    #[test]
    fn test_sample_points() {
        let (vertices, triangles) = get_obj();
        let points = sample_points_from_ppcm(&vertices, &triangles, 1.0);
        assert_eq!(points.len(), 259);
    }

    fn get_obj() -> (Vec<[f32; 3]>, Vec<[usize; 3]>) {
        let obj = &load_obj("tests/suzanne.obj", &GPU_LOAD_OPTIONS).unwrap().0[0].mesh;
        let vertices = obj
            .positions
            .chunks_exact(3)
            .map(|v| [v[0], v[1], v[2]])
            .collect::<Vec<[f32; 3]>>();
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
            .collect::<Vec<[usize; 3]>>();
        (vertices, triangles)
    }
}
