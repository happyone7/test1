using System.Collections.Generic;
using Tesseract.Core;
using UnityEngine;

namespace Soulspire.Tower
{
    public class TowerManager : Singleton<TowerManager>
    {
        [Header("타워 데이터")]
        public Data.TowerData[] availableTowers;

        [Header("배치 설정")]
        public LayerMask placementLayer;
        public LayerMask towerLayer;

        List<Tower> _placedTowers = new List<Tower>();
        public IReadOnlyList<Tower> PlacedTowers => _placedTowers;

        public Tower PlaceTower(Data.TowerData towerData, Vector3 position, int level = 1)
        {
            var go = Instantiate(towerData.prefab, position, Quaternion.identity);
            var tower = go.GetComponent<Tower>();
            if (tower == null) tower = go.AddComponent<Tower>();
            tower.Initialize(towerData, level);
            _placedTowers.Add(tower);
            return tower;
        }

        public bool TryMergeTower(Tower source, Tower target)
        {
            if (target.TryMerge(source))
            {
                _placedTowers.Remove(source);
                return true;
            }
            return false;
        }

        public void ClearAllTowers()
        {
            foreach (var tower in _placedTowers)
            {
                if (tower != null)
                    Destroy(tower.gameObject);
            }
            _placedTowers.Clear();
        }
    }
}
