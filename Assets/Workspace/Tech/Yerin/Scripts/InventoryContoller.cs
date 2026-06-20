using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryContoller : MonoBehaviour
{
    [Header("Inventory UI")]
    [SerializeField] private InventoryUI inventoryUI;

    private void OnInventory(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }
        Debug.Log("인벤토리 열기/닫기");

        if (inventoryUI == null)
        {
            Debug.LogWarning("InventoryUI를 찾을 수 없습니다.");
            return;
        }

        inventoryUI.Toggle();
    }
}
