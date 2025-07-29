use fastrand::Rng;

use crate::Triangle;
#[cfg(feature = "ffi")]
use crate::Vertex;
#[cfg(not(feature = "ffi"))]
use glam::Vec3A;

use super::Sampler;

macro_rules! point_sampler {
    ($point:ident) => {
        pub(crate) struct PointSampler<'mesh> {
            pub vertices: &'mesh [$point],
            pub normals: &'mesh [$point],
            pub sampled_points: &'mesh mut [$point],
            pub sampled_normals: &'mesh mut [$point],
        }
    };
}

#[cfg(feature = "ffi")]
point_sampler!(Vertex);

#[cfg(not(feature = "ffi"))]
point_sampler!(Vec3A);

impl PointSampler<'_> {
    /// Get a point on a triangle.
    /// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/90714a3b61dbc731d9e8dc4c4ca93c2ba1da5156/Assets/Script/VFXMeshBakingHelper.cs#L167
    #[cfg(feature = "ffi")]
    #[inline]
    fn sample_point(&self, u: f32, v: f32, w: f32, triangle: &Triangle) -> Vertex {
        self.vertices[triangle.a]
            .mul(u)
            .add(&self.vertices[triangle.b].mul(v))
            .add(&self.vertices[triangle.c].mul(w))
    }

    /// Get a point on a triangle.
    /// Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/90714a3b61dbc731d9e8dc4c4ca93c2ba1da5156/Assets/Script/VFXMeshBakingHelper.cs#L167
    #[cfg(not(feature = "ffi"))]
    #[inline]
    fn sample_point(&self, u: f32, v: f32, w: f32, triangle: &Triangle) -> Vec3A {
        u * self.vertices[triangle.a]
            + v * self.vertices[triangle.b]
            + w * self.vertices[triangle.c]
    }

    /// Set the average normal of a triangle.
    #[cfg(feature = "ffi")]
    #[inline]
    fn sample_normal(&self, triangle: &Triangle) -> Vertex {
        self.normals[triangle.a]
            .add(&self.normals[triangle.b])
            .add(&self.normals[triangle.c])
            .div(3.)
    }

    /// Set the average normal of a triangle.
    #[cfg(not(feature = "ffi"))]
    #[inline]
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
