using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    /// <summary>
    /// 타워 구매 패널 (PPT 명세 슬라이드 5-5)
    /// 500x340px, +구매 버튼 위, 테두리 #FFD84D 2px
    /// 각 행: 55px, 타워 이름 + 비용 버튼 (120x38px, 채우기=#153020, 테두리=#2BFF88)
    /// </summary>
    public class TowerPurchasePanel : MonoBehaviour
    {
        [Header("타워 행 프리팹")]
        public GameObject towerRowPrefab;

        [Header("컨텐츠 영역")]
        public Transform contentParent;

        [Header("제목")]
        public Text titleText;

        [Header("닫기")]
        public Button closeButton;

        [Header("UI 스프라이트")]
        public UISprites uiSprites;

        private Data.TowerData[] _towerDataList;

        void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            RefreshTowerList();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (gameObject.activeSelf)
                Hide();
            else
                Show();
        }

        private void RefreshTowerList()
        {
            // contentParent의 기존 자식 제거
            if (contentParent != null)
            {
                for (int i = contentParent.childCount - 1; i >= 0; i--)
                    Destroy(contentParent.GetChild(i).gameObject);
            }

            if (!Singleton<Tower.TowerManager>.HasInstance) return;
            _towerDataList = Tower.TowerManager.Instance.availableTowers;

            if (_towerDataList == null) return;

            foreach (var towerData in _towerDataList)
            {
                CreateTowerRow(towerData);
            }
        }

        private void CreateTowerRow(Data.TowerData data)
        {
            if (contentParent == null) return;

            GameObject row;
            if (towerRowPrefab != null)
            {
                row = Instantiate(towerRowPrefab, contentParent);
            }
            else
            {
                // 프리팹 없으면 런타임 생성
                row = CreateDefaultRow(data);
            }

            // 행 내 텍스트/버튼 설정
            var nameText = row.transform.Find("NameText")?.GetComponent<Text>();
            var costButton = row.transform.Find("CostButton")?.GetComponent<Button>();
            var costText = row.transform.Find("CostButton/CostText")?.GetComponent<Text>();

            if (nameText != null)
                nameText.text = data.towerName;

            if (costText != null)
                costText.text = $"{data.placeCost} Bit";

            if (costButton != null)
            {
                var capturedData = data;
                costButton.onClick.AddListener(() => OnPurchaseTower(capturedData));

                // 자금 부족 시 비활성화
                bool canAfford = true;
                if (Singleton<Core.RunManager>.HasInstance)
                    canAfford = Core.RunManager.Instance.BitEarned >= data.placeCost;
                costButton.interactable = canAfford;
            }
        }

        private GameObject CreateDefaultRow(Data.TowerData data)
        {
            // 프리팹 없을 때 코드로 행 생성
            var row = new GameObject($"TowerRow_{data.towerName}", typeof(RectTransform));
            row.transform.SetParent(contentParent, false);

            var rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(460, 55);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 10;
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 배경
            var rowImage = row.AddComponent<Image>();
            if (uiSprites != null && uiSprites.panelFrame != null)
            {
                rowImage.sprite = uiSprites.panelFrame;
                rowImage.type = Image.Type.Sliced;
            }
            rowImage.color = UIColors.BrightPanel;

            // 이름 텍스트
            var nameGo = new GameObject("NameText", typeof(RectTransform));
            nameGo.transform.SetParent(row.transform, false);
            var nameText = nameGo.AddComponent<Text>();
            nameText.text = data.towerName;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 14;
            nameText.color = UIColors.TextMain;
            nameText.alignment = TextAnchor.MiddleLeft;
            var nameLE = nameGo.AddComponent<LayoutElement>();
            nameLE.flexibleWidth = 1;
            nameLE.preferredHeight = 38;

            // 비용 버튼
            var btnGo = new GameObject("CostButton", typeof(RectTransform));
            btnGo.transform.SetParent(row.transform, false);
            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = UIColors.BtnGreenBg;
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            // UISprites가 있으면 SpriteSwap 적용, 없으면 기존 Outline 방식
            if (uiSprites != null)
            {
                uiSprites.ApplyAccentButton(btn);
            }
            else
            {
                var btnOutline = btnGo.AddComponent<Outline>();
                btnOutline.effectColor = UIColors.NeonGreen;
                btnOutline.effectDistance = new Vector2(2, -2);
            }
            var btnLE = btnGo.AddComponent<LayoutElement>();
            btnLE.preferredWidth = 120;
            btnLE.preferredHeight = 38;

            // 비용 텍스트
            var costGo = new GameObject("CostText", typeof(RectTransform));
            costGo.transform.SetParent(btnGo.transform, false);
            var costText = costGo.AddComponent<Text>();
            costText.text = $"{data.placeCost} Bit";
            costText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            costText.fontSize = 13;
            costText.fontStyle = FontStyle.Bold;
            costText.color = UIColors.Yellow;
            costText.alignment = TextAnchor.MiddleCenter;
            var costRect = costGo.GetComponent<RectTransform>();
            costRect.anchorMin = Vector2.zero;
            costRect.anchorMax = Vector2.one;
            costRect.sizeDelta = Vector2.zero;
            costRect.anchoredPosition = Vector2.zero;

            return row;
        }

        private void OnPurchaseTower(Data.TowerData data)
        {
            if (!Singleton<Core.RunManager>.HasInstance) return;
            var run = Core.RunManager.Instance;

            if (run.BitEarned < data.placeCost)
            {
                Debug.Log($"[TowerPurchasePanel] 자금 부족: {data.towerName} 비용={data.placeCost}, 보유={run.BitEarned}");
                return;
            }

            // TODO: 타워 배치 모드 진입 (배치 위치 선택 후 실제 배치)
            Debug.Log($"[TowerPurchasePanel] 타워 구매 시도: {data.towerName} 비용={data.placeCost}");
            Hide();
        }
    }
}
