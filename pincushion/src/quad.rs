use crate::vecs::{Vector2, Vector3, Vector3U};

/// A quad of four vertices.
#[derive(Copy, Clone)]
#[cfg_attr(feature = "ffi", safer_ffi::derive_ReprC)]
#[repr(C)]
pub struct Quad<T, U, V>
where
    T: Vector3,
    U: Vector3U,
    V: Vector2,
{
    pub vertex_0: T,
    pub vertex_1: T,
    pub vertex_2: T,
    pub vertex_3: T,
    pub triangle_0: U,
    pub triangle_1: U,
    pub uv_0: V,
    pub uv_1: V,
    pub uv_2: V,
    pub uv_3: V,
}

impl<T, U, V> Quad<T, U, V>
where
    T: Vector3,
    U: Vector3U,
    V: Vector2,
{
    pub fn new(position: T, size: f32) -> Self {
        let x = position.x();
        let y = position.y();
        let z = position.z();
        let s = size * 0.5;
        // Source: https://docs.unity3d.com/Manual/Example-CreatingaBillboardPlane.html
        Self {
            vertex_0: T::new(x - s, y - s, z),
            vertex_1: T::new(x + s, y - s, z),
            vertex_2: T::new(x - s, y + s, z),
            vertex_3: T::new(x + s, y + s, z),
            triangle_0: U::new(0, 2, 1),
            triangle_1: U::new(2, 3, 1),
            uv_0: V::new(0., 0.),
            uv_1: V::new(1., 0.),
            uv_2: V::new(0., 1.),
            uv_3: V::new(1., 1.),
        }
    }

    pub fn set_positions(&mut self, position: &T) {
        self.vertex_0.add(position);
        self.vertex_1.add(position);
        self.vertex_2.add(position);
        self.vertex_3.add(position);
    }

    pub fn set_vertices(&mut self, position: &T, size: f32) {
        let x = position.x();
        let y = position.y();
        let z = position.z();
        let s = size * 0.5;
        self.vertex_0 = T::new(x - s, y - s, z);
        self.vertex_1 = T::new(x + s, y - s, z);
        self.vertex_2 = T::new(x - s, y + s, z);
        self.vertex_3 = T::new(x + s, y + s, z);
    }
}
