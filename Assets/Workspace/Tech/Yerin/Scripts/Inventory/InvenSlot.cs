[System.Serializable]
public class InvenSlot
{
    public ItemData ItemData { get; private set; }
    public bool IsEmpty => ItemData == null;

    public void SetItem(ItemData itemData)
    {
        ItemData = itemData;
    }

    public ItemData Clear()
    {
        ItemData removed = ItemData;
        ItemData = null;
        return removed;
    }
}
