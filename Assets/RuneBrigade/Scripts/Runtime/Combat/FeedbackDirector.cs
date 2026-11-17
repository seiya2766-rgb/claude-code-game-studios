using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// Owns <see cref="Time.timeScale"/> for hit stop and fusion slow motion, so the
    /// two never fight. Times are real seconds. Nothing else should write timeScale.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FeedbackDirector : MonoBehaviour
    {
        [Tooltip("Optional camera shaker; leave empty to skip shake.")]
        [SerializeField] private CameraShake cameraShake;

        private float _hitStopUntil;
        private float _slowUntil;
        private float _slowScale = 1f;

        /// <summary>Freezes time for <paramref name="seconds"/> (keeps the longest request).</summary>
        public void HitStop(float seconds)
        {
            if (seconds <= 0f) return;
            _hitStopUntil = Mathf.Max(_hitStopUntil, Time.unscaledTime + seconds);
        }

        /// <summary>Slows time to <paramref name="scale"/> for <paramref name="seconds"/>.</summary>
        public void SlowMotion(float scale, float seconds)
        {
            if (seconds <= 0f) return;
            _slowScale = Mathf.Clamp(scale, 0.05f, 1f);
            _slowUntil = Mathf.Max(_slowUntil, Time.unscaledTime + seconds);
        }

        /// <summary>Shakes the camera if a shaker is assigned.</summary>
        public void Shake(float amplitude, float seconds)
        {
            if (cameraShake) cameraShake.Shake(amplitude, seconds);
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            if (now < _hitStopUntil) Time.timeScale = 0f;
            else if (now < _slowUntil) Time.timeScale = _slowScale;
            else Time.timeScale = 1f;
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
        }
    }
}
