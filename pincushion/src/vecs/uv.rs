#[safer_ffi::derive_ReprC]
#[derive(Copy, Clone, Default)]
#[repr(C)]
pub struct Uv {
    pub x: f32,
    pub y: f32,
}
