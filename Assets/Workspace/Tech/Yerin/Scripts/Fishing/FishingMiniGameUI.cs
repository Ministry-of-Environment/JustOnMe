using System;
using UnityEngine;
using UnityEngine.UI;

public class FishingMiniGameUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform barArea;
    [SerializeField] private RectTransform successZone;
    [SerializeField] private RectTransform cursor;

    [Header("Progress UI")]
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform progressFill;
    [SerializeField] private RectTransform fishIcon;

    private ItemData currentFish;
    private FishRarityDifficulty currentDifficulty;

    private int currentSuccessCount;
    private int requiredSuccessCount;

    private float cursorPosition;
    private float cursorDirection = 1f;
    private float cursorSpeed;
    private float successZoneCenter;
    private float successZoneSize;

    private bool isPlaying;

    private Action<ItemData> onSuccess;
    private Action<ItemData> onFail;

    private void Awake()
    {
        Hide();
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        MoveCursor();
    }

    public void StartMiniGame(
        ItemData fish,
        FishRarityDifficulty difficulty,
        Action<ItemData> successCallback,
        Action<ItemData> failCallback)
    {
        Debug.Log("FishingMiniGameUI.StartMiniGame 호출됨");

        currentFish = fish;
        currentDifficulty = difficulty;

        onSuccess = successCallback;
        onFail = failCallback;

        currentSuccessCount = 0;
        requiredSuccessCount = difficulty.RequiredSuccessCount;
        cursorSpeed = difficulty.CursorSpeed;
        successZoneSize = difficulty.SuccessZoneSize;

        Show();
        UpdateProgressUI();
        StartRound();

        isPlaying = true;
    }

    public void TryConfirm()
    {
        if (!isPlaying)
        {
            return;
        }

        CheckInput();
    }

    private void StartRound()
    {
        cursorPosition = 0f;
        cursorDirection = 1f;

        float halfZoneSize = successZoneSize * 0.5f;
        successZoneCenter = UnityEngine.Random.Range(halfZoneSize, 1f - halfZoneSize);

        UpdateSuccessZoneUI();
        UpdateCursorUI();
    }

    private void MoveCursor()
    {
        cursorPosition += cursorDirection * cursorSpeed * Time.deltaTime;

        if (cursorPosition >= 1f)
        {
            cursorPosition = 1f;
            cursorDirection = -1f;
        }
        else if (cursorPosition <= 0f)
        {
            cursorPosition = 0f;
            cursorDirection = 1f;
        }

        UpdateCursorUI();
    }

    private void CheckInput()
    {
        float halfZoneSize = successZoneSize * 0.5f;
        float min = successZoneCenter - halfZoneSize;
        float max = successZoneCenter + halfZoneSize;

        bool isSuccess = cursorPosition >= min && cursorPosition <= max;

        if (isSuccess)
        {
            currentSuccessCount++;
            UpdateProgressUI();

            Debug.Log($"낚시 판정 성공: {currentSuccessCount}/{requiredSuccessCount}");

            if (currentSuccessCount >= requiredSuccessCount)
            {
                CompleteSuccess();
            }
            else
            {
                StartRound();
            }
        }
        else
        {
            CompleteFail();
        }
    }

    private void CompleteSuccess()
    {
        isPlaying = false;
        Hide();

        Debug.Log("낚시 미니게임 성공");

        onSuccess?.Invoke(currentFish);
    }

    private void CompleteFail()
    {
        isPlaying = false;
        Hide();

        Debug.Log("낚시 미니게임 실패");

        onFail?.Invoke(currentFish);
    }

    private void UpdateCursorUI()
    {
        if (barArea == null || cursor == null)
        {
            return;
        }

        float barWidth = barArea.rect.width;
        float x = Mathf.Lerp(-barWidth * 0.5f, barWidth * 0.5f, cursorPosition);

        cursor.anchoredPosition = new Vector2(
            x,
            cursor.anchoredPosition.y
        );
    }

    private void UpdateSuccessZoneUI()
    {
        if (barArea == null || successZone == null)
        {
            return;
        }

        float barWidth = barArea.rect.width;
        float zoneWidth = barWidth * successZoneSize;
        float x = Mathf.Lerp(-barWidth * 0.5f, barWidth * 0.5f, successZoneCenter);

        successZone.sizeDelta = new Vector2(
            zoneWidth,
            successZone.sizeDelta.y
        );

        successZone.anchoredPosition = new Vector2(
            x,
            successZone.anchoredPosition.y
        );
    }

    private void UpdateProgressUI()
    {
        float progress = 0f;

        if (requiredSuccessCount > 0)
        {
            progress = (float)currentSuccessCount / requiredSuccessCount;
        }

        progress = Mathf.Clamp01(progress);

        float barHeight = progressBar.rect.height;

        if (progressFill != null)
        {
            progressFill.sizeDelta = new Vector2(
                progressFill.sizeDelta.x,
                barHeight * progress
            );
        }

        if (fishIcon != null)
        {
            float bottomY = -barHeight * 0.5f;
            float topY = barHeight * 0.5f;
            float y = Mathf.Lerp(bottomY, topY, progress);

            fishIcon.anchoredPosition = new Vector2(
                fishIcon.anchoredPosition.x,
                y
            );
        }
    }

    private void Show()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(false);
        }
    }
}