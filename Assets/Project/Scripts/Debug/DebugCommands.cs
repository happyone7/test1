#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using Tesseract.Core;
using Soulspire.Core;

namespace Soulspire.Debugging
{
    /// <summary>
    /// 디버그 치트 커맨드 모음. 모든 메서드는 static.
    /// DebugPanel에서 IMGUI 버튼으로 호출.
    /// </summary>
    public static class DebugCommands
    {
        // ═══════════════════════════════════════
        // 1. 특정 웨이브로 즉시 이동 (Jump to Wave)
        // ═══════════════════════════════════════

        /// <summary>
        /// 현재 런이 진행 중일 때, 지정한 웨이브 번호로 즉시 건너뛴다.
        /// 현재 스폰 중인 웨이브를 중단하고, WaveSpawner의 _currentWaveIndex를 강제 설정 후 재시작.
        /// </summary>
        public static void JumpToWave(int waveIndex)
        {
            if (!Singleton<RunManager>.HasInstance || !RunManager.Instance.IsRunning)
            {
                Debug.LogWarning("[DebugCmd] JumpToWave: 런이 진행 중이 아닙니다.");
                return;
            }

            if (!Singleton<Node.WaveSpawner>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] JumpToWave: WaveSpawner 인스턴스 없음");
                return;
            }

            var stage = RunManager.Instance.CurrentStage;
            if (stage == null || stage.waves == null)
            {
                Debug.LogWarning("[DebugCmd] JumpToWave: CurrentStage 또는 waves가 null");
                return;
            }

            if (waveIndex < 0 || waveIndex >= stage.waves.Length)
            {
                Debug.LogWarning($"[DebugCmd] JumpToWave: waveIndex={waveIndex} 범위 밖 (0~{stage.waves.Length - 1})");
                return;
            }

            // 현재 스폰 중단
            Node.WaveSpawner.Instance.StopSpawning();

            // 살아있는 노드 모두 제거
            var nodes = Object.FindObjectsByType<Node.Node>(FindObjectsSortMode.None);
            foreach (var node in nodes)
            {
                if (node.IsAlive)
                    Tesseract.ObjectPool.Poolable.TryPool(node.gameObject);
            }

            // RunManager 웨이브 인덱스 강제 설정
            RunManager.Instance.Debug_SetWaveIndex(waveIndex);

            // WaveSpawner 재시작 (해당 웨이브부터)
            Node.WaveSpawner.Instance.Debug_StartFromWave(stage, waveIndex);

            Debug.Log($"[DebugCmd] JumpToWave: Wave {waveIndex}로 이동 완료");
        }

        // ═══════════════════════════════════════
        // 2. 특정 스테이지 선택 시작 (Start Specific Stage)
        // ═══════════════════════════════════════

        /// <summary>
        /// 해금 조건을 무시하고 지정 스테이지로 즉시 런을 시작한다.
        /// </summary>
        public static void StartSpecificStage(int stageIndex)
        {
            if (!Singleton<GameManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] StartSpecificStage: GameManager 인스턴스 없음");
                return;
            }

            var gm = GameManager.Instance;
            if (gm.stages == null || gm.stages.Length == 0)
            {
                Debug.LogWarning("[DebugCmd] StartSpecificStage: stages 배열이 비어있음");
                return;
            }

            if (stageIndex < 0 || stageIndex >= gm.stages.Length)
            {
                Debug.LogWarning($"[DebugCmd] StartSpecificStage: stageIndex={stageIndex} 범위 밖 (0~{gm.stages.Length - 1})");
                return;
            }

