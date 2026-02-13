use safer_ffi::headers::{Language::CSharp, builder};

/// Generate native bindings.
pub fn generate() {
    builder()
        .with_language(CSharp)
        .to_file("../com.mit.pincushion/Runtime/PincushionNativeBindings.cs")
        .unwrap()
        .generate()
        .unwrap();
}
