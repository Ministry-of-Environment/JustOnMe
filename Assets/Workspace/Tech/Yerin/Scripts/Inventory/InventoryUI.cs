using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ItemDropper itemDropper;

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private InvenSlotUI[] slotUIs;
    [SerializeField] private Button discardButton;
    [SerializeField] private TMP_Text selectedItemText;
    [SerializeField] private TMP_Text messageText;

    [Header("Messages")]
    [SerializeField] private string selectEmptySlotMessage = "This slot is empty.";
    [SerializeField] private string selectItemMessageFormat = "Selected: {0}";
    [SerializeField] private string noSelectedItemMessage = "Select an item first.";
    [SerializeField] private string cannotDiscardMessage = "You can't discard this item here.";
    [SerializeField] private string discardFishMessage = "Released the fish.";
    [SerializeField] private string discardTrashMessage = "Discarded trash.";

    private Inventory inventory;
    private int selectedIndex = -1;

    public bool IsOpen => panel != null && panel.activeSelf;

    public event Action OnInventoryClosed;
    public event Action OnItemDiscarded;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        InitializeSlots();

        if (discardButton != null)
        {
            discardButton.onClick.RemoveListener(DiscardSelectedItem);
            discardButton.onClick.AddListener(DiscardSelectedItem);
        }

        ClearMessage();
        UpdateSelectedItemText();
    }

    private void Start()
    {
        BindInventoryFromManager();
        Refresh();
    }

    private void OnEnable()
    {
        BindInventoryFromManager();
    }

    private void OnDisable()
    {
        UnbindInventory();
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null)
            {
                continue;
            }

            slotUIs[i].Initialize(i, SelectSlot);
        }
    }

    private void BindInventoryFromManager()
    {
        if (inventory != null)
        {
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager가 없습니다.");
            return;
        }

        inventory = InventoryManager.Instance.Inventory;

        if (inventory == null)
        {
            Debug.LogWarning("InventoryManager에 Inventory가 연결되어 있지 않습니다.");
            return;
        }

        inventory.EnsureInitialized();
        inventory.OnInventoryChanged -= Refresh;
        inventory.OnInventoryChanged += Refresh;
    }

    private void UnbindInventory()
    {
        if (inventory == null)
        {
            return;
        }

        inventory.OnInventoryChanged -= Refresh;
        inventory = null;
    }

    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        BindInventoryFromManager();

        if (panel != null)
        {
            panel.SetActive(true);
        }

        selectedIndex = -1;
        ClearMessage();
        Refresh();
    }

    public void Close()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        selectedIndex = -1;
        ClearMessage();
        Refresh();

        OnInventoryClosed?.Invoke();
    }

    private void SelectSlot(int index)
    {
        selectedIndex = index;
        Refresh();

        if (inventory == null)
        {
            return;
        }

        ItemData selectedItem = inventory.GetItem(selectedIndex);

        if (selectedItem == null)
        {
            ShowMessage(selectEmptySlotMessage);
            return;
        }

        ShowMessage(string.Format(selectItemMessageFormat, selectedItem.ItemName));
    }

    private void DiscardSelectedItem()
    {
        BindInventoryFromManager();

        if (inventory == null)
        {
            Debug.LogWarning("Inventory가 연결되어 있지 않습니다.");
            return;
        }

        if (itemDropper == null)
        {
            Debug.LogWarning("ItemDropper가 연결되어 있지 않습니다.");
            return;
        }

        if (selectedIndex == -1)
        {
            ShowMessage(noSelectedItemMessage);
            return;
        }

        ItemData selectedItem = inventory.GetItem(selectedIndex);

        if (selectedItem == null)
        {
            ShowMessage(noSelectedItemMessage);
            return;
        }

        if (!itemDropper.CanDropItem(selectedItem))
        {
            ShowMessage(cannotDiscardMessage);
            Debug.Log($"{selectedItem.ItemName}은 현재 버릴 수 없습니다.");
            return;
        }

        bool dropped = itemDropper.TryDropItem(selectedItem);

        if (!dropped)
        {
            ShowMessage(cannotDiscardMessage);
            return;
        }

        inventory.RemoveItem(selectedIndex);

        ShowDiscardMessage(selectedItem);

        selectedIndex = -1;
        Refresh();

        OnItemDiscarded?.Invoke();
    }

    public void DropSelectedItemOutsideInventory()
    {
        // 지금은 버튼 방식이므로 이 함수는 외부에서 직접 연결하지 않아도 됨.
        // 혹시 나중에 드래그 앤 드롭으로 인벤토리 밖에 버릴 때 재사용 가능.
        DiscardSelectedItem();
    }

    private void ShowDiscardMessage(ItemData itemData)
    {
        if (itemData is FishData)
        {
            ShowMessage(discardFishMessage);
            return;
        }

        if (itemData is TrashData)
        {
            ShowMessage(discardTrashMessage);
            return;
        }

        ClearMessage();
    }

    private void Refresh()
    {
        if (inventory == null)
        {
            ClearSlots();
            UpdateSelectedItemText();
            return;
        }

        InvenSlot[] slots = inventory.Slots;

        if (slots == null)
        {
            ClearSlots();
            UpdateSelectedItemText();
            return;
        }

        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null)
            {
                continue;
            }

            ItemData itemData = null;

            if (i < slots.Length && !slots[i].IsEmpty)
            {
                itemData = slots[i].ItemData;
            }

            slotUIs[i].SetItem(itemData);
            slotUIs[i].SetSelected(i == selectedIndex);
        }

        UpdateSelectedItemText();
    }

    private void ClearSlots()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null)
            {
                continue;
            }

            slotUIs[i].Clear();
            slotUIs[i].SetSelected(false);
        }
    }

    private void UpdateSelectedItemText()
    {
        if (selectedItemText == null)
        {
            return;
        }

        if (inventory == null || selectedIndex == -1)
        {
            selectedItemText.text = "";
            return;
        }

        ItemData selectedItem = inventory.GetItem(selectedIndex);

        selectedItemText.text = selectedItem != null
            ? selectedItem.ItemName
            : "";
    }

    private void ShowMessage(string message)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = message;
    }

    private void ClearMessage()
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = "";
    }
}