use safer_ffi::headers::{builder, Language::CSharp};

/// Generate native bindings.
pub fn generate() {
    builder()
        .with_language(CSharp)
        .to_file(&format!(
            "../com.mit.pincushion/Runtime/PincushionNativeBindings.cs"
        ))
        .unwrap()
        .generate()
        .unwrap();
}
