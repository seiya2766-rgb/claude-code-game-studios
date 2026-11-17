using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// Shared combat numbers. Every default comes from design/game-concept.md
    /// ("戦闘と操作" and "敵とボス"); change them on the asset, not in code.
    /// </summary>
    [CreateAssetMenu(menuName = "Rune Brigade/Combat Tuning", fileName = "CombatTuning")]
    public sealed class CombatTuning : ScriptableObject
    {
        [Header("Statuses")]
        [Tooltip("Seconds a status lasts unless the ability says otherwise. Design: 4 s.")]
        [Min(0.1f)] public float defaultStatusDuration = 4f;
        [Tooltip("Statuses an enemy can carry at once. Design: 2.")]
        [Min(1)] public int maxStatusesPerEnemy = 2;
        [Tooltip("Buildup points needed for a bolt to apply its status.")]
        [Min(1f)] public float statusBuildupThreshold = 100f;
        [Tooltip("Buildup points lost per second.")]
        [Min(0f)] public float statusBuildupDecayPerSecond = 15f;

        [Header("Role switching")]
        [Tooltip("Seconds a role switch takes. Design: 0.4 s.")]
        [Min(0f)] public float switchDuration = 0.4f;
        [Tooltip("Seconds after a switch in which the first attack is boosted. Design: 2 s.")]
        [Min(0f)] public float switchStrikeWindow = 2f;
        [Tooltip("Switch-strike bonus as a fraction. Design: +50% = 0.5.")]
        [Min(0f)] public float switchStrikeBonus = 0.5f;

        [Header("Player")]
        [Tooltip("Player health at multiplier 1. Design: 500.")]
        [Min(1f)] public float playerBaseHealth = 500f;
        [Tooltip("Dash stock. Design: 2.")]
        [Min(1)] public int dashCharges = 2;
        [Tooltip("Seconds to refill one dash. Design: 3 s.")]
        [Min(0.1f)] public float dashRechargeSeconds = 3f;
        [Tooltip("Invulnerability per dash. Design: 0.2 s.")]
        [Min(0f)] public float dashInvulnerableSeconds = 0.2f;
        [Tooltip("Ultimate gauge size.")]
        [Min(1f)] public float ultimateGaugeMax = 1000f;
        [Tooltip("Gauge points per point of damage dealt.")]
        [Min(0f)] public float ultimatePerDamage = 0.5f;
        [Tooltip("Extra gauge points per fusion.")]
        [Min(0f)] public float ultimatePerCombo = 60f;
        [Tooltip("Gauge points per point of damage taken.")]
        [Min(0f)] public float ultimatePerDamageTaken = 0.3f;

        [Header("Hit feel (seconds, real time)")]
        [Tooltip("Hit stop on ordinary hits. Design: 30 ms.")]
        [Min(0f)] public float hitStopLight = 0.03f;
        [Tooltip("Hit stop on heavy hits (Warden finisher). Design: 80 ms.")]
        [Min(0f)] public float hitStopHeavy = 0.08f;
        [Tooltip("Hit stop on a fusion. Design: 120 ms.")]
        [Min(0f)] public float hitStopCombo = 0.12f;
        [Tooltip("Slow motion length after a fusion. Design: 0.2 s.")]
        [Min(0f)] public float comboSlowDuration = 0.2f;
        [Tooltip("Time scale during that slow motion. Design: 30% = 0.3.")]
        [Range(0.05f, 1f)] public float comboSlowTimeScale = 0.3f;

        [Header("Enemies")]
        [Tooltip("Enemies that may melee the player at once. Design: 3.")]
        [Min(0)] public int meleeAttackTokens = 3;
        [Tooltip("Enemies that may shoot the player at once. Design: 2.")]
        [Min(0)] public int rangedAttackTokens = 2;

        [Header("Party scaling")]
        [Tooltip("Enemy health per extra player. Design: 0.6.")]
        [Min(0f)] public float enemyHealthPerExtraPlayer = 0.6f;
        [Tooltip("Enemy count per extra player. Design: 0.4.")]
        [Min(0f)] public float enemyCountPerExtraPlayer = 0.4f;
    }
}
