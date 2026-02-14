using System;
using UnityEngine;

namespace Nodebreaker.Data
{
    [CreateAssetMenu(fileName = "Wave_", menuName = "Nodebreaker/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Serializable]
        public class SpawnGroup
        {
            public NodeData nodeData;
            public int count = 5;
            public float spawnInterval = 0.5f;
        }

        public SpawnGroup[] spawnGroups;
        public float delayBeforeWave = 3f;
    }
}
