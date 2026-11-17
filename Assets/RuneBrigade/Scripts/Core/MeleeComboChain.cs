using System;

namespace RuneBrigade.Core
{
    /// <summary>
    /// A melee string such as the Warden's 3 hits. Pressing attack within
    /// <see cref="ContinueWindow"/> seconds of the last hit advances to the next step;
    /// otherwise the chain restarts. Each hit has a recovery of <see cref="HitInterval"/>.
    /// </summary>
    public sealed class MeleeComboChain
    {
        private float _sinceLastHit = float.MaxValue;
        private int _nextStep;

        /// <summary>Creates a chain.</summary>
        /// <param name="steps">Hits in the string (at least 1).</param>
        /// <param name="hitInterval">Minimum seconds between hits.</param>
        /// <param name="continueWindow">Seconds after a hit in which the next press continues the chain.</param>
        public MeleeComboChain(int steps, float hitInterval, float continueWindow)
        {
            if (steps < 1) throw new ArgumentOutOfRangeException(nameof(steps));
            if (hitInterval < 0f || continueWindow < hitInterval) throw new ArgumentOutOfRangeException(nameof(continueWindow), "Window must be at least the hit interval.");
            Steps = steps;
            HitInterval = hitInterval;
            ContinueWindow = continueWindow;
        }

        /// <summary>Hits in the string.</summary>
        public int Steps { get; }

        /// <summary>Minimum seconds between hits.</summary>
        public float HitInterval { get; }

        /// <summary>Seconds in which the next press continues the chain.</summary>
        public float ContinueWindow { get; }

        /// <summary>Tries to swing.</summary>
        /// <param name="step">Zero-based step of this hit; the last step is the finisher.</param>
        /// <returns>False while still recovering from the previous hit.</returns>
        public bool TryAttack(out int step)
        {
            if (_sinceLastHit < HitInterval)
            {
                step = -1;
                return false;
            }
            if (_sinceLastHit > ContinueWindow) _nextStep = 0;

            step = _nextStep;
            _nextStep = (_nextStep + 1) % Steps;
            _sinceLastHit = 0f;
            return true;
        }

        /// <summary>True when <paramref name="step"/> is the last hit of the string.</summary>
        public bool IsFinisher(int step) => step == Steps - 1;

        /// <summary>Starts over from the first hit.</summary>
        public void Reset()
        {
            _nextStep = 0;
            _sinceLastHit = float.MaxValue;
        }

        /// <summary>Advances the timer.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || _sinceLastHit == float.MaxValue) return;
            _sinceLastHit += deltaTime;
        }
    }
}
