#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using Tesseract.Core;
using Soulspire.Core;

namespace Soulspire.Debugging
{
    /// <summary>
    /// IMGUI 기반 디버그 패널.
    /// DebugManager.PanelVisible이 true일 때만 표시.
    /// 9개의 높음 우선순위 디버그 기능을 제공.
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        // ── 패널 레이아웃 ──
        Rect _windowRect = new Rect(20, 80, 420, 700);
        Vector2 _scrollPos;

        // ── 입력 필드 상태 ──
        string _waveIndexInput = "0";
        string _stageIndexInput = "0";
        string _coreAmountInput = "10";
        string _soulAmountInput = "100";
        string _skillIdInput = "";
        string _skillLevelInput = "1";
        string _nodesKilledInput = "100";
        int _selectedSkillIndex;

        // ── 섹션 접기 상태 ──
        bool _foldWave = true;
        bool _foldStage = true;
        bool _foldState = true;
        bool _foldTreasure = true;
        bool _foldCore = true;
        bool _foldSkill = true;
        bool _foldFtue = true;
        bool _foldKillCount = true;
        bool _foldInfo = true;

        void OnGUI()
        {
            if (!Singleton<DebugManager>.HasInstance) return;
            if (!DebugManager.Instance.PanelVisible) return;

            _windowRect = GUI.Window(9999, _windowRect, DrawWindow, "Debug Panel (F12)");
        }

        void DrawWindow(int windowId)
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            // ── 게임 상태 정보 ──
            DrawInfoSection();

            GUILayout.Space(6);

            // ── 1. 웨이브 이동 ──
            DrawWaveJumpSection();

            // ── 2. 스테이지 선택 시작 ──
            DrawStageStartSection();

            // ── 3. 게임 상태 전환 ──
            DrawStateTransitionSection();

            // ── 4. 보물상자 ──
            DrawTreasureSection();

            // ── 5. Core 추가 ──
            DrawCoreSection();

            // ── 6. 스킬 레벨 설정 ──
            DrawSkillSection();

            // ── 7. FTUE 플래그 ──
            DrawFtueSection();

            // ── 9. 노드 킬 카운트 ──
            DrawKillCountSection();

            GUILayout.Space(10);

            GUILayout.EndScrollView();

            // 드래그 가능하게
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        // ═══════════════════════════════════════
        // 게임 상태 정보
        // ═══════════════════════════════════════

        void DrawInfoSection()
        {
            _foldInfo = GUILayout.Toggle(_foldInfo, "--- Game Info ---", "button");
            if (!_foldInfo) return;

            GUILayout.BeginVertical("box");

            string state = Singleton<GameManager>.HasInstance
                ? GameManager.Instance.State.ToString() : "N/A";
            string soul = Singleton<MetaManager>.HasInstance
                ? MetaManager.Instance.TotalSoul.ToString() : "N/A";
            string coreFragment = Singleton<MetaManager>.HasInstance
                ? MetaManager.Instance.TotalCoreFragment.ToString() : "N/A";
            string stageIdx = Singleton<MetaManager>.HasInstance
                ? MetaManager.Instance.CurrentStageIndex.ToString() : "N/A";
            string nodesKilled = Singleton<MetaManager>.HasInstance
                ? MetaManager.Instance.TotalNodesKilled.ToString() : "N/A";

            GUILayout.Label($"State: {state}  |  Soul: {soul}  |  CoreFragment: {coreFragment}");
            GUILayout.Label($"StageIdx: {stageIdx}  |  TotalKills: {nodesKilled}");

            if (Singleton<RunManager>.HasInstance && RunManager.Instance.IsRunning)
            {
                var rm = RunManager.Instance;
                GUILayout.Label($"Run: Wave {rm.CurrentWaveIndex}  |  HP: {rm.BaseHp}/{rm.BaseMaxHp}  |  Soul: {rm.SoulEarned}  |  Kills: {rm.NodesKilled}");
            }

            GUILayout.EndVertical();
        }

        // ═══════════════════════════════════════
        // 1. 웨이브 이동
        // ═══════════════════════════════════════

