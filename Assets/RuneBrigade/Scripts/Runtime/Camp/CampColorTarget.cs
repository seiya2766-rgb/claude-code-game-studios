using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// Fades renderers from a drained colour to their restored colour as names
    /// return. Follows the camp's overall restoration, or one feature (the village
    /// roofs turn coloured when its mist clears). Uses a MaterialPropertyBlock, so
    /// materials are shared and untouched.
    /// </summary>
    public sealed class CampColorTarget : MonoBehaviour
    {
        private enum Source { Overall, Feature }

        [SerializeField] private Renderer[] renderers;
        [Tooltip("Overall = the camp's restoration, 0 to 1. Feature = drained until that feature returns.")]
        [SerializeField] private Source source = Source.Overall;
        [SerializeField] private CampFeature feature = CampFeature.VillageMistCleared;
        [Tooltip("Colour with no names restored (art bible: the drained world).")]
        [SerializeField] private Color drainedColor = new Color(0.369f, 0.392f, 0.400f);
        [Tooltip("Colour once fully restored.")]
        [SerializeField] private Color restoredColor = new Color(0.557f, 0.353f, 0.235f);
        [Tooltip("Seconds to fade when the value changes while playing.")]
        [Min(0f)] [SerializeField] private float fadeSeconds = 1.5f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock _block;
        private float _current;
        private float _target;

        /// <summary>Sets the target from <paramref name="progress"/>; <paramref name="instant"/> skips the fade.</summary>
        public void Apply(CampProgress progress, bool instant)
        {
            _target = source == Source.Overall ? progress.Restoration : (progress.IsUnlocked(feature) ? 1f : 0f);
            if (instant)
            {
                _current = _target;
                Paint();
            }
        }

        private void Update()
        {
            if (Mathf.Approximately(_current, _target)) return;
            float step = fadeSeconds <= 0f ? 1f : Time.unscaledDeltaTime / fadeSeconds;
            _current = Mathf.MoveTowards(_current, _target, step);
            Paint();
        }

        private void Paint()
        {
            if (renderers == null) return;
            if (_block == null) _block = new MaterialPropertyBlock();
            Color color = Color.Lerp(drainedColor, restoredColor, _current);
            foreach (Renderer r in renderers)
            {
                if (!r) continue;
                r.GetPropertyBlock(_block);
                _block.SetColor(BaseColorId, color);
                _block.SetColor(ColorId, color);
                r.SetPropertyBlock(_block);
            }
        }
    }
}
