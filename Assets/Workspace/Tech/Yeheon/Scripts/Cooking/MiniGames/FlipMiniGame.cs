using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlipMiniGame : MonoBehaviour
{
    public Action OnMiniGameSuccess;
    public Action OnMiniGameFail;

    [SerializeField] private RectTransform flipBar;

    [SerializeField] private RectTransform cursor;

    [SerializeField] private RectTransform successZone;

    [SerializeField] private TMP_Text countText;

    [Header("Cursor Move")]
    [SerializeField] private float startMoveSpeed = 600f;

    [SerializeField] private float speedIncreasePerSuccess = 100f;

    [Header("Zone")]
    [SerializeField] private float startZoneWidth = 100f;

    [SerializeField] private float zoneDecreasePerSuccess = 25f;

    private float currentMoveSpeed;

    private int currentSuccessCount;

    private int targetSuccessCount;

    private bool movingRight;

    private bool gameRunning;

    private float barHalfWidth;

    private void Start()
    {
        barHalfWidth = flipBar.rect.width * 0.5f;
    }

    private void Update()
    {
        if (!gameRunning)
            return;

        MoveCursor();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckHit();
        }
    }


    public void StartGame()
    {
        gameRunning = true;

        currentSuccessCount = 0;

        targetSuccessCount = UnityEngine.Random.Range(2, 7);

        currentMoveSpeed = startMoveSpeed;

        movingRight = true;

        ResetCursor();

        ResetZone();

        UpdateCountUI();
    }

    public void StopMiniGame()
    {
        gameRunning = false;
    }

    private void MoveCursor()
    {
        Vector2 position = cursor.anchoredPosition;

        float direction = movingRight ? 1f : -1f;

        position.x += direction * currentMoveSpeed * Time.deltaTime;

        if (position.x >= barHalfWidth)
        {
            position.x = barHalfWidth;

            movingRight = false;
        }
        else if (position.x <= -barHalfWidth)
        {
            position.x = -barHalfWidth;

            movingRight = true;
        }

        cursor.anchoredPosition = position;
    }

    private void CheckHit()
    {
        float cursorX = cursor.anchoredPosition.x;

        float zoneX = successZone.anchoredPosition.x;

        float zoneHalfWidth = successZone.rect.width * 0.5f;

        bool success =
            cursorX >= zoneX - zoneHalfWidth &&
            cursorX <= zoneX + zoneHalfWidth;

        if (!success)
        {
            FailGame();
            return;
        }

        currentSuccessCount++;

        UpdateCountUI();

        if (currentSuccessCount >= targetSuccessCount)
        {
            SuccessGame();
            return;
        }

        IncreaseDifficulty();
    }

    private void IncreaseDifficulty()
    {
        currentMoveSpeed += speedIncreasePerSuccess;

        float newWidth =
            successZone.sizeDelta.x - zoneDecreasePerSuccess;

        newWidth = Mathf.Max(newWidth, 80f);

        successZone.sizeDelta =
            new Vector2(newWidth, successZone.sizeDelta.y);
    }

    private void ResetCursor()
    {
        cursor.anchoredPosition =
            new Vector2(-barHalfWidth, cursor.anchoredPosition.y);
    }

    private void ResetZone()
    {
        successZone.sizeDelta =
            new Vector2(startZoneWidth, successZone.sizeDelta.y);
    }

    private void UpdateCountUI()
    {
        countText.text =
            currentSuccessCount + " / " + targetSuccessCount;
    }

    private void SuccessGame()
    {
        gameRunning = false;

        countText.text = "SUCCESS";

        OnMiniGameSuccess?.Invoke();
    }

    private void FailGame()
    {
        gameRunning = false;

        countText.text = "FAIL";

        OnMiniGameFail?.Invoke();
    }
}