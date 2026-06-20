using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class HungerSystem : MonoBehaviour
{
    public static HungerSystem Instance { get; private set; }

    [Header("Hunger Settings")]
    [SerializeField, Min(1)] private int maxHunger = 100;
    [SerializeField, Range(0f, 1f)] private float startingHungerRatio = 1f;
    [SerializeField, Min(0f)] private float hungerDecayPerMinute = 5f;

    [Header("Runtime Drain")]
    [SerializeField, Min(0f)] private float hungerDecayMultiplier = 1f;

    [Header("Hunger State Thresholds")]
    [SerializeField, Range(0f, 1f)] private float fullThreshold = 0.8f;
    [SerializeField, Range(0f, 1f)] private float normalThreshold = 0.5f;
    [SerializeField, Range(0f, 1f)] private float hungryThreshold = 0.25f;
    [SerializeField, Range(0f, 1f)] private float criticalThreshold = 0.1f;

    [Header("Auto Drain")]
    [SerializeField] private bool drainAutomatically = true;
    [SerializeField] private bool initializeOnAwake = true;

    [Header("Recovery")]
    [SerializeField, Min(0)] private int defaultFoodRecovery = 25;

    [Header("Events")]
    public UnityEvent<int> OnHungerChanged;
    public UnityEvent<float> OnHungerRatioChanged;
    public UnityEvent<HungerState> OnHungerStateChanged;
    public UnityEvent OnHungerDepleted;

    public event Action<int> HungerChanged;
    public event Action<float> HungerRatioChanged;
    public event Action<HungerState> HungerStateChanged;
    public event Action HungerDepleted;

    private float currentHunger;
    private int lastNotifiedHunger;
    private HungerState currentState = HungerState.Full;
    private bool isInitialized;
    private bool isDepleted;

    public int CurrentHunger => Mathf.CeilToInt(currentHunger);
    public float CurrentHungerFloat => currentHunger;
    public float HungerRatio => maxHunger <= 0 ? 0f : currentHunger / maxHunger;
    public HungerState CurrentState => currentState;
    public bool IsDepleted => isDepleted;
    public float HungerDecayMultiplier => hungerDecayMultiplier;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (initializeOnAwake)
        {
            Initialize();
        }
    }

    private void Update()
    {
        if (!drainAutomatically || !isInitialized || isDepleted)
        {
            return;
        }

        AdvanceTime(Time.deltaTime / 60f, hungerDecayMultiplier);
    }

    private void OnValidate()
    {
        maxHunger = Mathf.Max(1, maxHunger);
        startingHungerRatio = Mathf.Clamp01(startingHungerRatio);
        hungerDecayPerMinute = Mathf.Max(0f, hungerDecayPerMinute);
        hungerDecayMultiplier = Mathf.Max(0f, hungerDecayMultiplier);
        defaultFoodRecovery = Mathf.Max(0, defaultFoodRecovery);

        fullThreshold = Mathf.Clamp01(fullThreshold);
        normalThreshold = Mathf.Clamp01(normalThreshold);
        hungryThreshold = Mathf.Clamp01(hungryThreshold);
        criticalThreshold = Mathf.Clamp01(criticalThreshold);

        if (normalThreshold > fullThreshold)
        {
            normalThreshold = fullThreshold;
        }

        if (hungryThreshold > normalThreshold)
        {
            hungryThreshold = normalThreshold;
        }

        if (criticalThreshold > hungryThreshold)
        {
            criticalThreshold = hungryThreshold;
        }
    }

    public void Initialize()
    {
        isInitialized = true;
        isDepleted = false;

        float startingHunger = maxHunger * startingHungerRatio;
        currentHunger = Mathf.Clamp(startingHunger, 0f, maxHunger);

        lastNotifiedHunger = Mathf.CeilToInt(currentHunger);
        currentState = GetStateFromRatio(HungerRatio);

        NotifyHungerValue(true);
        NotifyHungerRatio(true);
        NotifyHungerState(true);
    }

    public void SetHungerDecayMultiplier(float multiplier)
    {
        hungerDecayMultiplier = Mathf.Max(0f, multiplier);
    }

    public void AdvanceTime(float minutes)
    {
        AdvanceTime(minutes, 1f);
    }

    public void AdvanceTime(float minutes, float multiplier)
    {
        if (minutes <= 0f)
        {
            return;
        }

        EnsureInitialized();

        if (isDepleted)
        {
            return;
        }

        multiplier = Mathf.Max(0f, multiplier);

        float drainAmount = minutes * hungerDecayPerMinute * multiplier;
        SetHungerInternal(currentHunger - drainAmount, false);
    }

    public bool ConsumeFood(int recoveryAmount)
    {
        if (recoveryAmount <= 0)
        {
            return false;
        }

        EnsureInitialized();
        return SetHungerInternal(currentHunger + recoveryAmount, false);
    }

    public bool ConsumeDefaultFood()
    {
        return ConsumeFood(defaultFoodRecovery);
    }

    public bool SpendHunger(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        EnsureInitialized();
        return SetHungerInternal(currentHunger - amount, false);
    }

    public bool SpendHunger(float amount)
    {
        if (amount <= 0f)
        {
            return false;
        }

        EnsureInitialized();
        return SetHungerInternal(currentHunger - amount, false);
    }

    public void SetHunger(int value)
    {
        EnsureInitialized();
        SetHungerInternal(value, false);
    }

    public void SetHunger(float value)
    {
        EnsureInitialized();
        SetHungerInternal(value, false);
    }

    public HungerState GetStateFromRatio(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);

        if (ratio <= 0f)
        {
            return HungerState.Empty;
        }

        if (ratio >= fullThreshold)
        {
            return HungerState.Full;
        }

        if (ratio >= normalThreshold)
        {
            return HungerState.Normal;
        }

        if (ratio >= hungryThreshold)
        {
            return HungerState.Hungry;
        }

        if (ratio >= criticalThreshold)
        {
            return HungerState.Critical;
        }

        return HungerState.Starving;
    }

    private void EnsureInitialized()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    private bool SetHungerInternal(float value, bool forceNotify)
    {
        float nextHunger = Mathf.Clamp(value, 0f, maxHunger);
        HungerState nextState = GetStateFromRatio(nextHunger / maxHunger);

        bool valueChanged = !Mathf.Approximately(currentHunger, nextHunger);
        bool stateChanged = currentState != nextState;

        currentHunger = nextHunger;
        currentState = nextState;

        if (forceNotify || valueChanged)
        {
            NotifyHungerValue(forceNotify);
            NotifyHungerRatio(forceNotify);
        }

        if (forceNotify || stateChanged)
        {
            NotifyHungerState(forceNotify);
        }

        if (currentHunger <= 0f && !isDepleted)
        {
            isDepleted = true;

            OnHungerDepleted?.Invoke();
            HungerDepleted?.Invoke();

            if (GameEndingManager.Instance != null)
            {
                GameEndingManager.Instance.TriggerEnding(EndingType.Starvation);
            }
        }
        else if (currentHunger > 0f)
        {
            isDepleted = false;
        }

        return valueChanged || stateChanged;
    }

    private void NotifyHungerValue(bool forceNotify)
    {
        int roundedHunger = Mathf.CeilToInt(currentHunger);

        if (!forceNotify && lastNotifiedHunger == roundedHunger)
        {
            return;
        }

        lastNotifiedHunger = roundedHunger;

        OnHungerChanged?.Invoke(roundedHunger);
        HungerChanged?.Invoke(roundedHunger);
    }

    private void NotifyHungerRatio(bool forceNotify)
    {
        OnHungerRatioChanged?.Invoke(HungerRatio);
        HungerRatioChanged?.Invoke(HungerRatio);
    }

    private void NotifyHungerState(bool forceNotify)
    {
        OnHungerStateChanged?.Invoke(currentState);
        HungerStateChanged?.Invoke(currentState);
    }
}

public enum HungerState
{
    Full,
    Normal,
    Hungry,
    Critical,
    Starving,
    Empty
}