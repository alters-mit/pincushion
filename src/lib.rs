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

/// - `vertices`: The vertices as a flat slice of coordinates.
/// - `triangles`: The triangles as a flat slice of indices.
///
/// Returns: The volume of the mesh.
pub fn get_num_points(
    vertices: &[[f32; 3]],
    triangles: &[[usize; 3]],
    points_per_cm: f32,
) -> usize {
    let volume = triangles
        .iter()
        .map(|triangle| {
            signed_volume_of_triangle(
                &vertices[triangle[0]],
                &vertices[triangle[1]],
                &vertices[triangle[2]],
            )
        })
        .sum::<f32>()
        .abs();
    (volume * 100.0 * points_per_cm) as usize
}

/// Sample points on a mesh, given a density of points.
///
/// - `vertices`: The vertices.
/// - `triangles`: The triangle indices.
/// - `points_per_cm`: The number of points per centimeter.
///
/// Returns: An vec of sampled points.
pub fn sample_points_from_ppcm(
    vertices: &[[f32; 3]],
    triangles: &[[usize; 3]],
    points_per_cm: f32,
) -> Vec<[f32; 3]> {
    let mut points = vec![[0.0; 3]; get_num_points(vertices, triangles, points_per_cm)];
    sample_points(vertices, triangles, &mut points);
    points
}

/// Sample random points on the mesh.
///
/// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/master/Assets/Script/VFXMeshBakingHelper.cs
///
/// - `vertices`: The vertices.
/// - `triangles`: The triangle indices.
/// - `points`: A pre-defined slice of vertices that will be filled with points.
pub fn sample_points(vertices: &[[f32; 3]], triangles: &[[usize; 3]], points: &mut [[f32; 3]]) {
    let mut accumulated_triangle_area = vec![0.0; triangles.len()];
    // Get the area of the first triangle.
    accumulated_triangle_area[0] = get_triangle_area(
        &vertices[triangles[0][0]],
        &vertices[triangles[0][1]],
        &vertices[triangles[0][2]],
    );
    // Accumulate area.
    triangles[1..triangles.len()]
        .iter()
        .enumerate()
        .for_each(|(i, triangle)| {
            accumulated_triangle_area[i + 1] = accumulated_triangle_area[i]
                + get_triangle_area(
                    &vertices[triangle[0]],
                    &vertices[triangle[1]],
                    &vertices[triangle[2]],
                );
        });
    let mut rng = thread_rng();
    points
        .iter_mut()
        .zip(accumulated_triangle_area.iter())
        .for_each(|(point, area)| {
            let mut u = rng.gen_range(-1.0..1.0);
            let mut v = rng.gen_range(-1.0..1.0);
            let t = f32::sqrt(v);
            v = u * t;
            u = (1.0 - u) * t;
            let w = 1.0 - u - v;
            let area_index =
                get_area_index(rng.gen_range(-0.1..1.0) * *area, &accumulated_triangle_area);
            let triangle = &triangles[area_index];
            *point = add(
                &add(
                    &mul(&vertices[triangle[0]], u),
                    &mul(&vertices[triangle[1]], v),
                ),
                &mul(&vertices[triangle[2]], w),
            );
        });
}

/// Returns the signed volume of a triangle.
/// Source: https://stackoverflow.com/a/1568551
fn signed_volume_of_triangle(p0: &[f32; 3], p1: &[f32; 3], p2: &[f32; 3]) -> f32 {
    let v321 = p2[0] * p1[1] * p0[2];
    let v231 = p1[0] * p2[1] * p0[2];
    let v312 = p2[0] * p0[1] * p1[2];
    let v132 = p0[0] * p2[1] * p1[2];
    let v213 = p1[0] * p0[1] * p2[2];
    let v123 = p0[0] * p1[1] * p2[2];
    (1.0 / 6.0) * (-v321 + v231 + v312 - v132 - v213 + v123)
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

/// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/90714a3b61dbc731d9e8dc4c4ca93c2ba1da5156/Assets/Script/VFXMeshBakingHelper.cs#L211
fn get_area_index(area: f32, accumulated_triangle_area: &[f32]) -> usize {
    let mut min = 0;
    let mut max = accumulated_triangle_area.len() - 1;
    let mut mid = max >> 1;
    while max >= min {
        if accumulated_triangle_area[mid] >= area
            && (mid == 0 || accumulated_triangle_area[mid - 1] < area)
        {
            return mid;
        } else if area < accumulated_triangle_area[mid] {
            max = mid - 1;
        } else {
            min = mid + 1;
        }
        mid = (min + max) >> 1;
    }
    unreachable!()
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
