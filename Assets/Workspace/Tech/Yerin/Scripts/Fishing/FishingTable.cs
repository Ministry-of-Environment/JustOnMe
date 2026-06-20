using System.Collections.Generic;
using UnityEngine;

public class FishingTable : MonoBehaviour
{
    [Header("Fish Table")]
    [SerializeField] private FishPollutionTableSO fishesTable;

    [Header("Trash Table")]
    [SerializeField] private List<ItemData> trashes;

    [Header("Trash Weight By Pollution Level")]
    [SerializeField] private int level1TrashWeight = 20;
    [SerializeField] private int level2TrashWeight = 45;
    [SerializeField] private int level3TrashWeight = 80;

    public ItemData GetRandomFishingItem(PollutionLevel level)
    {
        PollutionFishGroup fishGroup = fishesTable.GetFishGroup(level);

        if (fishGroup == null)
        {
            Debug.LogWarning($"{level} 단계의 물고기 그룹이 없습니다.");
            return GetRandomTrash();
        }

        int fishTotalWeight = GetFishTotalWeight(fishGroup);
        int trashWeight = GetTrashWeight(level);

        int totalWeight = fishTotalWeight + trashWeight;

        if (totalWeight <= 0)
        {
            Debug.LogWarning($"{level} 단계에서 추첨 가능한 아이템이 없습니다.");
            return null;
        }

        int randomValue = Random.Range(0, totalWeight);

        if (randomValue < fishTotalWeight)
        {
            return GetRandomFishByWeight(fishGroup, randomValue);
        }

        return GetRandomTrash();
    }

    private int GetFishTotalWeight(PollutionFishGroup fishGroup)
    {
        int totalWeight = 0;

        foreach (FishData fish in fishGroup.Fishes)
        {
            if (fish == null)
            {
                continue;
            }

            totalWeight += fish.Weight;
        }

        return totalWeight;
    }

    private FishData GetRandomFishByWeight(PollutionFishGroup fishGroup, int randomValue)
    {
        int currentWeight = 0;

        foreach (FishData fish in fishGroup.Fishes)
        {
            if (fish == null)
            {
                continue;
            }

            currentWeight += fish.Weight;

            if (randomValue < currentWeight)
            {
                return fish;
            }
        }

        return null;
    }

    private ItemData GetRandomTrash()
    {
        if (trashes == null || trashes.Count == 0)
        {
            Debug.LogWarning("등록된 쓰레기 아이템이 없습니다.");
            return null;
        }

        int randomIndex = Random.Range(0, trashes.Count);
        return trashes[randomIndex];
    }

    private int GetTrashWeight(PollutionLevel level)
    {
        return level switch
        {
            PollutionLevel.Level1 => level1TrashWeight,
            PollutionLevel.Level2 => level2TrashWeight,
            PollutionLevel.Level3 => level3TrashWeight,
            _ => level1TrashWeight
        };
    }
}