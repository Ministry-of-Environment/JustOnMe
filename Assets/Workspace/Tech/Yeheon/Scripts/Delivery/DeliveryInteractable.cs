using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class DeliveryInteractable : MonoBehaviour
{
    [Header("Delivery Item")]
    [SerializeField] private TrashData deliveryTrash;

    [Header("Delivery Cost")]
    [SerializeField, Min(0)] private int deliveryPrice = 100;

    [Header("Hunger Recovery")]
    [SerializeField, Min(0)] private int deliveryHungerRecovery = 35;

    [Header("Input")]
    [SerializeField] private string playerTag = "Player";

    private bool playerInRange;
    private Inventory inventory;
    private HungerSystem hungerSystem;

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
        {
            return;
        }

        TryDeliverFood();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInRange = false;
    }

    private void TryDeliverFood()
    {
        BindSystems();

        if (deliveryTrash == null)
        {
            Debug.LogWarning("DeliveryInteractable에 배달용 쓰레기 아이템이 연결되어 있지 않습니다.");
            return;
        }

        if (inventory == null)
        {
            Debug.LogWarning("배달 인벤토리를 찾지 못했습니다.");
            return;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("CurrencyManager가 존재하지 않습니다.");
            return;
        }

        if (!CurrencyManager.Instance.TrySpendMoney(deliveryPrice))
        {
            Debug.Log("배달 주문 실패 : 돈 부족");
            return;
        }

        bool added = inventory.TryAddItem(deliveryTrash);

        if (!added)
        {
            Debug.LogWarning("인벤토리가 가득 차서 배달을 받을 수 없습니다.");
            return;
        }

        if (hungerSystem != null && deliveryHungerRecovery > 0)
        {
            hungerSystem.ConsumeFood(deliveryHungerRecovery);
        }

        Debug.Log($"배달 수령 완료: {deliveryTrash.ItemName} ");
    }

    private void BindSystems()
    {
        if (inventory == null && InventoryManager.Instance != null)
        {
            inventory = InventoryManager.Instance.Inventory;
        }

        if (hungerSystem == null)
        {
            hungerSystem = FindFirstObjectByType<HungerSystem>();
        }
    }
}
