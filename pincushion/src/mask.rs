//! These functions create a boolean mask for an array of a given size, and given a factor.
//! For example, if there are 3500 elements in an array and the factor is 0.5, then 1750 of the elements will be "true".
//! "True" is in quotes because we're using u32s, not booleans.
//! We're not using booleans because this is meant to be used in Unity for a rendering shader, and shaders want 32bit types.

use fastrand::Rng;
use safer_ffi::ffi_export;

/// Set the `mask_indices` to index values (0, 1, 2, etc.)
/// Then, shuffle `mask_indices`.
/// `seed` is the random seed.
#[ffi_export]
pub fn set_mask_indices(mask_indices: &mut safer_ffi::Vec<usize>, seed: u64) {
    let mut vec = Vec::from_iter(0..mask_indices.len());
    Rng::with_seed(seed).shuffle(&mut vec);
    mask_indices.copy_from_slice(&vec[0..]);
}

/// Set a vertex mask from the `steps`.
///
/// - `factor`: A value between 0 and 1. The number of "true" values will be `mask.len() * factor`
/// - `mask_indices`: A precalculated array from `set_mask_indices`.
///   This contains all indices in `mask` in a random order.
/// - `mask` The mask array. Unity wants this to be u32 instead of bool so `1` is equivalent to `true`.
#[ffi_export]
pub fn set_mask(factor: f32, mask_indices: &safer_ffi::Vec<usize>, mask: &mut safer_ffi::Vec<u32>) {
    // Reset the mask.
    mask.fill(0);
    // Get the number of true values as a fraction of the size of `mask`.
    let num_true = (mask.len() as f32 * factor.clamp(0., 1.)) as usize;
    mask_indices[0..num_true].iter().for_each(|i| mask[*i] = 1);
}

#[cfg(test)]
mod tests {
    use super::{set_mask, set_mask_indices};

    #[test]
    fn test_nths() {
        let num_points = 997;
        let mut mask_indices = vec![0; num_points].into();
        set_mask_indices(&mut mask_indices, 0);
        let mut mask = vec![0; num_points].into();
        for factor in [0., 0.1, 0.5, 1.] {
            set_mask(factor, &mask_indices, &mut mask);

            let num_true: usize = mask.iter().filter(|m| **m == 1).count();
            assert_eq!((factor * num_points as f32) as usize, num_true);
        }
    }
}
