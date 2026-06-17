using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class StatusUI : MonoBehaviour
{
    [Header("Systems")]
    public HungerSystem hungerSystem;
    public PollutionSystem pollutionSystem;

    [Header("UI Fill Images")]
    public Image hungerBarFill;
    public Image pollutionBarFill;

    [Header("Economy UI")]
    [SerializeField] private TMP_Text moneyText;

    [SerializeField] private TMP_Text dayText;

    [SerializeField] private TMP_Text monthText;

    [SerializeField] private TMP_Text rentText;

    [Header("References")]
    [SerializeField] private GamePeriodCycle periodCycle;
    [SerializeField] private RentSystem rentSystem;

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

        if (moneyText != null && CurrencyManager.Instance != null)
        {
            moneyText.text = $"{CurrencyManager.Instance.CurrentMoney} G";
        }

        if (periodCycle != null)
        {
            int day = periodCycle.PeriodCount;

            dayText.text = $"Day {day}";

            int month = ((day - 1) / 4) + 1;

            monthText.text = $"Month {month}";

            int remainDays = 4 - ((day - 1) % 4);

            rentText.text = $"Rent : {remainDays} Day Left";
        }
    }
}