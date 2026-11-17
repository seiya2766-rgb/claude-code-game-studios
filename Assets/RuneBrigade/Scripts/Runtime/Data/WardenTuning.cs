using UnityEngine;

namespace RuneBrigade
{
    /// <summary>守護者 numbers. Defaults: design/game-concept.md, "4つの役割".</summary>
    [CreateAssetMenu(menuName = "Rune Brigade/Warden Tuning", fileName = "WardenTuning")]
    public sealed class WardenTuning : ScriptableObject
    {
        [Header("Hammer string (left click)")]
        [Tooltip("Damage of each hit in the string. Design: 40 / 40 / 80.")]
        public float[] comboDamage = { 40f, 40f, 80f };
        [Tooltip("Minimum seconds between hits.")]
        [Min(0f)] public float hitInterval = 0.35f;
        [Tooltip("Seconds after a hit in which the next press continues the string.")]
        [Min(0f)] public float continueWindow = 0.9f;
        [Tooltip("Reach of the hammer in metres.")]
        [Min(0.1f)] public float meleeRange = 2.6f;
        [Tooltip("Radius of the hammer's hit sphere.")]
        [Min(0.1f)] public float meleeRadius = 1.4f;
        [Tooltip("Height the finisher launches enemies.")]
        [Min(0f)] public float finisherLaunchHeight = 2.5f;

        [Header("Guard (right click hold)")]
        [Tooltip("Frontal damage reduction while guarding. Design: 90%.")]
        [Range(0f, 1f)] public float guardReduction = 0.9f;
        [Tooltip("Seconds after raising the guard that count as a just guard. Design: 0.2 s.")]
        [Min(0f)] public float justGuardWindow = 0.2f;
        [Tooltip("Half-angle of the guarded cone in degrees.")]
        [Range(0f, 180f)] public float guardHalfAngle = 70f;
        [Tooltip("Damage sent back to the attacker on a just guard (tag ShieldGuard).")]
        [Min(0f)] public float reflectDamage = 60f;
        [Tooltip("Seconds a just-guarded enemy is staggered.")]
        [Min(0f)] public float justGuardStagger = 1.2f;

        [Header("Roar (Q)")]
        [Tooltip("Pull radius. Design: 8 m.")]
        [Min(0.1f)] public float roarRadius = 8f;
        [Tooltip("Cooldown. Design: 12 s.")]
        [Min(0f)] public float roarCooldown = 12f;
        [Tooltip("Seconds of 集結. Design: 4 s.")]
        [Min(0.1f)] public float gatherDuration = 4f;
        [Tooltip("Metres in front of the player where enemies are pulled.")]
        [Min(0f)] public float pullPointDistance = 4f;
        [Tooltip("Pull speed in metres per second.")]
        [Min(0f)] public float pullSpeed = 9f;
        [Tooltip("Seconds the pull lasts.")]
        [Min(0f)] public float pullDuration = 0.6f;

        [Header("Shield charge (E)")]
        [Tooltip("Cooldown. Design: 8 s.")]
        [Min(0f)] public float chargeCooldown = 8f;
        [Tooltip("Charge distance in metres.")]
        [Min(0f)] public float chargeDistance = 7f;
        [Tooltip("Seconds the charge takes.")]
        [Min(0.01f)] public float chargeTime = 0.25f;
        [Tooltip("Damage to enemies in the path.")]
        [Min(0f)] public float chargeDamage = 50f;
        [Tooltip("Width of the charge's hit area.")]
        [Min(0.1f)] public float chargeRadius = 1.6f;

        [Header("Ultimate: Earthbreaker (R)")]
        [Tooltip("Radius. Design: 10 m.")]
        [Min(0.1f)] public float ultimateRadius = 10f;
        [Tooltip("Damage to each enemy.")]
        [Min(0f)] public float ultimateDamage = 150f;
        [Tooltip("Launch height.")]
        [Min(0f)] public float ultimateLaunchHeight = 4f;
    }
}
