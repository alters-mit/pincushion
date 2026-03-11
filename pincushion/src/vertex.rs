#[safer_ffi::derive_ReprC]
#[derive(Copy, Clone, Default)]
#[repr(C)]
pub struct Vertex {
    pub x: f32,
    pub y: f32,
    pub z: f32,
}

impl Vertex {
    pub const fn from_slice(slice: &[f32]) -> Self {
        Self {
            x: slice[0],
            y: slice[1],
            z: slice[2],
        }
    }

    pub const fn add(self, other: Self) -> Self {
        Self {
            x: self.x + other.x,
            y: self.y + other.y,
            z: self.z + other.z,
        }
    }

    pub const fn sub(self, other: Self) -> Self {
        Self {
            x: self.x - other.x,
            y: self.y - other.y,
            z: self.z - other.z,
        }
    }

    pub const fn mul(self, value: f32) -> Self {
        Self {
            x: self.x * value,
            y: self.y * value,
            z: self.z * value,
        }
    }

    pub const fn div(self, value: f32) -> Self {
        Self {
            x: self.x / value,
            y: self.y / value,
            z: self.z / value,
        }
    }

    pub const fn cross(self, other: Self) -> Self {
        Self {
            x: self.y * other.z - other.y * self.z,
            y: self.z * other.x - other.z * self.x,
            z: self.x * other.y - other.x * self.y,
        }
    }

    pub const fn dot(self, other: Self) -> f32 {
        (self.x * other.x) + (self.y * other.y) + (self.z * other.z)
    }

    pub fn magnitude(self) -> f32 {
        f32::sqrt(self.dot(self))
    }
}
