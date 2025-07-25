#[cfg(not(feature = "ffi"))]
use glam::Vec3A;
use std::f32::consts::FRAC_1_SQRT_2;

macro_rules! sample_points_inner {
    ($self:ident, $vec:ident) => {
            /// Sample points on a mesh, given a density of points.
            /// Returns a vec of sampled points and a vec of the normals of those points.
            ///
            /// - `points_per_m`: The number of points per square meter.
            /// - `scale`: The uniform scale of the mesh.
            /// - `seed`: An optional random seed.
           pub fn sample_points(
        &$self,
        points_per_m: f32,
        scale: f32,
        seed: Option<u64>,
    ) -> (Vec<$vec>, Vec<$vec>) {
        let area = $self.get_area(scale);
        let num_points = get_num_points(area.total_area, points_per_m);
        let mut sampled_points = vec![$vec::default(); num_points];
        let mut sampled_normals = sampled_points.clone();
        $self.set_sampled_points(&area, &mut sampled_points, &mut sampled_normals, seed);

        (sampled_points, sampled_normals)
    }};
}

#[cfg(feature = "ffi")]
use crate::Vertex;
use crate::{
    get_num_points,
    sampler::{
        point_sampler::PointSampler, sample_normal, sample_point,
        triangle_sampler::TriangleSampler, Sampler,
    },
    Area, Triangle,
};

/// A mesh has vertices, triangles, and normals.
#[cfg(feature = "ffi")]
#[safer_ffi::derive_ReprC]
#[repr(C)]
pub struct Mesh {
    /// The (x, y, z) vertices of the mesh.
    pub vertices: safer_ffi::Vec<Vertex>,
    /// (x, y, z) groups of indices of `vertices`, comprising triangles.
    pub triangles: safer_ffi::Vec<Triangle>,
    /// (x, y, z) normal directional vectors.
    pub normals: safer_ffi::Vec<Vertex>,
}

/// A mesh has vertices, triangles, and normals.
#[cfg(not(feature = "ffi"))]
pub struct Mesh {
    /// The (x, y, z) vertices of the mesh.
    pub vertices: Vec<Vec3A>,
    /// (x, y, z) groups of indices of `vertices`, comprising triangles.
    pub triangles: Vec<Triangle>,
    /// (x, y, z) normal directional vectors.
    pub normals: Vec<Vec3A>,
}

impl Mesh {
    #[cfg(feature = "ffi")]
    pub fn new(vertices: Vec<Vertex>, triangles: Vec<Triangle>, normals: Vec<Vertex>) -> Self {
        Self {
            vertices: vertices.into(),
            triangles: triangles.into(),
            normals: normals.into(),
        }
    }

    #[cfg(not(feature = "ffi"))]
    pub fn new(vertices: Vec<Vec3A>, triangles: Vec<Triangle>, normals: Vec<Vec3A>) -> Self {
        Self {
            vertices,
            triangles,
            normals,
        }
    }

    /// Returns the mesh's [`Area`]
    ///
    /// - `scale`: The uniform scale of the mesh.
    pub fn get_area(&self, scale: f32) -> Area {
        #[cfg(feature = "ffi")]
        let areas = vec![0.0; self.triangles.len()].into();
        #[cfg(not(feature = "ffi"))]
        let areas = vec![0.0; self.triangles.len()];

        let mut area = Area {
            total_area: 0.,
            areas,
        };
        self.set_area(scale, &mut area);
        area
    }

    /// Set the [`Area`] of this mesh.
    ///
    /// - `scale`: The uniform scale of the mesh.
    /// - `area`: The [`Area`] of the mesh.
    pub fn set_area(&self, scale: f32, area: &mut Area) {
        area.total_area = 0.;
        let half_scale = scale * 0.5;
        self.triangles
            .iter()
            .zip(area.areas.iter_mut())
            .for_each(|(triangle, ar)| {
                // Get this triangle's area.
                let p0 = self.vertices[triangle.a];

                #[cfg(feature = "ffi")]
                let a = half_scale
                    * self.vertices[triangle.b]
                        .sub(&p0)
                        .cross(&self.vertices[triangle.c].sub(&p0))
                        .magnitude();
                #[cfg(not(feature = "ffi"))]
                let a = half_scale
                    * (self.vertices[triangle.b] - p0)
                        .cross(self.vertices[triangle.c] - p0)
                        .length();

                // Add to the total.
                area.total_area += a;
                *ar = a;
            });
    }

    #[cfg(feature = "ffi")]
    sample_points_inner!(self, Vertex);

    #[cfg(not(feature = "ffi"))]
    sample_points_inner!(self, Vec3A);

