using System;
using System.Collections.Generic;

namespace RuneBrigade.Core
{
    /// <summary>
    /// Holds the statuses on one enemy. At most <see cref="Capacity"/> at a time;
    /// applying a new one when full evicts the oldest. Re-applying refreshes the timer.
    /// Rules: design/game-concept.md, "状態と合成のルール".
    /// </summary>
    public sealed class StatusContainer
    {
        private struct Entry
        {
            public StatusType Type;
            public float Remaining;
            public long Order;
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private long _nextOrder;

        /// <summary>Creates a container that holds up to <paramref name="capacity"/> statuses.</summary>
        /// <param name="capacity">Maximum simultaneous statuses (at least 1).</param>
        public StatusContainer(int capacity)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be at least 1.");
            Capacity = capacity;
        }

        /// <summary>Maximum simultaneous statuses.</summary>
        public int Capacity { get; private set; }

        /// <summary>Number of statuses currently held.</summary>
        public int Count => _entries.Count;

        /// <summary>Raised when a status is applied or refreshed.</summary>
        public event Action<StatusType> Applied;

        /// <summary>Raised when a status ends: expired, evicted, consumed or cleared.</summary>
        public event Action<StatusType> Removed;

        /// <summary>Changes the capacity, evicting the oldest statuses if needed (e.g. a rune grants a third slot).</summary>
        /// <param name="capacity">New capacity (at least 1).</param>
        public void SetCapacity(int capacity)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be at least 1.");
            Capacity = capacity;
            while (_entries.Count > Capacity) RemoveAt(OldestIndex());
        }

        /// <summary>Applies a status for <paramref name="duration"/> seconds, or refreshes it if present.</summary>
        /// <param name="type">Status to apply.</param>
        /// <param name="duration">Seconds it lasts; must be positive.</param>
        public void Apply(StatusType type, float duration)
        {
            if (duration <= 0f) throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");

            int index = IndexOf(type);
            if (index >= 0)
            {
                Entry refreshed = _entries[index];
                refreshed.Remaining = Math.Max(refreshed.Remaining, duration);
                _entries[index] = refreshed;
            }
            else
            {
                if (_entries.Count >= Capacity) RemoveAt(OldestIndex());
                _entries.Add(new Entry { Type = type, Remaining = duration, Order = _nextOrder++ });
            }

            Applied?.Invoke(type);
        }

        /// <summary>True if the status is currently held.</summary>
        public bool Has(StatusType type) => IndexOf(type) >= 0;

        /// <summary>Seconds left on a status, or 0 when absent.</summary>
        public float RemainingTime(StatusType type)
        {
            int index = IndexOf(type);
            return index >= 0 ? _entries[index].Remaining : 0f;
        }

        /// <summary>Removes a status if held.</summary>
        /// <returns>True when something was removed.</returns>
        public bool Remove(StatusType type)
        {
            int index = IndexOf(type);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }

        /// <summary>Removes every status.</summary>
        public void Clear()
        {
            while (_entries.Count > 0) RemoveAt(_entries.Count - 1);
        }

        /// <summary>Advances timers and drops expired statuses.</summary>
        /// <param name="deltaTime">Seconds elapsed (scaled game time).</param>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                Entry entry = _entries[i];
                entry.Remaining -= deltaTime;
                if (entry.Remaining <= 0f) RemoveAt(i);
                else _entries[i] = entry;
            }
        }

        /// <summary>Copies the held statuses, oldest first, into <paramref name="buffer"/>.</summary>
        public void CopyTo(List<StatusType> buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            buffer.Clear();
            var sorted = new List<Entry>(_entries);
            sorted.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (Entry entry in sorted) buffer.Add(entry.Type);
        }

        private int IndexOf(StatusType type)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Type == type) return i;
            }
            return -1;
        }

        private int OldestIndex()
        {
            int oldest = 0;
            for (int i = 1; i < _entries.Count; i++)
            {
                if (_entries[i].Order < _entries[oldest].Order) oldest = i;
            }
            return oldest;
        }

        private void RemoveAt(int index)
        {
            StatusType type = _entries[index].Type;
            _entries.RemoveAt(index);
            Removed?.Invoke(type);
        }
    }
}
