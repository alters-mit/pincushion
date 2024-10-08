#[cfg(feature = "cs")]
fn main() {
    ::pincushion::cs::generate()
}

#[cfg(not(feature = "cs"))]
fn main() {}
