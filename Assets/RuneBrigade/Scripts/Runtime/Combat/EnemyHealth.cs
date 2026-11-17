using System;
using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// An enemy's health and statuses. Abilities call <see cref="ReceiveHit"/>; this
    /// resolves fusions through the <see cref="CombatDirector"/> and reports the result.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyHealth : MonoBehaviour
    {
        private CombatDirector _director;
        private HealthPool _health;
        private StatusContainer _statuses;
        private StatusBuildup _buildup;

        /// <summary>The enemy's statuses (read by its brain: frozen enemies stop).</summary>
        public StatusContainer Statuses => _statuses;

        /// <summary>The enemy's health pool.</summary>
        public HealthPool Health => _health;

        /// <summary>True once initialised and still alive.</summary>
        public bool IsAlive => _health != null && !_health.IsDead;

        /// <summary>Raised when the enemy dies.</summary>
        public event Action<EnemyHealth> Died;

        /// <summary>Sets the enemy up. Called by the spawner right after instantiation.</summary>
        /// <param name="director">The scene's combat director.</param>
        /// <param name="maxHealth">Health after party scaling.</param>
        public void Initialize(CombatDirector director, float maxHealth)
        {
            _director = director ? director : throw new ArgumentNullException(nameof(director));
            CombatTuning tuning = director.Tuning;
            _health = new HealthPool(maxHealth);
            _statuses = new StatusContainer(tuning.maxStatusesPerEnemy);
            _buildup = new StatusBuildup(tuning.statusBuildupThreshold, tuning.statusBuildupDecayPerSecond);
            _health.Died += OnDied;
        }

        /// <summary>Applies one hit: fusion check, damage, then statuses (which may fire a status fusion).</summary>
        public HitResult ReceiveHit(HitInfo hit)
        {
            var result = new HitResult();
            if (!IsAlive) return result;

            if (_director.Resolver.TryResolveHit(_statuses, hit.Tag, out ComboRule hitCombo))
            {
                result.Combo = hitCombo;
            }

            float comboMultiplier = result.Combo != null ? result.Combo.DamageMultiplier : 1f;
            float damage = DamageCalculator.Compute(hit.BaseDamage, hit.WeakPointMultiplier, hit.SwitchStrikeMultiplier, comboMultiplier);
            result.DamageDealt += _health.Damage(damage);

            if (IsAlive) ApplyStatuses(hit, ref result);

            result.Killed = !IsAlive;
            _director.ReportHit(this, hit, result);
            return result;
        }

        /// <summary>Damage from a fusion's splash. Cannot itself start a fusion.</summary>
        public float ReceiveSplash(float damage)
        {
            if (!IsAlive) return 0f;
            return _health.Damage(damage);
        }

        private void ApplyStatuses(HitInfo hit, ref HitResult result)
        {
            float duration = hit.StatusDuration > 0f ? hit.StatusDuration : _director.Tuning.defaultStatusDuration;
            bool applied = false;

            if (hit.AppliesStatus)
            {
                _statuses.Apply(hit.Status, duration);
                applied = true;
            }
            if (hit.BuildupPoints > 0f && _buildup.Add(hit.BuildupStatus, hit.BuildupPoints))
            {
                _statuses.Apply(hit.BuildupStatus, _director.Tuning.defaultStatusDuration);
                applied = true;
            }

            if (applied && result.Combo == null && _director.Resolver.TryResolveStatus(_statuses, out ComboRule statusCombo))
            {
                result.Combo = statusCombo;
                result.DamageDealt += _health.Damage(statusCombo.BaseDamage * statusCombo.DamageMultiplier);
            }
        }

        private void Update()
        {
            if (!IsAlive) return;
            _statuses.Tick(Time.deltaTime);
            _buildup.Tick(Time.deltaTime);
        }

        private void OnDied()
        {
            _statuses.Clear();
            Died?.Invoke(this);
        }
    }
}
