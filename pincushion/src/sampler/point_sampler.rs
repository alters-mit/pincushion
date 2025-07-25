use fastrand::Rng;
#[cfg(not(feature = "ffi"))]
use glam::Vec3A;

use crate::Triangle;
#[cfg(feature = "ffi")]
use crate::Vertex;

use super::{sample_normal, sample_point, Sampler};

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

impl Sampler for PointSampler<'_> {
    fn sample(&mut self, triangle: &Triangle, point_index: usize, rng: &mut Rng) {
        // Source: https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/master/Assets/Script/VFXMeshBakingHelper.cs
        let t = f32::sqrt(rng.f32());
        let u = 1. - t;
        let v = (1. - rng.f32()) * t;
        let w = 1. - u - v;

        sample_point(
            &mut self.sampled_points[point_index],
            u,
            v,
            w,
            triangle,
            self.vertices,
        );
        sample_normal(
            &mut self.sampled_normals[point_index],
            triangle,
            self.normals,
        );
    }
}
