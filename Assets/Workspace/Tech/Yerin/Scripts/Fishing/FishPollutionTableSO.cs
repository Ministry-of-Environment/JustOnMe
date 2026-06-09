using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FishPollutionTableSO", menuName = "Fishing/FishPollutionTableSO")]
public class FishPollutionTableSO : ScriptableObject
{
    [SerializeField] List<PollutionFishGroup> fishGroups = new();

    public PollutionFishGroup GetFishGroup(PollutionLevel pollutionLevel)
    {
        foreach (PollutionFishGroup group in fishGroups)
        {
            if (group.PollutionLevel == pollutionLevel)
            {
                return group;
            }
        }

        Debug.LogError($"{pollutionLevel} 레벨에 맞는 물고기 정보 데이터가 없습니다.");
        return null;
    }
}

public enum PollutionLevel
{
    Level1,
    Level2,
    Level3
}
