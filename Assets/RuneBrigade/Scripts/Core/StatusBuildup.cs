using System;
using System.Collections.Generic;

namespace RuneBrigade.Core
{
    /// <summary>
    /// Per-enemy buildup toward a status. Weak repeated hits (the Arcanist's bolt)
    /// add points; at <see cref="Threshold"/> the status triggers and that meter resets.
    /// Meters drain by <see cref="DecayPerSecond"/> when not fed.
    /// </summary>
    public sealed class StatusBuildup
    {
        private readonly Dictionary<StatusType, float> _meters = new Dictionary<StatusType, float>();
        private readonly List<StatusType> _keys = new List<StatusType>();

        /// <summary>Creates a buildup tracker.</summary>
        /// <param name="threshold">Points needed to trigger a status.</param>
        /// <param name="decayPerSecond">Points lost per second.</param>
        public StatusBuildup(float threshold, float decayPerSecond)
        {
            if (threshold <= 0f) throw new ArgumentOutOfRangeException(nameof(threshold));
            if (decayPerSecond < 0f) throw new ArgumentOutOfRangeException(nameof(decayPerSecond));
            Threshold = threshold;
            DecayPerSecond = decayPerSecond;
        }

        /// <summary>Points needed to trigger.</summary>
        public float Threshold { get; }

        /// <summary>Points lost per second.</summary>
        public float DecayPerSecond { get; }

        /// <summary>Current points toward <paramref name="type"/>.</summary>
        public float Value(StatusType type) => _meters.TryGetValue(type, out float value) ? value : 0f;

        /// <summary>Adds points toward a status.</summary>
        /// <returns>True when the threshold was reached; the meter is then reset to 0.</returns>
        public bool Add(StatusType type, float points)
        {
            if (points <= 0f) return false;
            float value = Value(type) + points;
            if (value >= Threshold)
            {
                _meters[type] = 0f;
                return true;
            }
            _meters[type] = value;
            return false;
        }

        /// <summary>Drains every meter.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || DecayPerSecond <= 0f || _meters.Count == 0) return;
            _keys.Clear();
            _keys.AddRange(_meters.Keys);
            foreach (StatusType key in _keys)
            {
                _meters[key] = Math.Max(0f, _meters[key] - DecayPerSecond * deltaTime);
            }
        }
    }
}
