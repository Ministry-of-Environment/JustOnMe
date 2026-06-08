using UnityEngine;

[System.Serializable]
public class FishRarityDifficulty
{
    [SerializeField] private FishRarity rarity;
    [SerializeField] private int requiredSuccessCount = 2;
    [SerializeField] private float cursorSpeed = 1.5f;
    [SerializeField] private float successZoneSize = 0.25f;

    public FishRarity Rarity => rarity;
    public int RequiredSuccessCount => requiredSuccessCount;
    public float CursorSpeed => cursorSpeed;
    public float SuccessZoneSize => successZoneSize;
}
