using System.Collections.Generic;
using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// 守護者: hammer string (finisher launches), guard with just guard and reflection,
    /// roar that gathers, shield charge, and the Earthbreaker ultimate.
    /// </summary>
    public sealed class WardenAbilities : RoleAbilities
    {
        [SerializeField] private WardenTuning tuning;

        private readonly List<EnemyHealth> _targets = new List<EnemyHealth>();
        private MeleeComboChain _chain;
        private Cooldown _roar;
        private Cooldown _charge;

        private void Awake()
        {
            _chain = new MeleeComboChain(tuning.comboDamage.Length, tuning.hitInterval, tuning.continueWindow);
            _roar = new Cooldown(tuning.roarCooldown);
            _charge = new Cooldown(tuning.chargeCooldown);
        }

        private void OnEnable()
        {
            if (health) health.Guarded += OnGuarded;
        }

        private void OnDisable()
        {
            if (health) health.Guarded -= OnGuarded;
        }

        /// <inheritdoc />
        public override void TickCooldowns(float deltaTime)
        {
            _chain.Tick(deltaTime);
            _roar.Tick(deltaTime);
            _charge.Tick(deltaTime);
        }

        /// <inheritdoc />
        public override void HandleInput(PlayerInputReader input)
        {
            bool guarding = input.SecondaryHeld;
            health.SetGuard(guarding, tuning.guardReduction, tuning.guardHalfAngle, tuning.justGuardWindow);
            motor.AbilitySpeedMultiplier = guarding ? 0.6f : 1f;
            if (guarding) return;

            if (input.PrimaryPressed && _chain.TryAttack(out int step)) Swing(step);
            if (input.Skill1Pressed && _roar.TryUse()) Roar();
            if (input.Skill2Pressed && _charge.TryUse()) Charge();
            if (input.UltimatePressed && controller.TryConsumeUltimate()) Earthbreaker();
        }

        /// <inheritdoc />
        public override void OnDeactivated()
        {
            health.SetGuard(false, 0f, 0f, 0f);
            motor.AbilitySpeedMultiplier = 1f;
            _chain.Reset();
        }

        /// <inheritdoc />
        public override string DebugStatus()
        {
            return "槌 " + (_chain.Steps) + "連撃 / 右:ガード\n" + Cd("Q 咆哮", _roar) + "\n" + Cd("E 盾突進", _charge);
        }

        private void Swing(int step)
        {
            bool finisher = _chain.IsFinisher(step);
            float strike = controller.ConsumeStrike();
            Vector3 center = motor.transform.position + Vector3.up + motor.Aim.forward * tuning.meleeRange;
            director.FindEnemies(center, tuning.meleeRadius, _targets);

            foreach (EnemyHealth enemy in _targets)
            {
                HitInfo hit = NewHit(tuning.comboDamage[step], finisher ? AttackTag.WardenFinisher : AttackTag.WardenCombo, enemy.transform.position, strike);
                hit.IsHeavy = finisher;
                if (finisher) hit = hit.WithStatus(StatusType.Launch);
                HitResult result = enemy.ReceiveHit(hit);
                if (finisher && !result.Killed) Launch(enemy, tuning.finisherLaunchHeight);
            }
        }

        private void Roar()
        {
            float strike = controller.ConsumeStrike();
            Vector3 pullPoint = motor.transform.position + motor.transform.forward * tuning.pullPointDistance;
            director.FindEnemies(motor.transform.position, tuning.roarRadius, _targets);
            foreach (EnemyHealth enemy in _targets)
            {
                HitInfo hit = NewHit(0f, AttackTag.Generic, enemy.transform.position, strike).WithStatus(StatusType.Gather, tuning.gatherDuration);
                enemy.ReceiveHit(hit);
                EnemyBrain brain = enemy.GetComponent<EnemyBrain>();
                if (brain) brain.PullTowards(pullPoint, tuning.pullSpeed, tuning.pullDuration);
            }
        }

        private void Charge()
        {
            float strike = controller.ConsumeStrike();
            Vector3 forward = motor.transform.forward;
            motor.Lunge(forward, tuning.chargeDistance, tuning.chargeTime);
            Vector3 center = motor.transform.position + Vector3.up + forward * (tuning.chargeDistance * 0.5f);
            director.FindEnemies(center, Mathf.Max(tuning.chargeRadius, tuning.chargeDistance * 0.5f), _targets);
            foreach (EnemyHealth enemy in _targets)
            {
                HitInfo hit = NewHit(tuning.chargeDamage, AttackTag.ShieldCharge, enemy.transform.position, strike);
                hit.IsHeavy = true;
                if (!enemy.ReceiveHit(hit).Killed)
                {
                    EnemyBrain brain = enemy.GetComponent<EnemyBrain>();
                    if (brain) brain.Knockback(forward, tuning.chargeDistance * 0.6f);
                }
            }
        }

        private void Earthbreaker()
        {
            float strike = controller.ConsumeStrike();
            director.FindEnemies(motor.transform.position, tuning.ultimateRadius, _targets);
            foreach (EnemyHealth enemy in _targets)
            {
                HitInfo hit = NewHit(tuning.ultimateDamage, AttackTag.Ultimate, enemy.transform.position, strike).WithStatus(StatusType.Launch);
                hit.IsHeavy = true;
                if (!enemy.ReceiveHit(hit).Killed) Launch(enemy, tuning.ultimateLaunchHeight);
            }
        }

        private void OnGuarded(EnemyBrain attacker, bool just)
        {
            if (!just || !attacker) return;
            EnemyHealth enemy = attacker.GetComponent<EnemyHealth>();
            if (enemy && enemy.IsAlive)
            {
                HitInfo hit = NewHit(tuning.reflectDamage, AttackTag.ShieldGuard, attacker.transform.position, 1f);
                hit.IsHeavy = true;
                enemy.ReceiveHit(hit);
            }
            attacker.Stagger(tuning.justGuardStagger);
        }

        private static void Launch(EnemyHealth enemy, float height)
        {
            EnemyBrain brain = enemy.GetComponent<EnemyBrain>();
            if (brain) brain.Launch(height);
        }
    }
}
