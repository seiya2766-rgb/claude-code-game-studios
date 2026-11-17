using System;
using System.Collections.Generic;
using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// One per scene. Holds the fusion resolver and the attack-token pool, applies
    /// fusion splash and hit feel, and raises events the HUD and player listen to.
    /// Referenced explicitly (inspector or Initialize calls), never through a singleton.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatDirector : MonoBehaviour
    {
        [SerializeField] private CombatTuning tuning;
        [SerializeField] private ComboTable comboTable;
        [SerializeField] private FeedbackDirector feedback;
        [Tooltip("Layers that hold enemy colliders.")]
        [SerializeField] private LayerMask enemyLayers = ~0;

        private readonly Collider[] _overlap = new Collider[128];
        private readonly HashSet<EnemyHealth> _uniqueTargets = new HashSet<EnemyHealth>();
        private ComboResolver _resolver;
        private AttackTokenPool _tokens;

        /// <summary>Shared tuning.</summary>
        public CombatTuning Tuning => tuning;

        /// <summary>The fusion table.</summary>
        public ComboTable ComboTable => comboTable;

        /// <summary>Fusion resolver built from <see cref="ComboTable"/>.</summary>
        public ComboResolver Resolver => _resolver;

        /// <summary>Attack tokens shared by every enemy.</summary>
        public AttackTokenPool Tokens => _tokens;

        /// <summary>Layers that hold enemy colliders.</summary>
        public LayerMask EnemyLayers => enemyLayers;

        /// <summary>Raised for every landed hit.</summary>
        public event Action<EnemyHealth, HitInfo, HitResult> HitLanded;

        /// <summary>Raised when a fusion fires: rule, table row, world position.</summary>
        public event Action<ComboRule, ComboTable.Entry, Vector3> ComboTriggered;

        private void Awake()
        {
            if (!tuning || !comboTable) throw new InvalidOperationException("CombatDirector needs CombatTuning and ComboTable assets.");
            _resolver = new ComboResolver(comboTable.BuildRules());
            _tokens = new AttackTokenPool(tuning.meleeAttackTokens, tuning.rangedAttackTokens);
        }

        /// <summary>Called by <see cref="EnemyHealth.ReceiveHit"/> after a hit resolves.</summary>
        public void ReportHit(EnemyHealth target, HitInfo hit, HitResult result)
        {
            if (result.Combo != null)
            {
                ComboTable.Entry entry = comboTable.Find(result.Combo.Id);
                Vector3 position = target ? target.transform.position : hit.Point;
                if (entry != null && entry.splashRadius > 0f) Splash(target, position, entry.splashRadius, result.DamageDealt);

                if (feedback)
                {
                    feedback.HitStop(tuning.hitStopCombo);
                    feedback.SlowMotion(tuning.comboSlowTimeScale, tuning.comboSlowDuration);
                    feedback.Shake(0.15f, 0.25f);
                }
                ComboTriggered?.Invoke(result.Combo, entry, position);
            }
            else if (feedback && result.DamageDealt > 0f)
            {
                feedback.HitStop(hit.IsHeavy ? tuning.hitStopHeavy : tuning.hitStopLight);
                if (hit.IsHeavy) feedback.Shake(0.08f, 0.15f);
            }

            HitLanded?.Invoke(target, hit, result);
        }

        /// <summary>
        /// Finds living enemies within <paramref name="radius"/> of <paramref name="center"/>,
        /// one entry per enemy even if it has several colliders.
        /// </summary>
        public void FindEnemies(Vector3 center, float radius, List<EnemyHealth> results)
        {
            results.Clear();
            _uniqueTargets.Clear();
            int count = Physics.OverlapSphereNonAlloc(center, radius, _overlap, enemyLayers, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                EnemyHealth enemy = _overlap[i].GetComponentInParent<EnemyHealth>();
                if (enemy && enemy.IsAlive && _uniqueTargets.Add(enemy)) results.Add(enemy);
            }
        }

        private readonly List<EnemyHealth> _splashTargets = new List<EnemyHealth>();

        private void Splash(EnemyHealth origin, Vector3 center, float radius, float damage)
        {
            FindEnemies(center, radius, _splashTargets);
            foreach (EnemyHealth enemy in _splashTargets)
            {
                if (enemy != origin) enemy.ReceiveSplash(damage);
            }
        }
    }
}
