#[safer_ffi::derive_ReprC]
#[derive(Copy, Clone, Default)]
#[repr(C)]
pub struct Triangle {
    pub a: usize,
    pub b: usize,
    pub c: usize,
}