using System;

namespace RuneBrigade.Core
{
    /// <summary>
    /// Enemy scaling by player count. Formulas (design/game-concept.md, "人数による調整"):
    /// health = 1 + healthPerExtra × (players − 1); count = 1 + countPerExtra × (players − 1).
    /// </summary>
    public static class PartyScaling
    {
        /// <summary>Enemy health multiplier for <paramref name="players"/> players (clamped to at least 1).</summary>
        public static float HealthMultiplier(int players, float healthPerExtraPlayer)
        {
            if (healthPerExtraPlayer < 0f) throw new ArgumentOutOfRangeException(nameof(healthPerExtraPlayer));
            return 1f + healthPerExtraPlayer * (Math.Max(1, players) - 1);
        }

        /// <summary>Enemy count multiplier for <paramref name="players"/> players (clamped to at least 1).</summary>
        public static float CountMultiplier(int players, float countPerExtraPlayer)
        {
            if (countPerExtraPlayer < 0f) throw new ArgumentOutOfRangeException(nameof(countPerExtraPlayer));
            return 1f + countPerExtraPlayer * (Math.Max(1, players) - 1);
        }

        /// <summary>Number of enemies to spawn: base count × multiplier, rounded half up, at least 1 when base > 0.</summary>
        public static int ScaledCount(int baseCount, int players, float countPerExtraPlayer)
        {
            if (baseCount <= 0) return 0;
            double scaled = baseCount * (double)CountMultiplier(players, countPerExtraPlayer);
            return Math.Max(1, (int)Math.Floor(scaled + 0.5));
        }
    }
}
