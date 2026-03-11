use fastrand::Rng;

#[cfg(feature = "ffi")]
use crate::Vec3;
use crate::{Triangle, Vector3};
#[cfg(not(feature = "ffi"))]
use glam::Vec3A;

use super::Sampler;

pub(crate) struct PointSampler<'mesh> {
    pub vertices: &'mesh [Vector3],
    pub normals: &'mesh [Vector3],
    pub sampled_points: &'mesh mut [Vector3],
    pub sampled_normals: &'mesh mut [Vector3],
}

impl PointSampler<'_> {
    /// Get a point on a triangle.
    /// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/90714a3b61dbc731d9e8dc4c4ca93c2ba1da5156/Assets/Script/VFXMeshBakingHelper.cs#L167
    #[cfg(feature = "ffi")]
    const fn sample_point(&self, u: f32, v: f32, w: f32, triangle: &Triangle) -> Vec3 {
        self.vertices[triangle.a]
            .mul(u)
            .add(self.vertices[triangle.b].mul(v))
            .add(self.vertices[triangle.c].mul(w))
    }

    /// Get a point on a triangle.
    /// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/90714a3b61dbc731d9e8dc4c4ca93c2ba1da5156/Assets/Script/VFXMeshBakingHelper.cs#L167
    #[cfg(not(feature = "ffi"))]
    fn sample_point(&self, u: f32, v: f32, w: f32, triangle: &Triangle) -> Vec3A {
        u * self.vertices[triangle.a]
            + v * self.vertices[triangle.b]
            + w * self.vertices[triangle.c]
    }

    /// Set the average normal of a triangle.
    #[cfg(feature = "ffi")]
    const fn sample_normal(&self, triangle: &Triangle) -> Vec3 {
        self.normals[triangle.a]
            .add(self.normals[triangle.b])
            .add(self.normals[triangle.c])
            .div(3.)
    }

    /// Set the average normal of a triangle.
    #[cfg(not(feature = "ffi"))]
    fn sample_normal(&self, triangle: &Triangle) -> Vec3A {
        (self.normals[triangle.a] + self.normals[triangle.b] + self.normals[triangle.c]) / 3.
    }
}

impl Sampler for PointSampler<'_> {
    fn sample(&mut self, triangle: &Triangle, point_index: usize, rng: &mut Rng) {
        // Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/master/Assets/Script/VFXMeshBakingHelper.cs
        let t = f32::sqrt(rng.f32());
        let u = 1. - t;
        let v = (1. - rng.f32()) * t;
        let w = 1. - u - v;

        self.sampled_points[point_index] = self.sample_point(u, v, w, triangle);
        self.sampled_normals[point_index] = self.sample_normal(triangle);
    }
}
