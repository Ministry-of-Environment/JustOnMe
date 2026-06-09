using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemDropper : MonoBehaviour
{
    [Header("Scene Rules")]
    [SerializeField] private string fishingSceneName = "Fishing";

    [Header("Drop Settings")]
    [SerializeField] private Transform dropOrigin;
    [SerializeField] private float dropRadius = 1.2f;

    [Header("Fish Drop Rule")]
    [SerializeField] private GameObject foodWastePrefab;

    public bool CanDropItem(ItemData itemData)
    {
        if (itemData == null)
        {
            return false;
        }

        if (itemData is FishData)
        {
            return true;
        }

        if (itemData is TrashData trash)
        {
            return trash.DropPrefab != null;
        }

        return false;
    }

    public bool TryDropItem(ItemData itemData)
    {
        if (itemData == null)
        {
            return false;
        }

        if (itemData is FishData fish)
        {
            return TryHandleFishDrop(fish);
        }

        if (itemData is TrashData trash)
        {
            return TryDropTrash(trash);
        }

        Debug.LogWarning($"버리기 처리가 정의되지 않은 아이템입니다: {itemData.ItemName}");
        return false;
    }

    private bool TryHandleFishDrop(FishData fish)
    {
        if (IsFishingScene())
        {
            return TryReleaseFish(fish);
        }

        return TryDropFishAsFoodWaste(fish);
    }

    private bool TryReleaseFish(FishData fish)
    {
        Debug.Log($"물고기를 방생했습니다: {fish.ItemName}");
        return true;
    }

    private bool TryDropFishAsFoodWaste(FishData fish)
    {
        if (foodWastePrefab == null)
        {
            Debug.LogWarning("음식물 쓰레기 프리팹이 연결되어 있지 않습니다.");
            return false;
        }

        SpawnPrefabAroundPlayer(foodWastePrefab);

        Debug.Log($"물고기를 음식물 쓰레기로 버렸습니다: {fish.ItemName}");
        return true;
    }

    private bool TryDropTrash(TrashData trash)
    {
        if (trash.DropPrefab == null)
        {
            Debug.LogWarning($"{trash.ItemName}의 DropPrefab이 없습니다.");
            return false;
        }

        SpawnPrefabAroundPlayer(trash.DropPrefab);

        Debug.Log($"쓰레기를 버렸습니다: {trash.ItemName}");
        return true;
    }

    private void SpawnPrefabAroundPlayer(GameObject prefab)
    {
        Vector3 origin = dropOrigin != null ? dropOrigin.position : transform.position;
        Vector2 randomOffset = Random.insideUnitCircle * dropRadius;

        Vector3 spawnPosition = origin + new Vector3(
            randomOffset.x,
            randomOffset.y,
            0f
        );

        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }

    private bool IsFishingScene()
    {
        return SceneManager.GetActiveScene().name == fishingSceneName;
    }
}