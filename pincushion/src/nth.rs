//! These functions create a boolean mask for an array of a given size.
//! Every nth value in the mask will be true, and the indices of those values won't vary by step size.

use safer_ffi::ffi_export;

/// Create an array of steps and fill it via `set_nth_steps`.
/// The result can be used with `set_nth_mask`.
pub fn get_nth_steps(num: usize) -> safer_ffi::Vec<u8> {
    let mut steps = vec![0; num].into();
    set_nth_steps(&mut steps);
    steps
}

/// For a hardcoded range of 1 to 100 (inclusive), step through `steps` and increment each stepped value by one.
/// `steps` can be used with `set_nth_mask`.
#[ffi_export]
pub fn set_nth_steps(steps: &mut safer_ffi::Vec<u8>) {
    (1..=100).for_each(|i| {
        steps.iter_mut().step_by(i).for_each(|s| *s += 1);
    });
}

/// Set a vertex mask from the `steps`.
///
/// - `step`: A value between 1 and 100 (inclusive).
///   The mask will have this many values equal to `1`: `steps.len() * step * 0.01`.
/// - `steps`: A precalculated array from `set_nth_steps`.
///   This is used to ensure that the incides of the true values in `mask` don't vary.
/// - `mask` A mask of true/false byte values. Unity wants this to be u32 instead of bool. 
#[ffi_export]
pub fn set_nth_mask(step: usize, steps: &safer_ffi::Vec<u8>, mask: &mut safer_ffi::Vec<u32>) {
    // Reset the mask.
    mask.fill(0);
    // Clamp the step.
    let step = step.clamp(1, 101);
    // Iterate through the mask and the steps.
    mask.chunks_mut(step)
        .zip(steps.chunks(step))
        .for_each(|(mask, steps)| {
            // Get the minimum value in the `steps` chunk and map it to the `mask` chunk.
            // This ensures that the position of the mask values don't change.
            match mask
                .iter_mut()
                .zip(steps.iter())
                .max_by(|(_, s0), (_, s1)| s0.cmp(s1))
            {
                Some((m, _)) => *m = 1,
                None => mask[0] = 1,
            }
        });
}

#[cfg(test)]
mod tests {
    use super::{get_nth_steps, set_nth_mask};

    #[test]
    fn test_nths() {
        let num_points = 997;

        let nth_steps = get_nth_steps(num_points);
        let mut mask = vec![0; num_points].into();
        let step = 10;
        set_nth_mask(step, &nth_steps, &mut mask);

        let num_true = mask.iter().filter(|m| **m == 1).count();
        assert_eq!(
            f32::ceil(num_points as f32 / num_true as f32) as usize,
            step
        );
    }
}
