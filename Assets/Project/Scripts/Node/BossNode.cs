using UnityEngine;

namespace Nodebreaker.Node
{
    /// <summary>
    /// 보스 Node 전용 컴포넌트.
    /// 일반 Node를 상속하여 보스 전용 UI/비주얼 확장 포인트를 제공한다.
    /// - HP 바 비율 프로퍼티 (UI 연동용)
    /// - 보스 등장 시 스케일 조정
    /// NodeData.type == Boss인 SO와 함께 사용한다.
    /// </summary>
    public class BossNode : Node
    {
        [Header("보스 비주얼")]
        [SerializeField] private float bossScale = 1.5f;

        /// <summary>
        /// 보스 HP 비율 (0~1). UI에서 보스 HP 바 표시에 사용.
        /// </summary>
        public float HpRatio => MaxHp > 0f ? CurrentHp / MaxHp : 0f;

        /// <summary>
        /// 초기화 후 보스 스케일 적용.
        /// </summary>
        public override void OnPop()
        {
            base.OnPop();
            transform.localScale = Vector3.one * bossScale;
        }
    }
}
