using System.Collections.Generic;
using UnityEngine;

public class FishTable : MonoBehaviour
{
    [SerializeField] FishPollutionTableSO fishesTable;

    public FishData GetRandomFish(PollutionLevel level)
    {
        int totalWeight = 0;

        PollutionFishGroup fishGroup = fishesTable.GetFishGroup(level);

        if (fishGroup == null)
        {
            return null;
        }

        foreach (FishData fish in fishGroup.Fishes)
        {
            totalWeight += fish.Weight;
        }

        if (totalWeight <= 0)
        {
            Debug.LogWarning($"{level} 단계에서 추첨 가능한 물고기가 없습니다.");
            return null;
        }

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (FishData fish in fishGroup.Fishes)
        {
            currentWeight += fish.Weight;

            if (randomValue < currentWeight)
            {
                return fish;
            }
        }

        return null;
    }
}
