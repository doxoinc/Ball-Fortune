using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("UI Elements")]
    public Button dropButton;          // Кнопка "Drop"
    public Text scoreText;             // Текст для отображения счета (общие монеты)
    public Button pauseButton;         // Кнопка "Pause"

    [Header("Ball Settings")]
    public Transform spawnPoint;       // Точка спавна шариков

    private bool isGameOver = false;

    [Header("Panels")]
    public GameObject pausePanel;      // Панель Pause
    public GameObject finishPanel;     // Панель Finish

    // Максимальное количество активных шариков
    public int maxActiveBalls = 20;

    private int totalScore = 0; // Общий счёт игрока
    public int TotalScore => totalScore; // Свойство для доступа к общему счёту

    // Reference to ObjectPooler
    public ObjectPooler objectPooler; // Назначьте через инспектор

    private void Awake()
    {
        // Поиск ObjectPooler по тегу "ObjectPooler"
        GameObject poolerObj = GameObject.FindGameObjectWithTag("ObjectPooler");
        if (poolerObj != null)
        {
            objectPooler = poolerObj.GetComponent<ObjectPooler>();
            if (objectPooler == null)
            {
                Debug.LogError("GameController: Объект с тегом 'ObjectPooler' не содержит компонент ObjectPooler.");
            }
            else
            {
                Debug.Log("GameController: ObjectPooler успешно назначен в Awake.");
            }
        }
        else
        {
            Debug.LogError("GameController: Объект с тегом 'ObjectPooler' не найден в сцене.");
        }
    }

    private void Start()
    {
        // Назначение слушателей кнопок
        if (dropButton != null)
        {
            dropButton.onClick.AddListener(OnDropButtonClicked);
            Debug.Log("GameController: DropButton слушатель добавлен.");
        }
        else
        {
            Debug.LogError("GameController: DropButton не назначен в инспекторе.");
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
            Debug.Log("GameController: PauseButton слушатель добавлен.");
        }
        else
        {
            Debug.LogError("GameController: PauseButton не назначен в инспекторе.");
        }

        // Проверка наличия ObjectPooler
        if (objectPooler == null)
        {
            objectPooler = FindObjectOfType<ObjectPooler>();
            if (objectPooler == null)
            {
                Debug.LogError("GameController: ObjectPooler не найден в сцене. Назначьте его через инспектор.");
            }
            else
            {
                Debug.Log("GameController: ObjectPooler найден через FindObjectOfType.");
            }
        }
        else
        {
            Debug.Log("GameController: ObjectPooler назначен через инспектор.");
        }

        // Подписка на событие обновления монет
        if (GameData.Instance != null)
        {
            GameData.Instance.OnCoinsUpdated += UpdateScoreUI;
            // Инициализируем отображение счета сразу
            UpdateScoreUI(GameData.Instance.GetTotalCoins());
            Debug.Log("GameController: Подписка на событие OnCoinsUpdated успешно.");
        }
        else
        {
            Debug.LogError("GameController: GameData Instance не найден в сцене.");
        }

        // Убедимся, что панели закрыты при старте
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (finishPanel != null)
            finishPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // Отписываемся от событий кнопок при уничтожении объекта
        if (dropButton != null)
        {
            dropButton.onClick.RemoveListener(OnDropButtonClicked);
            Debug.Log("GameController: DropButton слушатель удален.");
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            Debug.Log("GameController: PauseButton слушатель удален.");
        }

        // Отписываемся от события обновления монет
        if (GameData.Instance != null)
        {
            GameData.Instance.OnCoinsUpdated -= UpdateScoreUI;
            Debug.Log("GameController: Отписка от события OnCoinsUpdated выполнена.");
        }
    }

    /// <summary>
    /// Метод для запуска указанного количества шариков
    /// </summary>
    /// <param name="count">Количество шариков для запуска</param>
    public void SpawnBalls(int count)
    {
        Debug.Log($"GameController: Запуск {count} шариков.");

        if (isGameOver)
        {
            Debug.Log("GameController: Игра окончена. Никакие шарики не запускаются.");
            return;
        }

        if (objectPooler == null)
        {
            Debug.LogError("GameController: ObjectPooler не назначен.");
            return;
        }

        int currentActiveBalls = FindObjectsOfType<BallController>().Length;
        Debug.Log($"GameController: Текущее количество активных шариков: {currentActiveBalls}");

        if (currentActiveBalls + count > maxActiveBalls)
        {
            Debug.Log("GameController: Максимальное количество активных шариков достигнуто. Запуск невозможен.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject ballObj = objectPooler.GetPooledObject();
            if (ballObj != null)
            {
                BallController ball = ballObj.GetComponent<BallController>();
                if (ball != null)
                {
                    ball.ActivateBall(spawnPoint.position);
                    Debug.Log($"GameController: Запущен шарик {ballObj.name}.");
                }
                else
                {
                    Debug.LogError("GameController: Полученный объект не имеет компонента BallController.");
                }
            }
            else
            {
                Debug.LogError("GameController: Не удалось получить объект из пула.");
            }
        }

        // Воспроизводим звуковой эффект нажатия кнопки
        AudioManager.Instance?.PlayButtonClick();

        Debug.Log($"GameController: Запущено {count} шариков.");
    }

    /// <summary>
    /// Метод, вызываемый при нажатии на кнопку "Drop"
    /// </summary>
    private void OnDropButtonClicked()
    {
        Debug.Log("GameController: Кнопка Drop нажата.");

        if (GameData.Instance == null)
        {
            Debug.LogError("GameController: GameData Instance отсутствует.");
            return;
        }

        // Получаем текущую ставку из GameData
        int currentBet = GameData.Instance.GetCurrentBet();
        Debug.Log($"GameController: Текущая ставка: {currentBet} монет.");

        // Пытаемся списать монеты
        if (GameData.Instance.SpendCoins(currentBet))
        {
            Debug.Log($"GameController: Списано {currentBet} монет за запуск шарика.");
            // Запускаем один шарик
            SpawnBalls(1);
        }
        else
        {
            Debug.LogWarning("GameController: Недостаточно монет для ставки.");
            // Здесь можно добавить визуальное уведомление игроку (например, всплывающее сообщение)
        }
    }

    /// <summary>
    /// Метод для активации Тройного мяча
    /// </summary>
    public void ActivateTripleBall()
    {
        Debug.Log("GameController: Попытка использования TripleBall.");

        if (GameData.Instance == null)
        {
            Debug.LogError("GameController: GameData Instance отсутствует.");
            return;
        }

        // Получаем текущую ставку из GameData
        int currentBet = GameData.Instance.GetCurrentBet();
        Debug.Log($"GameController: Текущая ставка: {currentBet} монет.");

        // Пытаемся списать монеты только за один шарик
        if (GameData.Instance.SpendCoins(currentBet))
        {
            Debug.Log($"GameController: Списано {currentBet} монет за использование TripleBall.");
            // Запускаем три шарика
            SpawnBalls(3);
        }
        else
        {
            Debug.LogWarning("GameController: Недостаточно монет для использования TripleBall.");
            // Здесь можно добавить визуальное уведомление игроку
        }
    }

    /// <summary>
    /// Публичный метод для добавления очков (монет)
    /// </summary>
    /// <param name="amount">Количество добавляемых монет</param>
    public void AddScore(int amount)
    {
        totalScore += amount; // Обновляем общий счёт
        GameData.Instance.AddCoins(amount); // Добавляем монеты через GameData

        UpdateScoreUI(); // Обновляем UI
    }

    /// <summary>
    /// Метод для обновления текста счета при изменении монет
    /// </summary>
    /// <param name="totalCoins">Текущее количество монет</param>
    private void UpdateScoreUI(int totalCoins)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{totalCoins}";
            Debug.Log($"GameController: Обновлен счет. Текущие монеты: {totalCoins}");
        }
    }

    /// <summary>
    /// Перегрузка метода UpdateScoreUI для инициализации
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null && GameData.Instance != null)
        {
            scoreText.text = $"{GameData.Instance.GetTotalCoins()}";
            Debug.Log($"GameController: Обновлен счет. Текущие монеты: {GameData.Instance.GetTotalCoins()}");
        }
    }

    /// <summary>
    /// Метод для обработки окончания игры
    /// </summary>
    public void EndGame()
    {
        isGameOver = true;

        // Показать панель Finish
        if (finishPanel != null)
        {
            finishPanel.SetActive(true);
            Debug.Log("GameController: Панель Finish активирована.");
        }

        // Дополнительная логика окончания игры может быть добавлена здесь

        // Воспроизводим звуковой эффект завершения игры (опционально)
        // AudioManager.Instance?.PlayGameOverSound();
    }

    /// <summary>
    /// Метод, вызываемый при нажатии на кнопку "Pause"
    /// </summary>
    private void OnPauseButtonClicked()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f; // Пауза игры
            Debug.Log("GameController: Игра на паузе.");

            // Воспроизводим звуковой эффект нажатия кнопки
            AudioManager.Instance?.PlayButtonClick();
        }
    }

    /// <summary>
    /// Метод для возобновления игры из PausePanel
    /// </summary>
    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f; // Возобновление игры
            Debug.Log("GameController: Игра возобновлена.");
        }
    }

    /// <summary>
    /// Метод для выхода из игры (возврат в главное меню)
    /// </summary>
    public void ExitToMainMenu()
    {
        // Сохранение монет уже выполнено через SpendCoins/AddCoins
        Time.timeScale = 1f; // Восстановление времени
        SceneManager.LoadScene("MainMenu");
        Debug.Log("GameController: Выход в главное меню.");

        // Воспроизводим звуковой эффект нажатия кнопки
        AudioManager.Instance?.PlayButtonClick();
    }
}
