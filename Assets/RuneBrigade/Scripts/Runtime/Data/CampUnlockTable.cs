using System;
using System.Collections.Generic;
using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// What returns to the camp, and when. Defaults are the order decided in
    /// design/game-concept.md, "野営地". Change the asset, not code.
    /// </summary>
    [CreateAssetMenu(menuName = "Rune Brigade/Camp Unlock Table", fileName = "CampUnlockTable")]
    public sealed class CampUnlockTable : ScriptableObject
    {
        /// <summary>One row: <see cref="feature"/> returns once <see cref="milestone"/> has happened <see cref="threshold"/> times.</summary>
        [Serializable]
        public sealed class Entry
        {
            public CampFeature feature;
            public CampMilestone milestone;
            [Tooltip("0 = there from the start.")]
            [Min(0)] public int threshold;

            public Entry() { }

            public Entry(CampFeature feature, CampMilestone milestone, int threshold)
            {
                this.feature = feature;
                this.milestone = milestone;
                this.threshold = threshold;
            }
        }

        [Tooltip("Roles usable from the first run. Design: Warden and Arcanist.")]
        public RoleId[] baseRoles = { RoleId.Warden, RoleId.Arcanist };

        [Tooltip("Unlock rows. A feature with several rows returns when any one is met.")]
        public List<Entry> entries = DefaultEntries();

        /// <summary>Builds a fresh <see cref="CampProgress"/> from the table.</summary>
        public CampProgress CreateProgress()
        {
            var rules = new List<CampUnlockRule>(entries.Count);
            foreach (Entry entry in entries) rules.Add(new CampUnlockRule(entry.feature, entry.milestone, entry.threshold));
            return new CampProgress(rules, baseRoles);
        }

        private void Reset()
        {
            baseRoles = new[] { RoleId.Warden, RoleId.Arcanist };
            entries = DefaultEntries();
        }

        private static List<Entry> DefaultEntries()
        {
            return new List<Entry>
            {
                new Entry(CampFeature.Garo, CampMilestone.RunReturned, 0),
                new Entry(CampFeature.DepartureGate, CampMilestone.RunReturned, 0),
                new Entry(CampFeature.Forge, CampMilestone.RunReturned, 1),
                new Entry(CampFeature.Archive, CampMilestone.EnemyRecorded, 1),
                new Entry(CampFeature.HunterRole, CampMilestone.Area1BossDefeated, 1),
                new Entry(CampFeature.Watchtower, CampMilestone.Area1BossDefeated, 1),
                new Entry(CampFeature.VillageMistCleared, CampMilestone.Area1BossDefeated, 1),
                new Entry(CampFeature.OracleRole, CampMilestone.Area2BossDefeated, 1),
                new Entry(CampFeature.Wardrobe, CampMilestone.RunReturned, 3),
            };
        }
    }
}
