using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PepperMiniGame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform ringOuter;
    [SerializeField] private RectTransform ringInner;
    [SerializeField] private RectTransform centerPoint;

    [SerializeField] private TMP_Text countText;

    [SerializeField] private GameObject successText;
    [SerializeField] private GameObject missText;

    [Header("Scale Settings")]
    [SerializeField] private float startScale = 2.5f;
    [SerializeField] private float minScale = 0.2f;

    [Header("Target")]
    [SerializeField] private float targetScale = 1.0f;
    [SerializeField] private float successRange = 0.12f;

    [Header("Speed")]
    [SerializeField] private float shrinkSpeed = 1.8f;
    [SerializeField] private float speedIncreasePerSuccess = 0.25f;

    [Header("Difficulty")]
    [SerializeField] private float rangeDecreasePerSuccess = 0.015f;

    [Header("Success Count")]
    [SerializeField] private int minSuccessCount = 4;
    [SerializeField] private int maxSuccessCount = 7;

    private int currentSuccessCount;
    private int targetSuccessCount;

    private float currentShrinkSpeed;
    private float currentSuccessRange;

    private bool gameRunning;
    private bool roundActive;
    private bool isResetting;

    public Action OnMiniGameSuccess;

    private void Update()
    {
        if (!gameRunning || !roundActive || isResetting)
            return;

        UpdateRingScale();
        CheckInput();
    }

    public void StartGame()
    {
        gameObject.SetActive(true);

        successText.SetActive(false);
        missText.SetActive(false);

        countText.gameObject.SetActive(true);

        currentSuccessCount = 0;

        targetSuccessCount =
            UnityEngine.Random.Range(minSuccessCount, maxSuccessCount + 1);

        currentShrinkSpeed = shrinkSpeed;
        currentSuccessRange = successRange;

        UpdateCountText();

        gameRunning = true;

        StartRound();
    }

    private void StartRound()
    {
        roundActive = true;

        ringOuter.localScale = Vector3.one * startScale;
    }

    private void UpdateRingScale()
    {
        float currentScale = ringOuter.localScale.x;

        currentScale -= currentShrinkSpeed * Time.deltaTime;

        ringOuter.localScale = Vector3.one * currentScale;

        if (currentScale <= minScale)
        {
            StartCoroutine(ResetProgressRoutine());
        }
    }

    private void CheckInput()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckTiming();
        }
    }

    private void CheckTiming()
    {
        float currentScale = ringOuter.localScale.x;

        bool isSuccess =
            currentScale >= targetScale - currentSuccessRange &&
            currentScale <= targetScale + currentSuccessRange;

        if (isSuccess)
        {
            SuccessRound();
        }
        else
        {
            StartCoroutine(ResetProgressRoutine());
        }
    }

    private void SuccessRound()
    {
        currentSuccessCount++;

        UpdateCountText();

        if (currentSuccessCount >= targetSuccessCount)
        {
            SuccessGame();
            return;
        }

        currentShrinkSpeed += speedIncreasePerSuccess;

        currentSuccessRange -= rangeDecreasePerSuccess;

        currentSuccessRange = Mathf.Max(currentSuccessRange, 0.03f);

        StartRound();
    }

    private IEnumerator ResetProgressRoutine()
    {
        if (isResetting)
            yield break;

        isResetting = true;

        roundActive = false;

        countText.gameObject.SetActive(false);

        missText.SetActive(true);

        currentSuccessCount = 0;

        currentShrinkSpeed = shrinkSpeed;
        currentSuccessRange = successRange;

        UpdateCountText();

        yield return new WaitForSeconds(0.5f);

        missText.SetActive(false);

        countText.gameObject.SetActive(true);

        isResetting = false;

        StartRound();
    }

    private void SuccessGame()
    {
        gameRunning = false;
        roundActive = false;

        StartCoroutine(SuccessRoutine());
    }

    private IEnumerator SuccessRoutine()
    {
        countText.gameObject.SetActive(false);

        successText.SetActive(true);

        yield return new WaitForSeconds(1f);

        OnMiniGameSuccess?.Invoke();
    }

    private void UpdateCountText()
    {
        countText.text =
            $"{currentSuccessCount} / {targetSuccessCount}";
    }
}