using UnityEngine;

public class RentSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GamePeriodCycle periodCycle;

    [Header("Rent")]
    [SerializeField] private int rentAmount = 1000;

    public int RentAmount => rentAmount;

    private void Start()
    {
        if (periodCycle == null)
        {
            periodCycle = FindFirstObjectByType<GamePeriodCycle>();
        }
    }

    public bool IsRentDay()
    {
        if (periodCycle == null)
        {
            return false;
        }

        return periodCycle.PeriodCount % 4 == 0;
    }

   
}