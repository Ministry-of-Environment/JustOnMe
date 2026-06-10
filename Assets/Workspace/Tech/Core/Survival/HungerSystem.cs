using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class HungerSystem : MonoBehaviour
{
    [Header("Hunger Settings")]
    [SerializeField, Min(1)] private int maxHunger = 100;
    [SerializeField, Range(0f, 1f)] private float startingHungerRatio = 1f;
    [SerializeField, Min(0f)] private float hungerDecayPerMinute = 5f;

    [Header("Hunger State Thresholds")]
    [SerializeField, Range(0f, 1f)] private float fullThreshold = 0.8f;
    [SerializeField, Range(0f, 1f)] private float normalThreshold = 0.5f;
    [SerializeField, Range(0f, 1f)] private float hungryThreshold = 0.25f;
    [SerializeField, Range(0f, 1f)] private float criticalThreshold = 0.1f;

    [Header("Auto Drain")]
    [SerializeField] private bool drainAutomatically = false;
    [SerializeField] private bool initializeOnAwake = true;

    [Header("Recovery")]
    [SerializeField, Min(0)] private int defaultFoodRecovery = 25;

    [Header("Events")]
    public UnityEvent<int> OnHungerChanged;
    public UnityEvent<HungerState> OnHungerStateChanged;
    public UnityEvent OnHungerDepleted;

    public event Action<int> HungerChanged;
    public event Action<HungerState> HungerStateChanged;
    public event Action HungerDepleted;

    private int currentHunger;
    private HungerState currentState = HungerState.Full;
    private bool isInitialized;
    private bool isDepleted;
    private float pendingDrain;

    public int CurrentHunger => currentHunger;
    public float HungerRatio => maxHunger <= 0 ? 0f : (float)currentHunger / maxHunger;
    public HungerState CurrentState => currentState;
    public bool IsDepleted => isDepleted;

    private void Awake()
    {
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

        AdvanceTime(Time.deltaTime / 60f);
    }

    private void OnValidate()
    {
        maxHunger = Mathf.Max(1, maxHunger);
        startingHungerRatio = Mathf.Clamp01(startingHungerRatio);
        hungerDecayPerMinute = Mathf.Max(0f, hungerDecayPerMinute);
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
        pendingDrain = 0f;

        int startingHunger = Mathf.RoundToInt(maxHunger * startingHungerRatio);
        SetHungerInternal(startingHunger, true);
    }

    public void AdvanceTime(float minutes)
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

        pendingDrain += minutes * hungerDecayPerMinute;

        int drainAmount = Mathf.FloorToInt(pendingDrain);

        if (drainAmount <= 0)
        {
            return;
        }

        pendingDrain -= drainAmount;
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

    public void SetHunger(int value)
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

    private bool SetHungerInternal(int value, bool forceNotify)
    {
        int nextHunger = Mathf.Clamp(value, 0, maxHunger);
        HungerState nextState = GetStateFromRatio((float)nextHunger / maxHunger);

        bool valueChanged = currentHunger != nextHunger;
        bool stateChanged = currentState != nextState;

        currentHunger = nextHunger;
        currentState = nextState;

        if (forceNotify || valueChanged)
        {
            OnHungerChanged?.Invoke(currentHunger);
            HungerChanged?.Invoke(currentHunger);
        }

        if (forceNotify || stateChanged)
        {
            OnHungerStateChanged?.Invoke(currentState);
            HungerStateChanged?.Invoke(currentState);
        }

        if (currentHunger <= 0 && !isDepleted)
        {
            isDepleted = true;
            OnHungerDepleted?.Invoke();
            HungerDepleted?.Invoke();
        }
        else if (currentHunger > 0)
        {
            isDepleted = false;
        }

        return valueChanged || stateChanged;
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
