using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Data")]
    [SerializeField] private Inventory inventory;

    public Inventory Inventory => inventory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        if (inventory != null)
        {
            inventory.EnsureInitialized();
        }
    }
}