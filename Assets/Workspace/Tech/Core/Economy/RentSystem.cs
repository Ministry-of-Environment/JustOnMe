using UnityEngine;

public class RentSystem : MonoBehaviour
{
    public static RentSystem Instance { get; private set; }

    [Header("Rent")]
    [SerializeField] private int rentAmount = 1000;

    public int RentAmount => rentAmount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsRentDay()
    {
        if (GamePeriodCycle.Instance == null)
        {
            return false;
        }

        return GamePeriodCycle.Instance.PeriodCount % 4 == 0;
    }  
}