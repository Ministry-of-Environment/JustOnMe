using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusUI : MonoBehaviour
{
    [Header("UI Fill Images")]
    [SerializeField] private Image hungerBarFill;
    [SerializeField] private Image pollutionBarFill;

    [Header("Economy UI")]
    [SerializeField] private TMP_Text moneyText;

    [Header("Date UI")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text monthText;
    [SerializeField] private TMP_Text rentText;

    private void Update()
    {
        UpdateHungerUI();
        UpdatePollutionUI();
        UpdateMoneyUI();
        UpdateDateUI();
    }

    private void UpdateHungerUI()
    {
        if (HungerSystem.Instance == null || hungerBarFill == null)
        {
            return;
        }

        hungerBarFill.fillAmount = HungerSystem.Instance.HungerRatio;
    }

    private void UpdatePollutionUI()
    {
        if (PollutionSystem.Instance == null || pollutionBarFill == null)
        {
            return;
        }

        pollutionBarFill.fillAmount = PollutionSystem.Instance.PollutionRatio;
    }

    private void UpdateMoneyUI()
    {
        if (CurrencyManager.Instance == null || moneyText == null)
        {
            return;
        }

        moneyText.text = $"{CurrencyManager.Instance.CurrentMoney} G";
    }

    private void UpdateDateUI()
    {
        if (GamePeriodCycle.Instance == null)
        {
            return;
        }

        int day = GamePeriodCycle.Instance.PeriodCount;

        if (dayText != null)
        {
            dayText.text = $"Day {day}";
        }

        int month = ((day - 1) / 4) + 1;

        if (monthText != null)
        {
            monthText.text = $"Month {month}";
        }

        int remainDays = 4 - ((day - 1) % 4);

        if (rentText != null)
        {
            rentText.text = $"Rent : {remainDays} Day Left";
        }
    }
}