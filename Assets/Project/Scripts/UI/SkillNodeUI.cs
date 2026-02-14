using System;
using Tesseract.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Nodebreaker.UI
{
    public class SkillNodeUI : MonoBehaviour
    {
        [Header("데이터")]
        public Data.SkillNodeData data;

        [Header("UI")]
        public Image iconImage;
        public Text levelText;
        public Button button;
        public GameObject lockedOverlay;

        private Action<Data.SkillNodeData> _onSelected;

        public void Initialize(Action<Data.SkillNodeData> onSelected)
        {
            _onSelected = onSelected;

            if (button != null)
                button.onClick.AddListener(OnClick);

            if (data != null && iconImage != null && data.icon != null)
                iconImage.sprite = data.icon;

            Refresh();
        }

        public void Refresh()
        {
            if (data == null) return;

            int level = 0;
            if (Singleton<Core.MetaManager>.HasInstance)
                level = Core.MetaManager.Instance.GetSkillLevel(data.skillId);

            if (levelText != null)
                levelText.text = $"Lv.{level}/{data.maxLevel}";

            // MVP: lockedOverlay 항상 비활성
            if (lockedOverlay != null)
                lockedOverlay.SetActive(false);
        }

        private void OnClick()
        {
            _onSelected?.Invoke(data);
        }
    }
}
