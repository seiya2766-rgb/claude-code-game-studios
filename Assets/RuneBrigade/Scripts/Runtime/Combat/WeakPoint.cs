using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// Put on a child collider (head, ogre chest, giant's core). Hits that land on
    /// it are multiplied. Design: head ×2.5, weak point ×3.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeakPoint : MonoBehaviour
    {
        [Tooltip("Damage multiplier for hits on this collider.")]
        [Min(1f)] public float multiplier = 3f;
    }
}