        void DrawWaveJumpSection()
        {
            _foldWave = GUILayout.Toggle(_foldWave, "1. Jump to Wave", "button");
            if (!_foldWave) return;

            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Wave #:", GUILayout.Width(60));
            _waveIndexInput = GUILayout.TextField(_waveIndexInput, GUILayout.Width(60));
            if (GUILayout.Button("Go", GUILayout.Width(50)))
            {
                if (int.TryParse(_waveIndexInput, out int idx))
                    DebugCommands.JumpToWave(idx);
                else
                    Debug.LogWarning("[DebugCmd] JumpToWave: 올바른 숫자를 입력하세요.");
            }
            GUILayout.EndHorizontal();

            // 현재 웨이브/총 웨이브 표시
            if (Singleton<RunManager>.HasInstance && RunManager.Instance.IsRunning && RunManager.Instance.CurrentStage != null)
            {
                int total = RunManager.Instance.CurrentStage.waves != null
                    ? RunManager.Instance.CurrentStage.waves.Length : 0;
                GUILayout.Label($"  Current: {RunManager.Instance.CurrentWaveIndex} / {total}");
            }

            GUILayout.EndVertical();
        }

        // ═══════════════════════════════════════
        // 2. 스테이지 선택 시작
        // ═══════════════════════════════════════

