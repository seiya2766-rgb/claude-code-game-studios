using System;
using System.Collections.Generic;

namespace RuneBrigade.Core
{
    /// <summary>Things that happen during play and move the camp forward.</summary>
    public enum CampMilestone
    {
        /// <summary>Came back to camp from a run (cleared or defeated).</summary>
        RunReturned = 0,
        /// <summary>Recorded an enemy type for the first time.</summary>
        EnemyRecorded = 1,
        /// <summary>Defeated the area 1 boss (グラム).</summary>
        Area1BossDefeated = 2,
        /// <summary>Defeated the area 2 boss (腐竜).</summary>
        Area2BossDefeated = 3,
    }

    /// <summary>People, facilities and roles that return to the camp.</summary>
    public enum CampFeature
    {
        /// <summary>焚き火番のガロ (present from the start).</summary>
        Garo = 0,
        /// <summary>出撃門 (present from the start).</summary>
        DepartureGate = 1,
        /// <summary>刻印師ミルダ and the forge.</summary>
        Forge = 2,
        /// <summary>書記見習いトト and the archive.</summary>
        Archive = 3,
        /// <summary>Repaired watchtower (erosion level setting).</summary>
        Watchtower = 4,
        /// <summary>Wardrobe tent (cosmetics).</summary>
        Wardrobe = 5,
        /// <summary>狩人 can be used.</summary>
        HunterRole = 6,
        /// <summary>祈祷師 can be used.</summary>
        OracleRole = 7,
        /// <summary>The mist over the village below starts to clear.</summary>
        VillageMistCleared = 8,
    }

    /// <summary>A feature unlocks once <see cref="Milestone"/> has happened <see cref="Threshold"/> times. Threshold 0 = from the start.</summary>
    public sealed class CampUnlockRule
    {
        /// <summary>Creates a rule.</summary>
        public CampUnlockRule(CampFeature feature, CampMilestone milestone, int threshold)
        {
            if (threshold < 0) throw new ArgumentOutOfRangeException(nameof(threshold));
            Feature = feature;
            Milestone = milestone;
            Threshold = threshold;
        }

        /// <summary>What unlocks.</summary>
        public CampFeature Feature { get; }

        /// <summary>What has to happen.</summary>
        public CampMilestone Milestone { get; }

        /// <summary>How many times.</summary>
        public int Threshold { get; }
    }

    /// <summary>
    /// The camp's progress: milestone counts, and from them which people, facilities
    /// and roles are back, and how much colour has returned. Order and thresholds come
    /// from data. Design: design/game-concept.md, "野営地".
    /// </summary>
    public sealed class CampProgress
    {
        private static readonly int MilestoneCount = Enum.GetValues(typeof(CampMilestone)).Length;

        private readonly List<CampUnlockRule> _rules;
        private readonly List<RoleId> _baseRoles;
        private readonly HashSet<CampFeature> _initial = new HashSet<CampFeature>();
        private readonly HashSet<CampFeature> _restorable = new HashSet<CampFeature>();
        private readonly int[] _counts = new int[MilestoneCount];
        private readonly HashSet<CampFeature> _unlocked = new HashSet<CampFeature>();

        /// <summary>Creates progress with nothing recorded yet.</summary>
        /// <param name="rules">Unlock rules; a feature unlocks when any of its rules is met.</param>
        /// <param name="baseRoles">Roles usable from the very first run (design: Warden and Arcanist).</param>
        public CampProgress(IEnumerable<CampUnlockRule> rules, IEnumerable<RoleId> baseRoles)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (baseRoles == null) throw new ArgumentNullException(nameof(baseRoles));
            _rules = new List<CampUnlockRule>(rules);
            _baseRoles = new List<RoleId>(baseRoles);
            if (_baseRoles.Count == 0) throw new ArgumentException("At least one base role is required.", nameof(baseRoles));

