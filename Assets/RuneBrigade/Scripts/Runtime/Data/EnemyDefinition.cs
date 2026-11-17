using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>One enemy type. Defaults are the 群れゴブリン row of design/game-concept.md, "雑魚".</summary>
    [CreateAssetMenu(menuName = "Rune Brigade/Enemy Definition", fileName = "Enemy_")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Tooltip("Name for debugging.")]
        public string displayName = "群れゴブリン";
        [Tooltip("Prefab with EnemyBrain, EnemyHealth, NavMeshAgent and a collider.")]
        public EnemyBrain prefab;

        [Header("Stats")]
        [Tooltip("Health for one player. Goblin 60, skeleton 200, ogre 1200.")]
        [Min(1f)] public float maxHealth = 60f;
        [Tooltip("Move speed in metres per second.")]
        [Min(0f)] public float moveSpeed = 4.5f;

        [Header("Attack")]
        [Tooltip("Damage of one attack. Goblin 25, skeleton 50, ogre 120.")]
        [Min(0f)] public float attackDamage = 25f;
        [Tooltip("Distance at which it starts an attack.")]
        [Min(0.1f)] public float attackRange = 1.8f;
        [Tooltip("Warning time before the hit lands. Design: 0.6 to 1.0 s.")]
        [Min(0f)] public float telegraphSeconds = 0.6f;
        [Tooltip("Recovery after the attack: the opening to punish. Skeleton: 1 s.")]
        [Min(0f)] public float recoverSeconds = 0.8f;
        [Tooltip("Which token pool it draws from.")]
        public AttackKind attackKind = AttackKind.Melee;

        [Header("Look")]
        [Tooltip("Body colour while telegraphing (侵食の暗い紫 brightened).")]
        public Color telegraphColor = new Color(0.769f, 0.420f, 1f);
    }
}
