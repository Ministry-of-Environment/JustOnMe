using System;
using UnityEngine;
using UnityEngine.UI;

public class CookingTimer : MonoBehaviour
{
    [SerializeField] private Image timerBarFill;

    [SerializeField] private float maxTime = 120f;

    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;

    private float currentTime;

    private bool timerRunning;

    public Action OnTimerEnd;

    private void Update()
    {
        if (!timerRunning)
            return;

        currentTime -= Time.deltaTime;

        UpdateUI();

        if (currentTime <= 0f)
        {
            currentTime = 0f;

            timerRunning = false;

            OnTimerEnd?.Invoke();
        }
    }

    public void StartTimer()
    {
        currentTime = maxTime;

        timerRunning = true;

        UpdateUI();
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    private void UpdateUI()
    {
        float ratio = currentTime / maxTime;

        timerBarFill.fillAmount = ratio;

        if (ratio <= 0.2f)
        {
            timerBarFill.color = dangerColor;
        }
        else if (ratio <= 0.5f)
        {
            timerBarFill.color = warningColor;
        }
        else
        {
            timerBarFill.color = normalColor;
        }
    }
}