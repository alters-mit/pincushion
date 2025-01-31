namespace Pincushion
{
    /// <summary>
    /// How Pincushion renders sampled meshes.
    /// </summary>
    public enum PincushionRenderMode : byte
    {
        /// <summary>
        /// Hide the sampled meshes.
        /// Show the source meshes.
        /// Show the original camera background.
        /// </summary>
        DoNot = 0,
        /// <summary>
        /// Show the sampled meshes.
        /// Show the source meshes.
        /// </summary>
        WithSourceMeshes = 1,
        /// <summary>
        /// Show the sampled meshes.
        /// Hide the source meshes.
        /// </summary>
        WithoutSourceMeshes = 2,
        /// <summary>
        /// Hide sampled points facing away from the camera.
        /// Hide the source meshes.
        /// </summary>
        HideBackfacing = 3,
        /// <summary>
        /// Hide sampled points that would be behind source meshes.
        /// Hide the source meshes.
        /// </summary>
        OccludeBehind = 4
    }
}