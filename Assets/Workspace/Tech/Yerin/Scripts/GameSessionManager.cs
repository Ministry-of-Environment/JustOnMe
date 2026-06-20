using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "House";

    [Header("Game Length")]
    [SerializeField, Min(1)] private int totalWeeks = 16;

    [Header("Difficulty")]
    [SerializeField] private DifficultyType selectedDifficulty = DifficultyType.Normal;

    private int currentWeek = 1;
    private bool isGameStarted;
    private bool isGameFinished;

    public int CurrentWeek => currentWeek;
    public int TotalWeeks => totalWeeks;
    public DifficultyType SelectedDifficulty => selectedDifficulty;
    public bool IsGameStarted => isGameStarted;
    public bool IsGameFinished => isGameFinished;

    public float HungerMultiplier
    {
        get
        {
            switch (selectedDifficulty)
            {
                case DifficultyType.Easy:
                    return 0.5f;

                case DifficultyType.Hard:
                    return 1.5f;

                case DifficultyType.Normal:
                default:
                    return 1f;
            }
        }
    }

    public float PollutionMultiplier
    {
        get
        {
            switch (selectedDifficulty)
            {
                case DifficultyType.Easy:
                    return 0.5f;

                case DifficultyType.Hard:
                    return 1.5f;

                case DifficultyType.Normal:
                default:
                    return 1f;
            }
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectEasy() => selectedDifficulty = DifficultyType.Easy;

    public void SelectNormal() => selectedDifficulty = DifficultyType.Normal;

    public void SelectHard() => selectedDifficulty = DifficultyType.Hard;

    public void StartNewGame()
    {
        currentWeek = 1;
        isGameStarted = true;
        isGameFinished = false;

        Time.timeScale = 1f;

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void NotifyWeekFinished()
    {
        if (isGameFinished)
        {
            return;
        }

        if (currentWeek >= totalWeeks)
        {
            FinishGame();
            return;
        }

        currentWeek++;

        if (GamePeriodCycle.Instance != null)
        {
            GamePeriodCycle.Instance.StartNewWeek();
        }
    }

    private void FinishGame()
    {
        isGameFinished = true;

        PollutionLevel pollutionLevel = PollutionLevel.Level1;

        if (PollutionSystem.Instance != null)
        {
            pollutionLevel = PollutionSystem.Instance.CurrentPollutionLevel;
        }

        if (GameEndingManager.Instance != null)
        {
            GameEndingManager.Instance.CheckFinalPollutionEnding(pollutionLevel);
        }
    }
}

public enum DifficultyType
{
    Easy,
    Normal,
    Hard
}