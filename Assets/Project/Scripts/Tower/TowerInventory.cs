using System;
using System.Collections.Generic;
using Tesseract.Core;
using UnityEngine;

namespace Soulspire.Tower
{
    [Serializable]
    public class InventorySlot
    {
        public Data.TowerData data;
        public int level;

        public InventorySlot(Data.TowerData data, int level)
        {
            this.data = data;
            this.level = level;
        }
    }

    public class TowerInventory : Singleton<TowerInventory>
    {
        [Header("설정")]
        [SerializeField] private int maxSlots = 8;

        private List<InventorySlot> _slots = new List<InventorySlot>();

        public int MaxSlots => maxSlots;
        public int Count => _slots.Count;
        public bool IsFull => _slots.Count >= maxSlots;

        public event Action OnInventoryChanged;

        public bool TryAddTower(Data.TowerData data, int level = 1)
        {
            if (IsFull) return false;
            _slots.Add(new InventorySlot(data, level));
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _slots.Count) return;
            _slots.RemoveAt(index);
            OnInventoryChanged?.Invoke();
        }

        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= _slots.Count) return null;
            return _slots[index];
        }

        public void Clear()
        {
            _slots.Clear();
            OnInventoryChanged?.Invoke();
        }
    }
}
