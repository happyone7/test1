using System.Collections;
using System.Collections.Generic;
using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Soulspire.UI
{
    /// <summary>
    /// Hub 화면 UI 컨트롤러
    /// PPT 명세 기반 레이아웃: 상단 바(50px) + 스킬 트리(나머지) + 하단 바(55px)
    /// </summary>
    public class HubUI : MonoBehaviour
    {
        #region 색상 팔레트
        public static readonly Color ColorMainBg       = new Color32(0x0B, 0x0F, 0x1A, 0xFF);
        public static readonly Color ColorPanel        = new Color32(0x12, 0x1A, 0x2A, 0xFF);
        public static readonly Color ColorBrightPanel  = new Color32(0x1A, 0x24, 0x3A, 0xFF);
        public static readonly Color ColorNeonGreen    = new Color32(0x2B, 0xFF, 0x88, 0xFF);
        public static readonly Color ColorNeonBlue     = new Color32(0x37, 0xB6, 0xFF, 0xFF);
        public static readonly Color ColorNeonPurple   = new Color32(0xB8, 0x6C, 0xFF, 0xFF);
        public static readonly Color ColorRed          = new Color32(0xFF, 0x4D, 0x5A, 0xFF);
        public static readonly Color ColorYellowGold   = new Color32(0xFF, 0xD8, 0x4D, 0xFF);
        public static readonly Color ColorTextPrimary  = new Color32(0xD8, 0xE4, 0xFF, 0xFF);
        public static readonly Color ColorTextSecondary = new Color32(0xAF, 0xC3, 0xE8, 0xFF);
        public static readonly Color ColorBorder       = new Color32(0x5B, 0x6B, 0x8A, 0xFF);
        #endregion

        [Header("배경")]
        public Image backgroundImage;

        [Header("상단 바 - 재화")]
        [FormerlySerializedAs("totalBitText")]
        public Text totalSoulText;
        [FormerlySerializedAs("totalCoreText")]
        public Text totalCoreFragmentText;

        [Header("상단 바 - 방치 Bit 알림")]
        [FormerlySerializedAs("idleBitPanel")]
        public GameObject idleSoulPanel;
        [FormerlySerializedAs("idleBitText")]
        public Text idleSoulText;
        [FormerlySerializedAs("idleBitClaimButton")]
        public Button idleSoulClaimButton;

        [Header("스킬 트리 영역")]
        public ScrollRect skillScrollRect;

        [Header("스킬 노드 — 동적 생성")]
        public SkillNodeUI skillNodePrefab;       // 스킬 노드 프리팹 (동적 생성용)
        public RectTransform skillTreeContent;    // ScrollRect.content (노드 배치 컨테이너)
        public float gridSpacing = 150f;          // 노드 간 그리드 간격 (px)

        [Header("스킬 노드 — 수동 할당 (레거시)")]
        public SkillNodeUI[] skillNodes;

        [Header("상세 패널")]
        public GameObject detailPanel;
        public Text detailNameText;
        public Text detailDescText;
        public Text detailLevelText;
        public Text detailCostText;
        public Text detailEffectText;
        public Button purchaseButton;

        [Header("하단 바 - 스테이지 선택")]
        public Dropdown stageDropdown;

        [Header("하단 바 - 출격")]
        public Button startRunButton;

        [Header("하단 바 - 설정/종료")]
        public Button settingsButton;
        public Button quitButton;

        [Header("팝업 참조")]
        public SettingsPopup settingsPopup;
        public IdleSoulPopup idleSoulPopup;

        [Header("UI 스프라이트")]
        public UISprites uiSprites;

        [Header("아이콘 이미지")]
        [FormerlySerializedAs("bitIconImage")]
        public Image soulIconImage;     // Bit 재화 아이콘
        [FormerlySerializedAs("coreIconImage")]
        public Image coreFragmentIconImage;    // Core 재화 아이콘

        [Header("패널 배경 이미지")]
        public Image detailPanelImage; // 상세 패널 배경
        public Image topBarImage;      // 상단 바 배경
        public Image bottomBarImage;   // 하단 바 배경

        [Header("가이드 텍스트 오버레이")]
        public GameObject guideTextContainer;
        public Text guideText;
        public Image guideTextBackground;

        [Header("상세 패널 - 추가 (PPT 명세)")]
        public Text detailChangeBeforeText;  // 변경 전 값
        public Text detailChangeAfterText;   // 변경 후 값
        public Text purchaseButtonText;      // 구매 버튼 텍스트
        public Button detailCloseButton;     // 닫기 X 버튼

        private static readonly Color ColorOverlay = new Color32(0x05, 0x08, 0x12, 0xCC);

        private Data.SkillNodeData _selectedSkill;
        private int _selectedStageIndex;
        private int _pendingIdleSoul;
        private Coroutine _guideTextCoroutine;
        private List<SkillNodeUI> _dynamicNodes = new List<SkillNodeUI>();
        private bool _nodesGenerated;

        /// <summary>
        /// prerequisite 스킬 충족 여부 확인.
        /// prerequisiteIsOr == true: 하나라도 충족하면 true (OR)
        /// prerequisiteIsOr == false: 모두 충족해야 true (AND, 기본값)
        /// </summary>
        private bool ArePrerequisitesMet(Data.SkillNodeData skill)
        {
            if (skill == null) return false;
            if (skill.prerequisiteIds == null || skill.prerequisiteIds.Length == 0)
                return true;

            if (!Singleton<Core.MetaManager>.HasInstance) return false;
            var meta = Core.MetaManager.Instance;

            if (skill.prerequisiteIsOr)
            {
                // OR: 하나라도 충족하면 true
                foreach (var prereqId in skill.prerequisiteIds)
                {
                    if (string.IsNullOrEmpty(prereqId)) continue;
                    if (meta.GetSkillLevel(prereqId) > 0)
                        return true;
                }
                return false;
            }
            else
            {
                // AND: 모두 충족해야 true (기존 로직)
                foreach (var prereqId in skill.prerequisiteIds)
                {
                    if (string.IsNullOrEmpty(prereqId)) continue;
                    if (meta.GetSkillLevel(prereqId) <= 0)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 현재 드롭다운에서 선택된 스테이지 인덱스
        /// </summary>
        public int SelectedStageIndex => _selectedStageIndex;

        void Start()
        {
            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(OnPurchase);

            if (startRunButton != null)
                startRunButton.onClick.AddListener(OnStartRun);

            if (stageDropdown != null)
                stageDropdown.onValueChanged.AddListener(OnStageDropdownChanged);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettings);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuit);

            if (idleSoulClaimButton != null)
                idleSoulClaimButton.onClick.AddListener(OnClaimIdleSoul);

            if (detailCloseButton != null)
                detailCloseButton.onClick.AddListener(OnDetailClose);

            // 동적 스킬 노드 생성 (MetaManager.allSkills 기반)
            GenerateSkillNodes();

            // 수동 할당된 노드 초기화 (레거시 호환)
            foreach (var node in skillNodes)
            {
                if (node != null)
                    node.Initialize(OnSkillNodeSelected);
            }

            if (detailPanel != null)
                detailPanel.SetActive(false);

            if (idleSoulPanel != null)
                idleSoulPanel.SetActive(false);

            if (guideTextContainer != null)
                guideTextContainer.SetActive(false);

            // UI 스프라이트 적용
            ApplyUISprites();
        }

        /// <summary>
        /// UISprites SO에서 배경/패널/버튼/아이콘/드롭다운 스프라이트 적용.
        /// </summary>
        private void ApplyUISprites()
        {
            if (uiSprites == null) return;

            // 배경
            uiSprites.ApplyBackground(backgroundImage, true);

            // 패널 프레임
            uiSprites.ApplyPanelFrame(detailPanelImage);
            uiSprites.ApplyPanelFrame(topBarImage);
            uiSprites.ApplyPanelFrame(bottomBarImage);

            // 아이콘
            if (soulIconImage != null && uiSprites.iconSoul != null)
            {
                soulIconImage.sprite = uiSprites.iconSoul;
                soulIconImage.preserveAspect = true;
            }
            if (coreFragmentIconImage != null && uiSprites.iconCoreFragment != null)
            {
                coreFragmentIconImage.sprite = uiSprites.iconCoreFragment;
                coreFragmentIconImage.preserveAspect = true;
            }

            // 드롭다운 프레임
            if (stageDropdown != null)
            {
                var dropdownImage = stageDropdown.GetComponent<Image>();
                uiSprites.ApplyDropdownFrame(dropdownImage);
            }

            // 버튼 스프라이트
            uiSprites.ApplyAccentButton(startRunButton);
            uiSprites.ApplyAccentButton(purchaseButton);
            uiSprites.ApplyBasicButton(settingsButton);
            uiSprites.ApplyBasicButton(quitButton);
            uiSprites.ApplyBasicButton(idleSoulClaimButton);
        }

        // =====================================================================
        // 동적 스킬 노드 생성
        // =====================================================================

        /// <summary>
        /// MetaManager.allSkills 배열을 순회하여 SkillNodeUI를 동적으로 생성/배치합니다.
        /// skillNodePrefab 또는 skillTreeContent가 미할당이면 동적 생성을 건너뛰고
        /// 기존 수동 할당(skillNodes 배열) 방식으로 동작합니다.
        /// </summary>
        private void GenerateSkillNodes()
        {
            if (skillNodePrefab == null || skillTreeContent == null)
            {
                Debug.Log("[HubUI] skillNodePrefab 또는 skillTreeContent 미할당 — 수동 할당 모드로 동작");
                return;
            }

            if (!Singleton<Core.MetaManager>.HasInstance)
            {
                Debug.LogWarning("[HubUI] MetaManager 미초기화 — 동적 노드 생성 건너뜀");
                return;
            }

            var meta = Core.MetaManager.Instance;
            if (meta.allSkills == null || meta.allSkills.Length == 0)
            {
                Debug.LogWarning("[HubUI] MetaManager.allSkills 비어 있음");
                return;
            }

            _dynamicNodes.Clear();

            foreach (var skillData in meta.allSkills)
            {
                if (skillData == null) continue;

                // 수동 할당 배열에 이미 존재하는 노드는 건너뜀 (중복 방지)
                bool alreadyAssigned = false;
                foreach (var existingNode in skillNodes)
                {
                    if (existingNode != null && existingNode.data != null
                        && existingNode.data.skillId == skillData.skillId)
                    {
                        alreadyAssigned = true;
                        break;
                    }
                }
                if (alreadyAssigned) continue;

                // 프리팹 인스턴스 생성
                var nodeObj = Instantiate(skillNodePrefab, skillTreeContent);
                nodeObj.data = skillData;

                // UISprites 전달
                if (uiSprites != null)
                    nodeObj.uiSprites = uiSprites;

                // 그리드 위치 배치 (position * gridSpacing, 중앙 기준)
                var rt = nodeObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(
                        skillData.position.x * gridSpacing,
                        skillData.position.y * gridSpacing
                    );
                }

                // 초기화
                nodeObj.Initialize(OnSkillNodeSelected);
                _dynamicNodes.Add(nodeObj);
            }

            // 수동 할당된 노드도 그리드 위치에 맞게 재배치
            foreach (var node in skillNodes)
            {
                if (node == null || node.data == null) continue;

                var rt = node.GetComponent<RectTransform>();
                if (rt != null && node.transform.parent == skillTreeContent)
                {
                    rt.anchoredPosition = new Vector2(
                        node.data.position.x * gridSpacing,
                        node.data.position.y * gridSpacing
                    );
                }
            }

            _nodesGenerated = true;
            Debug.Log($"[HubUI] 동적 스킬 노드 {_dynamicNodes.Count}개 생성 완료 (전체 {meta.allSkills.Length}개)");
        }

        public virtual void Show()
        {
            // 부모 Canvas가 비활성일 수 있으므로 함께 활성화
            var parentCanvas = GetComponentInParent<Canvas>(true);
            if (parentCanvas != null)
                parentCanvas.gameObject.SetActive(true);

            gameObject.SetActive(true);
            RefreshAll();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public void RefreshAll()
        {
            if (Singleton<Core.MetaManager>.HasInstance)
            {
                var meta = Core.MetaManager.Instance;

                if (totalSoulText != null)
                    totalSoulText.text = $"Soul: {meta.TotalSoul:N0}";

                if (totalCoreFragmentText != null)
                    totalCoreFragmentText.text = $"Core Fragment: {meta.TotalCoreFragment}";

                RefreshStageDropdown();
                RefreshIdleSoulNotification();
            }

            // 수동 할당 노드 새로고침
            foreach (var node in skillNodes)
            {
                if (node != null)
                    node.Refresh();
            }

            // 동적 생성 노드 새로고침
            foreach (var node in _dynamicNodes)
            {
                if (node != null)
                    node.Refresh();
            }

            if (_selectedSkill != null)
                RefreshDetail();
        }

        private void RefreshStageDropdown()
        {
            if (stageDropdown == null) return;
            if (!Singleton<Core.MetaManager>.HasInstance) return;
            if (!Singleton<Core.GameManager>.HasInstance) return;

            var meta = Core.MetaManager.Instance;
            int maxStages = Core.GameManager.Instance.stages.Length;
            int unlockedCount = Mathf.Min(meta.CurrentStageIndex + 1, maxStages);

            // 드롭다운 재구성 중 onValueChanged 콜백으로 인한 인덱스 오염 방지
            stageDropdown.onValueChanged.RemoveListener(OnStageDropdownChanged);

            stageDropdown.ClearOptions();
            var options = new List<Dropdown.OptionData>();
            for (int i = 0; i < unlockedCount; i++)
            {
                options.Add(new Dropdown.OptionData($"Floor {i + 1}"));
            }
            stageDropdown.AddOptions(options);

            // 마지막 해금 스테이지를 기본 선택
            _selectedStageIndex = Mathf.Clamp(unlockedCount - 1, 0, maxStages - 1);
            stageDropdown.SetValueWithoutNotify(_selectedStageIndex);

            stageDropdown.onValueChanged.AddListener(OnStageDropdownChanged);
        }

        private void RefreshIdleSoulNotification()
        {
            if (idleSoulPanel == null) return;

            // MetaManager Idle Soul 수집기 연동
            if (Singleton<Core.MetaManager>.HasInstance)
                _pendingIdleSoul = Core.MetaManager.Instance.CalculateIdleSoul();

            if (_pendingIdleSoul > 0)
            {
                idleSoulPanel.SetActive(true);
                if (idleSoulText != null)
                    idleSoulText.text = $"방치 Soul: {_pendingIdleSoul:N0} [수령!]";
            }
            else
            {
                idleSoulPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 외부에서 방치 Bit 보상을 설정 (MetaManager 연동용)
        /// </summary>
        public void SetPendingIdleSoul(int amount)
        {
            _pendingIdleSoul = amount;
            RefreshIdleSoulNotification();
        }

        private void OnClaimIdleSoul()
        {
            if (_pendingIdleSoul <= 0) return;

            if (Singleton<Core.MetaManager>.HasInstance)
            {
                Core.MetaManager.Instance.CollectIdleSoul();
            }

            _pendingIdleSoul = 0;
            RefreshAll();
        }

        private void OnStageDropdownChanged(int index)
        {
            if (Singleton<Core.GameManager>.HasInstance)
            {
                int maxStages = Core.GameManager.Instance.stages.Length;
                _selectedStageIndex = Mathf.Clamp(index, 0, maxStages - 1);
            }
            else
            {
                _selectedStageIndex = index;
            }
        }

        private void OnSkillNodeSelected(Data.SkillNodeData data)
        {
            _selectedSkill = data;

            if (detailPanel != null)
                detailPanel.SetActive(true);

            RefreshDetail();
        }

        private void RefreshDetail()
        {
            if (_selectedSkill == null) return;

            int level = 0;
            bool canPurchase = false;
            int totalSoul = 0;
            int totalCoreFragment = 0;
            bool isLocked = !ArePrerequisitesMet(_selectedSkill);
            bool isCoreNode = _selectedSkill.isCoreNode;

            if (Singleton<Core.MetaManager>.HasInstance)
            {
                var meta = Core.MetaManager.Instance;
                level = meta.GetSkillLevel(_selectedSkill.skillId);
                canPurchase = !isLocked && meta.CanPurchaseSkill(_selectedSkill.skillId);
                totalSoul = meta.TotalSoul;
                totalCoreFragment = meta.TotalCoreFragment;
            }

            bool isMaxLevel = level >= _selectedSkill.maxLevel;

            // 제목
            if (detailNameText != null)
            {
                if (isLocked)
                {
                    detailNameText.text = $"{_selectedSkill.skillName} (잠금)";
                }
                else if (isCoreNode)
                {
                    // Core 노드: maxLevel==1이므로 레벨 진행 표시 불필요
                    if (isMaxLevel)
                        detailNameText.text = $"{_selectedSkill.skillName} (해금됨)";
                    else
                        detailNameText.text = _selectedSkill.skillName;
                }
                else
                {
                    // Soul 노드: 레벨 진행 표시
                    if (isMaxLevel)
                        detailNameText.text = $"{_selectedSkill.skillName} Lv{level} (MAX)";
                    else
                        detailNameText.text = $"{_selectedSkill.skillName} Lv{level} -> Lv{level + 1}";
                }
            }

            if (detailDescText != null)
            {
                if (isLocked)
                {
                    // 선행 스킬 이름 표시
                    string prereqNames = GetPrerequisiteNames(_selectedSkill);
                    detailDescText.text = $"선행 스킬 필요: {prereqNames}";
                }
                else
                {
                    detailDescText.text = _selectedSkill.description;
                }
            }

            // 레벨 텍스트
            if (detailLevelText != null)
            {
                if (isCoreNode)
                    detailLevelText.text = isMaxLevel ? "해금 완료" : "미해금";
                else
                    detailLevelText.text = $"Lv.{level}/{_selectedSkill.maxLevel}";
            }

            // 효과 텍스트
            if (detailEffectText != null)
            {
                if (isLocked)
                {
                    detailEffectText.text = $"효과: {_selectedSkill.effectType} (잠금)";
                }
                else if (isCoreNode)
                {
                    // Core 노드: Before/After 대신 효과 설명만 표시
                    detailEffectText.text = _selectedSkill.description;
                }
                else
                {
                    float currentValue = _selectedSkill.GetTotalValue(level);
                    if (isMaxLevel)
                        detailEffectText.text = $"효과: {_selectedSkill.effectType} +{currentValue}";
                    else
                    {
                        float nextValue = _selectedSkill.GetTotalValue(level + 1);
                        detailEffectText.text = $"효과: {_selectedSkill.effectType} +{currentValue} -> +{nextValue}";
                    }
                }
            }

            // 변경 전/후 텍스트 — Core 노드는 숨김
            if (detailChangeBeforeText != null)
            {
                if (isCoreNode)
                {
                    detailChangeBeforeText.text = "";
                }
                else
                {
                    float currentValue = _selectedSkill.GetTotalValue(level);
                    detailChangeBeforeText.text = isLocked ? "현재: -" : $"현재: +{currentValue}";
                }
            }

            if (detailChangeAfterText != null)
            {
                if (isCoreNode)
                {
                    detailChangeAfterText.text = "";
                }
                else if (isLocked)
                {
                    detailChangeAfterText.text = "잠금 상태";
                }
                else if (isMaxLevel)
                {
                    detailChangeAfterText.text = "최대 레벨";
                }
                else
                {
                    float nextValue = _selectedSkill.GetTotalValue(level + 1);
                    detailChangeAfterText.text = $"변경 후: +{nextValue}";
                }
            }

            // 비용 텍스트 — Core 노드는 Core Fragment 단위로 표시
            if (detailCostText != null)
            {
                if (isLocked)
                {
                    detailCostText.text = "잠금";
                    detailCostText.color = ColorTextSecondary;
                }
                else if (isMaxLevel)
                {
                    detailCostText.text = isCoreNode ? "해금됨" : "MAX";
                    detailCostText.color = ColorTextSecondary;
                }
                else
                {
                    int cost = _selectedSkill.GetCost(level);
                    if (isCoreNode)
                    {
                        detailCostText.text = $"{cost} Core Fragment";
                        detailCostText.color = totalCoreFragment >= cost ? ColorYellowGold : ColorRed;
                    }
                    else
                    {
                        detailCostText.text = $"{cost} Soul";
                        detailCostText.color = totalSoul >= cost ? ColorYellowGold : ColorRed;
                    }
                }
            }

            // 구매 버튼 텍스트 — Core 노드는 "해금"
            if (purchaseButtonText != null)
            {
                if (isLocked)
                    purchaseButtonText.text = "잠금";
                else if (isMaxLevel)
                    purchaseButtonText.text = isCoreNode ? "해금됨" : "최대";
                else
                    purchaseButtonText.text = isCoreNode ? "해금" : "구매";
            }

            if (purchaseButton != null)
                purchaseButton.interactable = canPurchase;
        }

        /// <summary>
        /// 선행 스킬 이름을 쉼표로 구분하여 반환
        /// </summary>
        private string GetPrerequisiteNames(Data.SkillNodeData skill)
        {
            if (skill.prerequisiteIds == null || skill.prerequisiteIds.Length == 0)
                return "";

            var names = new List<string>();

            if (Singleton<Core.MetaManager>.HasInstance)
            {
                var meta = Core.MetaManager.Instance;
                foreach (var prereqId in skill.prerequisiteIds)
                {
                    if (string.IsNullOrEmpty(prereqId)) continue;
                    // allSkills에서 이름 찾기
                    if (meta.allSkills != null)
                    {
                        foreach (var s in meta.allSkills)
                        {
                            if (s != null && s.skillId == prereqId)
                            {
                                names.Add(s.skillName);
                                break;
                            }
                        }
                    }
                }
            }

            return names.Count > 0 ? string.Join(", ", names) : string.Join(", ", skill.prerequisiteIds);
        }

        private void OnPurchase()
        {
            if (_selectedSkill == null) return;

            if (Singleton<Core.MetaManager>.HasInstance)
            {
                Core.MetaManager.Instance.TryPurchaseSkill(_selectedSkill.skillId);
                RefreshAll();
            }
        }

        private void OnStartRun()
        {
            if (Singleton<Core.GameManager>.HasInstance)
                Core.GameManager.Instance.StartRun(_selectedStageIndex);
        }

        private void OnSettings()
        {
            if (settingsPopup != null)
                settingsPopup.Show();
            else
                Debug.Log("[HubUI] Settings popup not assigned");
        }

        private void OnDetailClose()
        {
            if (detailPanel != null)
                detailPanel.SetActive(false);
            _selectedSkill = null;
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // =====================================================================
        // FTUE 가이드 텍스트 오버레이
        // =====================================================================

        /// <summary>
        /// 화면 하단 중앙에 가이드 텍스트를 2초 표시 후 페이드아웃합니다.
        /// </summary>
        public void ShowGuideText(string text)
        {
            if (guideTextContainer == null || guideText == null) return;

            if (_guideTextCoroutine != null)
                StopCoroutine(_guideTextCoroutine);

            _guideTextCoroutine = StartCoroutine(GuideTextRoutine(text));
        }

        private IEnumerator GuideTextRoutine(string text)
        {
            guideTextContainer.SetActive(true);
            guideText.text = text;
            guideText.color = ColorTextPrimary;

            if (guideTextBackground != null)
                guideTextBackground.color = ColorOverlay;

            var canvasGroup = guideTextContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = guideTextContainer.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 1f;

            // 2초 표시
            yield return new WaitForSecondsRealtime(2f);

            // 0.5초 페이드아웃
            float fadeTime = 0.5f;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - (elapsed / fadeTime);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            guideTextContainer.SetActive(false);
            _guideTextCoroutine = null;
        }
    }
}
