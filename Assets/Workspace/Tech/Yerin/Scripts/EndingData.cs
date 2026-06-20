using UnityEngine;

[CreateAssetMenu (fileName = "EndingData", menuName = "Game/Ending/Ending Data")]
public class EndingData : ScriptableObject
{
    [Header("Ending")]
    [SerializeField] private EndingType endingType;
    [SerializeField] private string title;
    [TextArea(3, 8)]
    [SerializeField] private string description;

    [Header("Visual")]
    [SerializeField] private Sprite endingImage;

    public EndingType EndingType => endingType;
    public string Title => title;
    public string Description => description;
    public Sprite EndingImage => endingImage;
}