            foreach (CampUnlockRule rule in _rules)
            {
                if (rule.Threshold == 0) _initial.Add(rule.Feature);
            }
            foreach (CampUnlockRule rule in _rules)
            {
                if (!_initial.Contains(rule.Feature)) _restorable.Add(rule.Feature);
            }
            Recompute(null);
        }

        /// <summary>Raised once for each feature the moment it unlocks (not when loading a save).</summary>
        public event Action<CampFeature> Unlocked;

        /// <summary>True until the player has come back from a run once.</summary>
        public bool IsFirstRun => Count(CampMilestone.RunReturned) == 0;

        /// <summary>
        /// How much colour has returned to the camp, 0 to 1: the share of features
        /// that were not there at the start and are back now.
        /// </summary>
        public float Restoration
        {
            get
            {
                if (_restorable.Count == 0) return 1f;
                int back = 0;
                foreach (CampFeature feature in _restorable)
                {
                    if (_unlocked.Contains(feature)) back++;
                }
                return back / (float)_restorable.Count;
            }
        }

        /// <summary>Times <paramref name="milestone"/> has happened.</summary>
        public int Count(CampMilestone milestone) => _counts[(int)milestone];

        /// <summary>True when the feature is back.</summary>
        public bool IsUnlocked(CampFeature feature) => _unlocked.Contains(feature);

        /// <summary>True when the role can be used.</summary>
        public bool IsRoleUnlocked(RoleId role)
        {
            if (_baseRoles.Contains(role)) return true;
            switch (role)
            {
                case RoleId.Hunter: return IsUnlocked(CampFeature.HunterRole);
                case RoleId.Oracle: return IsUnlocked(CampFeature.OracleRole);
                default: return false;
            }
        }

        /// <summary>Usable roles in role order.</summary>
        public List<RoleId> UnlockedRoles()
        {
            var roles = new List<RoleId>();
            foreach (RoleId role in (RoleId[])Enum.GetValues(typeof(RoleId)))
            {
                if (IsRoleUnlocked(role)) roles.Add(role);
            }
            return roles;
        }

        /// <summary>Records that <paramref name="milestone"/> happened, raising <see cref="Unlocked"/> for anything new.</summary>
        /// <returns>The features that unlocked because of this call, in table order.</returns>
        public List<CampFeature> Record(CampMilestone milestone, int times = 1)
        {
            var newlyUnlocked = new List<CampFeature>();
            if (times <= 0) return newlyUnlocked;
            _counts[(int)milestone] += times;
            Recompute(newlyUnlocked);
            foreach (CampFeature feature in newlyUnlocked) Unlocked?.Invoke(feature);
            return newlyUnlocked;
        }

        /// <summary>Milestone counts for saving, indexed by <see cref="CampMilestone"/>.</summary>
        public int[] ToCounts() => (int[])_counts.Clone();

        /// <summary>Restores counts from a save. Raises no events. Missing or extra entries are ignored.</summary>
        public void LoadCounts(int[] counts)
        {
            Array.Clear(_counts, 0, _counts.Length);
            if (counts != null)
            {
                for (int i = 0; i < Math.Min(counts.Length, _counts.Length); i++) _counts[i] = Math.Max(0, counts[i]);
            }
            Recompute(null);
        }

        private void Recompute(List<CampFeature> newlyUnlocked)
        {
            foreach (CampUnlockRule rule in _rules)
            {
                if (_unlocked.Contains(rule.Feature)) continue;
                if (_counts[(int)rule.Milestone] < rule.Threshold) continue;
                _unlocked.Add(rule.Feature);
                newlyUnlocked?.Add(rule.Feature);
            }

            if (newlyUnlocked == null)
            {
                // Loading can lower counts (a reset): drop features whose rules are no longer met.
                _unlocked.RemoveWhere(feature => !AnyRuleMet(feature));
            }
        }

        private bool AnyRuleMet(CampFeature feature)
        {
            foreach (CampUnlockRule rule in _rules)
            {
                if (rule.Feature == feature && _counts[(int)rule.Milestone] >= rule.Threshold) return true;
            }
            return false;
        }
    }
}
