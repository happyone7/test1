using System.Collections;
using Tesseract.Core;
using Tesseract.ObjectPool;
using UnityEngine;
using Nodebreaker.Audio;

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
                bool isBossWave = IsBossWave(wave);

                if (isBossWave)
                {
                    SoundManager.Instance.PlaySfx(SoundKeys.BossAppear, 1f);
                    SoundManager.Instance.PlayBgm(SoundKeys.BgmBoss, 0.6f);
                }
                else
                {
                    SoundManager.Instance.PlaySfx(SoundKeys.WaveStart, 0.9f);
                }

                yield return new WaitForSeconds(wave.delayBeforeWave);
                yield return StartCoroutine(SpawnWave(wave));

                // 웨이브의 모든 노드가 죽거나 도착할 때까지 대기
                while (_aliveCount > 0)
                    yield return null;

                if (isBossWave)
                    SoundManager.Instance.PlaySfx(SoundKeys.BossDefeat, 1f);
                else
                    SoundManager.Instance.PlaySfx(SoundKeys.WaveClear, 0.9f);

                _currentWaveIndex++;

                // RunManager 웨이브 카운터 동기화
                if (Singleton<Core.RunManager>.HasInstance)
                    Core.RunManager.Instance.OnSingleWaveCleared();

                // 마지막 웨이브가 아닌 경우에만 타워 드롭
                if (_currentWaveIndex < _currentStage.waves.Length)
                    DropRandomTowerToInventory();
            }

            // 모든 웨이브 완료 → RunManager에 알림
            if (Singleton<Core.RunManager>.HasInstance)
                Core.RunManager.Instance.OnAllWavesCompleted();
        }

        private static bool IsBossWave(Data.WaveData wave)
        {
            if (wave == null || wave.spawnGroups == null) return false;
            foreach (var group in wave.spawnGroups)
            {
                if (group == null || group.nodeData == null) continue;
                if (group.nodeData.type == Data.NodeType.Boss)
                    return true;
            }
            return false;
        }

        IEnumerator SpawnWave(Data.WaveData wave)
        {
            _spawning = true;

            // spawnRateMultiplier > 1 이면 스폰 간격이 줄어듦 (적이 더 빨리 나옴 = 난이도 상향)
            // spawnRateMultiplier < 1 이면 스폰 간격이 늘어남 (적이 더 느리게 나옴 = 여유)
            float spawnRateMul = 1f;
            if (Singleton<Core.RunManager>.HasInstance)
                spawnRateMul = Core.RunManager.Instance.CurrentModifiers.spawnRateMultiplier;
            // 역수 적용: 스킬은 "스폰 속도 감소"를 의미하므로 간격은 곱연산
            float intervalMul = spawnRateMul > 0f ? 1f / spawnRateMul : 1f;

            foreach (var group in wave.spawnGroups)
            {
                for (int i = 0; i < group.count; i++)
                {
                    SpawnNode(group.nodeData);
                    yield return new WaitForSeconds(group.spawnInterval * intervalMul);
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

        public void StopSpawning()
        {
            StopAllCoroutines();
            _spawning = false;
            _aliveCount = 0;
        }

        public void OnNodeRemoved()
        {
            _aliveCount = Mathf.Max(0, _aliveCount - 1);
        }

        private void DropRandomTowerToInventory()
        {
            if (!Singleton<Tower.TowerManager>.HasInstance) return;

            var available = Tower.TowerManager.Instance.availableTowers;
            if (available == null || available.Length == 0) return;

            if (!Singleton<Tower.TowerInventory>.HasInstance) return;

            var inventory = Tower.TowerInventory.Instance;
            if (inventory.IsFull)
            {
                Debug.Log("[WaveSpawner] 인벤토리가 가득 차서 타워 드롭 불가");
                return;
            }

            var randomTower = available[Random.Range(0, available.Length)];
            inventory.TryAddTower(randomTower, 1);
            Debug.Log($"[WaveSpawner] 웨이브 클리어 보상: {randomTower.towerName} Lv1 획득");
        }
    }
}
