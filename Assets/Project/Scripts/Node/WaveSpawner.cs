using System.Collections;
using Tesseract.Core;
using Tesseract.ObjectPool;
using UnityEngine;

namespace Nodebreaker.Node
{
    public class WaveSpawner : Singleton<WaveSpawner>
    {
        [Header("경로")]
        public Transform[] waypoints;

        [Header("스폰 위치")]
        public Transform spawnPoint;

        Data.StageData _currentStage;
        int _currentWaveIndex;
        int _aliveCount;
        bool _spawning;

        public int AliveCount => _aliveCount;

        public void StartWaves(Data.StageData stage)
        {
            _currentStage = stage;
            _currentWaveIndex = 0;
            _aliveCount = 0;
            StartCoroutine(SpawnAllWaves());
        }

        IEnumerator SpawnAllWaves()
        {
            while (_currentWaveIndex < _currentStage.waves.Length)
            {
                var wave = _currentStage.waves[_currentWaveIndex];
                yield return new WaitForSeconds(wave.delayBeforeWave);
                yield return StartCoroutine(SpawnWave(wave));

                // 웨이브의 모든 노드가 죽거나 도착할 때까지 대기
                while (_aliveCount > 0)
                    yield return null;

                _currentWaveIndex++;
            }

            if (Singleton<Core.RunManager>.HasInstance)
                Core.RunManager.Instance.OnWaveCompleted();
        }

        IEnumerator SpawnWave(Data.WaveData wave)
        {
            _spawning = true;
            foreach (var group in wave.spawnGroups)
            {
                for (int i = 0; i < group.count; i++)
                {
                    SpawnNode(group.nodeData);
                    yield return new WaitForSeconds(group.spawnInterval);
                }
            }
            _spawning = false;
        }

        void SpawnNode(Data.NodeData nodeData)
        {
            var go = Poolable.TryGetPoolable(nodeData.prefab, spawnPoint);
            var node = go.GetComponent<Node>();
            if (node != null)
            {
                node.Initialize(
                    nodeData,
                    waypoints,
                    _currentStage.hpMultiplier,
                    _currentStage.speedMultiplier,
                    _currentStage.bitDropMultiplier
                );
                _aliveCount++;
            }
        }

        public void OnNodeRemoved()
        {
            _aliveCount = Mathf.Max(0, _aliveCount - 1);
        }
    }
}
