using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// Shows <see cref="presentRoot"/> when a feature has returned and
    /// <see cref="absentRoot"/> when it has not (an empty tent, a broken tower,
    /// the mist over the village).
    /// </summary>
    public sealed class CampFeatureView : MonoBehaviour
    {
        [SerializeField] private CampFeature feature;
        [Tooltip("Shown once the feature is back (the NPC, the repaired tower).")]
        [SerializeField] private GameObject presentRoot;
        [Tooltip("Shown while it is not (an empty tent, the mist). Optional.")]
        [SerializeField] private GameObject absentRoot;

        /// <summary>Which feature this object stands for.</summary>
        public CampFeature Feature => feature;

        /// <summary>Shows the right objects for <paramref name="progress"/>.</summary>
        public void Apply(CampProgress progress)
        {
            bool back = progress.IsUnlocked(feature);
            if (presentRoot) presentRoot.SetActive(back);
            if (absentRoot) absentRoot.SetActive(!back);
        }
    }
}
