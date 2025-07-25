/// The surface area of a mesh and of its triangles.
#[cfg(feature = "ffi")]
#[safer_ffi::derive_ReprC]
#[repr(C)]
pub struct Area {
    /// The total surface area of the mesh in square meters.
    pub total_area: f32,
    /// The area of each triangle in the mesh in square meters.
    pub areas: safer_ffi::Vec<f32>,
}

/// The surface area of a mesh and of its triangles.
#[cfg(not(feature = "ffi"))]
pub struct Area {
    /// The total surface area of the mesh in square meters.
    pub total_area: f32,
    /// The area of each triangle in the mesh in square meters.
    pub areas: Vec<f32>,
}
