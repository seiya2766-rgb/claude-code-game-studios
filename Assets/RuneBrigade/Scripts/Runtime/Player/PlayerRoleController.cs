using System;
using System.Collections.Generic;
using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// Owns the <see cref="RoleSwitcher"/> and the ultimate gauge, routes input to the
    /// active kit, and applies the role's health and speed multipliers.
    /// Keys 1-4 select the usable roles in the order of <see cref="kits"/>. With a
    /// <see cref="CampProgressStore"/> assigned, roles the camp has not unlocked yet
    /// are left out (design: Warden and Arcanist first, Hunter and Oracle later).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerRoleController : MonoBehaviour
    {
        [SerializeField] private CombatTuning tuning;
        [SerializeField] private CombatDirector director;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private FirstPersonMotor motor;
        [SerializeField] private PlayerHealth health;
        [Tooltip("Role kits in key order (1 = first). Prototype: Warden, Arcanist.")]
        [SerializeField] private List<RoleAbilities> kits = new List<RoleAbilities>();
        [Tooltip("Optional. When set, only roles the camp has unlocked can be used.")]
        [SerializeField] private CampProgressStore progressStore;

        private RoleSwitcher _switcher;
        private UltimateGauge _gauge;
        private readonly Dictionary<RoleId, RoleAbilities> _byRole = new Dictionary<RoleId, RoleAbilities>();

        /// <summary>The switcher (HUD reads it).</summary>
        public RoleSwitcher Switcher => _switcher;

        /// <summary>The ultimate gauge.</summary>
        public UltimateGauge Gauge => _gauge;

        /// <summary>The active kit.</summary>
        public RoleAbilities ActiveKit => _byRole[_switcher.Current];

        /// <summary>Raised when a switch completes.</summary>
        public event Action<RoleAbilities> RoleChanged;

        private void Awake()
        {
            if (kits.Count == 0) throw new InvalidOperationException("PlayerRoleController needs at least one role kit.");
            if (progressStore)
            {
                progressStore.Load();
                CampProgress progress = progressStore.Progress;
                List<RoleAbilities> usable = kits.FindAll(kit => progress.IsRoleUnlocked(kit.Role));
                if (usable.Count > 0) kits = usable;
                else Debug.LogWarning("No role kit is unlocked in camp progress; using every kit.");
            }

            var roles = new List<RoleId>();
            foreach (RoleAbilities kit in kits)
            {
                _byRole.Add(kit.Role, kit);
                roles.Add(kit.Role);
            }
            _switcher = new RoleSwitcher(roles, roles[0], tuning.switchDuration, tuning.switchStrikeWindow, tuning.switchStrikeBonus);
            _switcher.SwitchStarted += OnSwitchStarted;
            _switcher.SwitchCompleted += OnSwitchCompleted;
            _gauge = new UltimateGauge(tuning.ultimateGaugeMax);
        }

        private void Start()
        {
            ApplyRole(ActiveKit);
            ActiveKit.OnActivated();
        }

        private void OnEnable()
        {
            if (director) director.HitLanded += OnHitLanded;
            if (health) health.DamageTaken += OnDamageTaken;
        }

        private void OnDisable()
        {
            if (director) director.HitLanded -= OnHitLanded;
            if (health) health.DamageTaken -= OnDamageTaken;
        }

        /// <summary>Spends the switch-strike bonus. Call once per attack action.</summary>
        public float ConsumeStrike() => _switcher.ConsumeStrikeMultiplier();

        /// <summary>Empties the ultimate gauge if full.</summary>
        public bool TryConsumeUltimate() => _gauge.TryConsume();

        private void Update()
        {
            float dt = Time.deltaTime;
            _switcher.Tick(dt);
            foreach (RoleAbilities kit in kits) kit.TickCooldowns(dt);

            if (input.RoleKeyPressed >= 0 && input.RoleKeyPressed < kits.Count)
            {
                _switcher.TrySwitch(kits[input.RoleKeyPressed].Role);
            }

            if (!_switcher.IsSwitching) ActiveKit.HandleInput(input);
        }

        private void OnSwitchStarted(RoleId from, RoleId to)
        {
            _byRole[from].OnDeactivated();
        }

        private void OnSwitchCompleted(RoleId role)
        {
            RoleAbilities kit = _byRole[role];
            ApplyRole(kit);
            kit.OnActivated();
            RoleChanged?.Invoke(kit);
        }

        private void ApplyRole(RoleAbilities kit)
        {
            health.Pool.SetMultiplier(kit.Definition.healthMultiplier);
            motor.SpeedMultiplier = kit.Definition.moveSpeedMultiplier;
        }

        private void OnHitLanded(EnemyHealth target, HitInfo hit, HitResult result)
        {
            _gauge.Add(result.DamageDealt * tuning.ultimatePerDamage);
            if (result.Combo != null) _gauge.Add(tuning.ultimatePerCombo);
        }

        private void OnDamageTaken(float amount)
        {
            _gauge.Add(amount * tuning.ultimatePerDamageTaken);
        }
    }
}
