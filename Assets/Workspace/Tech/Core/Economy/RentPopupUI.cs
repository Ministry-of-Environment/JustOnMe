using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RentPopupUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text rentText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private Button payButton;

    [Header("References")]
    [SerializeField] private SleepManager sleepManager;

    private void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (payButton != null)
        {
            payButton.onClick.AddListener(PayRent);
        }
    }

    public void OpenPopup()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (RentSystem.Instance != null && rentText != null)
        {
            rentText.text = $"Rent : {RentSystem.Instance.RentAmount} G";
        }

        if (CurrencyManager.Instance != null && moneyText != null)
        {
            moneyText.text = $"Cash reserves : {CurrencyManager.Instance.CurrentMoney} G";
        }
    }

    public void ClosePopup()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void PayRent()
    {
        if (RentSystem.Instance == null)
        {
            Debug.LogWarning("RentSystem이 존재하지 않습니다.");
            return;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("CurrencyManager가 존재하지 않습니다.");
            return;
        }

        bool success = CurrencyManager.Instance.TrySpendMoney(
            RentSystem.Instance.RentAmount
        );

        if (!success)
        {
            TriggerEvictionEnding();
            return;
        }

        ClosePopup();

        if (sleepManager != null)
        {
            sleepManager.ContinueSleep();
        }
        else
        {
            Debug.LogWarning("SleepManager가 할당되지 않았습니다.");
        }
    }

    private void TriggerEvictionEnding()
    {
        Debug.Log("월세를 납부하지 못했습니다.");

        ClosePopup();

        if (GameEndingManager.Instance != null)
        {
            GameEndingManager.Instance.TriggerEnding(EndingType.Eviction);
        }
        else
        {
            Debug.LogWarning("GameEndingManager가 존재하지 않습니다.");
        }
    }
}