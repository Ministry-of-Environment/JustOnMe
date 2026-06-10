using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingController : MonoBehaviour
{
    [Header("Fish")]
    [SerializeField] private FishTable fishTable;
    [SerializeField] private PollutionLevel currentPollutionLevel = PollutionLevel.Level1;
    [SerializeField] private PollutionSystem pollutionSystem;

    [Header("Mini Game")]
    [SerializeField] private FishingMiniGameUI miniGameUI;
    [SerializeField] private List<FishRarityDifficulty> difficulties = new();

    [Header("Inventory UI")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Guide UI")]
    [SerializeField] private TMP_Text guideText;
    [SerializeField] private string readyGuideMessage = "F : Try fishing";
    [SerializeField] private string miniGameGuideMessage = "Space : Timing";
    [SerializeField] private string fishingSuccessMessage = "성공했다!";
    [SerializeField] private string fishAddedMessage = "물고기를 인벤토리에 넣었습니다.";
    [SerializeField] private string inventoryFullMessage = "인벤토리가 꽉 찼습니다. I로 인벤토리를 열어 공간을 비워주세요.";
    [SerializeField] private string fishReleasedMessage = "물고기를 놓아주었습니다.";

    private Inventory inventory;
    private bool isFishing;
    private FishData pendingFish;

    private void Start()
    {
        BindInventory();
        BindPollutionSystem();
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

    private void OnInventory(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (inventoryUI == null)
        {
            Debug.LogWarning("InventoryUI를 찾을 수 없습니다.");
            return;
        }

        inventoryUI.Toggle();
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

        inventoryUI.OnInventoryClosed -= ReleasePendingFish;
        inventoryUI.OnItemDiscarded -= TryAddPendingFishToInventory;

        inventoryUI.OnInventoryClosed += ReleasePendingFish;
        inventoryUI.OnItemDiscarded += TryAddPendingFishToInventory;
    }

    private void BindPollutionSystem()
    {
        if (pollutionSystem != null)
        {
            return;
        }

        pollutionSystem = FindFirstObjectByType<PollutionSystem>();
    }

    private void UnbindInventoryUI()
    {
        if (inventoryUI == null)
        {
            return;
        }

        inventoryUI.OnInventoryClosed -= ReleasePendingFish;
        inventoryUI.OnItemDiscarded -= TryAddPendingFishToInventory;
    }

    private void TryStartFishing()
    {
        if (isFishing)
        {
            return;
        }

        if (pendingFish != null)
        {
            SetGuideText(inventoryFullMessage);
            return;
        }

        if (fishTable == null)
        {
            Debug.LogError("FishTable이 없습니다.");
            return;
        }

        if (miniGameUI == null)
        {
            Debug.LogError("FishingMiniGameUI가 없습니다.");
            return;
        }

        FishData selectedFish = fishTable.GetRandomFish(GetCurrentPollutionLevel());

        if (selectedFish == null)
        {
            Debug.LogWarning("현재 오염도에서 잡을 수 있는 물고기가 없습니다.");
            return;
        }

        FishRarityDifficulty difficulty = GetDifficulty(selectedFish.Rarity);

        if (difficulty == null)
        {
            Debug.LogError($"{selectedFish.Rarity} 희귀도에 해당하는 난이도 데이터가 없습니다.");
            return;
        }

        isFishing = true;
        SetGuideText(miniGameGuideMessage);

        Debug.Log($"물고기가 걸렸습니다: {selectedFish.ItemName} / 희귀도: {selectedFish.Rarity}");

        miniGameUI.StartMiniGame(
            selectedFish,
            difficulty,
            OnFishingSuccess,
            OnFishingFail
        );
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
        if (pollutionSystem != null)
        {
            return pollutionSystem.CurrentPollutionLevel;
        }

        return currentPollutionLevel;
    }

    private void OnFishingSuccess(FishData fish)
    {
        isFishing = false;

        Debug.Log($"물고기 잡기 성공: {fish.ItemName}");

        SetGuideText(fishingSuccessMessage);

        if (inventory == null)
        {
            BindInventory();
        }

        if (inventory == null)
        {
            return;
        }

        bool added = inventory.TryAddItem(fish);

        if (added)
        {
            SetGuideText(fishAddedMessage);
            Debug.Log($"인벤토리에 물고기 추가: {fish.ItemName}");
            return;
        }

        pendingFish = fish;

        SetGuideText(inventoryFullMessage);
        Debug.Log($"인벤토리가 가득 차서 물고기 임시 대기: {fish.ItemName}");
    }

    private void OnFishingFail(FishData fish)
    {
        isFishing = false;

        SetGuideText(readyGuideMessage);

        Debug.Log($"물고기를 놓쳤습니다: {fish.ItemName}");
    }

    private void TryAddPendingFishToInventory()
    {
        if (pendingFish == null)
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

        bool added = inventory.TryAddItem(pendingFish);

        if (!added)
        {
            SetGuideText(inventoryFullMessage);
            return;
        }

        Debug.Log($"대기 중이던 물고기 인벤토리 추가: {pendingFish.ItemName}");

        pendingFish = null;
        SetGuideText(fishAddedMessage);
    }

    private void ReleasePendingFish()
    {
        if (pendingFish == null)
        {
            return;
        }

        Debug.Log($"인벤토리를 닫아 물고기를 놓아줌: {pendingFish.ItemName}");

        pendingFish = null;
        SetGuideText(fishReleasedMessage);
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
