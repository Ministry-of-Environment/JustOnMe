using UnityEngine;

public class ItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField] private ItemType itemType;

    public string ItemName => itemName;
    public Sprite Icon => icon;
    public ItemType ItemType => itemType;
}

public enum ItemType
{
    Fish,
    Trash
}