using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 게임의 기간과 단계(페이즈)를 관리하는 클래스
/// </summary>
public class GamePeriodCycle : MonoBehaviour
{
    public static GamePeriodCycle Instance { get; private set; }

    [Header("Period Settings")]
    [SerializeField] private float phaseDuration = 10f;

    [Header("Hunger Integration")]
    [SerializeField] private HungerSystem hungerSystem;
    [SerializeField] private bool drainHungerByCycle = true;

    [Header("Result Wait Hunger Penalty")]
    [SerializeField, Min(1f)] private float resultWaitHungerMultiplier = 2f;

    private PeriodPhase currentPhase = PeriodPhase.Beginning;
    private float timer;
    private int periodCount = 1;

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResolveHungerSystem();
        Initialize();
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        ApplySmoothHungerDrain(Time.deltaTime);

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

    public void Initialize()
    {
        periodCount = 1;
        timer = 0f;
        currentPhase = PeriodPhase.Beginning;

        isInitialized = true;
        isRunning = false;
        isCycleEnded = false;

        OnPeriodChanged?.Invoke(periodCount);
        OnPhaseChanged?.Invoke(currentPhase);

        Debug.Log("기간 시스템 초기화 완료");

        StartCycle();
    }

    [ContextMenu("Start Cycle")]
    public void StartCycle()
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (isCycleEnded)
        {
            return;
        }

        isRunning = true;

        Debug.Log("기간 진행 시작");
    }

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

    private void ChangePhase(PeriodPhase nextPhase)
    {
        currentPhase = nextPhase;
        timer = 0f;

        OnPhaseChanged?.Invoke(currentPhase);

        Debug.Log($"현재 기간 단계: {currentPhase}");
    }

    public void GoToNextPeriod()
    {
        if (currentPhase != PeriodPhase.ResultWait)
        {
            return;
        }

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.NotifyWeekFinished();
            return;
        }

        periodCount++;
        OnPeriodChanged?.Invoke(periodCount);

        ChangePhase(PeriodPhase.Beginning);

        Debug.Log($"{periodCount}번째 기간 시작");
    }

    public void StartNewWeek()
    {
        if (isCycleEnded)
        {
            return;
        }

        if (GameSessionManager.Instance != null)
        {
            periodCount = GameSessionManager.Instance.CurrentWeek;
        }
        else
        {
            periodCount++;
        }

        timer = 0f;

        OnPeriodChanged?.Invoke(periodCount);

        ChangePhase(PeriodPhase.Beginning);

        Debug.Log($"{periodCount}번째 기간 시작");
    }

    private void ApplySmoothHungerDrain(float deltaTime)
    {
        if (!drainHungerByCycle)
        {
            return;
        }

        ResolveHungerSystem();

        if (hungerSystem == null)
        {
            return;
        }

        if (hungerSystem.IsDepleted)
        {
            return;
        }

        float multiplier = 1f;

        if (currentPhase == PeriodPhase.ResultWait)
        {
            multiplier *= resultWaitHungerMultiplier;
        }

        if (GameSessionManager.Instance != null)
        {
            multiplier *= GameSessionManager.Instance.HungerMultiplier;
        }

        hungerSystem.AdvanceTime(deltaTime / 60f, multiplier);
    }

    private void ResolveHungerSystem()
    {
        if (hungerSystem != null)
        {
            return;
        }

        hungerSystem = HungerSystem.Instance;

        if (hungerSystem == null)
        {
            hungerSystem = FindFirstObjectByType<HungerSystem>();
        }
    }

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
        if (!isInitialized)
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
}

public enum PeriodPhase
{
    Beginning,
    Progress,
    Peak,
    Ending,
    ResultWait
}