using UnityEngine;

namespace Pincushion
{
    public abstract class PincushionGenerator : MonoBehaviour
    {
        /// <summary>
        /// The number of points per square meter.
        /// </summary>
        public float pointsPerM = 0.015f;
        /// <summary>
        /// The radius of each point in meters.
        /// </summary>
        public float pointRadius = 0.02f;
        /// <summary>
        /// What to do with the points once they've been sampled.
        /// </summary>
        public PincushionStaticCreationMode mode = PincushionStaticCreationMode.replace;


        protected abstract void Awake();
    }
}