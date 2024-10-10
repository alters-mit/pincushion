use crate::{Triangle, Uv, Vertex};

pub struct Quads {
    pub vertices: Vec<[Vertex; 4]>,
    pub triangles: Vec<[Triangle; 2]>,
    pub normals: Vec<[Vertex; 4]>,
    pub uvs: Vec<[Uv; 4]>,
}