        void DrawStageStartSection()
        {
            _foldStage = GUILayout.Toggle(_foldStage, "2. Start Specific Stage", "button");
            if (!_foldStage) return;

            GUILayout.BeginVertical("box");

            int stageCount = 0;
            if (Singleton<GameManager>.HasInstance && GameManager.Instance.stages != null)
                stageCount = GameManager.Instance.stages.Length;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Stage #:", GUILayout.Width(60));
            _stageIndexInput = GUILayout.TextField(_stageIndexInput, GUILayout.Width(60));
            if (GUILayout.Button("Start", GUILayout.Width(60)))
            {
                if (int.TryParse(_stageIndexInput, out int idx))
                    DebugCommands.StartSpecificStage(idx);
                else
                    Debug.LogWarning("[DebugCmd] StartSpecificStage: 올바른 숫자를 입력하세요.");
            }
            GUILayout.EndHorizontal();

            // 스테이지 목록 버튼
            if (Singleton<GameManager>.HasInstance && GameManager.Instance.stages != null)
            {
                GUILayout.BeginHorizontal();
                for (int i = 0; i < stageCount; i++)
                {
                    var s = GameManager.Instance.stages[i];
                    string label = s != null ? $"S{i}: {s.stageName}" : $"S{i}";
                    if (GUILayout.Button(label, GUILayout.Height(25)))
                    {
                        DebugCommands.StartSpecificStage(i);
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Label($"  Available: {stageCount} stages");
            GUILayout.EndVertical();
        }

        // ═══════════════════════════════════════
        // 3. 게임 상태 전환
        // ═══════════════════════════════════════

        void DrawStateTransitionSection()
        {
            _foldState = GUILayout.Toggle(_foldState, "3. GoToState", "button");
            if (!_foldState) return;

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Title", GUILayout.Height(30)))
                DebugCommands.GoToTitle();
            if (GUILayout.Button("Hub", GUILayout.Height(30)))
                DebugCommands.GoToHub();
            if (GUILayout.Button("InGame", GUILayout.Height(30)))
                DebugCommands.GoToInGame();
            if (GUILayout.Button("RunEnd", GUILayout.Height(30)))
                DebugCommands.GoToRunEnd();

            GUILayout.EndHorizontal();

            string current = Singleton<GameManager>.HasInstance
                ? GameManager.Instance.State.ToString() : "N/A";
            GUILayout.Label($"  Current State: {current}");

            GUILayout.EndVertical();
        }

        // ═══════════════════════════════════════
        // 4. 보물상자
        // ═══════════════════════════════════════

        void DrawTreasureSection()
        {
            _foldTreasure = GUILayout.Toggle(_foldTreasure, "4. Force Treasure Chest", "button");
            if (!_foldTreasure) return;

            GUILayout.BeginVertical("box");

            if (GUILayout.Button("Force Treasure Drop", GUILayout.Height(30)))
            {
                DebugCommands.ForceTreasureDrop();
            }

            if (Singleton<TreasureManager>.HasInstance)
            {
                var tm = TreasureManager.Instance;
                GUILayout.Label($"  Active Effects: {tm.ActiveEffects.Count}  |  Waiting: {tm.IsWaitingForChoice}");
            }
            else
            {
                GUILayout.Label("  TreasureManager: Not Found");
            }

            GUILayout.EndVertical();
        }

        // ═══════════════════════════════════════
        // 5. Core 추가
        // ═══════════════════════════════════════

        void DrawCoreSection()
        {
            _foldCore = GUILayout.Toggle(_foldCore, "5. Add Core / Bit", "button");
            if (!_foldCore) return;

            GUILayout.BeginVertical("box");

            // Core
            GUILayout.Label("Core:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+10", GUILayout.Width(50)))
                DebugCommands.AddCore(10);
            if (GUILayout.Button("+50", GUILayout.Width(50)))
                DebugCommands.AddCore(50);
            if (GUILayout.Button("+100", GUILayout.Width(50)))
                DebugCommands.AddCore(100);
            _coreAmountInput = GUILayout.TextField(_coreAmountInput, GUILayout.Width(60));
            if (GUILayout.Button("Add", GUILayout.Width(40)))
            {
                if (int.TryParse(_coreAmountInput, out int amt))
                    DebugCommands.AddCore(amt);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // Bit
            GUILayout.Label("Bit (Gold):");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100", GUILayout.Width(50)))
                DebugCommands.AddSoul(100);
            if (GUILayout.Button("+500", GUILayout.Width(50)))
                DebugCommands.AddSoul(500);
            if (GUILayout.Button("+1000", GUILayout.Width(55)))
                DebugCommands.AddSoul(1000);
            _soulAmountInput = GUILayout.TextField(_soulAmountInput, GUILayout.Width(60));
            if (GUILayout.Button("Add", GUILayout.Width(40)))
            {
                if (int.TryParse(_soulAmountInput, out int amt))
                    DebugCommands.AddSoul(amt);
            }
            GUILayout.EndHorizontal();

            if (Singleton<MetaManager>.HasInstance)
            {
                GUILayout.Label($"  Total Soul: {MetaManager.Instance.TotalSoul}  |  Total CoreFragment: {MetaManager.Instance.TotalCoreFragment}");
            }

            GUILayout.EndVertical();
        }

        // ═══════════════════════════════════════
        // 6. 스킬 레벨 설정
        // ═══════════════════════════════════════

        void DrawSkillSection()
        {
            _foldSkill = GUILayout.Toggle(_foldSkill, "6. Set Skill Level", "button");
            if (!_foldSkill) return;

            GUILayout.BeginVertical("box");

            if (!Singleton<MetaManager>.HasInstance || MetaManager.Instance.allSkills == null
                || MetaManager.Instance.allSkills.Length == 0)
            {
                GUILayout.Label("  Skills: N/A (MetaManager 또는 allSkills 없음)");
                GUILayout.EndVertical();
                return;
            }

            var skills = MetaManager.Instance.allSkills;

            // 스킬 목록 (버튼으로 선택)
            GUILayout.Label("Select Skill:");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i] == null) continue;
                string label = skills[i].skillName ?? skills[i].skillId;
                int currentLv = MetaManager.Instance.GetSkillLevel(skills[i].skillId);
                string btnText = $"{label} (Lv{currentLv}/{skills[i].maxLevel})";

                bool isSelected = (_selectedSkillIndex == i);
                var prevBg = GUI.backgroundColor;
                if (isSelected) GUI.backgroundColor = Color.cyan;

                if (GUILayout.Button(btnText, GUILayout.Height(25)))
                {
                    _selectedSkillIndex = i;
                    _skillIdInput = skills[i].skillId;
                }

                GUI.backgroundColor = prevBg;

                // 4개마다 줄바꿈
                if ((i + 1) % 3 == 0 && i < skills.Length - 1)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // 선택된 스킬 레벨 설정
            GUILayout.BeginHorizontal();
            GUILayout.Label("Skill ID:", GUILayout.Width(60));
            _skillIdInput = GUILayout.TextField(_skillIdInput, GUILayout.Width(120));
            GUILayout.Label("Lv:", GUILayout.Width(25));
            _skillLevelInput = GUILayout.TextField(_skillLevelInput, GUILayout.Width(40));
            if (GUILayout.Button("Set", GUILayout.Width(40)))
            {
                if (int.TryParse(_skillLevelInput, out int lv))
                    DebugCommands.SetSkillLevel(_skillIdInput, lv);
                else
                    Debug.LogWarning("[DebugCmd] SetSkillLevel: 올바른 숫자를 입력하세요.");
            }
            GUILayout.EndHorizontal();

            // 퀵 버튼: 모든 스킬 Max
            if (GUILayout.Button("All Skills MAX", GUILayout.Height(25)))
            {
                foreach (var s in skills)
                {
                    if (s == null) continue;
                    DebugCommands.SetSkillLevel(s.skillId, s.maxLevel);
                }
            }

            // 퀵 버튼: 모든 스킬 리셋
            if (GUILayout.Button("All Skills Reset (Lv 0)", GUILayout.Height(25)))
            {
                foreach (var s in skills)
                {
                    if (s == null) continue;
                    DebugCommands.SetSkillLevel(s.skillId, 0);
                }
            }

            GUILayout.EndVertical();
        }

        // ═══════════════════════════════════════
        // 7. FTUE 플래그
        // ═══════════════════════════════════════

        void DrawFtueSection()
        {
            _foldFtue = GUILayout.Toggle(_foldFtue, "7. FTUE Flags", "button");
            if (!_foldFtue) return;

            GUILayout.BeginVertical("box");

            if (!Singleton<MetaManager>.HasInstance)
            {
                GUILayout.Label("  MetaManager: N/A");
                GUILayout.EndVertical();
                return;
            }

            string[] flagNames = new string[]
            {
                "[0] FirstPlay Complete",
                "[1] FirstTowerPlacement",
                "[2] FirstUpgrade",
                "[3] FirstStageClear",
                "[4] HubFirstVisit"
            };

            for (int i = 0; i < flagNames.Length; i++)
            {
                GUILayout.BeginHorizontal();
                bool current = MetaManager.Instance.GetFtueFlag(i);
                GUILayout.Label($"{flagNames[i]}: {(current ? "ON" : "OFF")}", GUILayout.Width(250));

                if (GUILayout.Button(current ? "Turn OFF" : "Turn ON", GUILayout.Width(80)))
                {
                    DebugCommands.SetFtueFlag(i, !current);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(4);
            if (GUILayout.Button("Reset All FTUE Flags", GUILayout.Height(25)))
            {
                DebugCommands.ResetAllFtueFlags();
            }

            GUILayout.EndVertical();
        }

        // ═══════════════════════════════════════
        // 9. 노드 킬 카운트
        // ═══════════════════════════════════════

        void DrawKillCountSection()
        {
            _foldKillCount = GUILayout.Toggle(_foldKillCount, "9. Node Kill Count", "button");
            if (!_foldKillCount) return;

            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Kill Count:", GUILayout.Width(80));
            _nodesKilledInput = GUILayout.TextField(_nodesKilledInput, GUILayout.Width(80));
            if (GUILayout.Button("Set", GUILayout.Width(40)))
            {
                if (int.TryParse(_nodesKilledInput, out int count))
                    DebugCommands.SetNodesKilled(count);
                else
                    Debug.LogWarning("[DebugCmd] SetNodesKilled: 올바른 숫자를 입력하세요.");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100", GUILayout.Width(50)))
            {
                if (Singleton<MetaManager>.HasInstance)
                    DebugCommands.SetNodesKilled(MetaManager.Instance.TotalNodesKilled + 100);
            }
            if (GUILayout.Button("+500", GUILayout.Width(50)))
            {
                if (Singleton<MetaManager>.HasInstance)
                    DebugCommands.SetNodesKilled(MetaManager.Instance.TotalNodesKilled + 500);
            }
            if (GUILayout.Button("Reset 0", GUILayout.Width(60)))
            {
                DebugCommands.SetNodesKilled(0);
            }
            GUILayout.EndHorizontal();

            if (Singleton<MetaManager>.HasInstance)
                GUILayout.Label($"  Current: {MetaManager.Instance.TotalNodesKilled}");

            GUILayout.EndVertical();
        }
    }
}
#endif
