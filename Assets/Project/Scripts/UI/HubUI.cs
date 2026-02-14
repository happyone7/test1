using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    // TODO: Unity 재시작 후 Tesseract.UI.UIPanel 상속으로 교체
    public class HubUI : MonoBehaviour
    {
        [Header("재화")]
        public Text totalBitText;
        public Text totalCoreText;

        [Header("스테이지")]
        public Text stageInfoText;

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

        [Header("출전")]
        public Button startRunButton;

        private Data.SkillNodeData _selectedSkill;

        void Start()
        {
            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(OnPurchase);

            if (startRunButton != null)
                startRunButton.onClick.AddListener(OnStartRun);

            foreach (var node in skillNodes)
            {
                if (node != null)
                    node.Initialize(OnSkillNodeSelected);
            }

            if (detailPanel != null)
                detailPanel.SetActive(false);
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
                    totalBitText.text = $"Bit: {meta.TotalBit}";

                if (totalCoreText != null)
                    totalCoreText.text = $"Core: {meta.TotalCore}";

                if (stageInfoText != null)
                    stageInfoText.text = $"Stage {meta.CurrentStageIndex + 1}";
            }

            foreach (var node in skillNodes)
            {
                if (node != null)
                    node.Refresh();
            }

            if (_selectedSkill != null)
                RefreshDetail();
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

            if (Singleton<Core.MetaManager>.HasInstance)
            {
                var meta = Core.MetaManager.Instance;
                level = meta.GetSkillLevel(_selectedSkill.skillId);
                canPurchase = meta.CanPurchaseSkill(_selectedSkill.skillId);
            }

            if (detailNameText != null)
                detailNameText.text = _selectedSkill.skillName;

            if (detailDescText != null)
                detailDescText.text = _selectedSkill.description;

            if (detailLevelText != null)
                detailLevelText.text = $"Lv.{level}/{_selectedSkill.maxLevel}";

            if (detailCostText != null)
            {
                if (level < _selectedSkill.maxLevel)
                    detailCostText.text = $"비용: {_selectedSkill.GetCost(level)} Bit";
                else
                    detailCostText.text = "MAX";
            }

            if (detailEffectText != null)
            {
                float currentValue = _selectedSkill.GetTotalValue(level);
                detailEffectText.text = $"효과: {_selectedSkill.effectType} +{currentValue}";
            }

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
                Core.GameManager.Instance.StartRun();
        }
    }
}
