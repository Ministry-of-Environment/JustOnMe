using UnityEngine;
using static InventoryUI;
using UnityEngine.InputSystem;

public class TrashYard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Layer Settings")]
    [SerializeField] private LayerMask playerLayer;

    private bool playerInRange;

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (inventoryUI == null)
            {
                Debug.LogWarning("InventoryUI is not assigned in the Inspector.");
                return;
            }

            if (inventoryUI.IsOpen)
            {
                inventoryUI.Close();
            }
            else
            {
                inventoryUI.Open(InventoryMode.TrashYard);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsInLayerMask(other.gameObject.layer, playerLayer))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsInLayerMask(other.gameObject.layer, playerLayer))
        {
            playerInRange = false;
        }
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}