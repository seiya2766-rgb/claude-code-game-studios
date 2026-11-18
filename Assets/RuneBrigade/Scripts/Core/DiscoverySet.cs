using System;
using System.Collections.Generic;

namespace RuneBrigade.Core
{
    /// <summary>
    /// Ids the player has found, kept for the archive: fusions discovered, enemy
    /// types recorded. Order of discovery is kept.
    /// </summary>
    public sealed class DiscoverySet
    {
        private readonly List<string> _order = new List<string>();
        private readonly HashSet<string> _ids = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>Ids in the order they were found.</summary>
        public IReadOnlyList<string> Ids => _order;

        /// <summary>Number found.</summary>
        public int Count => _order.Count;

        /// <summary>True when <paramref name="id"/> has been found.</summary>
        public bool Contains(string id) => id != null && _ids.Contains(id);

        /// <summary>Marks <paramref name="id"/> as found.</summary>
        /// <returns>True the first time only.</returns>
        public bool Discover(string id)
        {
            if (string.IsNullOrEmpty(id) || !_ids.Add(id)) return false;
            _order.Add(id);
            return true;
        }

        /// <summary>Replaces the contents from a save. Blanks and duplicates are skipped.</summary>
        public void Load(IEnumerable<string> ids)
        {
            _order.Clear();
            _ids.Clear();
            if (ids == null) return;
            foreach (string id in ids) Discover(id);
        }
    }
}
