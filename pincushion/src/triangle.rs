/// Indices of three vertices that comprise a triangle in a mesh.
#[cfg_attr(feature = "ffi", safer_ffi::derive_ReprC)]
#[cfg_attr(feature = "ffi", repr(C))]
#[derive(Copy, Clone, Default)]
pub struct Triangle {
    pub a: usize,
    pub b: usize,
    pub c: usize,
}

impl From<&[u32]> for Triangle {
    fn from(value: &[u32]) -> Self {
        Self {
            a: value[0] as usize,
            b: value[1] as usize,
            c: value[2] as usize,
        }
    }
}