            gm.StartRun(stageIndex);
            Debug.Log($"[DebugCmd] StartSpecificStage: Stage {stageIndex} ({gm.stages[stageIndex].stageName}) 시작");
        }

        // ═══════════════════════════════════════
        // 3. 게임 상태 강제 전환 (GoToState)
        // ═══════════════════════════════════════

        /// <summary>
        /// Title 상태로 강제 전환.
        /// </summary>
        public static void GoToTitle()
        {
            if (!Singleton<GameManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] GoToTitle: GameManager 인스턴스 없음");
                return;
            }

            // 런 중이면 정리
            CleanupCurrentState();

            GameManager.Instance.Debug_SetState(GameState.Title);
            Debug.Log("[DebugCmd] GoToTitle: State=Title");
        }

        /// <summary>
        /// Hub 상태로 강제 전환.
        /// </summary>
        public static void GoToHub()
        {
            if (!Singleton<GameManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] GoToHub: GameManager 인스턴스 없음");
                return;
            }

            GameManager.Instance.GoToHub();
            Debug.Log("[DebugCmd] GoToHub: State=Hub");
        }

        /// <summary>
        /// InGame 상태로 강제 전환 (현재 스테이지로 런 시작).
        /// </summary>
        public static void GoToInGame()
        {
            if (!Singleton<GameManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] GoToInGame: GameManager 인스턴스 없음");
                return;
            }

            GameManager.Instance.StartRun(-1); // 현재 스테이지 인덱스 사용
            Debug.Log("[DebugCmd] GoToInGame: State=InGame");
        }

        /// <summary>
        /// RunEnd 상태로 강제 전환.
        /// </summary>
        public static void GoToRunEnd()
        {
            if (!Singleton<GameManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] GoToRunEnd: GameManager 인스턴스 없음");
                return;
            }

            GameManager.Instance.Debug_SetState(GameState.RunEnd);
            Debug.Log("[DebugCmd] GoToRunEnd: State=RunEnd");
        }

        // ═══════════════════════════════════════
        // 4. 보물상자 강제 드롭/오픈 (Force Treasure Chest)
        // ═══════════════════════════════════════

        /// <summary>
        /// 보물상자 드롭을 강제 발동한다.
        /// TreasureManager가 있으면 OnWaveCleared를 확률 무시하고 직접 호출.
        /// </summary>
        public static void ForceTreasureDrop()
        {
            if (!Singleton<TreasureManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] ForceTreasureDrop: TreasureManager 인스턴스 없음 (보물상자 시스템 미구현 또는 씬에 없음)");
                return;
            }

            var tm = TreasureManager.Instance;
            if (tm.treasureConfig == null)
            {
                Debug.LogWarning("[DebugCmd] ForceTreasureDrop: TreasureConfig가 할당되지 않음");
                return;
            }

            // 강제 드롭: TreasureManager의 OnWaveCleared 대신 직접 이벤트 발동
            tm.Debug_ForceDrop();
            Debug.Log("[DebugCmd] ForceTreasureDrop: 보물상자 강제 드롭 완료");
        }

        // ═══════════════════════════════════════
        // 5. Core(프리미엄 재화) 추가 (Add Core)
        // ═══════════════════════════════════════

        /// <summary>
        /// Core 재화를 지정량만큼 추가한다.
        /// </summary>
        public static void AddCore(int amount)
        {
            if (!Singleton<MetaManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] AddCore: MetaManager 인스턴스 없음");
                return;
            }

            MetaManager.Instance.Debug_AddCoreFragment(amount);
            Debug.Log($"[DebugCmd] AddCoreFragment: +{amount} Core (총 {MetaManager.Instance.TotalCoreFragment})");
        }

        // ═══════════════════════════════════════
        // 6. 특정 스킬 노드 개별 레벨 설정 (Set Skill Level)
        // ═══════════════════════════════════════

        /// <summary>
        /// 지정 스킬의 레벨을 강제 설정한다.
        /// </summary>
        public static void SetSkillLevel(string skillId, int level)
        {
            if (!Singleton<MetaManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] SetSkillLevel: MetaManager 인스턴스 없음");
                return;
            }

            MetaManager.Instance.Debug_SetSkillLevel(skillId, level);
            Debug.Log($"[DebugCmd] SetSkillLevel: {skillId} = Lv.{level}");
        }

        // ═══════════════════════════════════════
        // 7. FTUE 플래그 리셋/설정 (FTUE Control)
        // ═══════════════════════════════════════

        /// <summary>
        /// 지정 FTUE 플래그를 설정/해제한다.
        /// </summary>
        public static void SetFtueFlag(int index, bool value)
        {
            if (!Singleton<MetaManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] SetFtueFlag: MetaManager 인스턴스 없음");
                return;
            }

            MetaManager.Instance.SetFtueFlag(index, value);
            Debug.Log($"[DebugCmd] SetFtueFlag: [{index}] = {value}");
        }

        /// <summary>
        /// 모든 FTUE 플래그를 초기화한다 (전체 false).
        /// </summary>
        public static void ResetAllFtueFlags()
        {
            if (!Singleton<MetaManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] ResetAllFtueFlags: MetaManager 인스턴스 없음");
                return;
            }

            MetaManager.Instance.Debug_ResetAllFtueFlags();
            Debug.Log("[DebugCmd] ResetAllFtueFlags: 모든 FTUE 플래그 초기화 완료");
        }

        // ═══════════════════════════════════════
        // 9. 노드 킬 카운트 조작 (Set Nodes Killed)
        // ═══════════════════════════════════════

        /// <summary>
        /// MetaManager의 totalNodesKilled 값을 직접 설정한다.
        /// </summary>
        public static void SetNodesKilled(int count)
        {
            if (!Singleton<MetaManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] SetNodesKilled: MetaManager 인스턴스 없음");
                return;
            }

            MetaManager.Instance.Debug_SetNodesKilled(count);
            Debug.Log($"[DebugCmd] SetNodesKilled: totalNodesKilled = {count}");
        }

        /// <summary>
        /// Bit(일반 재화)를 추가한다.
        /// </summary>
        public static void AddSoul(int amount)
        {
            if (!Singleton<MetaManager>.HasInstance)
            {
                Debug.LogWarning("[DebugCmd] AddSoul: MetaManager 인스턴스 없음");
                return;
            }

            MetaManager.Instance.Debug_AddSoul(amount);
            Debug.Log($"[DebugCmd] AddSoul: +{amount} Soul (총 {MetaManager.Instance.TotalSoul})");
        }

        // ── 유틸 ──

        static void CleanupCurrentState()
        {
            // 런 중이면 정리
            if (Singleton<RunManager>.HasInstance && RunManager.Instance.IsRunning)
            {
                RunManager.Instance.ResetRun();
            }

            if (Singleton<Node.WaveSpawner>.HasInstance)
                Node.WaveSpawner.Instance.StopSpawning();

            // 살아있는 노드 제거
            var nodes = Object.FindObjectsByType<Node.Node>(FindObjectsSortMode.None);
            foreach (var node in nodes)
            {
                if (node.IsAlive)
                    Tesseract.ObjectPool.Poolable.TryPool(node.gameObject);
            }

            // 타워 정리
            if (Singleton<Tower.TowerManager>.HasInstance)
                Tower.TowerManager.Instance.ClearAllTowers();

            if (Singleton<Tower.PlacementGrid>.HasInstance)
                Tower.PlacementGrid.Instance.ClearOccupied();
        }
    }
}
#endif
