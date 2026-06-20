using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]private static BuildingSceneEntrance currentEntrance;
    [SerializeField] private string targetSceneName;
    [SerializeField] bool isInTrigger = false;

    private void Awake()
    {
        currentEntrance = null;
    }

    private void OnInteract(InputValue value)
    {
        Debug.Log("OnInteract called");

        if (currentEntrance == null)
        {
            if (isInTrigger == true)
            {
                SceneManager.LoadScene(targetSceneName);
                return;
            }
            return;
        }
        else
        {
            Debug.Log($"Current entrance: {currentEntrance.name}");
        }

        currentEntrance.Enter();
    }

    public static void SetCurrentEntrance(BuildingSceneEntrance entrance)
    {
        currentEntrance = entrance;
        Debug.Log($"Current entrance set to: {currentEntrance.name}");
    }

    public static void ClearCurrentEntrance(BuildingSceneEntrance entrance)
    {
        if (currentEntrance == entrance)
        {
            currentEntrance = null;
        }
    }
}