using System;
using System.Collections.Generic;
using System.IO;
using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// The camp's saved progress, shared by the camp and the run scenes through this
    /// asset (no singleton). Every change is saved at once, and each scene calls
    /// <see cref="Load"/> when it starts, so in-memory state never outlives a scene.
    /// Saved as JSON in <c>Application.persistentDataPath</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "Rune Brigade/Camp Progress Store", fileName = "CampProgressStore")]
    public sealed class CampProgressStore : ScriptableObject
    {
        [Serializable]
        private sealed class SaveData
        {
            public int version = 1;
            public int[] milestoneCounts = new int[0];
            public string[] discoveredCombos = new string[0];
            public string[] recordedEnemies = new string[0];
            public int[] pendingAnnouncements = new int[0];
        }

        [SerializeField] private CampUnlockTable unlockTable;
        [Tooltip("File name under Application.persistentDataPath.")]
        [SerializeField] private string fileName = "camp-progress.json";

        [NonSerialized] private CampProgress _progress;
        [NonSerialized] private DiscoverySet _combos;
        [NonSerialized] private DiscoverySet _enemies;
        [NonSerialized] private readonly List<CampFeature> _pending = new List<CampFeature>();

        /// <summary>Unlock state. Call <see cref="Load"/> first.</summary>
        public CampProgress Progress => _progress ?? throw new InvalidOperationException("CampProgressStore.Load() was not called.");

        /// <summary>Fusions discovered (archive).</summary>
        public DiscoverySet Combos => _combos;

        /// <summary>Enemy types recorded (archive).</summary>
        public DiscoverySet Enemies => _enemies;

        /// <summary>Full path of the save file.</summary>
        public string SavePath => Path.Combine(Application.persistentDataPath, fileName);

        /// <summary>Reads the save file (or starts fresh when there is none or it is unreadable).</summary>
        public void Load()
        {
            if (!unlockTable) throw new InvalidOperationException("CampProgressStore needs a CampUnlockTable.");
            _progress = unlockTable.CreateProgress();
            _combos = new DiscoverySet();
            _enemies = new DiscoverySet();
            _pending.Clear();

            SaveData data = ReadFile();
            if (data == null) return;
            _progress.LoadCounts(data.milestoneCounts);
            _combos.Load(data.discoveredCombos);
            _enemies.Load(data.recordedEnemies);
            foreach (int feature in data.pendingAnnouncements)
            {
                if (Enum.IsDefined(typeof(CampFeature), feature)) _pending.Add((CampFeature)feature);
            }
        }

        /// <summary>Records a milestone; anything it unlocks is queued for the camp to announce. Saves.</summary>
        public List<CampFeature> Record(CampMilestone milestone)
        {
            List<CampFeature> unlocked = Progress.Record(milestone);
            _pending.AddRange(unlocked);
            Save();
            return unlocked;
        }

        /// <summary>Records an enemy type; the first record of each type counts as a milestone. Saves.</summary>
        /// <returns>True when this type was new.</returns>
        public bool RecordEnemy(string enemyId)
        {
            if (!_enemies.Discover(enemyId)) return false;
            Record(CampMilestone.EnemyRecorded);
            return true;
        }

        /// <summary>Marks a fusion as discovered. Saves when new.</summary>
        /// <returns>True when it was new.</returns>
        public bool DiscoverCombo(string comboId)
        {
            if (!_combos.Discover(comboId)) return false;
            Save();
            return true;
        }

        /// <summary>Returns and clears the features waiting to be announced. Saves.</summary>
        public List<CampFeature> TakePendingAnnouncements()
        {
            var taken = new List<CampFeature>(_pending);
            _pending.Clear();
            Save();
            return taken;
        }

        /// <summary>Deletes all progress (debug).</summary>
        public void ResetAll()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            Load();
        }

        /// <summary>Writes the current state to disk.</summary>
        public void Save()
        {
            var data = new SaveData
            {
                milestoneCounts = Progress.ToCounts(),
                discoveredCombos = new List<string>(_combos.Ids).ToArray(),
                recordedEnemies = new List<string>(_enemies.Ids).ToArray(),
                pendingAnnouncements = _pending.ConvertAll(f => (int)f).ToArray(),
            };
            try
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            }
            catch (IOException e)
            {
                Debug.LogError("Camp progress could not be saved to " + SavePath + ": " + e.Message);
            }
        }

        private SaveData ReadFile()
        {
            if (!File.Exists(SavePath)) return null;
            try
            {
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Camp progress at " + SavePath + " was unreadable and is ignored: " + e.Message);
                return null;
            }
        }
    }
}
