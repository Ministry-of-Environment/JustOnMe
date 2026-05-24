using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 게임의 기간과 단계(페이즈)를 관리하는 클래스
/// </summary>
public class GamePeriodCycle : MonoBehaviour
{
    [Header("Period Settings")]
    [SerializeField] private float phaseDuration = 10f; // 각 페이즈 지속 시간 (초)

    private PeriodPhase currentPhase = PeriodPhase.Beginning;
    private float timer;    // 현재 페이즈에서 경과된 시간
    private int periodCount = 1;    // 현재 기간 횟수

    private bool isInitialized;
    private bool isRunning;
    private bool isCycleEnded;

    public UnityEvent<PeriodPhase> OnPhaseChanged;
    public UnityEvent<int> OnPeriodChanged;
    public UnityEvent OnCycleEnded;

    public PeriodPhase CurrentPhase => currentPhase;
    public int PeriodCount => periodCount;
    public bool IsRunning => isRunning;
    public bool IsCycleEnded => isCycleEnded;

    #region Unity Events
    private void Update()
    {
        // 결과 대기 구간에서는 타이머가 작동하지 않도록 함
        if (currentPhase == PeriodPhase.ResultWait)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= phaseDuration)
        {
            timer = 0f;
            MoveToNextPhase();
        }
    }
    #endregion

    public void Initialize()
    {
        periodCount = 1;
        timer = 0f;
        currentPhase = PeriodPhase.Beginning;

        isInitialized = true;
        isRunning = false;

        OnPeriodChanged?.Invoke(periodCount);
        OnPhaseChanged?.Invoke(currentPhase);

        Debug.Log("기간 시스템 초기화 완료");
    }

    #region On Cycle
    [ContextMenu("Start Cycle")]
    public void StartCycle()
    {
        if (isInitialized == false)
        {
            Initialize();
        }

        isRunning = true;

        Debug.Log("기간 진행 시작");
    }

    /// <summary>
    /// 현재 단계에 따라 다음 단계로 넘기는 메서드
    /// </summary>
    private void MoveToNextPhase()
    {
        switch (currentPhase)
        {
            case PeriodPhase.Beginning:
                ChangePhase(PeriodPhase.Progress);
                break;

            case PeriodPhase.Progress:
                ChangePhase(PeriodPhase.Peak);
                break;

            case PeriodPhase.Peak:
                ChangePhase(PeriodPhase.Ending);
                break;

            case PeriodPhase.Ending:
                ChangePhase(PeriodPhase.ResultWait);
                break;
        }
    }

    /// <summary>
    /// 실제로 상태를 바꾸는 메서드
    /// 
    /// 상태가 바뀔 때마다 이벤트를 발생
    /// </summary>
    /// <param name="nextPhase">다음 페이즈</param>
    private void ChangePhase(PeriodPhase nextPhase)
    {
        currentPhase = nextPhase;
        timer = 0f;

        OnPhaseChanged?.Invoke(currentPhase);

        Debug.Log($"현재 기간 단계: {currentPhase}");
    }

    /// <summary>
    /// 페이즈가 ResultWait일 때, 다음 기간으로 넘어가는 메서드
    /// </summary>
    public void GoToNextPeriod()
    {
        if (currentPhase != PeriodPhase.ResultWait)
        {
            return;
        }

        periodCount++;
        OnPeriodChanged?.Invoke(periodCount);

        ChangePhase(PeriodPhase.Beginning);

        Debug.Log($"{periodCount}번째 기간 시작");
    }
    #endregion

    #region Stop & Resume & End Cycle
    public void StopCycle()
    {
        if (isCycleEnded)
        {
            return;
        }

        isRunning = false;
    }

    public void ResumeCycle()
    {
        if (isInitialized == false)
        {
            return;
        }

        if (isCycleEnded)
        {
            return;
        }

        isRunning = true;
    }

    public void EndCycle()
    {
        if (isCycleEnded)
        {
            return;
        }

        isRunning = false;
        isCycleEnded = true;
        timer = 0f;

        OnCycleEnded?.Invoke();

        Debug.Log("게임 종료");
    }
    #endregion
}

public enum PeriodPhase
{
    Beginning,   // 새벽처럼 보이는 시작 구간
    Progress,   // 초반 구간
    Peak,       // 정점 구간
    Ending,     // 저녁처럼 보이는 끝 구간
    ResultWait  // 결과 대기 구간
}

