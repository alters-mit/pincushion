use rand::distributions::Uniform;
use safer_ffi::derive_ReprC;

use crate::{
    get_num_points,
    sampler::{
        point_sampler::PointSampler, sample_normal, sample_point,
        triangle_sampler::TriangleSampler, Sampler,
    },
    vecs::{Triangle, Vertex},
    Area,
};

/// A mesh has vertices, triangles, and normals.
#[derive_ReprC]
#[repr(C)]
pub struct Mesh {
    /// The (x, y, z) vertices of the mesh.
    pub vertices: safer_ffi::Vec<Vertex>,
    /// (x, y, z) groups of indices of `vertices`, comprising triangles.
    pub triangles: safer_ffi::Vec<Triangle>,
    /// (x, y, z) normal directional vectors.
    pub normals: safer_ffi::Vec<Vertex>,
}

impl Mesh {
    pub fn new(vertices: Vec<Vertex>, triangles: Vec<Triangle>, normals: Vec<Vertex>) -> Self {
        Self {
            vertices: vertices.into(),
            triangles: triangles.into(),
            normals: normals.into(),
        }
    }

    /// - `scale`: The uniform scale of the mesh.
    ///
    /// Returns: The `Area` of the mesh.
    pub fn get_area(&self, scale: f32) -> Area {
        let mut area = Area {
            total_area: 0.,
            areas: vec![0.0; self.triangles.len()].into(),
        };
        self.set_area(scale, &mut area);
        area
    }

    /// Set a pre-allocated slice of areas on the mesh.
    ///
    /// - `scale`: The uniform scale of the mesh.
    /// - `area`: The area of the mesh.
    ///
    /// Returns: The total surface area of the mesh in square meters.
    pub fn set_area(&self, scale: f32, area: &mut Area) {
        area.total_area = 0.;
        self.triangles
            .iter()
            .zip(area.areas.iter_mut())
            .for_each(|(triangle, ar)| {
                // Get this triangle's area.
                let a = Vertex::get_triangle_area(
                    &self.vertices[triangle.a],
                    &self.vertices[triangle.b],
                    &self.vertices[triangle.c],
                ) * scale;
                // Add to the total.
                area.total_area += a;
                *ar = a;
            });
    }

    /// Sample points on a mesh, given a density of points.
    ///
    /// - `points_per_m`: The number of points per square meter.
    /// - `scale`: The uniform scale of the mesh.
    ///
    /// Returns: An vec of sampled points and a vec of normals for each point.
    pub fn sample_points(&self, points_per_m: f32, scale: f32) -> (Vec<Vertex>, Vec<Vertex>) {
        let area = self.get_area(scale);
        let num_points = get_num_points(area.total_area, points_per_m);
        let mut sampled_points = vec![Vertex::default(); num_points];
        let mut sampled_normals = sampled_points.clone();
        self.set_sampled_points(&area, &mut sampled_points, &mut sampled_normals);
        (sampled_points, sampled_normals)
    }

    /// Fill pre-allocated slices with sampled points and normals.
    ///
    /// - `area`: The area of the mesh.
    /// - `sampled_points`: (x, y, z) sampled points. The size can differ from `triangles` and `areas`.
    /// - `sampled_normals`: Normal directional vectors, one per sampled point. This must be the same size as `points`.
    pub fn set_sampled_points(
        &self,
        area: &Area,
        sampled_points: &mut [Vertex],
        sampled_normals: &mut [Vertex],
    ) {
        let range = Uniform::new(0., 1.);
        let num_points = sampled_points.len();
        let mut sampler = PointSampler {
            vertices: &self.vertices,
            normals: &self.normals,
            sampled_points,
            sampled_normals,
            range: &range,
        };
        sampler.sample_points(area, num_points, &self.triangles);
    }

    /// Get the triangles at which points can be sampled.
    /// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
    ///
    /// - `points_per_m`: The number of points per square meter. The mesh's unit of measurement is assumed to be meters.
    /// - `area`: The `Area` of the mesh.
    ///
    /// Returns: The sampled triangles.
    pub fn sample_triangles(&self, points_per_m: f32, area: &Area) -> Vec<Triangle> {
        let mut samples = vec![Triangle::default(); get_num_points(area.total_area, points_per_m)];
        self.set_sampled_triangles(area, &mut samples);
        samples
    }

    /// Set a pre-allocated slice of triangles at which points can be sampled.
    /// This is useful for deformable meshes in situations where the positions will change but not the triangles we want to derive positions from.
    ///
    /// - `area`: The `Area` of the mesh.
    /// - `sampled_triangles`: A pre-defined slice of triangles that will be set in this function. The size can differ from `triangles` and `areas` and must match the number of points that will be sampled.
    pub fn set_sampled_triangles(&self, area: &Area, sampled_triangles: &mut [Triangle]) {
        let num_points = sampled_triangles.len();
        let mut sampler = TriangleSampler { sampled_triangles };
        sampler.sample_points(area, num_points, &self.triangles);
    }

    /// Given pre-sampled triangles, sample vertices.
    /// The position of the vertex relative to the spatial area of the triangle is deterministic.
    /// In constrast, points sampled via `sample_points` and `set_sampled_points` will be at a random point on a sampled triangle.
    ///
    /// - `sampled_mesh`: The mesh with the sampled points, triangles, and normals.
    pub fn set_presampled_mesh(&self, sampled_mesh: &mut Mesh) {
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
                sample_point(point, 0.5, 0.5, triangle, &self.vertices);
                sample_normal(normal, triangle, &self.normals);
            });
    }

    /// Load a .obj file.
    ///
    /// Returns: The vertices, the triangles, and the normals.
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
        let (points, _) = mesh.sample_points(80., 1.);
        assert_eq!(points.len(), 997);
    }
}
