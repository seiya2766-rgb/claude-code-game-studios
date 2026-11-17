using System;

namespace RuneBrigade.Core
{
    /// <summary>A single ability cooldown. Ticks whether or not its role is active.</summary>
    public sealed class Cooldown
    {
        /// <summary>Creates a ready cooldown of <paramref name="duration"/> seconds.</summary>
        public Cooldown(float duration)
        {
            if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration));
            Duration = duration;
        }

        /// <summary>Full cooldown length in seconds.</summary>
        public float Duration { get; private set; }

        /// <summary>Seconds until ready.</summary>
        public float Remaining { get; private set; }

        /// <summary>True when the ability can be used.</summary>
        public bool IsReady => Remaining <= 0f;

        /// <summary>0 when just used, 1 when ready.</summary>
        public float Progress => Duration <= 0f ? 1f : 1f - Remaining / Duration;

        /// <summary>Uses the ability if ready and starts the cooldown.</summary>
        /// <returns>True when it was ready.</returns>
        public bool TryUse()
        {
            if (!IsReady) return false;
            Remaining = Duration;
            return true;
        }

        /// <summary>Changes the length (e.g. from tuning data) without resetting progress.</summary>
        public void SetDuration(float duration)
        {
            if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration));
            Duration = duration;
            if (Remaining > Duration) Remaining = Duration;
        }

        /// <summary>Makes it ready immediately.</summary>
        public void Reset() => Remaining = 0f;

        /// <summary>Advances the timer.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || Remaining <= 0f) return;
            Remaining = Math.Max(0f, Remaining - deltaTime);
        }
    }
}
