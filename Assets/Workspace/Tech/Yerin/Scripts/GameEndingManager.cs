using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndingManager : MonoBehaviour
{
    public static GameEndingManager Instance { get; private set; }

    [Header("Ending Data")]
    [SerializeField] private List<EndingData> endings = new List<EndingData>();

    [Header("References")]
    [SerializeField] private EndingUI endingUI;

    [Header("Scene")]
    [SerializeField] private bool pauseGameOnEnding = true;
    [SerializeField] private bool destroyDontDestroyOnLoadObjectsOnTitle = true;

    private bool isEnded;

    public bool IsEnded => isEnded;

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

    public void SetEndingUI(EndingUI endingUI)
    {
        this.endingUI = endingUI;
    }

    public void TriggerEnding(EndingType endingType)
    {
        if (isEnded)
        {
            return;
        }

        isEnded = true;

        if (pauseGameOnEnding)
        {
            Time.timeScale = 0f;
        }

        EndingData endingData = GetEndingData(endingType);

        if (endingData == null)
        {
            Debug.LogWarning($"EndingData를 찾을 수 없습니다: {endingType}");
            return;
        }

        if (endingUI != null)
        {
            endingUI.ShowEnding(endingData);
        }
        else
        {
            Debug.LogWarning("EndingUI가 할당되지 않았습니다.");
        }

        Debug.Log($"Ending Triggered: {endingType}");
    }

    public void CheckStarvation(int currentHunger)
    {
        if (isEnded)
        {
            return;
        }

        if (currentHunger <= 0)
        {
            TriggerEnding(EndingType.Starvation);
        }
    }

    public void CheckRentFailed(bool rentPaid)
    {
        if (isEnded)
        {
            return;
        }

        if (!rentPaid)
        {
            TriggerEnding(EndingType.Eviction);
        }
    }

    public void CheckFinalPollutionEnding(PollutionLevel pollutionLevel)
    {
        if (isEnded)
        {
            return;
        }

        switch (pollutionLevel)
        {
            case PollutionLevel.Level1:
                TriggerEnding(EndingType.GoodLife);
                break;

            case PollutionLevel.Level2:
                TriggerEnding(EndingType.SickDeath);
                break;

            case PollutionLevel.Level3:
            default:
                TriggerEnding(EndingType.WorldCollapse);
                break;
        }
    }

    public void GoToTitle(string titleSceneName)
    {
        Time.timeScale = 1f;
        isEnded = false;

        if (destroyDontDestroyOnLoadObjectsOnTitle)
        {
            DestroyAllDontDestroyOnLoadObjects();
        }

        SceneManager.LoadScene(titleSceneName);
    }

    private EndingData GetEndingData(EndingType endingType)
    {
        foreach (EndingData ending in endings)
        {
            if (ending != null && ending.EndingType == endingType)
            {
                return ending;
            }
        }

        return null;
    }

    private void DestroyAllDontDestroyOnLoadObjects()
    {
        MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        HashSet<GameObject> rootObjects = new HashSet<GameObject>();

        foreach (MonoBehaviour behaviour in allBehaviours)
        {
            if (behaviour == null)
            {
                continue;
            }

            GameObject rootObject = behaviour.transform.root.gameObject;

            if (rootObject.scene.name == "DontDestroyOnLoad")
            {
                rootObjects.Add(rootObject);
            }
        }

        foreach (GameObject rootObject in rootObjects)
        {
            Destroy(rootObject);
        }
    }
}

public enum EndingType
{
    GoodLife,
    SickDeath,
    WorldCollapse,
    Starvation,
    Eviction
}