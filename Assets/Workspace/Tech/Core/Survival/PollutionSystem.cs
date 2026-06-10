using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PollutionSystem : MonoBehaviour
{
    [Header("Pollution Settings")]
    [SerializeField, Min(1)] private int maxPollution = 100;
    [SerializeField, Min(0)] private int startingPollution = 0;

    [Header("Pollution Thresholds")]
    [SerializeField, Range(0f, 1f)] private float level1Threshold = 0.34f;
    [SerializeField, Range(0f, 1f)] private float level2Threshold = 0.67f;

    [Header("Action Values")]
    [SerializeField, Min(0)] private int illegalDumpPollution = 15;

    [Header("Scene Exceptions")]
    [SerializeField] private List<string> noPollutionIncreaseSceneNames = new();

    [Header("Auto Initialize")]
    [SerializeField] private bool initializeOnAwake = true;

    [Header("Events")]
    public UnityEvent<int> OnPollutionChanged;
    public UnityEvent<PollutionLevel> OnPollutionLevelChanged;
    public UnityEvent OnPollutionMaxed;

    public event Action<int> PollutionChanged;
    public event Action<PollutionLevel> PollutionLevelChanged;
    public event Action PollutionMaxed;

    private int currentPollution;
    private PollutionLevel currentPollutionLevel = PollutionLevel.Level1;
    private bool isInitialized;
    private bool maxEventRaised;

    public int CurrentPollution => currentPollution;
    public float PollutionRatio => maxPollution <= 0 ? 0f : (float)currentPollution / maxPollution;
    public PollutionLevel CurrentPollutionLevel => currentPollutionLevel;
    public bool IsMaxed => currentPollution >= maxPollution;

    private void Awake()
    {
        if (initializeOnAwake)
        {
            Initialize();
        }
    }

    private void OnValidate()
    {
        maxPollution = Mathf.Max(1, maxPollution);
        startingPollution = Mathf.Clamp(startingPollution, 0, maxPollution);
        illegalDumpPollution = Mathf.Max(0, illegalDumpPollution);

        level1Threshold = Mathf.Clamp01(level1Threshold);
        level2Threshold = Mathf.Clamp01(level2Threshold);

        if (level2Threshold < level1Threshold)
        {
            level2Threshold = level1Threshold;
        }
    }

    public void Initialize()
    {
        isInitialized = true;
        maxEventRaised = false;

        SetPollutionInternal(startingPollution, true);
    }

    public bool AddIllegalDump()
    {
        if (IsIllegalDumpIgnoredInCurrentScene())
        {
            return false;
        }

        return AddPollution(illegalDumpPollution);
    }

    public bool AddPollution(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        EnsureInitialized();
        return SetPollutionInternal(currentPollution + amount, false);
    }

    public void SetPollution(int value)
    {
        EnsureInitialized();
        SetPollutionInternal(value, false);
    }

    public PollutionLevel GetPollutionLevelFromRatio(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);

        if (ratio < level1Threshold)
        {
            return PollutionLevel.Level1;
        }

        if (ratio < level2Threshold)
        {
            return PollutionLevel.Level2;
        }

        return PollutionLevel.Level3;
    }

    private void EnsureInitialized()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    private bool IsIllegalDumpIgnoredInCurrentScene()
    {
        if (noPollutionIncreaseSceneNames == null || noPollutionIncreaseSceneNames.Count == 0)
        {
            return false;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        for (int i = 0; i < noPollutionIncreaseSceneNames.Count; i++)
        {
            string sceneName = noPollutionIncreaseSceneNames[i];

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                continue;
            }

            if (string.Equals(sceneName.Trim(), currentSceneName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private bool SetPollutionInternal(int value, bool forceNotify)
    {
        int nextPollution = Mathf.Clamp(value, 0, maxPollution);
        PollutionLevel nextLevel = GetPollutionLevelFromRatio((float)nextPollution / maxPollution);

        bool valueChanged = currentPollution != nextPollution;
        bool levelChanged = currentPollutionLevel != nextLevel;

        currentPollution = nextPollution;
        currentPollutionLevel = nextLevel;

        if (forceNotify || valueChanged)
        {
            OnPollutionChanged?.Invoke(currentPollution);
            PollutionChanged?.Invoke(currentPollution);
        }

        if (forceNotify || levelChanged)
        {
            OnPollutionLevelChanged?.Invoke(currentPollutionLevel);
            PollutionLevelChanged?.Invoke(currentPollutionLevel);
        }

        if (IsMaxed && !maxEventRaised)
        {
            maxEventRaised = true;
            OnPollutionMaxed?.Invoke();
            PollutionMaxed?.Invoke();
        }
        else if (!IsMaxed)
        {
            maxEventRaised = false;
        }

        return valueChanged || levelChanged;
    }
}
