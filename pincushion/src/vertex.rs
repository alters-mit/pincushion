#[safer_ffi::derive_ReprC]
#[derive(Copy, Clone, Default)]
#[repr(C)]
pub struct Vertex {
    pub x: f32,
    pub y: f32,
    pub z: f32,
}

impl Vertex {
    pub fn add(&self, other: &Self) -> Self {
        Self {
            x: self.x + other.x,
            y: self.y + other.y,
            z: self.z + other.z,
        }
    }

    pub fn sub(&self, other: &Self) -> Self {
        Self {
            x: self.x - other.x,
            y: self.y - other.y,
            z: self.z - other.z,
        }
    }

    pub fn mul(&self, value: f32) -> Self {
        Self {
            x: self.x * value,
            y: self.y * value,
            z: self.z * value,
        }
    }

    pub fn mul_mut(&mut self, value: f32) {
        self.x *= value;
        self.y *= value;
        self.z *= value;
    }

    pub fn div(&self, value: f32) -> Self {
        Self {
            x: self.x / value,
            y: self.y / value,
            z: self.z / value,
        }
    }

    pub fn cross(&self, other: &Self) -> Self {
        Self {
            x: self.y * other.z - other.y * self.z,
            y: self.z * other.x - other.z * self.x,
            z: self.x * other.y - other.x * self.y,
        }
    }

    pub fn dot(&self, other: &Self) -> f32 {
        (self.x * other.x) + (self.y * other.y) + (self.z * other.z)
    }

    pub fn magnitude(&self) -> f32 {
        f32::sqrt(self.dot(self))
    }

    /// Returns the area of a triangle.
    /// Source: <https://github.com/PaulDemeulenaere/vfx-uniform-mesh-sampling/blob/90714a3b61dbc731d9e8dc4c4ca93c2ba1da5156/Assets/Script/VFXMeshBakingHelper.cs#L202>
    pub fn get_triangle_area(p0: &Self, p1: &Self, p2: &Self) -> f32
    where
        Self: Sized,
    {
        0.5 * &p1.sub(p0).cross(&p2.sub(p0)).magnitude()
    }
}
