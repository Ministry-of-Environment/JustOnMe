using UnityEngine;
using static InventoryUI;
using UnityEngine.InputSystem;

public class FishShopNPC : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryUI inventoryUI;
    private bool playerInRange;

    void Update()
    {
        if (!playerInRange)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (inventoryUI == null)
            {
                Debug.LogWarning("InventoryUI가 인스펙터에 할당되지 않았습니다.");
                return;
            }

            if (inventoryUI.IsOpen)
            {
                inventoryUI.Close();
            }
            else
            {
                inventoryUI.Open(InventoryMode.Sell);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}