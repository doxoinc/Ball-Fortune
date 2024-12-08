using UnityEngine;
using System;
using System.Collections.Generic;

public enum RewardState
{
    Locked = 0,
    Available = 1,
    Unlocked = 2
}

[Serializable]
public class PurchasedItem
{
    public string itemName;    // Название предмета
    public int quantity;       // Количество купленных предметов
}

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    public bool SoundEnabled { get; private set; } = true;
    public bool MusicEnabled { get; private set; } = true;

    // Событие для уведомления об обновлении монет
    public event Action<int> OnCoinsUpdated;

    // Событие для уведомления об обновлении состояний наград
    public event Action<int, RewardState> OnRewardStateChanged;

    private int totalCoins;
    private bool isFirstRun;

    // Текущая ставка игрока
    private int currentBet = 1; // Начальное значение ставки

    // Структура для хранения состояния наград по дням
    [Serializable]
    public class DailyReward
    {
        public int dayNumber; // Номер дня (1-7)
        public RewardState state; // Состояние награды
    }

    public List<DailyReward> dailyRewards = new List<DailyReward>();

    [Header("Purchased Items")]
    public List<PurchasedItem> purchasedItems = new List<PurchasedItem>(); // Список купленных предметов

    private void Awake()
    {
        // Реализация Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
            LoadCoins();
            LoadPurchasedItems(); // Загрузка купленных предметов
            LoadFirstRunFlag();
            LoadDailyRewards();
            LoadBet(); // Загрузка текущей ставки
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Настройки Звука и Музыки

    // Установка состояния звука
    public void SetSoundEnabled(bool enabled)
    {
        SoundEnabled = enabled;
        PlayerPrefs.SetInt("SoundEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"GameData: Sound Enabled set to: {enabled}");
    }

    // Установка состояния музыки
    public void SetMusicEnabled(bool enabled)
    {
        MusicEnabled = enabled;
        PlayerPrefs.SetInt("MusicEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"GameData: Music Enabled set to: {enabled}");
    }

    // Загрузка настроек из PlayerPrefs
    private void LoadSettings()
    {
        SoundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        MusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        Debug.Log($"GameData: Loaded Settings - SoundEnabled: {SoundEnabled}, MusicEnabled: {MusicEnabled}");
    }

    #endregion

    #region Управление Монетами

    // Метод для добавления монет
    public void AddCoins(int amount)
    {
        totalCoins += amount;
        SaveCoins();
        OnCoinsUpdated?.Invoke(totalCoins);
        Debug.Log($"GameData: Added {amount} coins. Total coins: {totalCoins}");
    }

    // Метод для снятия монет
    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            SaveCoins();
            OnCoinsUpdated?.Invoke(totalCoins);
            Debug.Log($"GameData: Spent {amount} coins. Total coins: {totalCoins}");
            return true;
        }
        else
        {
            Debug.LogWarning("GameData: Not enough coins!");
            return false;
        }
    }

    // Получение текущего баланса монет
    public int GetTotalCoins()
    {
        return totalCoins;
    }

    // Загрузка монет из PlayerPrefs
    private void LoadCoins()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        Debug.Log($"GameData: Loaded {totalCoins} coins.");
    }

    // Сохранение монет в PlayerPrefs
    private void SaveCoins()
    {
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();
        Debug.Log($"GameData: Saved {totalCoins} coins.");
    }

    #endregion

    #region Управление Ставками

    // Метод для установки текущей ставки
    public bool SetCurrentBet(int newBet)
    {
        if (newBet < 1 || newBet > 50)
        {
            Debug.LogWarning("GameData: Bet must be between 1 and 50.");
            return false;
        }

        currentBet = newBet;
        PlayerPrefs.SetInt("CurrentBet", currentBet);
        PlayerPrefs.Save();
        Debug.Log($"GameData: Current Bet set to: {currentBet}");

        return true;
    }

    // Метод для получения текущей ставки
    public int GetCurrentBet()
    {
        return currentBet;
    }

    // Загрузка текущей ставки из PlayerPrefs
    private void LoadBet()
    {
        currentBet = PlayerPrefs.GetInt("CurrentBet", 1);
        Debug.Log($"GameData: Loaded Current Bet = {currentBet}");
    }

    #endregion

    #region Управление Первым Запуском

    // Загрузка флажка первого запуска
    private void LoadFirstRunFlag()
    {
        isFirstRun = PlayerPrefs.GetInt("IsFirstRun", 1) == 1;
        Debug.Log($"GameData: IsFirstRun = {isFirstRun}");
    }

    // Установка флажка первого запуска
    public void SetFirstRun(bool value)
    {
        isFirstRun = value;
        PlayerPrefs.SetInt("IsFirstRun", value ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"GameData: Set IsFirstRun to {isFirstRun}");
    }

    // Получение флажка первого запуска
    public bool GetFirstRun()
    {
        return isFirstRun;
    }

    #endregion

    #region Управление Ежедневными Наградами

    // Загрузка ежедневных наград
    private void LoadDailyRewards()
    {
        dailyRewards.Clear(); // Очистка списка перед загрузкой

        // Загрузка состояний наград из PlayerPrefs
        for (int i = 1; i <= 7; i++)
        {
            int defaultState = i == 1 ? (int)RewardState.Available : (int)RewardState.Locked;
            DailyReward reward = new DailyReward
            {
                dayNumber = i,
                state = (RewardState)PlayerPrefs.GetInt("DailyRewardDay" + i, defaultState)
            };
            dailyRewards.Add(reward);
        }

        // Загрузка последней даты получения награды
        string lastDateString = PlayerPrefs.GetString("LastRewardClaimTime", string.Empty);
        DateTime lastRewardDate;
        if (!string.IsNullOrEmpty(lastDateString))
        {
            long binaryTime;
            if (long.TryParse(lastDateString, out binaryTime))
            {
                lastRewardDate = DateTime.FromBinary(binaryTime);
            }
            else
            {
                lastRewardDate = DateTime.MinValue;
            }
        }
        else
        {
            lastRewardDate = DateTime.MinValue;
        }

        Debug.Log($"GameData: LastRewardDate = {lastRewardDate}");

        // Проверка, если все награды собраны и прошёл день, сбрасываем награды
        bool allUnlocked = true;
        foreach (var reward in dailyRewards)
        {
            if (reward.state != RewardState.Unlocked)
            {
                allUnlocked = false;
                break;
            }
        }

        if (allUnlocked)
        {
            if (lastRewardDate != DateTime.MinValue && DateTime.Now >= lastRewardDate.AddDays(1))
            {
                Debug.Log("GameData: 24 часа прошло с последней награды. Сбрасываем ежедневные награды.");
                ResetDailyRewards();
            }
        }

        Debug.Log("GameData: Daily Rewards Loaded.");
    }

    // Установка состояния ежедневной награды с возможностью подавления события
    public void SetDailyRewardState(int day, RewardState state, bool suppressEvent = false)
    {
        DailyReward reward = dailyRewards.Find(r => r.dayNumber == day);
        if (reward != null)
        {
            reward.state = state;
            PlayerPrefs.SetInt("DailyRewardDay" + day, (int)state); // Приведение к int
            PlayerPrefs.Save();
            if (!suppressEvent)
            {
                OnRewardStateChanged?.Invoke(day, state);
            }
            Debug.Log($"GameData: Daily Reward Day {day} set to {state}.");
        }
        else
        {
            Debug.LogError($"GameData: Daily Reward Day {day} не найден.");
        }
    }

    // Получение состояния ежедневной награды
    public RewardState GetDailyRewardState(int day)
    {
        DailyReward reward = dailyRewards.Find(r => r.dayNumber == day);
        if (reward != null)
        {
            return reward.state;
        }
        return RewardState.Locked;
    }

    // Установка времени последнего получения награды
    public void SetLastRewardClaimTime(DateTime time)
    {
        PlayerPrefs.SetString("LastRewardClaimTime", time.ToBinary().ToString());
        PlayerPrefs.Save();
        Debug.Log($"GameData: Set LastRewardClaimTime to {time}");
    }

    // Получение времени до следующей награды
    public TimeSpan GetTimeUntilNextReward()
    {
        string timeString = PlayerPrefs.GetString("LastRewardClaimTime", string.Empty);
        DateTime lastClaimTime;
        if (!string.IsNullOrEmpty(timeString))
        {
            long binaryTime;
            if (long.TryParse(timeString, out binaryTime))
            {
                lastClaimTime = DateTime.FromBinary(binaryTime);
            }
            else
            {
                lastClaimTime = DateTime.MinValue;
            }
        }
        else
        {
            lastClaimTime = DateTime.MinValue;
        }

        if (lastClaimTime == DateTime.MinValue)
        {
            // Никогда не получал награду
            return TimeSpan.Zero;
        }

        DateTime nextRewardTime = lastClaimTime.AddDays(1); // Награда доступна через 24 часа
        TimeSpan timeLeft = nextRewardTime - DateTime.Now;

        if (timeLeft.TotalSeconds < 0)
        {
            return TimeSpan.Zero;
        }

        return timeLeft;
    }

    // Сброс всех ежедневных наград
    public void ResetDailyRewards()
    {
        // Сброс всех наград до Locked, кроме первого дня
        for (int i = 0; i < dailyRewards.Count; i++)
        {
            if (i == 0)
            {
                SetDailyRewardState(dailyRewards[i].dayNumber, RewardState.Available, true);
            }
            else
            {
                SetDailyRewardState(dailyRewards[i].dayNumber, RewardState.Locked, true);
            }
        }

        // Устанавливаем время последнего получения награды на текущее время
        SetLastRewardClaimTime(DateTime.Now);

        // Вызываем событие для массового обновления UI
        OnRewardStateChanged?.Invoke(0, RewardState.Locked);

        Debug.Log("GameData: Ежедневные награды сброшены.");
    }

    #endregion

    #region Управление Купленными Предметами

    /// <summary>
    /// Метод для добавления купленного предмета
    /// </summary>
    /// <param name="itemName">Название предмета</param>
    /// <param name="quantity">Количество купленного предмета</param>
    public void AddPurchasedItem(string itemName, int quantity)
    {
        PurchasedItem existingItem = purchasedItems.Find(item => item.itemName == itemName);
        if (existingItem != null)
        {
            existingItem.quantity += quantity;
        }
        else
        {
            purchasedItems.Add(new PurchasedItem { itemName = itemName, quantity = quantity });
        }
        SavePurchasedItems();
        Debug.Log($"GameData: Добавлен предмет - {itemName} x{quantity}. Всего: {existingItem?.quantity ?? quantity}");
    }

    /// <summary>
    /// Метод для использования купленного предмета
    /// </summary>
    /// <param name="itemName">Название предмета</param>
    /// <returns>Возвращает true, если предмет был использован, иначе false</returns>
    public bool UsePurchasedItem(string itemName)
    {
        PurchasedItem item = purchasedItems.Find(i => i.itemName == itemName);
        if (item != null && item.quantity > 0)
        {
            item.quantity--;
            SavePurchasedItems();
            Debug.Log($"GameData: Использован предмет - {itemName}. Осталось: {item.quantity}");
            return true;
        }
        Debug.LogWarning($"GameData: Попытка использовать {itemName}, но предметов недостаточно.");
        return false;
    }

    /// <summary>
    /// Метод для получения количества купленного предмета
    /// </summary>
    /// <param name="itemName">Название предмета</param>
    /// <returns>Количество купленных предметов</returns>
    public int GetPurchasedItemQuantity(string itemName)
    {
        PurchasedItem item = purchasedItems.Find(i => i.itemName == itemName);
        return item != null ? item.quantity : 0;
    }

    /// <summary>
    /// Метод для сохранения купленных предметов
    /// </summary>
    private void SavePurchasedItems()
    {
        string json = JsonUtility.ToJson(new PurchasedItemsWrapper { purchasedItems = this.purchasedItems });
        PlayerPrefs.SetString("PurchasedItems", json);
        PlayerPrefs.Save();
        Debug.Log("GameData: Купленные предметы сохранены.");
    }

    /// <summary>
    /// Метод для загрузки купленных предметов
    /// </summary>
    private void LoadPurchasedItems()
    {
        string json = PlayerPrefs.GetString("PurchasedItems", "{}");
        if (!string.IsNullOrEmpty(json))
        {
            PurchasedItemsWrapper wrapper = JsonUtility.FromJson<PurchasedItemsWrapper>(json);
            if (wrapper != null && wrapper.purchasedItems != null)
            {
                this.purchasedItems = wrapper.purchasedItems;
            }
        }
        Debug.Log("GameData: Купленные предметы загружены.");
    }

    [Serializable]
    private class PurchasedItemsWrapper
    {
        public List<PurchasedItem> purchasedItems;
    }

    #endregion

    #region Сброс Данных

    /// <summary>
    /// Метод сброса данных через инспектор
    /// </summary>
    [ContextMenu("Reset All Data")]
    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("GameData: Все PlayerPrefs удалены.");

        // Сброс локальных переменных
        SoundEnabled = true;
        MusicEnabled = true;
        totalCoins = 0;
        isFirstRun = true;
        dailyRewards.Clear();
        purchasedItems.Clear();

        // Повторное сохранение значений по умолчанию
        SaveCoins();
        SetSoundEnabled(true);
        SetMusicEnabled(true);
        SetFirstRun(true);

        // Инициализация ежедневных наград
        LoadDailyRewards();

        // Инициализация текущей ставки
        SetCurrentBet(1);

        // Сохранение купленных предметов
        SavePurchasedItems();

        // Вызов событий для обновления монет и наград
        OnCoinsUpdated?.Invoke(totalCoins);
        foreach (var reward in dailyRewards)
        {
            OnRewardStateChanged?.Invoke(reward.dayNumber, reward.state);
        }
    }

    #endregion
}
