using System;

namespace RuneBrigade.Core
{
    /// <summary>
    /// Final damage of one hit: base × weak point × switch strike × fusion.
    /// Each factor is 1 when it does not apply.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>Computes the damage of a hit.</summary>
        /// <param name="baseDamage">Ability damage from tuning data.</param>
        /// <param name="weakPointMultiplier">Head or weak-point multiplier, 1 for a body hit.</param>
        /// <param name="switchStrikeMultiplier">From <see cref="RoleSwitcher.ConsumeStrikeMultiplier"/>.</param>
        /// <param name="comboMultiplier">From the fusion rule, 1 when none fired.</param>
        public static float Compute(float baseDamage, float weakPointMultiplier, float switchStrikeMultiplier, float comboMultiplier)
        {
            if (baseDamage <= 0f) return 0f;
            if (weakPointMultiplier <= 0f || switchStrikeMultiplier <= 0f || comboMultiplier <= 0f)
                throw new ArgumentOutOfRangeException(nameof(weakPointMultiplier), "Multipliers must be positive.");
            return baseDamage * weakPointMultiplier * switchStrikeMultiplier * comboMultiplier;
        }
    }
}
