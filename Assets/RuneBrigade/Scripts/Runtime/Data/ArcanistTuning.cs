using UnityEngine;

namespace RuneBrigade
{
    /// <summary>術士 numbers. Defaults: design/game-concept.md, "4つの役割".</summary>
    [CreateAssetMenu(menuName = "Rune Brigade/Arcanist Tuning", fileName = "ArcanistTuning")]
    public sealed class ArcanistTuning : ScriptableObject
    {
        [Header("Element bolt (left click hold)")]
        [Tooltip("Damage per bolt. Design: 30.")]
        [Min(0f)] public float boltDamage = 30f;
        [Tooltip("Bolts per second.")]
        [Min(0.1f)] public float boltsPerSecond = 6f;
        [Tooltip("Range in metres.")]
        [Min(1f)] public float boltRange = 60f;
        [Tooltip("Buildup points each bolt adds toward its element's status.")]
        [Min(0f)] public float boltBuildup = 25f;

        [Header("Element blast (Q)")]
        [Tooltip("Cooldown. Design: 10 s.")]
        [Min(0f)] public float blastCooldown = 10f;
        [Tooltip("Damage to each enemy.")]
        [Min(0f)] public float blastDamage = 60f;
        [Tooltip("Radius in metres.")]
        [Min(0.1f)] public float blastRadius = 4f;
        [Tooltip("How far away the blast can be aimed.")]
        [Min(1f)] public float blastRange = 30f;

        [Header("Blink (E)")]
        [Tooltip("Cooldown. Design: 8 s.")]
        [Min(0f)] public float blinkCooldown = 8f;
        [Tooltip("Distance in metres.")]
        [Min(0f)] public float blinkDistance = 8f;

        [Header("Ultimate: Meteor (R)")]
        [Tooltip("Damage to each enemy. Design: 500.")]
        [Min(0f)] public float meteorDamage = 500f;
        [Tooltip("Radius in metres.")]
        [Min(0.1f)] public float meteorRadius = 6f;
        [Tooltip("Seconds from cast to impact.")]
        [Min(0f)] public float meteorDelay = 0.8f;
        [Tooltip("How far away the meteor can be aimed.")]
        [Min(1f)] public float meteorRange = 40f;
        [Tooltip("Optional marker spawned at the impact point during the delay.")]
        public GameObject meteorMarkerPrefab;
    }
}