    /// Fill pre-allocated slices with sampled points and normals.
    ///
    /// - `area`: The `Area` of the mesh.
    /// - `sampled_points`: (x, y, z) sampled points. The size can differ from `triangles` and `areas`.
    /// - `sampled_normals`: Normal directional vectors, one per sampled point. This must be the same length as `sampled_points`.
    /// - `seed`: An optional random seed.
    pub fn set_sampled_points(
        &self,
        area: &Area,
        #[cfg(feature = "ffi")] sampled_points: &mut [Vertex],
        #[cfg(feature = "ffi")] sampled_normals: &mut [Vertex],
        #[cfg(not(feature = "ffi"))] sampled_points: &mut [Vec3A],
        #[cfg(not(feature = "ffi"))] sampled_normals: &mut [Vec3A],
        seed: Option<u64>,
    ) {
        let num_points = sampled_points.len();
        let mut sampler = PointSampler {
            vertices: &self.vertices,
            normals: &self.normals,
            sampled_points,
            sampled_normals,
        };
        sampler.sample_points(area, num_points, &self.triangles, seed);
    }

    /// Get the triangles at which points can be sampled.
    /// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
    ///
    /// - `points_per_m`: The number of points per square meter. The mesh's unit of measurement is assumed to be meters.
    /// - `area`: The `Area` of the mesh.
    /// - `seed`: An optional random seed.
    pub fn sample_triangles(
        &self,
        points_per_m: f32,
        area: &Area,
        seed: Option<u64>,
    ) -> Vec<Triangle> {
        let mut samples = vec![Triangle::default(); get_num_points(area.total_area, points_per_m)];
        self.set_sampled_triangles(area, &mut samples, seed);
        samples
    }

    /// Set a pre-allocated slice of triangles at which points can be sampled.
    /// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
    ///
    /// - `area`: The `Area` of the mesh.
    /// - `sampled_triangles`: A pre-defined slice of triangles that will be set in this function. The size must match the number of points that will be sampled.
    /// - `seed`: An optional random seed.
    pub fn set_sampled_triangles(
        &self,
        area: &Area,
        sampled_triangles: &mut [Triangle],
        seed: Option<u64>,
    ) {
        let num_points = sampled_triangles.len();
        let mut sampler = TriangleSampler { sampled_triangles };
        sampler.sample_points(area, num_points, &self.triangles, seed);
    }

    /// Given pre-sampled triangles, sample vertices.
    /// The position of the vertex relative to the spatial area of the triangle is deterministic.
    /// In contrast, points sampled via [`Self::sample_points`] and [`Self::set_sampled_points`] will be at a random point on a sampled triangle.
    ///
    /// - `sampled_mesh`: The mesh with the sampled points, triangles, and normals.
    pub fn set_presampled_mesh(&self, sampled_mesh: &mut Mesh) {
        // Hardcode the U, V, W parameters.
        const U: f32 = 1. - FRAC_1_SQRT_2;
        const V: f32 = 0.5 * FRAC_1_SQRT_2;
        const W: f32 = 1. - U - V;

        sampled_mesh
            .vertices
            .iter_mut()
            .zip(
                sampled_mesh
                    .triangles
                    .iter()
                    .zip(sampled_mesh.normals.iter_mut()),
            )
            .for_each(|(point, (triangle, normal))| {
                sample_point(point, U, V, W, triangle, &self.vertices);
                sample_normal(normal, triangle, &self.normals);
            });
    }

    /// Load a .obj file.
    /// Returns the vertices, the triangles, and the normals.
    #[cfg(feature = "obj")]
    pub fn from_obj<P>(path: P) -> Self
    where
        P: AsRef<std::path::Path> + std::fmt::Debug,
    {
        let obj = &tobj::load_obj(path, &tobj::GPU_LOAD_OPTIONS).unwrap().0[0].mesh;
        let vertices = obj
            .positions
            .chunks_exact(3)
            .map(|vertex| Vertex {
                x: vertex[0],
                y: vertex[1],
                z: vertex[2],
            })
            .collect::<Vec<Vertex>>();
        let triangles = obj
            .indices
            .chunks_exact(3)
            .map(|triangle| Triangle {
                a: triangle[0] as usize,
                b: triangle[1] as usize,
                c: triangle[2] as usize,
            })
            .collect::<Vec<Triangle>>();
        let normals = obj
            .normals
            .chunks_exact(3)
            .map(|normal| Vertex {
                x: normal[0],
                y: normal[1],
                z: normal[2],
            })
            .collect::<Vec<Vertex>>();
        Self::new(vertices, triangles, normals)
    }
}

#[cfg(test)]
mod tests {
    #[cfg(feature = "obj")]
    #[test]
    fn test_sample_points() {
        let mesh = super::Mesh::from_obj("tests/suzanne.obj");
        let (points, _) = mesh.sample_points(80., 1., Some(0));
        assert_eq!(points.len(), 997);
    }
}
