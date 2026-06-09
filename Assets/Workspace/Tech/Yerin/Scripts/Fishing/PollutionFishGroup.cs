using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PollutionFishGroup
{
    [SerializeField] private PollutionLevel pollutionLevel;
    [SerializeField] private List<FishData> fishes = new();

    public PollutionLevel PollutionLevel => pollutionLevel;
    public List<FishData> Fishes => fishes;
}
