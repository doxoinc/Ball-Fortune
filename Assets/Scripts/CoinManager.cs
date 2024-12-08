using UnityEngine;
using System;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    // Событие для уведомления об изменении баланса монет
    public event Action<int> OnCoinChanged;

    private int totalCoins;

    private void Awake()
    {
        // Реализация Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCoins();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Подписка на событие обновления монет из GameData
    private void Start()
    {
        if (GameData.Instance != null)
        {
            GameData.Instance.OnCoinsUpdated += UpdateCoins;
        }
    }

    private void OnDestroy()
    {
        if (GameData.Instance != null)
        {
            GameData.Instance.OnCoinsUpdated -= UpdateCoins;
        }
    }

    // Метод для добавления монет
    public void AddCoins(int amount)
    {
        GameData.Instance.AddCoins(amount);
        // Событие будет вызвано из GameData
    }

    // Метод для снятия монет
    public bool SpendCoins(int amount)
    {
        return GameData.Instance.SpendCoins(amount);
        // Событие будет вызвано из GameData
    }

    // Получение текущего баланса монет
    public int GetTotalCoins()
    {
        return GameData.Instance.GetTotalCoins();
    }

    // Обработка обновления монет из GameData
    private void UpdateCoins(int newTotal)
    {
        totalCoins = newTotal;
        OnCoinChanged?.Invoke(totalCoins);
        Debug.Log("CoinManager: Баланс монет обновлен до " + totalCoins);
    }

    // Загрузка монет из GameData
    private void LoadCoins()
    {
        if (GameData.Instance != null)
        {
            totalCoins = GameData.Instance.GetTotalCoins();
            Debug.Log("CoinManager: Загружено " + totalCoins + " монет.");
        }
    }
}
