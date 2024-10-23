use safer_ffi::prelude::derive_ReprC;

/// The surface area of a mesh and of its triangles.
#[derive_ReprC]
#[repr(C)]
pub struct Area {
    /// The total surface area of the mesh in square meters.
    pub total_area: f32,
    /// The area of each triangle in the mesh in square meters.
    pub areas: safer_ffi::Vec<f32>,
}
