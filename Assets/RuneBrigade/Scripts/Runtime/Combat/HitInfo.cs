using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>Everything an ability tells an enemy about one hit.</summary>
    public struct HitInfo
    {
        /// <summary>Ability damage before multipliers.</summary>
        public float BaseDamage;
        /// <summary>What kind of attack this is (fusion triggers read it).</summary>
        public AttackTag Tag;
        /// <summary>True when the hit applies <see cref="Status"/> outright.</summary>
        public bool AppliesStatus;
        /// <summary>Status applied outright when <see cref="AppliesStatus"/> is set.</summary>
        public StatusType Status;
        /// <summary>Seconds the status lasts; 0 = the tuning default.</summary>
        public float StatusDuration;
        /// <summary>Buildup points toward <see cref="BuildupStatus"/>; 0 = none.</summary>
        public float BuildupPoints;
        /// <summary>Status the buildup feeds.</summary>
        public StatusType BuildupStatus;
        /// <summary>Weak-point multiplier; set by <see cref="WeakPoint"/>, 1 otherwise.</summary>
        public float WeakPointMultiplier;
        /// <summary>From <see cref="RoleSwitcher.ConsumeStrikeMultiplier"/>, 1 otherwise.</summary>
        public float SwitchStrikeMultiplier;
        /// <summary>True for heavy hits (longer hit stop).</summary>
        public bool IsHeavy;
        /// <summary>World position of the impact.</summary>
        public Vector3 Point;
        /// <summary>Role that dealt the hit.</summary>
        public RoleId SourceRole;

        /// <summary>A hit with neutral multipliers.</summary>
        public static HitInfo Create(float baseDamage, AttackTag tag, RoleId source, Vector3 point)
        {
            return new HitInfo
            {
                BaseDamage = baseDamage,
                Tag = tag,
                SourceRole = source,
                Point = point,
                WeakPointMultiplier = 1f,
                SwitchStrikeMultiplier = 1f,
            };
        }

        /// <summary>Returns a copy that also applies <paramref name="status"/>.</summary>
        public HitInfo WithStatus(StatusType status, float duration = 0f)
        {
            HitInfo copy = this;
            copy.AppliesStatus = true;
            copy.Status = status;
            copy.StatusDuration = duration;
            return copy;
        }

        /// <summary>Returns a copy that adds buildup toward <paramref name="status"/>.</summary>
        public HitInfo WithBuildup(StatusType status, float points)
        {
            HitInfo copy = this;
            copy.BuildupStatus = status;
            copy.BuildupPoints = points;
            return copy;
        }
    }

    /// <summary>What happened when a hit landed.</summary>
    public struct HitResult
    {
        /// <summary>Damage actually dealt (fusions included).</summary>
        public float DamageDealt;
        /// <summary>The fusion that fired, or null.</summary>
        public ComboRule Combo;
        /// <summary>True when the hit killed the enemy.</summary>
        public bool Killed;
    }
}
