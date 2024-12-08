using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class DailyRewards : MonoBehaviour
{
    [System.Serializable]
    public class DayRewardUI
    {
        public int dayNumber;               // Номер дня (1-7)
        public Image dayImage;              // Объект Image для отображения состояния дня
        public Sprite lockedSprite;         // Спрайт для состояния Locked
        public Sprite availableSprite;      // Спрайт для состояния Available
        public Sprite unlockedSprite;       // Спрайт для состояния Unlocked
    }

    public List<DayRewardUI> dayRewards = new List<DayRewardUI>(); // Список для 7 дней наград

    public Button getButton;             // Кнопка "Get"
    public Text rewardAmountText;        // Текст для отображения количества монет

    private int currentDay = 1;          // Текущий день награды

    // Количество монет за каждый день (день 1 - 7)
    public List<int> coinsPerDay = new List<int> { 100, 200, 300, 400, 500, 600, 700 };

    private Coroutine timerCoroutine;    // Корутин для обновления таймера
    private bool isCoroutineRunning = false; // Флаг для отслеживания запущенной корутины

    void Start()
    {
        // Проверка наличия GameData
        if (GameData.Instance == null)
        {
            Debug.LogError("DailyRewards: GameData instance not found in the scene. Please add a GameData object.");
            return;
        }

        // Подписываемся на событие изменения состояния наград
        GameData.Instance.OnRewardStateChanged += OnRewardStateChanged;

        // Инициализируем UI элементов наград
        InitializeRewardsUI();

        // Добавляем слушатель нажатия на кнопку "Get"
        if (getButton != null)
        {
            getButton.onClick.AddListener(OnGetButtonClicked);
        }

        // Обновляем UI на основе текущего состояния наград
        UpdateUI();
    }

    void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        if (GameData.Instance != null)
        {
            GameData.Instance.OnRewardStateChanged -= OnRewardStateChanged;
        }

        // Удаляем слушатель нажатия на кнопку "Get"
        if (getButton != null)
        {
            getButton.onClick.RemoveListener(OnGetButtonClicked);
        }

        // Останавливаем корутину, если она запущена
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            isCoroutineRunning = false;
        }
    }

    #region Инициализация UI

    // Инициализация UI элементов наград
    private void InitializeRewardsUI()
    {
        foreach (var dayReward in dayRewards)
        {
            // Устанавливаем начальное состояние Image
            UpdateDayImage(dayReward.dayNumber, GameData.Instance.GetDailyRewardState(dayReward.dayNumber));
        }
    }

    // Обновление изображения дня награды
    private void UpdateDayImage(int day, RewardState state)
    {
        DayRewardUI dayReward = dayRewards.Find(dr => dr.dayNumber == day);
        if (dayReward != null && dayReward.dayImage != null)
        {
            switch (state)
            {
                case RewardState.Locked:
                    dayReward.dayImage.sprite = dayReward.lockedSprite;
                    break;
                case RewardState.Available:
                    dayReward.dayImage.sprite = dayReward.availableSprite;
                    break;
                case RewardState.Unlocked:
                    dayReward.dayImage.sprite = dayReward.unlockedSprite;
                    break;
            }
        }
    }

    #endregion

    #region Обработка Событий

    // Обработка изменения состояния награды
    private void OnRewardStateChanged(int day, RewardState state)
    {
        if (day == 0)
        {
            // Массовое обновление UI после сброса наград
            InitializeRewardsUI();
        }
        else
        {
            // Обновляем конкретную награду
            UpdateDayImage(day, state);
        }
        UpdateUI();
    }

    #endregion

    #region Обновление UI

    // Обновление UI: "Get" кнопка и текст награды
    private void UpdateUI()
    {
        // Определяем текущий доступный день
        currentDay = FindAvailableDay();

        if (currentDay > 7)
        {
            // Все награды получены
            getButton.interactable = false;
            rewardAmountText.text = GetTimeUntilNextRewardText();

            // Запускаем корутину для обновления таймера, если она не запущена
            if (!isCoroutineRunning)
            {
                if (timerCoroutine != null)
                {
                    StopCoroutine(timerCoroutine);
                }
                timerCoroutine = StartCoroutine(UpdateTimer());
                isCoroutineRunning = true;
            }
            return;
        }

        RewardState currentState = GameData.Instance.GetDailyRewardState(currentDay);

        if (currentState == RewardState.Available)
        {
            // Награда доступна для получения
            getButton.interactable = true;
            rewardAmountText.text = $"{coinsPerDay[currentDay - 1]} Coins";

            // Останавливаем корутину таймера, если она запущена
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
                isCoroutineRunning = false;
            }
        }
        else
        {
            // Награда недоступна
            getButton.interactable = false;
            rewardAmountText.text = "No rewards available today.";

            // Останавливаем корутину таймера, если она запущена
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
                isCoroutineRunning = false;
            }
        }
    }

    // Поиск первого доступного дня награды
    private int FindAvailableDay()
    {
        foreach (var dayReward in dayRewards)
        {
            if (GameData.Instance.GetDailyRewardState(dayReward.dayNumber) == RewardState.Available)
            {
                return dayReward.dayNumber;
            }
        }
        return dayRewards.Count + 1; // Все награды получены
    }

    // Получение текста таймера до следующей награды
    private string GetTimeUntilNextRewardText()
    {
        TimeSpan timeLeft = GameData.Instance.GetTimeUntilNextReward();
        if (timeLeft.TotalSeconds <= 0)
        {
            return "Reward available now!";
        }
        else
        {
            int hours = (int)timeLeft.TotalHours;
            int minutes = timeLeft.Minutes;
            return $"{hours}h {minutes}m";
        }
    }

    #endregion

    #region Корутин

    // Корутина для обновления таймера каждые 60 секунд
    private IEnumerator UpdateTimer()
    {
        while (isCoroutineRunning)
        {
            TimeSpan timeLeft = GameData.Instance.GetTimeUntilNextReward();
            if (timeLeft.TotalSeconds <= 0)
            {
                // Сбрасываем награды
                GameData.Instance.ResetDailyRewards();
                // Обновляем UI
                UpdateUI();
                // Останавливаем корутину
                isCoroutineRunning = false;
                yield break;
            }
            rewardAmountText.text = $"{(int)timeLeft.TotalHours}h {timeLeft.Minutes}m";
            yield return new WaitForSeconds(60f); // Обновление каждую минуту
        }
    }

    #endregion

    #region Обработка Кнопок

    // Метод, вызываемый при нажатии на кнопку "Get"
    public void OnGetButtonClicked()
    {
        if (currentDay > 7)
        {
            Debug.LogWarning("DailyRewards: All daily rewards have been claimed.");
            return;
        }

        RewardState currentState = GameData.Instance.GetDailyRewardState(currentDay);
        if (currentState == RewardState.Available)
        {
            // Получение награды
            int rewardAmount = coinsPerDay[currentDay - 1];
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.AddCoins(rewardAmount);
                Debug.Log($"DailyRewards: Day {currentDay} reward claimed: {rewardAmount} coins.");
            }
            else
            {
                Debug.LogError("DailyRewards: CoinManager instance not found in the scene.");
            }

            // Устанавливаем состояние награды на Unlocked
            GameData.Instance.SetDailyRewardState(currentDay, RewardState.Unlocked);

            // Устанавливаем время последнего получения награды
            GameData.Instance.SetLastRewardClaimTime(DateTime.Now);

            // Активируем следующую награду, если есть
            if (currentDay < 7)
            {
                RewardState nextState = GameData.Instance.GetDailyRewardState(currentDay + 1);
                if (nextState == RewardState.Locked)
                {
                    GameData.Instance.SetDailyRewardState(currentDay + 1, RewardState.Available);
                }
            }

            // Обновляем UI после получения награды
            UpdateUI();
        }
        else
        {
            Debug.LogWarning($"DailyRewards: Reward for day {currentDay} is not available.");
        }
    }

    #endregion
}
