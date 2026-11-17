using System;

namespace RuneBrigade.Core
{
    /// <summary>
    /// A health bar with a base maximum and a role multiplier. Changing the multiplier
    /// keeps the current fraction, so switching to a sturdier role does not heal.
    /// Rule: design/game-concept.md, "役割の切り替え".
    /// </summary>
    public sealed class HealthPool
    {
        /// <summary>Creates a full pool.</summary>
        /// <param name="baseMax">Maximum health at multiplier 1.</param>
        /// <param name="multiplier">Starting multiplier.</param>
        public HealthPool(float baseMax, float multiplier = 1f)
        {
            if (baseMax <= 0f) throw new ArgumentOutOfRangeException(nameof(baseMax));
            if (multiplier <= 0f) throw new ArgumentOutOfRangeException(nameof(multiplier));
            BaseMax = baseMax;
            Multiplier = multiplier;
            Current = Max;
        }

        /// <summary>Maximum health at multiplier 1.</summary>
        public float BaseMax { get; private set; }

        /// <summary>Current role multiplier.</summary>
        public float Multiplier { get; private set; }

        /// <summary>Effective maximum.</summary>
        public float Max => BaseMax * Multiplier;

        /// <summary>Current health.</summary>
        public float Current { get; private set; }

        /// <summary>Current / Max.</summary>
        public float Fraction => Current / Max;

        /// <summary>True at zero health.</summary>
        public bool IsDead => Current <= 0f;

        /// <summary>Raised after health changes, with the new value.</summary>
        public event Action<float> Changed;

        /// <summary>Raised once when health reaches zero.</summary>
        public event Action Died;

        /// <summary>Changes the multiplier, keeping the current fraction.</summary>
        public void SetMultiplier(float multiplier)
        {
            if (multiplier <= 0f) throw new ArgumentOutOfRangeException(nameof(multiplier));
            float fraction = Fraction;
            Multiplier = multiplier;
            Current = Max * fraction;
            Changed?.Invoke(Current);
        }

        /// <summary>Changes the base maximum (e.g. party scaling), keeping the current fraction.</summary>
        public void SetBaseMax(float baseMax)
        {
            if (baseMax <= 0f) throw new ArgumentOutOfRangeException(nameof(baseMax));
            float fraction = Fraction;
            BaseMax = baseMax;
            Current = Max * fraction;
            Changed?.Invoke(Current);
        }

        /// <summary>Removes health. Ignored once dead.</summary>
        /// <returns>The damage actually taken.</returns>
        public float Damage(float amount)
        {
            if (amount <= 0f || IsDead) return 0f;
            float taken = Math.Min(amount, Current);
            Current -= taken;
            Changed?.Invoke(Current);
            if (IsDead) Died?.Invoke();
            return taken;
        }

        /// <summary>Restores health up to the maximum. Ignored once dead.</summary>
        /// <returns>The amount actually healed.</returns>
        public float Heal(float amount)
        {
            if (amount <= 0f || IsDead) return 0f;
            float healed = Math.Min(amount, Max - Current);
            Current += healed;
            if (healed > 0f) Changed?.Invoke(Current);
            return healed;
        }

        /// <summary>Brings a dead pool back at <paramref name="fraction"/> of the maximum.</summary>
        public void Revive(float fraction)
        {
            if (fraction <= 0f || fraction > 1f) throw new ArgumentOutOfRangeException(nameof(fraction));
            Current = Max * fraction;
            Changed?.Invoke(Current);
        }
    }
}
