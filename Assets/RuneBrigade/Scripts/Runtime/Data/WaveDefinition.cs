using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// The waves of one arena room, in order. Prototype pacing (design/game-concept.md,
    /// "敵とボス"): goblin swarm → skeletons → ogre plus goblins.
    /// </summary>
    [CreateAssetMenu(menuName = "Rune Brigade/Wave Definition", fileName = "Waves_")]
    public sealed class WaveDefinition : ScriptableObject
    {
        /// <summary>A group of one enemy type inside a wave.</summary>
        [Serializable]
        public sealed class Group
        {
            [Tooltip("Enemy type.")]
            public EnemyDefinition enemy;
            [Tooltip("How many for one player; scaled up for more players.")]
            [Min(0)] public int count = 6;
        }

        /// <summary>One wave.</summary>
        [Serializable]
        public sealed class Wave
        {
            [Tooltip("Label for the HUD.")]
            public string label = "Wave";
            [Tooltip("Enemy groups in this wave.")]
            public List<Group> groups = new List<Group>();
            [Tooltip("Seconds between spawning each enemy.")]
            [Min(0f)] public float spawnInterval = 0.15f;
        }

        [Tooltip("Waves in order.")]
        public List<Wave> waves = new List<Wave>();
        [Tooltip("Seconds of rest after a wave is cleared.")]
        [Min(0f)] public float restSeconds = 3f;
    }
}
