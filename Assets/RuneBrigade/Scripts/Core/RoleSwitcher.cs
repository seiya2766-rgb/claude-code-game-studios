using System;
using System.Collections.Generic;

namespace RuneBrigade.Core
{
    /// <summary>
    /// Switching between roles in solo play. A switch takes <see cref="SwitchDuration"/>
    /// seconds; afterwards the first attack within <see cref="StrikeWindow"/> seconds
    /// gets the switch-strike bonus. Rules: design/game-concept.md, "役割の切り替え".
    /// </summary>
    public sealed class RoleSwitcher
    {
        private readonly List<RoleId> _available;
        private float _switchTimer;
        private float _strikeTimer;
        private bool _strikeArmed;

        /// <summary>Creates a switcher.</summary>
        /// <param name="available">Roles this player may use (solo: all four; co-op: their share).</param>
        /// <param name="startRole">Role to start in; must be available.</param>
        /// <param name="switchDuration">Seconds a switch takes.</param>
        /// <param name="strikeWindow">Seconds after a switch in which the bonus applies.</param>
        /// <param name="strikeBonus">Bonus as a fraction, e.g. 0.5 = +50%.</param>
        public RoleSwitcher(IEnumerable<RoleId> available, RoleId startRole, float switchDuration, float strikeWindow, float strikeBonus)
        {
            if (available == null) throw new ArgumentNullException(nameof(available));
            _available = new List<RoleId>(available);
            if (!_available.Contains(startRole)) throw new ArgumentException("Start role must be available.", nameof(startRole));
            if (switchDuration < 0f || strikeWindow < 0f || strikeBonus < 0f) throw new ArgumentOutOfRangeException(nameof(switchDuration), "Timings and bonus must not be negative.");

            Current = startRole;
            SwitchDuration = switchDuration;
            StrikeWindow = strikeWindow;
            StrikeBonus = strikeBonus;
        }

        /// <summary>The role in use (during a switch, the role being switched to).</summary>
        public RoleId Current { get; private set; }

        /// <summary>The role before the last switch.</summary>
        public RoleId Previous { get; private set; }

        /// <summary>Roles this player may use.</summary>
        public IReadOnlyList<RoleId> Available => _available;

        /// <summary>Seconds a switch takes.</summary>
        public float SwitchDuration { get; set; }

        /// <summary>Seconds after a switch in which the bonus applies.</summary>
        public float StrikeWindow { get; set; }

        /// <summary>Switch-strike bonus as a fraction.</summary>
        public float StrikeBonus { get; set; }

        /// <summary>True while the switch animation plays; the player cannot attack.</summary>
        public bool IsSwitching => _switchTimer > 0f;

        /// <summary>True when the next attack gets the switch-strike bonus.</summary>
        public bool IsStrikeReady => _strikeArmed && !IsSwitching && _strikeTimer > 0f;

        /// <summary>Raised when a switch starts: (from, to).</summary>
        public event Action<RoleId, RoleId> SwitchStarted;

        /// <summary>Raised when a switch finishes, with the new role.</summary>
        public event Action<RoleId> SwitchCompleted;

        /// <summary>Starts switching to <paramref name="role"/>.</summary>
        /// <returns>False if the role is current, unavailable, or a switch is already playing.</returns>
        public bool TrySwitch(RoleId role)
        {
            if (role == Current || IsSwitching || !_available.Contains(role)) return false;

            Previous = Current;
            Current = role;
            _strikeArmed = false;
            _switchTimer = SwitchDuration;
            SwitchStarted?.Invoke(Previous, Current);
            if (_switchTimer <= 0f) CompleteSwitch();
            return true;
        }

        /// <summary>
        /// Call when an attack lands or fires. Returns the damage multiplier for it
        /// and spends the switch-strike bonus if it was ready.
        /// </summary>
        public float ConsumeStrikeMultiplier()
        {
            if (!IsStrikeReady) return 1f;
            _strikeArmed = false;
            return 1f + StrikeBonus;
        }

        /// <summary>Advances the switch and strike timers.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;

            if (_switchTimer > 0f)
            {
                _switchTimer -= deltaTime;
                if (_switchTimer <= 0f) CompleteSwitch();
                return;
            }

            if (_strikeTimer > 0f)
            {
                _strikeTimer -= deltaTime;
                if (_strikeTimer <= 0f) _strikeArmed = false;
            }
        }

        private void CompleteSwitch()
        {
            _switchTimer = 0f;
            _strikeTimer = StrikeWindow;
            _strikeArmed = StrikeWindow > 0f;
            SwitchCompleted?.Invoke(Current);
        }
    }
}
