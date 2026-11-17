using System;
using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// The player's single health bar (role switches change only its multiplier),
    /// dash invulnerability, the Warden's guard, and the solo one-time revive.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private CombatTuning tuning;
        [SerializeField] private FirstPersonMotor motor;
        [Tooltip("Solo play: revive once per run. Design: ルーンの加護.")]
        [SerializeField] private bool soloRevive = true;
        [Range(0.05f, 1f)] [SerializeField] private float reviveFraction = 0.5f;

        private HealthPool _pool;
        private bool _guarding;
        private float _guardReduction;
        private float _guardHalfAngle;
        private float _justGuardUntil;
        private bool _reviveUsed;

        /// <summary>The health pool.</summary>
        public HealthPool Pool => _pool;

        /// <summary>True while guarding.</summary>
        public bool IsGuarding => _guarding;

        /// <summary>True when the solo revive has been spent.</summary>
        public bool ReviveUsed => _reviveUsed;

        /// <summary>Raised when an attack meets the guard: attacker, was it a just guard.</summary>
        public event Action<EnemyBrain, bool> Guarded;

        /// <summary>Raised with the damage actually taken.</summary>
        public event Action<float> DamageTaken;

        /// <summary>Raised when the player is down for good.</summary>
        public event Action Defeated;

        private void Awake()
        {
            _pool = new HealthPool(tuning.playerBaseHealth);
        }

        /// <summary>Raises or lowers the guard. Raising starts the just-guard window.</summary>
        public void SetGuard(bool active, float reduction, float halfAngle, float justGuardWindow)
        {
            if (active && !_guarding) _justGuardUntil = Time.time + justGuardWindow;
            _guarding = active;
            _guardReduction = reduction;
            _guardHalfAngle = halfAngle;
        }

        /// <summary>An enemy's attack lands on the player.</summary>
        /// <param name="damage">Attack damage.</param>
        /// <param name="attacker">The enemy, for guard direction and reflection.</param>
        public void ReceiveEnemyAttack(float damage, EnemyBrain attacker)
        {
            if (_pool.IsDead || (motor && motor.IsInvulnerable)) return;

            if (_guarding && attacker && IsInFront(attacker.transform.position))
            {
                bool just = Time.time <= _justGuardUntil;
                Guarded?.Invoke(attacker, just);
                if (just) return;
                damage *= 1f - _guardReduction;
            }

            float taken = _pool.Damage(damage);
            if (taken > 0f) DamageTaken?.Invoke(taken);

            if (_pool.IsDead)
            {
                if (soloRevive && !_reviveUsed)
                {
                    _reviveUsed = true;
                    _pool.Revive(reviveFraction);
                }
                else
                {
                    Defeated?.Invoke();
                }
            }
        }

        private bool IsInFront(Vector3 point)
        {
            Vector3 to = point - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 1e-4f) return true;
            return Vector3.Angle(transform.forward, to) <= _guardHalfAngle;
        }
    }
}
