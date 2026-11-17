using System.Collections;
using System.Collections.Generic;
using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// 術士: element bolts that build up statuses, right click to cycle fire / ice /
    /// lightning, an element blast, a blink, and the Meteor ultimate.
    /// </summary>
    public sealed class ArcanistAbilities : RoleAbilities
    {
        /// <summary>The staff's current element.</summary>
        public enum Element
        {
            /// <summary>炎 → 炎上.</summary>
            Fire = 0,
            /// <summary>氷 → 凍結.</summary>
            Ice = 1,
            /// <summary>雷 → 帯電.</summary>
            Lightning = 2,
        }

        [SerializeField] private ArcanistTuning tuning;
        [Tooltip("Layers a bolt can hit (enemies and level geometry).")]
        [SerializeField] private LayerMask boltLayers = ~0;

        private readonly List<EnemyHealth> _targets = new List<EnemyHealth>();
        private Cooldown _bolt;
        private Cooldown _blast;
        private Cooldown _blink;

        /// <summary>The staff's current element.</summary>
        public Element CurrentElement { get; private set; } = Element.Fire;

        /// <summary>The status the current element applies.</summary>
        public StatusType CurrentStatus => StatusFor(CurrentElement);

        private void Awake()
        {
            _bolt = new Cooldown(1f / tuning.boltsPerSecond);
            _blast = new Cooldown(tuning.blastCooldown);
            _blink = new Cooldown(tuning.blinkCooldown);
        }

        /// <summary>Status applied by <paramref name="element"/>.</summary>
        public static StatusType StatusFor(Element element)
        {
            switch (element)
            {
                case Element.Ice: return StatusType.Freeze;
                case Element.Lightning: return StatusType.Shock;
                default: return StatusType.Burn;
            }
        }

        /// <inheritdoc />
        public override void TickCooldowns(float deltaTime)
        {
            _bolt.Tick(deltaTime);
            _blast.Tick(deltaTime);
            _blink.Tick(deltaTime);
        }

        /// <inheritdoc />
        public override void HandleInput(PlayerInputReader input)
        {
            if (input.SecondaryPressed) CurrentElement = (Element)(((int)CurrentElement + 1) % 3);
            if (input.PrimaryHeld && _bolt.TryUse()) Bolt();
            if (input.Skill1Pressed && _blast.TryUse()) Blast();
            if (input.Skill2Pressed && _blink.TryUse()) motor.Teleport(motor.Aim.forward, tuning.blinkDistance);
            if (input.UltimatePressed && controller.TryConsumeUltimate()) StartCoroutine(Meteor(AimPoint(tuning.meteorRange)));
        }

        /// <inheritdoc />
        public override string DebugStatus()
        {
            string element = CurrentElement == Element.Fire ? "炎" : CurrentElement == Element.Ice ? "氷" : "雷";
            return "属性: " + element + "（右クリックで切替）\n" + Cd("Q 属性魔法", _blast) + "\n" + Cd("E 瞬間移動", _blink);
        }

        private void Bolt()
        {
            float strike = controller.ConsumeStrike();
            if (!Physics.Raycast(AimRay(), out RaycastHit hitInfo, tuning.boltRange, boltLayers, QueryTriggerInteraction.Collide)) return;

            EnemyHealth enemy = hitInfo.collider.GetComponentInParent<EnemyHealth>();
            if (!enemy || !enemy.IsAlive) return;

            HitInfo hit = NewHit(tuning.boltDamage, AttackTag.ElementBolt, hitInfo.point, strike).WithBuildup(CurrentStatus, tuning.boltBuildup);
            WeakPoint weak = hitInfo.collider.GetComponent<WeakPoint>();
            if (weak) hit.WeakPointMultiplier = weak.multiplier;
            enemy.ReceiveHit(hit);
        }

        private void Blast()
        {
            float strike = controller.ConsumeStrike();
            Vector3 point = AimPoint(tuning.blastRange);
            director.FindEnemies(point, tuning.blastRadius, _targets);
            foreach (EnemyHealth enemy in _targets)
            {
                enemy.ReceiveHit(NewHit(tuning.blastDamage, AttackTag.ElementBlast, enemy.transform.position, strike).WithStatus(CurrentStatus));
            }
        }

        private IEnumerator Meteor(Vector3 point)
        {
            float strike = controller.ConsumeStrike();
            GameObject marker = tuning.meteorMarkerPrefab ? Instantiate(tuning.meteorMarkerPrefab, point, Quaternion.identity) : null;
            yield return new WaitForSeconds(tuning.meteorDelay);
            if (marker) Destroy(marker);

            director.FindEnemies(point, tuning.meteorRadius, _targets);
            var hitList = new List<EnemyHealth>(_targets);
            foreach (EnemyHealth enemy in hitList)
            {
                HitInfo hit = NewHit(tuning.meteorDamage, AttackTag.Meteor, enemy.transform.position, strike);
                hit.IsHeavy = true;
                enemy.ReceiveHit(hit);
            }
        }
    }
}
