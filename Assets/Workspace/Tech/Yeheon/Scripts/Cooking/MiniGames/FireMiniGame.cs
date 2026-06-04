using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FireMiniGame : MonoBehaviour
{
    public Action OnMiniGameSuccess;
    public Action OnMiniGameFail;

    [SerializeField] private Button fireButton;

    [SerializeField] private TMP_Text progressText;

    [SerializeField] private TMP_Text stateText;

    [SerializeField] private int requiredSuccessCount = 5;

    [SerializeField] private float reactionTimeLimit = 1f;

    [SerializeField] private Vector2 randomDelayRange = new Vector2(0.5f, 2f);

    private int currentSuccessCount;

    private bool waitingForReaction;

    private bool gameRunning;

    private bool canClick;

    private Coroutine gameRoutine;

    private void Awake()
    {
        fireButton.onClick.AddListener(OnFireButtonClicked);
    }

    public void StartGame()
    {
        gameRunning = true;

        currentSuccessCount = 0;

        UpdateProgressUI();

        stateText.text = "READY";

        canClick = true;

        waitingForReaction = false;
    }

    public void StopMiniGame()
    {
        gameRunning = false;

        if (gameRoutine != null)
        {
            StopCoroutine(gameRoutine);
        }
    }

    private void OnFireButtonClicked()
    {
        if (!gameRunning)
            return;

        if (!canClick)
        {
            FailGame();
            return;
        }

        if (!waitingForReaction)
        {
            StartRandomDelay();
            return;
        }

        waitingForReaction = false;

        currentSuccessCount++;

        UpdateProgressUI();

        if (currentSuccessCount >= requiredSuccessCount)
        {
            SuccessGame();
            return;
        }

        StartRandomDelay();
    }

    private void StartRandomDelay()
    {
        canClick = false;

        stateText.text = "WAIT";

        if (gameRoutine != null)
        {
            StopCoroutine(gameRoutine);
            gameRoutine = null;
        }

        gameRoutine = StartCoroutine(RandomDelayRoutine());
    }

    private IEnumerator RandomDelayRoutine()
    {
        float randomDelay = UnityEngine.Random.Range(
            randomDelayRange.x,
            randomDelayRange.y
        );

        yield return new WaitForSeconds(randomDelay);

        canClick = true;

        waitingForReaction = true;

        stateText.text = "CLICK";

        float timer = 0f;

        while (timer < reactionTimeLimit)
        {
            timer += Time.deltaTime;

            if (!waitingForReaction)
            {
                yield break;
            }

            yield return null;
        }

        FailGame();
    }

    private void UpdateProgressUI()
    {
        progressText.text =
            currentSuccessCount + " / " + requiredSuccessCount;
    }

    private void SuccessGame()
    {
        gameRunning = false;

        canClick = false;

        waitingForReaction = false;

        if (gameRoutine != null)
        {
            StopCoroutine(gameRoutine);
            gameRoutine = null;
        }

        stateText.text = "SUCCESS";

        OnMiniGameSuccess?.Invoke();
    }

    private void FailGame()
    {
        gameRunning = false;

        canClick = false;

        waitingForReaction = false;

        if (gameRoutine != null)
        {
            StopCoroutine(gameRoutine);
            gameRoutine = null;
        }

        stateText.text = "FAIL";

        OnMiniGameFail?.Invoke();
    }
}