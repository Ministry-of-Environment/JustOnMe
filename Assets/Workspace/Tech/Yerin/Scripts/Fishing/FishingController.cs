using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingController : MonoBehaviour
{
    [Header("Fish")]
    [SerializeField] private FishingTable fishTable;
    [SerializeField] private PollutionLevel currentPollutionLevel = PollutionLevel.Level1;

    [Header("Mini Game")]
    [SerializeField] private FishingMiniGameUI miniGameUI;
    [SerializeField] private List<FishRarityDifficulty> difficulties = new();

    [Header("Inventory UI")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Trash Drop")]
    [SerializeField] private GameObject trashDropPrefab;
    [SerializeField] private float trashDropRadius = 1.2f;

    [Header("Guide UI")]
    [SerializeField]private TMP_Text guideText;
    [SerializeField]private string readyGuideMessage = "F : Try fishing";
    [SerializeField]private string miniGameGuideMessage = "Space : Timing";
    [SerializeField]private string fishingSuccessMessage = "Success!";
    [SerializeField]private string fishAddedMessage = "Fish added to inventory.";
    [SerializeField]private string trashAddedMessage = "Trash added to inventory.";
    [SerializeField]private string inventoryFullMessage = "Inventory full. Press I to make space.";
    [SerializeField]private string fishReleasedMessage = "Fish released.";
    [SerializeField]private string trashDroppedMessage = "Trash dropped nearby.";

    private Inventory inventory;
    private bool isFishing;
    private ItemData pendingItem;

    private void Start()
    {
        BindInventory();
        BindInventoryUI();

        SetGuideText(readyGuideMessage);
    }

    private void OnDestroy()
    {
        UnbindInventoryUI();
    }

    private void OnFishing(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        TryStartFishing();
    }

    private void OnFishingConfirm(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (!isFishing)
        {
            return;
        }

        if (miniGameUI == null)
        {
            return;
        }

        miniGameUI.TryConfirm();
    }

    private void BindInventory()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager가 없습니다. 씬에 InventoryManager 오브젝트가 있는지 확인하세요.");
            return;
        }

        inventory = InventoryManager.Instance.Inventory;

        if (inventory == null)
        {
            Debug.LogError("InventoryManager에 Inventory가 연결되어 있지 않습니다.");
            return;
        }

        inventory.EnsureInitialized();
    }

    private void BindInventoryUI()
    {
        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
        }

        if (inventoryUI == null)
        {
            Debug.LogWarning("FishingController가 InventoryUI를 찾지 못했습니다.");
            return;
        }

        inventoryUI.OnInventoryClosed -= ReleasePendingItem;
        inventoryUI.OnItemDiscarded -= TryAddPendingItemToInventory;

        inventoryUI.OnInventoryClosed += ReleasePendingItem;
        inventoryUI.OnItemDiscarded += TryAddPendingItemToInventory;
    }

    private void UnbindInventoryUI()
    {
        if (inventoryUI == null)
        {
            return;
        }

        inventoryUI.OnInventoryClosed -= ReleasePendingItem;
        inventoryUI.OnItemDiscarded -= TryAddPendingItemToInventory;
    }

    private void TryStartFishing()
    {
        if (isFishing)
        {
            return;
        }

        if (pendingItem != null)
        {
            SetGuideText(inventoryFullMessage);
            return;
        }

        if (fishTable == null)
        {
            Debug.LogError("FishingTable이 없습니다.");
            return;
        }

        if (miniGameUI == null)
        {
            Debug.LogError("FishingMiniGameUI가 없습니다.");
            return;
        }

        ItemData selectedItem = fishTable.GetRandomFishingItem(GetCurrentPollutionLevel());

        if (selectedItem == null)
        {
            Debug.LogWarning("현재 오염도에서 잡을 수 있는 아이템이 없습니다.");
            return;
        }

        FishRarityDifficulty difficulty = GetDifficultyByItem(selectedItem);

        if (difficulty == null)
        {
            Debug.LogError($"{selectedItem.ItemName}에 사용할 낚시 난이도가 없습니다.");
            return;
        }

        isFishing = true;
        SetGuideText(miniGameGuideMessage);

        if (selectedItem.ItemType == ItemType.Trash)
        {
            Debug.Log($"쓰레기가 걸렸습니다: {selectedItem.ItemName}");
        }
        else if (selectedItem.ItemType == ItemType.Fish)
        {
            FishData selectedFish = selectedItem as FishData;

            if (selectedFish != null)
            {
                Debug.Log($"물고기가 걸렸습니다: {selectedFish.ItemName} / 희귀도: {selectedFish.Rarity}");
            }
        }

        miniGameUI.StartMiniGame(
            selectedItem,
            difficulty,
            OnFishingSuccess,
            OnFishingFail
        );
    }

    private FishRarityDifficulty GetDifficultyByItem(ItemData item)
    {
        if (item.ItemType == ItemType.Trash)
        {
            return GetDifficulty(FishRarity.Uncommon);
        }

        FishData fish = item as FishData;

        if (fish == null)
        {
            return null;
        }

        return GetDifficulty(fish.Rarity);
    }

    private FishRarityDifficulty GetDifficulty(FishRarity rarity)
    {
        foreach (FishRarityDifficulty difficulty in difficulties)
        {
            if (difficulty.Rarity == rarity)
            {
                return difficulty;
            }
        }

        return null;
    }

    private PollutionLevel GetCurrentPollutionLevel()
    {
        if (PollutionSystem.Instance != null)
        {
            return PollutionSystem.Instance.CurrentPollutionLevel;
        }

        return currentPollutionLevel;
    }

    private void OnFishingSuccess(ItemData caughtItem)
    {
        isFishing = false;

        Debug.Log($"낚시 성공: {caughtItem.ItemName}");

        SetGuideText(fishingSuccessMessage);

        if (inventory == null)
        {
            BindInventory();
        }

        if (inventory == null)
        {
            return;
        }

        bool added = inventory.TryAddItem(caughtItem);

        if (added)
        {
            if (caughtItem.ItemType == ItemType.Trash)
            {
                SetGuideText(trashAddedMessage);
                Debug.Log($"인벤토리에 쓰레기 추가: {caughtItem.ItemName}");
            }
            else
            {
                SetGuideText(fishAddedMessage);
                Debug.Log($"인벤토리에 물고기 추가: {caughtItem.ItemName}");
            }

            return;
        }

        pendingItem = caughtItem;

        SetGuideText(inventoryFullMessage);
        Debug.Log($"인벤토리가 가득 차서 임시 대기: {caughtItem.ItemName}");
    }

    private void OnFishingFail(ItemData caughtItem)
    {
        isFishing = false;

        SetGuideText(readyGuideMessage);

        if (caughtItem.ItemType == ItemType.Trash)
        {
            Debug.Log($"쓰레기를 건져 올리지 못했습니다: {caughtItem.ItemName}");
        }
        else
        {
            Debug.Log($"물고기를 놓쳤습니다: {caughtItem.ItemName}");
        }
    }

    private void TryAddPendingItemToInventory()
    {
        if (pendingItem == null)
        {
            return;
        }

        if (inventory == null)
        {
            BindInventory();
        }

        if (inventory == null)
        {
            return;
        }

        bool added = inventory.TryAddItem(pendingItem);

        if (!added)
        {
            SetGuideText(inventoryFullMessage);
            return;
        }

        Debug.Log($"대기 중이던 아이템 인벤토리 추가: {pendingItem.ItemName}");

        if (pendingItem.ItemType == ItemType.Trash)
        {
            SetGuideText(trashAddedMessage);
        }
        else
        {
            SetGuideText(fishAddedMessage);
        }

        pendingItem = null;
    }

    private void ReleasePendingItem()
    {
        if (pendingItem == null)
        {
            return;
        }

        if (pendingItem.ItemType == ItemType.Trash)
        {
            SpawnTrashAroundPlayer();
            Debug.Log($"인벤토리를 닫아 쓰레기를 주변에 버림: {pendingItem.ItemName}");

            pendingItem = null;
            SetGuideText(trashDroppedMessage);
            return;
        }

        if (pendingItem.ItemType == ItemType.Fish)
        {
            Debug.Log($"인벤토리를 닫아 물고기를 놓아줌: {pendingItem.ItemName}");

            pendingItem = null;
            SetGuideText(fishReleasedMessage);
            return;
        }

        pendingItem = null;
        SetGuideText(readyGuideMessage);
    }

    private void SpawnTrashAroundPlayer()
    {
        if (trashDropPrefab == null)
        {
            Debug.LogWarning("trashDropPrefab이 설정되어 있지 않아 쓰레기를 생성할 수 없습니다.");
            return;
        }

        Vector2 randomOffset = Random.insideUnitCircle.normalized * trashDropRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        Instantiate(trashDropPrefab, spawnPosition, Quaternion.identity);
    }

    private void SetGuideText(string message)
    {
        if (guideText == null)
        {
            return;
        }

        guideText.text = message;
    }
}