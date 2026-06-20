using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Money")]
    [SerializeField] private int startMoney = 1000;

    private int currentMoney;

    public int CurrentMoney => currentMoney;

    public event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentMoney = startMoney;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentMoney += amount;

        OnMoneyChanged?.Invoke(currentMoney);

        Debug.Log($"돈 획득 +{amount}G (현재 {currentMoney}G)");
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (currentMoney < amount)
        {
            Debug.Log($"돈 부족 ({currentMoney} / {amount})");
            return false;
        }

        currentMoney -= amount;

        OnMoneyChanged?.Invoke(currentMoney);

        Debug.Log($"돈 사용 -{amount}G (현재 {currentMoney}G)");

        return true;
    }

    public bool CanAfford(int amount)
    {
        return currentMoney >= amount;
    }
}