using System;
using System.Collections.Generic;

namespace RuneBrigade.Core
{
    /// <summary>
    /// Limits how many enemies may attack at once (melee 3, ranged 2 by default),
    /// so the player is never hit from every side at the same moment.
    /// An enemy must hold a token to attack. Rule: design/game-concept.md, "設計の原則".
    /// </summary>
    public sealed class AttackTokenPool
    {
        private readonly Dictionary<int, AttackKind> _holders = new Dictionary<int, AttackKind>();
        private readonly int[] _capacity = new int[2];
        private readonly int[] _inUse = new int[2];

        /// <summary>Creates a pool.</summary>
        /// <param name="meleeCapacity">Simultaneous melee attackers.</param>
        /// <param name="rangedCapacity">Simultaneous ranged attackers.</param>
        public AttackTokenPool(int meleeCapacity, int rangedCapacity)
        {
            SetCapacity(AttackKind.Melee, meleeCapacity);
            SetCapacity(AttackKind.Ranged, rangedCapacity);
        }

        /// <summary>Changes a pool's size. Existing holders keep their tokens.</summary>
        public void SetCapacity(AttackKind kind, int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity[(int)kind] = capacity;
        }

        /// <summary>Token count for <paramref name="kind"/>.</summary>
        public int Capacity(AttackKind kind) => _capacity[(int)kind];

        /// <summary>Tokens of <paramref name="kind"/> held right now.</summary>
        public int InUse(AttackKind kind) => _inUse[(int)kind];

        /// <summary>True when <paramref name="ownerId"/> holds a token.</summary>
        public bool Holds(int ownerId) => _holders.ContainsKey(ownerId);

        /// <summary>Takes a token for <paramref name="ownerId"/>. Holding one already counts as success.</summary>
        /// <returns>True when the owner holds a token afterwards.</returns>
        public bool TryAcquire(int ownerId, AttackKind kind)
        {
            if (_holders.ContainsKey(ownerId)) return true;
            int k = (int)kind;
            if (_inUse[k] >= _capacity[k]) return false;
            _inUse[k]++;
            _holders.Add(ownerId, kind);
            return true;
        }

        /// <summary>Returns the owner's token, if any. Safe to call twice (e.g. on death).</summary>
        public void Release(int ownerId)
        {
            if (!_holders.TryGetValue(ownerId, out AttackKind kind)) return;
            _holders.Remove(ownerId);
            _inUse[(int)kind]--;
        }
    }
}
