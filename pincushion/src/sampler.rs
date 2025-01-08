use rand::{distributions::Uniform, rngs::ThreadRng, thread_rng, Rng};

use crate::{Area, Triangle, Vertex};

pub(crate) mod point_sampler;
pub(crate) mod triangle_sampler;

/// A trait used to sample points or triangles.
pub(crate) trait Sampler {
    fn sample(&mut self, triangle: &Triangle, point_index: usize, rng: &mut ThreadRng);

    fn sample_points(&mut self, area: &Area, num_points: usize, triangles: &[Triangle]) {
        // The area per point is used to uniformly sample the points.
        let area_per_point = area.total_area / num_points as f32;
        let mut rng = thread_rng();
        // When sampling points, start at this index.
        let mut start_index_point = 0;
        // When choosing trandom triangles, start at this index.
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
                if start_index_triangle == area_index {
                    let triangle = &triangles[start_index_triangle];
                    (0..num_points_in_area)
                        .for_each(|i| self.sample(triangle, start_index_point + i, &mut rng));
                } else {
                    let range = Uniform::new_inclusive(start_index_triangle, area_index);
                    (0..num_points_in_area).for_each(|i| {
                        let triangle = &triangles[rng.sample(range)];
                        self.sample(triangle, start_index_point + i, &mut rng);
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
/// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/master/Assets/Script/VFXMeshBakingHelper.cs
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

/// Set the average normal of a triangle.
pub(crate) fn sample_normal(normal: &mut Vertex, triangle: &Triangle, normals: &[Vertex]) {
    *normal = normals[triangle.a]
        .add(&normals[triangle.b])
        .add(&normals[triangle.c])
        .div(3.)
}
