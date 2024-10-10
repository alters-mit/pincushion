use safer_ffi::headers::{builder, Language::CSharp};

/// Generate native bindings.
pub fn generate() {
    builder()
        .with_language(CSharp)
        .to_file(&format!("../PincushionCs/PincushionCs/PincushionNativeBindings.cs"))
        .unwrap()
        .generate()
        .unwrap();
}
