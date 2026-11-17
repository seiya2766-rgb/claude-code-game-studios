using System;
using System.Collections.Generic;

namespace RuneBrigade.Core
{
    /// <summary>
    /// Decides whether a hit or a new status sets off a fusion spell. Rules are
    /// checked in table order and the first match wins; its statuses are consumed.
    /// </summary>
    public sealed class ComboResolver
    {
        private readonly List<ComboRule> _rules;

        /// <summary>Creates a resolver over <paramref name="rules"/> (order = priority).</summary>
        public ComboResolver(IEnumerable<ComboRule> rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            _rules = new List<ComboRule>(rules);
        }

        /// <summary>The rules, in priority order.</summary>
        public IReadOnlyList<ComboRule> Rules => _rules;

        /// <summary>
        /// Checks the hit-triggered rules for an attack with <paramref name="tag"/>
        /// against <paramref name="target"/>. On a match the rule's statuses are removed.
        /// </summary>
        /// <param name="target">The struck enemy's statuses.</param>
        /// <param name="tag">The landing attack.</param>
        /// <param name="rule">The fusion that fired, or null.</param>
        /// <returns>True when a fusion fired.</returns>
        public bool TryResolveHit(StatusContainer target, AttackTag tag, out ComboRule rule)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            foreach (ComboRule candidate in _rules)
            {
                if (candidate.IsStatusTriggered || !candidate.IsTriggeredBy(tag)) continue;
                if (!candidate.IsSatisfiedBy(target)) continue;
                Consume(target, candidate);
                rule = candidate;
                return true;
            }
            rule = null;
            return false;
        }

        /// <summary>
        /// Checks the status-triggered rules after a status was applied
        /// (e.g. Burn then Shock = 爆雷). On a match the rule's statuses are removed.
        /// </summary>
        /// <param name="target">The enemy's statuses, after the new status was applied.</param>
        /// <param name="rule">The fusion that fired, or null.</param>
        /// <returns>True when a fusion fired.</returns>
        public bool TryResolveStatus(StatusContainer target, out ComboRule rule)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            foreach (ComboRule candidate in _rules)
            {
                if (!candidate.IsStatusTriggered || !candidate.IsSatisfiedBy(target)) continue;
                Consume(target, candidate);
                rule = candidate;
                return true;
            }
            rule = null;
            return false;
        }

        private static void Consume(StatusContainer target, ComboRule rule)
        {
            foreach (StatusType status in rule.RequiredStatuses) target.Remove(status);
        }
    }
}
