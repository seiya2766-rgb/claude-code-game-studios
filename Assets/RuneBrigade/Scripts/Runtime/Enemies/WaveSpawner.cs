using System;
using System.Collections;
using System.Collections.Generic;
using RuneBrigade.Core;
using UnityEngine;
using UnityEngine.AI;

namespace RuneBrigade
{
    /// <summary>
    /// Runs one arena's waves in order. Enemy counts and health scale with
    /// <see cref="playerCount"/> through <see cref="PartyScaling"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private CombatDirector director;
        [SerializeField] private WaveDefinition waves;
        [SerializeField] private PlayerHealth player;
        [Tooltip("Where enemies appear; used round-robin.")]
        [SerializeField] private Transform[] spawnPoints;
        [Tooltip("Players in the session. Prototype is solo, but scaling is live for testing.")]
        [Range(1, 4)] [SerializeField] private int playerCount = 1;
        [SerializeField] private bool startOnPlay = true;
        [Tooltip("Seconds before the first wave.")]
        [Min(0f)] [SerializeField] private float startDelay = 2f;

        private readonly HashSet<EnemyHealth> _alive = new HashSet<EnemyHealth>();
        private int _spawnIndex;

        /// <summary>Label of the wave in progress, for the HUD.</summary>
        public string CurrentLabel { get; private set; } = "";

        /// <summary>Enemies alive right now.</summary>
        public int AliveCount => _alive.Count;

        /// <summary>True once every wave is cleared.</summary>
        public bool IsCleared { get; private set; }

        /// <summary>Raised when every wave is cleared.</summary>
        public event Action Cleared;

        private void Start()
        {
            if (startOnPlay) Begin();
        }

        /// <summary>Starts the waves from the beginning.</summary>
        public void Begin()
        {
            StopAllCoroutines();
            IsCleared = false;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            if (spawnPoints == null || spawnPoints.Length == 0) throw new InvalidOperationException("WaveSpawner needs spawn points.");
            yield return new WaitForSeconds(startDelay);

            for (int w = 0; w < waves.waves.Count; w++)
            {
                WaveDefinition.Wave wave = waves.waves[w];
                CurrentLabel = string.IsNullOrEmpty(wave.label) ? "Wave " + (w + 1) : wave.label;

                foreach (WaveDefinition.Group group in wave.groups)
                {
                    int count = PartyScaling.ScaledCount(group.count, playerCount, director.Tuning.enemyCountPerExtraPlayer);
                    for (int i = 0; i < count; i++)
                    {
                        Spawn(group.enemy);
                        if (wave.spawnInterval > 0f) yield return new WaitForSeconds(wave.spawnInterval);
                    }
                }

                while (_alive.Count > 0) yield return null;
                if (w < waves.waves.Count - 1) yield return new WaitForSeconds(waves.restSeconds);
            }

            CurrentLabel = "クリア";
            IsCleared = true;
            Cleared?.Invoke();
        }

        private void Spawn(EnemyDefinition definition)
        {
            if (!definition || !definition.prefab) return;
            Transform point = spawnPoints[_spawnIndex++ % spawnPoints.Length];
            Vector3 position = point.position;
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 4f, NavMesh.AllAreas)) position = hit.position;

            EnemyBrain brain = Instantiate(definition.prefab, position, point.rotation);
            float health = definition.maxHealth * PartyScaling.HealthMultiplier(playerCount, director.Tuning.enemyHealthPerExtraPlayer);
            brain.Initialize(director, definition, player, health);

            EnemyHealth enemy = brain.GetComponent<EnemyHealth>();
            _alive.Add(enemy);
            enemy.Died += e => _alive.Remove(e);
        }
    }
}
