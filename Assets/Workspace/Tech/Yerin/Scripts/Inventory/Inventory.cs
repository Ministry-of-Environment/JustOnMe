using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int capacity = 8;

    private InvenSlot[] slots;

    public int Capacity => capacity;

    public InvenSlot[] Slots
    {
        get
        {
            EnsureInitialized();
            return slots;
        }
    }

    public event Action OnInventoryChanged;

    private void Awake()
    {
        EnsureInitialized();
    }

    public void EnsureInitialized()
    {
        if (slots != null && slots.Length == capacity)
        {
            return;
        }

        slots = new InvenSlot[capacity];

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new InvenSlot();
        }
    }

    public bool TryAddItem(ItemData itemData)
    {
        EnsureInitialized();

        if (itemData == null)
        {
            Debug.LogWarning("추가하려는 아이템 데이터가 없습니다.");
            return false;
        }

        int emptyIndex = GetEmptySlotIndex();

        if (emptyIndex == -1)
        {
            return false;
        }

        slots[emptyIndex].SetItem(itemData);
        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool IsFull()
    {
        EnsureInitialized();

        return GetEmptySlotIndex() == -1;
    }

    public ItemData GetItem(int index)
    {
        EnsureInitialized();

        if (!IsValidIndex(index))
        {
            return null;
        }

        return slots[index].ItemData;
    }

    public ItemData RemoveItem(int index)
    {
        EnsureInitialized();

        if (!IsValidIndex(index))
        {
            return null;
        }

        if (slots[index].IsEmpty)
        {
            return null;
        }

        ItemData removedItem = slots[index].Clear();
        OnInventoryChanged?.Invoke();

        return removedItem;
    }

    private int GetEmptySlotIndex()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < slots.Length;
    }

}