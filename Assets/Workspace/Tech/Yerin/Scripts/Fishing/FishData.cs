using UnityEngine;

[CreateAssetMenu(fileName = "FishData", menuName = "Fishing/FishData")]
public class FishData : ItemData
{
    [SerializeField] int price;
    [SerializeField] FishRarity rarity;

    public int Price => price;
    public int Weight => GetWeight();
    public FishRarity Rarity => rarity;

    private int GetWeight()
    {
        return rarity switch
        {
            FishRarity.Common => 50,
            FishRarity.Uncommon => 30,
            FishRarity.Rare => 15,
            FishRarity.Legendary => 5,
            _ => 0
        };
    }
}

public enum FishRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}