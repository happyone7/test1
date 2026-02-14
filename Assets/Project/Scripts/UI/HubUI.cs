using System.Collections.Generic;
using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
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
        public Text totalBitText;
        public Text totalCoreText;

        [Header("상단 바 - 방치 Bit 알림")]
        public GameObject idleBitPanel;
        public Text idleBitText;
        public Button idleBitClaimButton;

        [Header("스킬 트리 영역")]
        public ScrollRect skillScrollRect;

        [Header("스킬 노드")]
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
        public IdleBitPopup idleBitPopup;

        [Header("UI 스프라이트")]
        public UISprites uiSprites;

        [Header("아이콘 이미지")]
        public Image bitIconImage;     // Bit 재화 아이콘
        public Image coreIconImage;    // Core 재화 아이콘

        [Header("패널 배경 이미지")]
        public Image detailPanelImage; // 상세 패널 배경
        public Image topBarImage;      // 상단 바 배경
        public Image bottomBarImage;   // 하단 바 배경

        [Header("상세 패널 - 추가 (PPT 명세)")]
        public Text detailChangeBeforeText;  // 변경 전 값
        public Text detailChangeAfterText;   // 변경 후 값
        public Text purchaseButtonText;      // 구매 버튼 텍스트
        public Button detailCloseButton;     // 닫기 X 버튼

        private Data.SkillNodeData _selectedSkill;
        private int _selectedStageIndex;
        private int _pendingIdleBit;

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

            if (idleBitClaimButton != null)
                idleBitClaimButton.onClick.AddListener(OnClaimIdleBit);

            if (detailCloseButton != null)
                detailCloseButton.onClick.AddListener(OnDetailClose);

            foreach (var node in skillNodes)
            {
                if (node != null)
                    node.Initialize(OnSkillNodeSelected);
            }

            if (detailPanel != null)
                detailPanel.SetActive(false);

            if (idleBitPanel != null)
                idleBitPanel.SetActive(false);

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
            if (bitIconImage != null && uiSprites.iconBit != null)
            {
                bitIconImage.sprite = uiSprites.iconBit;
                bitIconImage.preserveAspect = true;
            }
            if (coreIconImage != null && uiSprites.iconCore != null)
            {
                coreIconImage.sprite = uiSprites.iconCore;
                coreIconImage.preserveAspect = true;
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
            uiSprites.ApplyBasicButton(idleBitClaimButton);
        }

        public virtual void Show()
        {
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

                if (totalBitText != null)
                    totalBitText.text = $"Bit: {meta.TotalBit:N0}";

                if (totalCoreText != null)
                    totalCoreText.text = $"Core: {meta.TotalCore}";

                RefreshStageDropdown();
                RefreshIdleBitNotification();
            }

            foreach (var node in skillNodes)
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
                options.Add(new Dropdown.OptionData($"Stage {i + 1}"));
            }
            stageDropdown.AddOptions(options);

            // 마지막 해금 스테이지를 기본 선택
            _selectedStageIndex = Mathf.Clamp(unlockedCount - 1, 0, maxStages - 1);
            stageDropdown.SetValueWithoutNotify(_selectedStageIndex);

            stageDropdown.onValueChanged.AddListener(OnStageDropdownChanged);
        }

        private void RefreshIdleBitNotification()
        {
            if (idleBitPanel == null) return;

            // TODO: MetaManager에 방치 보상 시스템 구현 시 연동
            // 현재는 _pendingIdleBit 필드로 외부에서 설정 가능
            if (_pendingIdleBit > 0)
            {
                idleBitPanel.SetActive(true);
                if (idleBitText != null)
                    idleBitText.text = $"방치 Bit: {_pendingIdleBit:N0} [수령!]";
            }
            else
            {
                idleBitPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 외부에서 방치 Bit 보상을 설정 (MetaManager 연동용)
        /// </summary>
        public void SetPendingIdleBit(int amount)
        {
            _pendingIdleBit = amount;
            RefreshIdleBitNotification();
        }

        private void OnClaimIdleBit()
        {
            if (_pendingIdleBit <= 0) return;

            if (Singleton<Core.MetaManager>.HasInstance)
            {
                Core.MetaManager.Instance.AddRunRewards(_pendingIdleBit, 0, false, 0);
            }

            _pendingIdleBit = 0;
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
            int totalBit = 0;

            if (Singleton<Core.MetaManager>.HasInstance)
            {
                var meta = Core.MetaManager.Instance;
                level = meta.GetSkillLevel(_selectedSkill.skillId);
                canPurchase = meta.CanPurchaseSkill(_selectedSkill.skillId);
                totalBit = meta.TotalBit;
            }

            bool isMaxLevel = level >= _selectedSkill.maxLevel;

            // 제목: "공격력 Lv3 -> Lv4" (PPT 명세)
            if (detailNameText != null)
            {
                if (isMaxLevel)
                    detailNameText.text = $"{_selectedSkill.skillName} Lv{level} (MAX)";
                else
                    detailNameText.text = $"{_selectedSkill.skillName} Lv{level} -> Lv{level + 1}";
            }

            if (detailDescText != null)
                detailDescText.text = _selectedSkill.description;

            if (detailLevelText != null)
                detailLevelText.text = $"Lv.{level}/{_selectedSkill.maxLevel}";

            // 효과 변경 전/후
            if (detailEffectText != null)
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

            // 변경 전/후 텍스트 (새 필드)
            if (detailChangeBeforeText != null)
            {
                float currentValue = _selectedSkill.GetTotalValue(level);
                detailChangeBeforeText.text = $"현재: +{currentValue}";
            }

            if (detailChangeAfterText != null)
            {
                if (isMaxLevel)
                    detailChangeAfterText.text = "최대 레벨";
                else
                {
                    float nextValue = _selectedSkill.GetTotalValue(level + 1);
                    detailChangeAfterText.text = $"변경 후: +{nextValue}";
                }
            }

            // 비용 텍스트 (자금 부족 시 빨간색)
            if (detailCostText != null)
            {
                if (isMaxLevel)
                {
                    detailCostText.text = "MAX";
                    detailCostText.color = ColorTextSecondary;
                }
                else
                {
                    int cost = _selectedSkill.GetCost(level);
                    detailCostText.text = $"{cost} Bit";
                    detailCostText.color = totalBit >= cost ? ColorYellowGold : ColorRed;
                }
            }

            // 구매 버튼 텍스트
            if (purchaseButtonText != null)
                purchaseButtonText.text = isMaxLevel ? "최대" : "구매";

            if (purchaseButton != null)
                purchaseButton.interactable = canPurchase;
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
    }
}
