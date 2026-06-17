using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RentPopupUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text rentText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private Button payButton;

    [Header("References")]
    [SerializeField] private RentSystem rentSystem;
    [SerializeField] private SleepManager sleepManager;

    private void Start()
    {
        panel.SetActive(false);

        payButton.onClick.AddListener(PayRent);
    }

    public void OpenPopup()
    {
        panel.SetActive(true);

        rentText.text =
            $"Rent : {rentSystem.RentAmount} G";

        moneyText.text =
            $"Cash reserves : {CurrencyManager.Instance.CurrentMoney} G";
    }

    public void PayRent()
    {
        bool success =
            CurrencyManager.Instance.TrySpendMoney(
                rentSystem.RentAmount);

        if (!success)
        {
            HandleGameOver();
            return;
        }

        panel.SetActive(false);

        sleepManager.ContinueSleep();
    }
    private void HandleGameOver()
    {
        Debug.Log("월세를 납부하지 못했습니다.");

        SceneManager.LoadScene("Title");
    }
}