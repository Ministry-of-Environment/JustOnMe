using System;
using UnityEngine;
using UnityEngine.UI;

public class InvenSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject selectedFrame;

    private int index;
    private Action<int> onClicked;

    public void Initialize(int slotIndex, Action<int> clickCallback)
    {
        index = slotIndex;
        onClicked = clickCallback;

        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }

        SetSelected(false);
        Clear();
    }

    public void SetItem(ItemData itemData)
    {
        if (itemData == null)
        {
            Clear();
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = itemData.Icon;
            iconImage.gameObject.SetActive(true);
        }
    }

    public void Clear()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectedFrame != null)
        {
            selectedFrame.SetActive(isSelected);
        }
    }

    private void OnClick()
    {
        // 여기서는 버리지 않음.
        // InventoryUI에게 "이 슬롯을 선택했다"만 알려줌.
        onClicked?.Invoke(index);
    }
}
