#[safer_ffi::derive_ReprC]
#[derive(Copy, Clone, Default)]
#[repr(C)]
pub struct Triangle {
    pub a: usize,
    pub b: usize,
    pub c: usize,
}

impl Triangle {
    pub fn add_mut(&mut self, value: usize) {
        self.a += value;
        self.b += value;
        self.c += value;
    }
}
