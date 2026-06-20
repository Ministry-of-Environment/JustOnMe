using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingSceneEntrance : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName;

    [Header("UI")]
    [SerializeField] private GameObject interactionText;

    [Header("Layer Settings")]
    [SerializeField] private LayerMask playerLayer;

    private bool isPlayerInRange;

    private void Start()
    {
        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsInLayerMask(other.gameObject.layer, playerLayer))
        {
            Debug.Log("플레이어 아님!");
            return;
        }

        Debug.Log("플레이어 맞음!");
        isPlayerInRange = true;

        PlayerInteraction.SetCurrentEntrance(this);

        if (interactionText != null)
        {
            interactionText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsInLayerMask(other.gameObject.layer, playerLayer))
        {
            return;
        }

        isPlayerInRange = false;

        PlayerInteraction.ClearCurrentEntrance(this);

        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }

    public void Enter()
    {
        if (!isPlayerInRange)
        {
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("Target Scene Name is empty.");
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}