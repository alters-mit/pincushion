use fastrand::Rng;
#[cfg(not(feature = "ffi"))]
use glam::Vec3A;
#[cfg(feature = "ffi")]
use crate::Vertex;
use crate::{Area, Triangle};

pub(crate) mod point_sampler;
pub(crate) mod triangle_sampler;

/// A trait used to sample points or triangles.
pub(crate) trait Sampler {
    fn sample(&mut self, triangle: &Triangle, point_index: usize, rng: &mut Rng);

    fn sample_points(
        &mut self,
        area: &Area,
        num_points: usize,
        triangles: &[Triangle],
        seed: Option<u64>,
    ) {
        // The area per point is used to uniformly sample the points.
        let area_per_point = area.total_area / num_points as f32;
        let mut rng = match seed {
            Some(seed) => Rng::with_seed(seed),
            None => Rng::new(),
        };
        // When sampling points, start at this index.
        let mut start_index_point = 0;
        // When choosing random triangles, start at this index.
        let mut start_index_triangle = 0;
        // The accumulated triangle area. This is used to set the end indices.
        let mut total_accumulated_area = 0.0;
        for (area_index, area) in area.areas.iter().enumerate() {
            // Add area.
            total_accumulated_area += *area;
            // We have enough area.
            if total_accumulated_area >= area_per_point {
                // Derive how many points we can fit in the accumulated area.
                let num_points_in_area_f = (total_accumulated_area / area_per_point).ceil();
                let mut num_points_in_area = num_points_in_area_f as usize;
                // Clamp to avoid exceeding the total number of points.
                if start_index_point + num_points_in_area >= num_points {
                    num_points_in_area = num_points - start_index_point;
                }
                // Sample some points.
                // If there is only one triangle, then sample the points in that triangle repeatedly.
                let range = start_index_point..start_index_point + num_points_in_area;
                if start_index_triangle == area_index {
                    let triangle = &triangles[start_index_triangle];
                    range.for_each(|point_index| self.sample(triangle, point_index, &mut rng));
                }
                // If there are multiple triangles, get a Uniform distribution (for efficiency) and randomly select triangles.
                else {
                    range.for_each(|point_index| {
                        let triangle = &triangles[rng.usize(start_index_triangle..=area_index)];
                        self.sample(triangle, point_index, &mut rng);
                    });
                }
                // Start adding points at the offset.
                start_index_point += num_points_in_area;
                // Reset the accumulated area, keeping the remainder.
                total_accumulated_area -= area_per_point * num_points_in_area_f;
                // Increment to the next starting triangle.
                start_index_triangle = area_index + 1;
            }
        }
    }
}

/// Get a point on a triangle.
/// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/90714a3b61dbc731d9e8dc4c4ca93c2ba1da5156/Assets/Script/VFXMeshBakingHelper.cs#L167
#[cfg(feature = "ffi")]
pub(crate) fn sample_point(
    point: &mut Vertex,
    u: f32,
    v: f32,
    w: f32,
    triangle: &Triangle,
    vertices: &[Vertex],
) {
    *point = vertices[triangle.a]
        .mul(u)
        .add(&vertices[triangle.b].mul(v))
        .add(&vertices[triangle.c].mul(w));
}

/// Get a point on a triangle.
/// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/90714a3b61dbc731d9e8dc4c4ca93c2ba1da5156/Assets/Script/VFXMeshBakingHelper.cs#L167
#[cfg(not(feature = "ffi"))]
pub(crate) fn sample_point(
    point: &mut Vec3A,
    u: f32,
    v: f32,
    w: f32,
    triangle: &Triangle,
    vertices: &[Vec3A],
) {
    *point = u * vertices[triangle.a] + v * vertices[triangle.b] + w * vertices[triangle.c];
}

/// Set the average normal of a triangle.
#[cfg(feature = "ffi")]
pub(crate) fn sample_normal(normal: &mut Vertex, triangle: &Triangle, normals: &[Vertex]) {
    *normal = normals[triangle.a]
        .add(&normals[triangle.b])
        .add(&normals[triangle.c])
        .div(3.)
}

/// Set the average normal of a triangle.
#[cfg(not(feature = "ffi"))]
pub(crate) fn sample_normal(normal: &mut Vec3A, triangle: &Triangle, normals: &[Vec3A]) {
    *normal = (normals[triangle.a]
        + normals[triangle.b]
        + normals[triangle.c]) / 3.;
}
