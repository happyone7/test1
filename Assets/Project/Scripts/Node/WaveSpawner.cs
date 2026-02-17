using System.Collections;
using Tesseract.Core;
using Tesseract.ObjectPool;
using UnityEngine;
using Soulspire.Audio;

namespace Soulspire.Node
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

                    // FTUE: 보스 웨이브 가이드
                    if (Singleton<UI.FTUEManager>.HasInstance)
                        UI.FTUEManager.Instance.TriggerInGame("BossWave", UI.FTUEManager.GuideBossWave);

                    // 보스 HP 바 표시
                    ShowBossHpBar(wave);
                }
                else
                {
                    SoundManager.Instance.PlaySfx(SoundKeys.WaveStart, 0.9f);
                }

                // FTUE: 첫 웨이브 가이드
                if (_currentWaveIndex == 0 && Singleton<UI.FTUEManager>.HasInstance)
                    UI.FTUEManager.Instance.TriggerInGame("FirstWave", UI.FTUEManager.GuideFirstWave);

                yield return new WaitForSeconds(wave.delayBeforeWave);
                yield return StartCoroutine(SpawnWave(wave));

                // 웨이브의 모든 노드가 죽거나 도착할 때까지 대기
                while (_aliveCount > 0)
                    yield return null;

                if (isBossWave)
                {
                    SoundManager.Instance.PlaySfx(SoundKeys.BossDefeat, 1f);

                    // 보스 처치 시 타워 보물상자 드롭
                    if (Singleton<Core.TreasureManager>.HasInstance)
                        Core.TreasureManager.Instance.OnBossKilled();

                    // 보스 HP 바 숨김
                    var inGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
                    if (inGameUI != null)
                        inGameUI.HideBossHpBar();
                }
                else
                    SoundManager.Instance.PlaySfx(SoundKeys.WaveClear, 0.9f);

                _currentWaveIndex++;

                // RunManager 웨이브 카운터 동기화
                if (Singleton<Core.RunManager>.HasInstance)
                    Core.RunManager.Instance.OnSingleWaveCleared();

                // 마지막 웨이브가 아닌 경우에만 타워 드롭
                if (_currentWaveIndex < _currentStage.waves.Length)
                {
                    DropRandomTowerToInventory();

                }
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
                    _currentStage.soulDropMultiplier
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

        /// <summary>
        /// 보스 웨이브의 첫 보스 데이터를 기반으로 InGameUI에 보스 HP 바를 표시합니다.
        /// </summary>
        private void ShowBossHpBar(Data.WaveData wave)
        {
            if (wave == null || wave.spawnGroups == null) return;

            foreach (var group in wave.spawnGroups)
            {
                if (group == null || group.nodeData == null) continue;
                if (group.nodeData.type == Data.NodeType.Boss)
                {
                    float scaledHp = group.nodeData.hp;
                    if (_currentStage != null)
                        scaledHp *= _currentStage.hpMultiplier;

                    string bossName = group.nodeData.nodeName;

                    var inGameUI = Object.FindFirstObjectByType<UI.InGameUI>(FindObjectsInactive.Include);
                    if (inGameUI != null)
                        inGameUI.ShowBossHpBar(bossName, scaledHp);

                    break;
                }
            }
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

            // FTUE: 첫 타워 드롭 가이드
            if (Singleton<UI.FTUEManager>.HasInstance)
                UI.FTUEManager.Instance.TriggerInGame("FirstTowerDrop", UI.FTUEManager.GuideFirstTowerDrop);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // ── 디버그 전용 메서드 ──

        /// <summary>
        /// 지정 웨이브부터 스폰을 재시작합니다 (디버그용).
        /// StopSpawning() 호출 후에 사용해야 합니다.
        /// </summary>
        public void Debug_StartFromWave(Data.StageData stage, int waveIndex)
        {
            _currentStage = stage;
            _currentWaveIndex = waveIndex;
            _aliveCount = 0;
            StartCoroutine(SpawnAllWaves());
        }
#endif
    }
}
