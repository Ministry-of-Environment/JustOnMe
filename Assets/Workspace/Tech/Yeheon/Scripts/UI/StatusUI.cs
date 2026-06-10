using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    [Header("Systems")]
    public HungerSystem hungerSystem;
    public PollutionSystem pollutionSystem;

    [Header("UI Fill Images")]
    public Image hungerBarFill;
    public Image pollutionBarFill;

    void Update()
    {
        if (hungerSystem != null && hungerBarFill != null)
        {
            hungerBarFill.fillAmount = hungerSystem.HungerRatio;
        }

        if (pollutionSystem != null && pollutionBarFill != null)
        {
            pollutionBarFill.fillAmount = pollutionSystem.PollutionRatio;
        }
    }
}