using System.Collections;
using UnityEngine.Serialization;
using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Soulspire.UI
{
    public class InGameUI : MonoBehaviour
    {
        // === PPT 명세 색상 팔레트 ===
        private static readonly Color ColorPanel = new Color32(0x12, 0x1A, 0x2A, 0xFF);       // #121A2A
        private static readonly Color ColorBrightPanel = new Color32(0x1A, 0x24, 0x3A, 0xFF);  // #1A243A
        private static readonly Color ColorNeonGreen = new Color32(0x2B, 0xFF, 0x88, 0xFF);    // #2BFF88
        private static readonly Color ColorNeonBlue = new Color32(0x37, 0xB6, 0xFF, 0xFF);     // #37B6FF
        private static readonly Color ColorNeonPurple = new Color32(0xB8, 0x6C, 0xFF, 0xFF);   // #B86CFF
        private static readonly Color ColorOrange = new Color32(0xFF, 0x9A, 0x3D, 0xFF);       // #FF9A3D
        private static readonly Color ColorYellow = new Color32(0xFF, 0xD8, 0x4D, 0xFF);       // #FFD84D
        private static readonly Color ColorRed = new Color32(0xFF, 0x4D, 0x5A, 0xFF);          // #FF4D5A
        private static readonly Color ColorTextMain = new Color32(0xD8, 0xE4, 0xFF, 0xFF);     // #D8E4FF
        private static readonly Color ColorTextSub = new Color32(0xAF, 0xC3, 0xE8, 0xFF);      // #AFC3E8
        private static readonly Color ColorBorder = new Color32(0x5B, 0x6B, 0x8A, 0xFF);       // #5B6B8A
        private static readonly Color ColorOverlay = new Color32(0x05, 0x08, 0x12, 0xCC);       // #050812 80%
        private static readonly Color ColorHpGreen = new Color32(0x44, 0xCC, 0x44, 0xFF);      // #44CC44
        private static readonly Color ColorHpBg = new Color32(0x33, 0x11, 0x11, 0xFF);         // #331111
        private static readonly Color ColorSpeedActive = new Color32(0x15, 0x30, 0x20, 0xFF);  // #153020
        private static readonly Color ColorSpeedInactive = new Color32(0x1A, 0x24, 0x3A, 0xFF);// #1A243A
        private static readonly Color ColorSoulGreen = new Color32(0x2B, 0xFF, 0x88, 0xFF);     // #2BFF88
        private static readonly Color ColorWarningRed = new Color32(0xFF, 0x4D, 0x5A, 0x80);   // #FF4D5A 반투명

        [Header("상단 HUD")]
        public Text waveText;
        [FormerlySerializedAs("bitText")]
        public Text soulText;
        public Text baseHpLabelText;   // "기지 HP:" 라벨
        public Image hpBarBackground;   // HP 바 배경 (#331111)
        public Image hpBarFill;          // HP 바 채우기 (#44CC44, fillAmount)
        public Text hpBarValueText;      // HP 바 위 "16/20" 텍스트
        public Image hpWarningOverlay;   // HP < 30% 경고 화면 가장자리 붉게

        [Header("하단 타워 인벤토리")]
        public Transform towerListParent;
        public InventoryBarUI inventoryBar;

        [Header("배속/일시정지")]
        public Button[] speedButtons;    // x1, x2, x3
        public Text[] speedButtonTexts;  // 속도 버튼 텍스트
        public Image[] speedButtonImages;// 속도 버튼 배경 이미지
        public Button pauseButton;
        public Text pauseButtonText;
        public Image pauseButtonImage;

        [Header("런 종료 패널")]
        public GameObject runEndPanel;
        public Text runEndTitleText;
        [FormerlySerializedAs("runEndBitText")]
        public Text runEndSoulText;
        public Button hubButton;
        public Button retryButton;

        [Header("런 종료 패널 - 추가 정보")]
        public Text runEndWaveText;
        public Text runEndNodesText;
        [FormerlySerializedAs("runEndCoreText")]
        public Text runEndCoreFragmentText;

        [Header("타워 구매/정보 패널")]
        public TowerPurchasePanel towerPurchasePanel;
        public TowerInfoTooltip towerInfoTooltip;
        public Button towerPurchaseButton;  // +구매 버튼

        [Header("UI 스프라이트")]
        public UISprites uiSprites;

        [Header("InGame 컨테이너 (Hub에서 숨길 대상)")]
        public GameObject topHudContainer;      // Canvas/TopHUD
        public GameObject bottomBarContainer;   // Canvas/BottomBar

        [Header("런 종료 패널 - PPT 명세 추가")]
        public GameObject runEndOverlay;        // 전체 화면 반투명 오버레이
        public Image runEndPanelBody;           // 800x700 메인 패널 (배경 이미지)
        public Outline runEndPanelOutline;      // 패널 테두리 (Outline 컴포넌트)
        public Text runEndStageNameText;        // 스테이지 이름 (클리어 시)
        public GameObject runEndUnlockNotice;   // 새 스테이지 해금 알림
        public Text runEndUnlockText;           // 해금 알림 텍스트
        public Text hubButtonText;              // Hub 버튼 텍스트
        public Text retryButtonText;            // 재도전/다음 스테이지 버튼 텍스트
        public Outline hubButtonOutline;        // Hub 버튼 Outline
        public Outline retryButtonOutline;      // 재도전 버튼 Outline

        [Header("런 종료 패널 - 보유 Bit 총액")]
        [FormerlySerializedAs("runEndTotalBitText")]
        public Text runEndTotalSoulText;         // "보유 Bit: 1,234" 텍스트

        [Header("보스 HP 바")]
        public GameObject bossHpBarContainer;   // 보스 HP 바 전체 컨테이너
        public Text bossNameText;               // 보스 이름 텍스트
        public Image bossHpBarFill;             // 보스 HP 게이지 (fillAmount)
        public Text bossHpValueText;            // 보스 HP 수치 텍스트

        [Header("가이드 텍스트 오버레이")]
        public GameObject guideTextContainer;   // 하단 중앙 가이드 텍스트 컨테이너
        public Text guideText;                  // 가이드 텍스트
        public Image guideTextBackground;       // 반투명 배경

        [Header("보스 처치 Core 보상 팝업")]
        public GameObject corePopupContainer;   // Core 팝업 컨테이너
        public Text corePopupText;              // "+N Core" 텍스트

        [Header("보물상자 3택 선택")]
        public TreasureChoiceUI treasureChoiceUI;  // 보물상자 3택 패널 (독립 이벤트 구독)

        // === Slider 하위호환 (기존 참조 유지) ===
        [HideInInspector] public Slider baseHpSlider;
        [HideInInspector] public Text baseHpText;

        private static readonly float[] SpeedValues = { 1f, 2f, 3f };
        private int _currentSpeedIndex;
        private float _savedTimeScale = 1f;
        private bool _isPaused;
        private Coroutine _slideUpCoroutine;
        private Coroutine _guideTextCoroutine;
        private Coroutine _corePopupCoroutine;
        private float _bossMaxHp;
        private bool _bossHpBarVisible;


        void Start()
        {
            if (hubButton != null)
                hubButton.onClick.AddListener(OnGoToHub);
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetry);

            InitSpeedButtons();

            if (runEndPanel != null)
                runEndPanel.SetActive(false);

            // HP 경고 오버레이 초기 숨김
            if (hpWarningOverlay != null)
                hpWarningOverlay.gameObject.SetActive(false);

            // 타워 구매 패널/버튼 비활성화 (Sprint 1: 인벤토리 드래그 시스템으로 전환)
            if (towerPurchasePanel != null)
                towerPurchasePanel.gameObject.SetActive(false);
            if (towerPurchaseButton != null)
                towerPurchaseButton.gameObject.SetActive(false);

            // 보스 HP 바 초기 숨김
            if (bossHpBarContainer != null)
                bossHpBarContainer.SetActive(false);
            _bossHpBarVisible = false;

            // 가이드 텍스트 초기 숨김
            if (guideTextContainer != null)
                guideTextContainer.SetActive(false);

            // Core 팝업 초기 숨김
            if (corePopupContainer != null)
                corePopupContainer.SetActive(false);

            // UI 스프라이트 적용
            ApplyUISprites();
        }

        /// <summary>
        /// UISprites SO에서 HP바/패널/버튼 스프라이트 적용.
        /// </summary>
        private void ApplyUISprites()
        {
            if (uiSprites == null) return;

            // HP 바 프레임 & 채우기
            uiSprites.ApplyHpBarFrame(hpBarBackground);
            uiSprites.ApplyHpBarFill(hpBarFill);

            // 런 종료 패널 배경
            uiSprites.ApplyPanelFrame(runEndPanelBody);

            // 런 종료 버튼
            uiSprites.ApplyBasicButton(hubButton);
            uiSprites.ApplyAccentButton(retryButton);

            // 속도/일시정지 버튼
            if (speedButtons != null)
            {
                foreach (var btn in speedButtons)
                {
                    if (btn != null)
                        uiSprites.ApplyBasicButton(btn);
                }
            }
            uiSprites.ApplyBasicButton(pauseButton);
        }

        void Update()
        {
            if (!Singleton<Core.RunManager>.HasInstance) return;
            var run = Core.RunManager.Instance;

            // 웨이브 카운터
            if (waveText != null)
            {
                int total = run.CurrentStage != null ? run.CurrentStage.waves.Length : 0;
                waveText.text = $"\uc6e8\uc774\ube0c: {run.CurrentWaveIndex + 1}/{total}";
            }

            // Bit 카운터
            if (soulText != null)
                soulText.text = $"Bit: {run.SoulEarned}";

            // 기지 HP 라벨
            if (baseHpLabelText != null)
                baseHpLabelText.text = "\uae30\uc9c0 HP:";

            // HP 바 (Image.fillAmount 방식)
            if (hpBarFill != null && run.BaseMaxHp > 0)
            {
                float hpRatio = (float)run.BaseHp / run.BaseMaxHp;
                hpBarFill.fillAmount = hpRatio;
            }

            // HP 바 텍스트
            if (hpBarValueText != null)
                hpBarValueText.text = $"{run.BaseHp}/{run.BaseMaxHp}";

            // HP < 30% 경고
            if (hpWarningOverlay != null && run.BaseMaxHp > 0)
            {
                float hpRatio = (float)run.BaseHp / run.BaseMaxHp;
                bool showWarning = hpRatio < 0.3f && run.BaseHp > 0;
                hpWarningOverlay.gameObject.SetActive(showWarning);
            }

            // 하위호환: 기존 Slider가 연결되어 있으면 업데이트
            if (baseHpSlider != null)
            {
                baseHpSlider.maxValue = run.BaseMaxHp;
                baseHpSlider.value = run.BaseHp;
            }

            if (baseHpText != null)
                baseHpText.text = $"HP: {run.BaseHp}/{run.BaseMaxHp}";
        }

public void ShowRunEnd(bool cleared, int soulEarned, int nodesKilled, int coreFragmentEarned)
        {
            // 런 종료 시 배속을 x1로 리셋
            Time.timeScale = 1f;
            _currentSpeedIndex = 0;
            _isPaused = false;
            UpdateSpeedButtonVisuals();

            // 오버레이 활성화
            if (runEndOverlay != null)
                runEndOverlay.SetActive(true);

            // 패널 활성화
            if (runEndPanel != null)
                runEndPanel.SetActive(true);

            // === 테마 색상 적용 (클리어=초록, 패배=빨강) ===
            Color themeColor = cleared ? ColorNeonGreen : ColorRed;

            // 패널 테두리
            if (runEndPanelOutline != null)
            {
                runEndPanelOutline.effectColor = themeColor;
                runEndPanelOutline.effectDistance = new Vector2(2, 2);
            }

            // 제목 텍스트
            if (runEndTitleText != null)
            {
                runEndTitleText.text = cleared ? "스테이지 클리어!" : "패배";
                runEndTitleText.color = themeColor;
                runEndTitleText.fontSize = 36;
                runEndTitleText.fontStyle = FontStyle.Bold;
            }

            // 스테이지 이름 (클리어 시만)
            if (runEndStageNameText != null)
            {
                if (cleared && Singleton<Core.RunManager>.HasInstance && Core.RunManager.Instance.CurrentStage != null)
                {
                    runEndStageNameText.gameObject.SetActive(true);
                    runEndStageNameText.text = Core.RunManager.Instance.CurrentStage.stageName;
                    runEndStageNameText.color = ColorTextSub;
                    runEndStageNameText.fontSize = 14;
                }
                else
                {
                    runEndStageNameText.gameObject.SetActive(false);
                }
            }

            // Bit 획득 정보
            if (runEndSoulText != null)
            {
                runEndSoulText.text = $"획득 Bit:  +{soulEarned}";
                runEndSoulText.color = ColorTextMain;
            }

            // 웨이브 도달 정보 (패배 시만)
            if (runEndWaveText != null)
            {
                int totalWaves = Singleton<Core.RunManager>.HasInstance && Core.RunManager.Instance.CurrentStage != null
                    ? Core.RunManager.Instance.CurrentStage.waves.Length : 0;
                int reachedWave = Singleton<Core.RunManager>.HasInstance
                    ? Mathf.Min(Core.RunManager.Instance.CurrentWaveIndex + 1, totalWaves) : 0;

                if (cleared)
                {
                    runEndWaveText.gameObject.SetActive(false);
                }
                else
                {
                    runEndWaveText.gameObject.SetActive(true);
                    runEndWaveText.text = $"도달 웨이브:  {reachedWave}/{totalWaves}";
                    runEndWaveText.color = ColorTextMain;
                }
            }

            // 처치 Node (패배 시만)
            if (runEndNodesText != null)
            {
                if (!cleared)
                {
                    runEndNodesText.gameObject.SetActive(true);
                    runEndNodesText.text = $"처치 Node:  {nodesKilled}";
                    runEndNodesText.color = ColorTextMain;
                }
                else
                {
                    runEndNodesText.gameObject.SetActive(false);
                }
            }

            // Core 획득 (클리어 시만, 보라색)
            if (runEndCoreFragmentText != null)
            {
                if (coreFragmentEarned > 0)
                {
                    runEndCoreFragmentText.gameObject.SetActive(true);
                    runEndCoreFragmentText.text = $"획득 Core:  {coreFragmentEarned}";
                    runEndCoreFragmentText.color = ColorNeonPurple;
                }
                else
                {
                    runEndCoreFragmentText.gameObject.SetActive(false);
                }
            }

            // 보유 Bit 총액 표시 (런 보상 적립 후)
            if (runEndTotalSoulText != null)
            {
                if (Singleton<Core.MetaManager>.HasInstance)
                {
                    int totalSoul = Core.MetaManager.Instance.TotalSoul;
                    runEndTotalSoulText.text = $"보유 Bit:  {totalSoul:N0}";
                    runEndTotalSoulText.color = ColorSoulGreen;
                    runEndTotalSoulText.gameObject.SetActive(true);
                }
                else
                {
                    runEndTotalSoulText.gameObject.SetActive(false);
                }
            }

            // 새 스테이지 해금 알림
            if (runEndUnlockNotice != null)
            {
                bool showUnlock = false;
                if (cleared && Singleton<Core.MetaManager>.HasInstance && Singleton<Core.GameManager>.HasInstance)
                {
                    int stageCount = Core.GameManager.Instance.stages.Length;
                    int unlockedIndex = Core.MetaManager.Instance.CurrentStageIndex;
                    // 클리어로 새 스테이지가 해금되었고, 아직 갈 수 있는 다음 스테이지가 있으면
                    showUnlock = unlockedIndex < stageCount && unlockedIndex > 0;
                }

                runEndUnlockNotice.SetActive(showUnlock);
                if (showUnlock && runEndUnlockText != null)
                {
                    runEndUnlockText.text = "새 스테이지 해금!";
                    runEndUnlockText.color = ColorYellow;
                    runEndUnlockText.fontStyle = FontStyle.Bold;
                }
            }

            // 버튼 텍스트 및 스타일
            if (hubButtonText != null)
                hubButtonText.text = cleared ? "[ Hub ]" : "[ Hub (스킬 트리) ]";
            if (retryButtonText != null)
                retryButtonText.text = cleared ? "[ 다음 스테이지 ]" : "[ 즉시 재도전 ]";

            // 버튼 Outline 색상
            if (hubButtonOutline != null)
                hubButtonOutline.effectColor = ColorNeonBlue;
            if (retryButtonOutline != null)
                retryButtonOutline.effectColor = ColorNeonGreen;

            // 슬라이드 업 애니메이션
            if (_slideUpCoroutine != null)
                StopCoroutine(_slideUpCoroutine);
            _slideUpCoroutine = StartCoroutine(SlideUpAnimation());
        }

private IEnumerator SlideUpAnimation()
        {
            RectTransform panelRect = runEndPanelBody != null
                ? runEndPanelBody.rectTransform
                : runEndPanel != null ? runEndPanel.GetComponent<RectTransform>() : null;

            if (panelRect == null) yield break;

            Vector2 targetPos = Vector2.zero;
            Vector2 startPos = new Vector2(0f, -400f);

            float duration = 0.3f;
            float elapsed = 0f;

            panelRect.anchoredPosition = startPos;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                // ease-out cubic
                t = 1f - (1f - t) * (1f - t) * (1f - t);
                panelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            panelRect.anchoredPosition = targetPos;
            _slideUpCoroutine = null;
        }


        private void InitSpeedButtons()
        {
            if (speedButtons != null)
            {
                for (int i = 0; i < speedButtons.Length && i < SpeedValues.Length; i++)
                {
                    int index = i;
                    float speed = SpeedValues[i];
                    if (speedButtons[i] != null)
                        speedButtons[i].onClick.AddListener(() => SetSpeed(index, speed));
                }
            }

            if (pauseButton != null)
                pauseButton.onClick.AddListener(TogglePause);

            _currentSpeedIndex = 0;
            _isPaused = false;
            UpdateSpeedButtonVisuals();
        }

        private void SetSpeed(int index, float speed)
        {
            _currentSpeedIndex = index;
            _isPaused = false;
            Time.timeScale = speed;
            UpdateSpeedButtonVisuals();
        }

        private void TogglePause()
        {
            if (_isPaused)
            {
                // 일시정지 해제 - 이전 배속으로 복원
                _isPaused = false;
                Time.timeScale = SpeedValues[_currentSpeedIndex];
            }
            else
            {
                // 일시정지
                _isPaused = true;
                Time.timeScale = 0f;
            }
            UpdateSpeedButtonVisuals();
        }

        private void UpdateSpeedButtonVisuals()
        {
            // 속도 버튼 스타일: 활성=#153020+#2BFF88 테두리, 비활성=#1A243A+#5B6B8A 테두리
            if (speedButtons != null)
            {
                for (int i = 0; i < speedButtons.Length; i++)
                {
                    if (speedButtons[i] == null) continue;

                    bool isActive = (i == _currentSpeedIndex) && !_isPaused;

                    // 배경 이미지 색상
                    if (speedButtonImages != null && i < speedButtonImages.Length && speedButtonImages[i] != null)
                    {
                        speedButtonImages[i].color = isActive ? ColorSpeedActive : ColorSpeedInactive;
                    }

                    // Outline 색상은 코드로 직접 변경 (Outline 컴포넌트가 있다면)
                    var outline = speedButtons[i].GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.effectColor = isActive ? ColorNeonGreen : ColorBorder;
                    }

                    // ColorBlock 방식 대체 (Outline 없을 경우)
                    var colors = speedButtons[i].colors;
                    colors.normalColor = isActive ? ColorSpeedActive : ColorSpeedInactive;
                    colors.highlightedColor = isActive ? ColorSpeedActive : ColorSpeedInactive;
                    colors.pressedColor = isActive ? ColorSpeedActive : ColorSpeedInactive;
                    colors.selectedColor = isActive ? ColorSpeedActive : ColorSpeedInactive;
                    speedButtons[i].colors = colors;
                }
            }

            // 일시정지 버튼
            if (pauseButton != null)
            {
                if (pauseButtonImage != null)
                {
                    pauseButtonImage.color = _isPaused ? ColorSpeedActive : ColorSpeedInactive;
                }

                var outline = pauseButton.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = _isPaused ? ColorOrange : ColorBorder;
                }

                var pauseColors = pauseButton.colors;
                pauseColors.normalColor = _isPaused ? ColorSpeedActive : ColorSpeedInactive;
                pauseColors.highlightedColor = _isPaused ? ColorSpeedActive : ColorSpeedInactive;
                pauseColors.pressedColor = _isPaused ? ColorSpeedActive : ColorSpeedInactive;
                pauseColors.selectedColor = _isPaused ? ColorSpeedActive : ColorSpeedInactive;
                pauseButton.colors = pauseColors;
            }
        }

        public void HideAll()
        {
            Debug.Log("[InGameUI] HideAll 호출");
            // Canvas 루트를 비활성화하지 않음! InGame 전용 요소만 숨기기
            SetInGameElementsActive(false);

            if (runEndPanel != null)
                runEndPanel.SetActive(false);
            if (runEndOverlay != null)
                runEndOverlay.SetActive(false);
            if (towerPurchasePanel != null)
                towerPurchasePanel.Hide();
            if (towerInfoTooltip != null)
                towerInfoTooltip.Hide();

            // 보스 HP 바 숨김
            HideBossHpBar();

            // 가이드 텍스트 숨김
            if (_guideTextCoroutine != null)
            {
                StopCoroutine(_guideTextCoroutine);
                _guideTextCoroutine = null;
            }
            if (guideTextContainer != null)
                guideTextContainer.SetActive(false);

            // Core 팝업 숨김
            if (_corePopupCoroutine != null)
            {
                StopCoroutine(_corePopupCoroutine);
                _corePopupCoroutine = null;
            }
            if (corePopupContainer != null)
                corePopupContainer.SetActive(false);

            // 보물상자 3택 패널 숨김
            if (treasureChoiceUI != null)
                treasureChoiceUI.Hide();
        }

        public void ShowAll()
        {
            // 안전장치: 부모 Canvas가 비활성이면 활성화
            var parentCanvas = GetComponentInParent<Canvas>(true);
            if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
            {
                Debug.LogWarning($"[InGameUI] ShowAll: 부모 Canvas '{parentCanvas.name}'가 비활성 상태 — 강제 활성화");
                parentCanvas.gameObject.SetActive(true);
            }

            Debug.Log($"[InGameUI] ShowAll 호출: topHud={topHudContainer != null}, bottomBar={bottomBarContainer != null}, inventoryBar={inventoryBar != null}");
            SetInGameElementsActive(true);
        }

        private void SetInGameElementsActive(bool active)
        {
            // 직접 참조로 InGame 컨테이너 활성/비활성 (간접 참조 시 잘못된 오브젝트 제어 위험)
            if (topHudContainer != null)
                topHudContainer.SetActive(active);

            if (bottomBarContainer != null)
                bottomBarContainer.SetActive(active);

            if (inventoryBar != null)
                inventoryBar.gameObject.SetActive(active);

            // HP 경고 오버레이 - 항상 false로 시작, Update()에서 HP 비율에 따라 자동 관리
            if (hpWarningOverlay != null)
                hpWarningOverlay.gameObject.SetActive(false);
        }

        private void OnTowerPurchaseToggle()
        {
            if (towerPurchasePanel != null)
                towerPurchasePanel.Toggle();
        }

        void OnGoToHub()
        {
            if (runEndPanel != null) runEndPanel.SetActive(false);
            if (Singleton<Core.GameManager>.HasInstance)
                Core.GameManager.Instance.GoToHub();
        }

        void OnRetry()
        {
            if (runEndPanel != null) runEndPanel.SetActive(false);
            if (Singleton<Core.GameManager>.HasInstance)
                Core.GameManager.Instance.StartRun();
        }

        // =====================================================================
        // 보스 HP 바
        // =====================================================================

        /// <summary>
        /// 보스 HP 바를 표시합니다. 보스 웨이브 시작 시 호출.
        /// </summary>
        public void ShowBossHpBar(string bossName, float maxHp)
        {
            _bossMaxHp = maxHp;
            _bossHpBarVisible = true;

            if (bossHpBarContainer != null)
                bossHpBarContainer.SetActive(true);

            if (bossNameText != null)
            {
                bossNameText.text = bossName;
                bossNameText.color = ColorRed;
                bossNameText.fontStyle = FontStyle.Bold;
            }

            if (bossHpBarFill != null)
                bossHpBarFill.fillAmount = 1f;

            if (bossHpValueText != null)
                bossHpValueText.text = $"{maxHp:F0}/{maxHp:F0}";
        }

        /// <summary>
        /// 보스 HP 갱신.
        /// </summary>
        public void UpdateBossHp(float currentHp)
        {
            if (!_bossHpBarVisible) return;

            float ratio = _bossMaxHp > 0 ? Mathf.Clamp01(currentHp / _bossMaxHp) : 0f;

            if (bossHpBarFill != null)
                bossHpBarFill.fillAmount = ratio;

            if (bossHpValueText != null)
                bossHpValueText.text = $"{Mathf.Max(0f, currentHp):F0}/{_bossMaxHp:F0}";
        }

        /// <summary>
        /// 보스 HP 바를 숨깁니다. 보스 처치 시 호출.
        /// </summary>
        public void HideBossHpBar()
        {
            _bossHpBarVisible = false;

            if (bossHpBarContainer != null)
                bossHpBarContainer.SetActive(false);
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
            guideText.color = ColorTextMain;

            // 배경 반투명 리셋
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

        // =====================================================================
        // 보스 처치 Core 보상 팝업
        // =====================================================================

        /// <summary>
        /// "+N Core" 팝업을 1초 표시 후 페이드아웃합니다.
        /// </summary>
        public void ShowCoreFragmentPopup(int coreAmount)
        {
            if (corePopupContainer == null || corePopupText == null) return;

            if (_corePopupCoroutine != null)
                StopCoroutine(_corePopupCoroutine);

            _corePopupCoroutine = StartCoroutine(CorePopupRoutine(coreAmount));
        }

        private IEnumerator CorePopupRoutine(int coreAmount)
        {
            corePopupContainer.SetActive(true);
            corePopupText.text = $"+{coreAmount} Core";
            corePopupText.color = ColorNeonPurple;
            corePopupText.fontSize = 32;
            corePopupText.fontStyle = FontStyle.Bold;

            var canvasGroup = corePopupContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = corePopupContainer.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 1f;

            // 1초 표시
            yield return new WaitForSecondsRealtime(1f);

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
            corePopupContainer.SetActive(false);
            _corePopupCoroutine = null;
        }
    }
}
