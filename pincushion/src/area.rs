use crate::Vek;

/// The surface area of a mesh and of each of its triangles.
#[cfg_attr(feature = "ffi", safer_ffi::derive_ReprC)]
#[cfg_attr(feature = "ffi", repr(C))]
pub struct Area {
    /// The total surface area of the mesh.
    pub total_area: f32,
    /// The area of each triangle in the mesh.
    pub areas: Vek<f32>,
}
