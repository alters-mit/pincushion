use crate::Floats;

/// The surface area of a mesh and of its triangles.
#[cfg_attr(feature = "ffi", derive_ReprC)]
#[cfg_attr(feature = "ffi", repr(C))]
pub struct Area {
    /// The total surface area of the mesh in square meters.
    pub total_area: f32,
    /// The area of each triangle in the mesh in square meters.
    pub areas: Floats,
}
