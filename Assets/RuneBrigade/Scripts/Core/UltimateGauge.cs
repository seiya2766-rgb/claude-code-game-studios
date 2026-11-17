using System;

namespace RuneBrigade.Core
{
    /// <summary>The ultimate (R) gauge. Fills from hits, fusions and damage taken.</summary>
    public sealed class UltimateGauge
    {
        /// <summary>Creates an empty gauge of size <paramref name="max"/>.</summary>
        public UltimateGauge(float max)
        {
            if (max <= 0f) throw new ArgumentOutOfRangeException(nameof(max));
            Max = max;
        }

        /// <summary>Points needed for an ultimate.</summary>
        public float Max { get; }

        /// <summary>Points stored.</summary>
        public float Value { get; private set; }

        /// <summary>True when full.</summary>
        public bool IsFull => Value >= Max;

        /// <summary>Value / Max.</summary>
        public float Fraction => Value / Max;

        /// <summary>Adds points, capped at <see cref="Max"/>.</summary>
        public void Add(float points)
        {
            if (points <= 0f) return;
            Value = Math.Min(Max, Value + points);
        }

        /// <summary>Empties the gauge if full.</summary>
        /// <returns>True when the ultimate can fire.</returns>
        public bool TryConsume()
        {
            if (!IsFull) return false;
            Value = 0f;
            return true;
        }
    }
}
