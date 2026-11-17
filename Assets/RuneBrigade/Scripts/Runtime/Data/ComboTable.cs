using System;
using System.Collections.Generic;
using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// The fusion table as data. Order is priority. Prototype defaults
    /// (design/game-concept.md, "状態と合成のルール"):
    /// 殲滅 = Gather + Meteor (×4); 砕氷 = Freeze + WardenFinisher / ChargedShot (×5);
    /// 爆雷 = Burn + Shock, no trigger, 120 damage in 4 m; 反射雷 = Shock + ShieldGuard (×3).
    /// </summary>
    [CreateAssetMenu(menuName = "Rune Brigade/Combo Table", fileName = "ComboTable")]
    public sealed class ComboTable : ScriptableObject
    {
        /// <summary>One row of the table, editable in the inspector.</summary>
        [Serializable]
        public sealed class Entry
        {
            [Tooltip("Stable id, e.g. annihilate.")]
            public string id = "annihilate";
            [Tooltip("Name shown on screen.")]
            public string displayName = "殲滅";
            [Tooltip("Statuses the enemy must hold.")]
            public StatusType[] requiredStatuses = { StatusType.Gather };
            [Tooltip("Attacks that set it off. Leave empty to fire as soon as the statuses are all present.")]
            public AttackTag[] triggerTags = { AttackTag.Meteor };
            [Tooltip("Multiplier on the triggering hit.")]
            [Min(0.01f)] public float damageMultiplier = 4f;
            [Tooltip("Damage for a rule with no trigger tags.")]
            [Min(0f)] public float baseDamage;
            [Tooltip("If above 0, the fusion's damage also hits other enemies in this radius.")]
            [Min(0f)] public float splashRadius;
            [Tooltip("Colours of the two-colour shard burst (art bible).")]
            public Color colorA = Color.white;
            public Color colorB = Color.white;
        }

        [Tooltip("Fusion rules in priority order.")]
        public List<Entry> entries = new List<Entry>();

        /// <summary>Builds the core rules from the table.</summary>
        public List<ComboRule> BuildRules()
        {
            var rules = new List<ComboRule>(entries.Count);
            foreach (Entry entry in entries)
            {
                rules.Add(new ComboRule(entry.id, entry.displayName, entry.requiredStatuses, entry.triggerTags, entry.damageMultiplier, entry.baseDamage));
            }
            return rules;
        }

        /// <summary>Finds the table row for a rule id, or null.</summary>
        public Entry Find(string id)
        {
            foreach (Entry entry in entries)
            {
                if (entry.id == id) return entry;
            }
            return null;
        }

        private void Reset()
        {
            entries = new List<Entry>
            {
                new Entry
                {
                    id = "annihilate", displayName = "殲滅",
                    requiredStatuses = new[] { StatusType.Gather }, triggerTags = new[] { AttackTag.Meteor },
                    damageMultiplier = 4f,
                    colorA = new Color(0.949f, 0.702f, 0.239f), colorB = new Color(1f, 0.353f, 0.212f),
                },
                new Entry
                {
                    id = "shatter", displayName = "砕氷",
                    requiredStatuses = new[] { StatusType.Freeze }, triggerTags = new[] { AttackTag.WardenFinisher, AttackTag.ChargedShot },
                    damageMultiplier = 5f,
                    colorA = new Color(0.498f, 0.890f, 1f), colorB = new Color(0.839f, 0.969f, 1f),
                },
                new Entry
                {
                    id = "detonate", displayName = "爆雷",
                    requiredStatuses = new[] { StatusType.Burn, StatusType.Shock }, triggerTags = new AttackTag[0],
                    damageMultiplier = 1f, baseDamage = 120f, splashRadius = 4f,
                    colorA = new Color(1f, 0.353f, 0.212f), colorB = new Color(0.769f, 0.420f, 1f),
                },
                new Entry
                {
                    id = "reflect", displayName = "反射雷",
                    requiredStatuses = new[] { StatusType.Shock }, triggerTags = new[] { AttackTag.ShieldGuard },
                    damageMultiplier = 3f,
                    colorA = new Color(0.769f, 0.420f, 1f), colorB = new Color(0.949f, 0.702f, 0.239f),
                },
            };
        }
    }
}
