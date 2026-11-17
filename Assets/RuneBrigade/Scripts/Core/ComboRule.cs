using System;
using System.Collections.Generic;

namespace RuneBrigade.Core
{
    /// <summary>
    /// One fusion spell: the statuses it needs, and the attack tags that set it off.
    /// A rule with no trigger tags fires the moment its statuses are all present
    /// (e.g. 爆雷 = Burn + Shock). Table: design/game-concept.md, "状態と合成のルール".
    /// </summary>
    public sealed class ComboRule
    {
        private readonly StatusType[] _requiredStatuses;
        private readonly AttackTag[] _triggerTags;

        /// <summary>Creates a rule.</summary>
        /// <param name="id">Stable id, e.g. "annihilate".</param>
        /// <param name="displayName">Name shown on screen, e.g. "殲滅".</param>
        /// <param name="requiredStatuses">Statuses the target must hold (at least one).</param>
        /// <param name="triggerTags">Attack tags that set it off; empty = fires on status application.</param>
        /// <param name="damageMultiplier">Multiplier on the triggering hit (or on <paramref name="baseDamage"/> for status rules).</param>
        /// <param name="baseDamage">Damage for status-triggered rules, which have no hit to multiply.</param>
        public ComboRule(string id, string displayName, IEnumerable<StatusType> requiredStatuses,
            IEnumerable<AttackTag> triggerTags, float damageMultiplier, float baseDamage = 0f)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Id is required.", nameof(id));
            if (requiredStatuses == null) throw new ArgumentNullException(nameof(requiredStatuses));
            _requiredStatuses = new List<StatusType>(requiredStatuses).ToArray();
            if (_requiredStatuses.Length == 0) throw new ArgumentException("At least one status is required.", nameof(requiredStatuses));
            _triggerTags = triggerTags == null ? new AttackTag[0] : new List<AttackTag>(triggerTags).ToArray();
            if (damageMultiplier <= 0f) throw new ArgumentOutOfRangeException(nameof(damageMultiplier));

            Id = id;
            DisplayName = string.IsNullOrEmpty(displayName) ? id : displayName;
            DamageMultiplier = damageMultiplier;
            BaseDamage = baseDamage;
        }

        /// <summary>Stable id.</summary>
        public string Id { get; }

        /// <summary>Name shown on screen.</summary>
        public string DisplayName { get; }

        /// <summary>Multiplier applied to the triggering hit.</summary>
        public float DamageMultiplier { get; }

        /// <summary>Damage dealt by a status-triggered rule.</summary>
        public float BaseDamage { get; }

        /// <summary>Statuses the target must hold.</summary>
        public IReadOnlyList<StatusType> RequiredStatuses => _requiredStatuses;

        /// <summary>Attack tags that set it off. Empty = status-triggered.</summary>
        public IReadOnlyList<AttackTag> TriggerTags => _triggerTags;

        /// <summary>True when the rule fires on status application rather than on a hit.</summary>
        public bool IsStatusTriggered => _triggerTags.Length == 0;

        /// <summary>True when <paramref name="tag"/> is one of this rule's triggers.</summary>
        public bool IsTriggeredBy(AttackTag tag) => Array.IndexOf(_triggerTags, tag) >= 0;

        /// <summary>True when <paramref name="statuses"/> holds every required status.</summary>
        public bool IsSatisfiedBy(StatusContainer statuses)
        {
            if (statuses == null) return false;
            foreach (StatusType required in _requiredStatuses)
            {
                if (!statuses.Has(required)) return false;
            }
            return true;
        }
    }
}
