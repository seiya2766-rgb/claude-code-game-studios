using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// Shakes a child transform of the camera rig (put the Camera under it).
    /// Runs on real time so it still moves during hit stop.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraShake : MonoBehaviour
    {
        private float _amplitude;
        private float _until;
        private float _duration;

        /// <summary>Starts or strengthens a shake.</summary>
        public void Shake(float amplitude, float seconds)
        {
            if (amplitude <= 0f || seconds <= 0f) return;
            _amplitude = Mathf.Max(_amplitude * Remaining01(), amplitude);
            _duration = seconds;
            _until = Time.unscaledTime + seconds;
        }

        private float Remaining01()
        {
            if (_duration <= 0f) return 0f;
            return Mathf.Clamp01((_until - Time.unscaledTime) / _duration);
        }

        private void LateUpdate()
        {
            float strength = _amplitude * Remaining01();
            if (strength <= 0f)
            {
                transform.localPosition = Vector3.zero;
                return;
            }
            float t = Time.unscaledTime * 40f;
            transform.localPosition = new Vector3(
                (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f * strength,
                (Mathf.PerlinNoise(0f, t) - 0.5f) * 2f * strength,
                0f);
        }
    }
}
