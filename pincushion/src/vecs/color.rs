use super::Vertex;

#[safer_ffi::derive_ReprC]
#[derive(Copy, Clone, Default)]
#[repr(C)]
pub struct Color {
    pub r: u8,
    pub g: u8,
    pub b: u8,
    pub a: u8,
}

impl From<&Vertex> for Color {
    fn from(value: &Vertex) -> Self {
        Self {
            r: (value.x * 255.).max(255.) as u8,
            g: (value.y * 255.).max(255.) as u8,
            b: (value.z * 255.).max(255.) as u8,
            a: 1,
        }
    }
}
