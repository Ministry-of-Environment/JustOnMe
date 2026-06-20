using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SleepManager : MonoBehaviour
{
    [Header("Sleep Settings")]
    [SerializeField] private float sleepDelay = 1f;
    [SerializeField] private string cannotSleepMessage = "현재는 잠을 잘 수 없습니다.";

    [Header("Events")]
    public UnityEvent OnRentPaymentRequired;
    public UnityEvent OnSleepStarted;
    public UnityEvent OnSleepFinished;

    private bool isSleeping;

    public bool IsSleeping => isSleeping;

    public void StartSleep()
    {
        if (isSleeping)
        {
            return;
        }

        if (GameEndingManager.Instance != null && GameEndingManager.Instance.IsEnded)
        {
            return;
        }

        if (GamePeriodCycle.Instance == null)
        {
            Debug.LogError("GamePeriodCycle이 존재하지 않습니다.");
            return;
        }

        if (GamePeriodCycle.Instance.CurrentPhase != PeriodPhase.ResultWait)
        {
            Debug.Log(cannotSleepMessage);
            return;
        }

        if (RentSystem.Instance != null && RentSystem.Instance.IsRentDay())
        {
            Debug.Log("월세 납부 필요");
            OnRentPaymentRequired?.Invoke();
            return;
        }

        ContinueSleep();
    }

    public void ContinueSleep()
    {
        if (isSleeping)
        {
            return;
        }

        if (GameEndingManager.Instance != null && GameEndingManager.Instance.IsEnded)
        {
            return;
        }

        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        isSleeping = true;

        Debug.Log("잠자기 시작");

        OnSleepStarted?.Invoke();

        yield return new WaitForSecondsRealtime(sleepDelay);

        if (GamePeriodCycle.Instance != null)
        {
            GamePeriodCycle.Instance.GoToNextPeriod();
        }

        if (GameEndingManager.Instance != null && GameEndingManager.Instance.IsEnded)
        {
            isSleeping = false;
            yield break;
        }

        OnSleepFinished?.Invoke();

        isSleeping = false;
    }
}