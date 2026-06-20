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
    [SerializeField] private TMP_Text discardButtonText;
    [SerializeField] private TMP_Text selectedItemText;
    [SerializeField] private TMP_Text messageText;

    [Header("Messages")]
    [SerializeField] private string selectEmptySlotMessage = "This slot is empty.";
    [SerializeField] private string selectItemMessageFormat = "Selected: {0}";
    [SerializeField] private string noSelectedItemMessage = "Select an item first.";
    [SerializeField] private string cannotDiscardMessage = "You can't discard this item here.";
    [SerializeField] private string discardFishMessage = "Released the fish.";
    [SerializeField] private string discardTrashMessage = "Dropped trash.";
    [SerializeField] private string cannotDisposeHereMessage = "You can only dispose of trash here.";
    [SerializeField] private string disposeTrashMessageFormat = "Disposed {0}.";
    [SerializeField] private string selectTrashForDisposeMessageFormat = "Dispose: {0}";
    [SerializeField] private string sellFishMessageFormat = "Sell: {0} / {1}";
    [SerializeField] private string soldFishMessageFormat = "Sold {0} +{1}";

    private Inventory inventory;
    private int selectedIndex = -1;
    private InventoryMode currentMode = InventoryMode.Normal;

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
            Debug.LogWarning("InventoryManager does not exist.");
            return;
        }

        inventory = InventoryManager.Instance.Inventory;

        if (inventory == null)
        {
            Debug.LogWarning("InventoryManager has no Inventory assigned.");
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
            Open(InventoryMode.Normal);
        }
    }

    public void Open()
    {
        Open(InventoryMode.Normal);
    }

    public void Open(InventoryMode mode)
    {
        BindInventoryFromManager();

        currentMode = mode;

        if (panel != null)
        {
            panel.SetActive(true);
        }

        selectedIndex = -1;
        ClearMessage();
        Refresh();
        UpdateDiscardButtonText();
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

        if (currentMode == InventoryMode.Sell)
        {
            ShowSellModeMessage(selectedItem);
            return;
        }

        if (currentMode == InventoryMode.TrashYard)
        {
            ShowTrashYardModeMessage(selectedItem);
            return;
        }

        ShowMessage(string.Format(selectItemMessageFormat, selectedItem.ItemName));
    }

    private void ShowSellModeMessage(ItemData selectedItem)
    {
        if (selectedItem is FishData fish)
        {
            ShowMessage(string.Format(sellFishMessageFormat, fish.ItemName, fish.Price));
            return;
        }

        ShowMessage(cannotDiscardMessage);
    }

    private void ShowTrashYardModeMessage(ItemData selectedItem)
    {
        if (selectedItem.ItemType == ItemType.Trash)
        {
            ShowMessage(string.Format(selectTrashForDisposeMessageFormat, selectedItem.ItemName));
            return;
        }

        ShowMessage(cannotDisposeHereMessage);
    }

    private void DiscardSelectedItem()
    {
        if (currentMode == InventoryMode.Sell)
        {
            SellSelectedItem();
            return;
        }

        if (currentMode == InventoryMode.TrashYard)
        {
            DisposeSelectedTrash();
            return;
        }

        DiscardSelectedItemNormalMode();
    }

    private void DiscardSelectedItemNormalMode()
    {
        BindInventoryFromManager();

        if (inventory == null)
        {
            Debug.LogWarning("Inventory is not connected.");
            return;
        }

        if (itemDropper == null)
        {
            Debug.LogWarning("ItemDropper is not connected.");
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
            Debug.Log($"{selectedItem.ItemName} cannot be discarded here.");
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

    private void SellSelectedItem()
    {
        BindInventoryFromManager();

        if (inventory == null)
        {
            return;
        }

        if (selectedIndex == -1)
        {
            ShowMessage(noSelectedItemMessage);
            return;
        }

        ItemData item = inventory.GetItem(selectedIndex);

        if (item is not FishData fish)
        {
            ShowMessage(cannotDiscardMessage);
            return;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("CurrencyManager does not exist.");
            return;
        }

        CurrencyManager.Instance.AddMoney(fish.Price);

        inventory.RemoveItem(selectedIndex);

        ShowMessage(string.Format(soldFishMessageFormat, fish.ItemName, fish.Price));

        selectedIndex = -1;
        Refresh();

        OnItemDiscarded?.Invoke();
    }

    private void DisposeSelectedTrash()
    {
        BindInventoryFromManager();

        if (inventory == null)
        {
            return;
        }

        if (selectedIndex == -1)
        {
            ShowMessage(noSelectedItemMessage);
            return;
        }

        ItemData item = inventory.GetItem(selectedIndex);

        if (item == null)
        {
            ShowMessage(noSelectedItemMessage);
            return;
        }

        if (item.ItemType != ItemType.Trash)
        {
            ShowMessage(cannotDisposeHereMessage);
            return;
        }

        inventory.RemoveItem(selectedIndex);

        ShowMessage(string.Format(disposeTrashMessageFormat, item.ItemName));

        selectedIndex = -1;
        Refresh();

        OnItemDiscarded?.Invoke();
    }

    public void DropSelectedItemOutsideInventory()
    {
        DiscardSelectedItem();
    }

    private void ShowDiscardMessage(ItemData itemData)
    {
        if (itemData == null)
        {
            ClearMessage();
            return;
        }

        if (itemData.ItemType == ItemType.Fish)
        {
            ShowMessage(discardFishMessage);
            return;
        }

        if (itemData.ItemType == ItemType.Trash)
        {
            ShowMessage(discardTrashMessage);
            return;
        }

        ClearMessage();
    }

    private void UpdateDiscardButtonText()
    {
        if (discardButtonText == null)
        {
            return;
        }

        discardButtonText.text = currentMode switch
        {
            InventoryMode.Sell => "Sell",
            InventoryMode.TrashYard => "Dispose",
            _ => "Discard"
        };
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

    public enum InventoryMode
    {
        Normal,
        Sell,
        TrashYard
    }
}