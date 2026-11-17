using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// Base class for one role's kit. Every kit ticks its cooldowns all the time
    /// (inactive roles keep cooling down), but only the active kit gets input.
    /// </summary>
    public abstract class RoleAbilities : MonoBehaviour
    {
        [SerializeField] protected RoleDefinition definition;
        [SerializeField] protected CombatDirector director;
        [SerializeField] protected FirstPersonMotor motor;
        [SerializeField] protected PlayerHealth health;
        [SerializeField] protected PlayerRoleController controller;

        /// <summary>Which role this kit belongs to.</summary>
        public RoleId Role => definition.role;

        /// <summary>The role's data.</summary>
        public RoleDefinition Definition => definition;

        /// <summary>Called every frame while this role is active and not switching.</summary>
        public abstract void HandleInput(PlayerInputReader input);

        /// <summary>Called every frame whether or not the role is active.</summary>
        public abstract void TickCooldowns(float deltaTime);

        /// <summary>Called when the player switches away (drop the guard, stop channels).</summary>
        public virtual void OnDeactivated() { }

        /// <summary>Called when the role becomes active.</summary>
        public virtual void OnActivated() { }

        /// <summary>One line per ability for the debug HUD.</summary>
        public abstract string DebugStatus();

        /// <summary>A hit with this role as source and the switch-strike multiplier already applied.</summary>
        protected HitInfo NewHit(float damage, AttackTag tag, Vector3 point, float strikeMultiplier)
        {
            HitInfo hit = HitInfo.Create(damage, tag, Role, point);
            hit.SwitchStrikeMultiplier = strikeMultiplier;
            return hit;
        }

        /// <summary>Aim ray from the camera.</summary>
        protected Ray AimRay() => new Ray(motor.Aim.position, motor.Aim.forward);

        /// <summary>Point the player is aiming at, up to <paramref name="range"/>.</summary>
        protected Vector3 AimPoint(float range)
        {
            Ray ray = AimRay();
            return Physics.Raycast(ray, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Ignore) ? hit.point : ray.GetPoint(range);
        }

        /// <summary>Formats a cooldown for the HUD.</summary>
        protected static string Cd(string name, Cooldown cooldown)
        {
            return cooldown.IsReady ? name + " 準備OK" : name + " " + cooldown.Remaining.ToString("0.0") + "s";
        }
    }
}
