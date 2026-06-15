using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SleepManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GamePeriodCycle periodCycle;

    [Header("Sleep Settings")]
    [SerializeField] private float sleepDelay = 1f;
    [SerializeField] private string cannotSleepMessage = "현재는 잠을 잘 수 없습니다.";

    private bool isSleeping;

    public bool IsSleeping => isSleeping;

    // 잠자기 시작 이벤트
    public UnityEvent OnSleepStarted;

    // 다음 날 전환 완료 이벤트
    public UnityEvent OnSleepFinished;

    /// <summary>
    /// 잠자기 시작 요청
    /// </summary>
    public void StartSleep()
    {
        if (isSleeping)
        {
            return;
        }

        if (periodCycle == null)
        {
            Debug.LogError("SleepManager에 GamePeriodCycle이 연결되어 있지 않습니다.");
            return;
        }

        if (periodCycle.CurrentPhase != PeriodPhase.ResultWait)
        {
            Debug.Log(cannotSleepMessage);
            return;
        }

        StartCoroutine(SleepRoutine());
    }

    /// <summary>
    /// 실제 잠자기 진행 루틴
    /// </summary>
    private IEnumerator SleepRoutine()
    {
        isSleeping = true;

        Debug.Log("잠자기 시작");

        // 잠자기 시작 이벤트
        OnSleepStarted?.Invoke();

        // 나중에:
        // 화면 페이드 아웃
        // 저장
        // 정산
        // 사운드
        // 연출
        // 등을 여기에 추가 가능

        yield return new WaitForSeconds(sleepDelay);

        // 다음 기간(다음 날) 진행
        periodCycle.GoToNextPeriod();

        // 잠자기 종료 이벤트
        OnSleepFinished?.Invoke();

        isSleeping = false;
    }
}
