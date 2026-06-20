using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EndingUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private Image endingImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button goToTitleButton;

    [Header("Title Scene")]
    [SerializeField] private string titleSceneName = "Title";

    private void Awake()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }

        if (goToTitleButton != null)
        {
            goToTitleButton.onClick.AddListener(GoToTitle);
        }
    }

    private void Start()
    {
        if (GameEndingManager.Instance != null)
        {
            GameEndingManager.Instance.SetEndingUI(this);
        }
    }

    public void ShowEnding(EndingData endingData)
    {
        if (endingData == null)
        {
            Debug.LogWarning("EndingData가 null입니다.");
            return;
        }

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
        }

        if (endingImage != null)
        {
            endingImage.sprite = endingData.EndingImage;
            endingImage.enabled = endingData.EndingImage != null;
        }

        if (titleText != null)
        {
            titleText.text = endingData.Title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = endingData.Description;
        }
    }

    private void GoToTitle()
    {
        if (GameEndingManager.Instance != null)
        {
            GameEndingManager.Instance.GoToTitle(titleSceneName);
        }
    }
}