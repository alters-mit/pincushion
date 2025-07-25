#[cfg_attr(feature = "ffi", derive_ReprC)]
#[cfg_attr(feature = "ffi", repr(C))]
#[derive(Copy, Clone, Default)]
pub struct Triangle {
    pub a: usize,
    pub b: usize,
    pub c: usize,
}
