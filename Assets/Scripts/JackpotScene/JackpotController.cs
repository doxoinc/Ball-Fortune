using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class JackpotController : MonoBehaviour
{
    [Header("UI Elements")]
    public Button dropButton; // "Drop" Button
    public Text coinsText; // Text to display coins
    public PanelMessageController panelMessage; // Reference to the message panel controller
    public PanelWinController panelWin; // Reference to the win panel controller
    public List<Image> symbolImages; // List of symbol images (Symbol1 - Symbol9)
    public Button pauseButton; // Pause Button
    public Button infoButton; // Info Button
    public PanelPauseController panelPause; // Reference to the pause panel controller
    public PanelInfoController panelInfo; // Reference to the info panel controller

    [Header("Symbols")]
    public List<Sprite> symbolSprites; // List of symbol sprites (Cherry, Seven, Bell, Bar)
    
    [Header("Game Settings")]
    public int spinCost = 50; // Cost per spin
    public int jackpotReward = 500; // Reward for matching symbols

    [Header("Spin Settings")]
    public float spinDuration = 2f; // Duration of the spin in seconds
    public float initialSpinSpeed = 10f; // Initial spin speed (number of symbol changes per second)

    [Header("Panels")]
    public List<Image> panelImages; // List of panel images corresponding to each symbol

    [Header("Highlight Settings")]
    public Color defaultPanelColor = new Color(1f, 1f, 1f, 0.5f); // Белый с 50% прозрачности
    public Color highlightColor = new Color(1f, 1f, 0f, 1f); // Желтый полностью непрозрачный

    private bool isSpinning = false;
    private float currentSpinSpeed; // Current spin speed
    private float spinSpeedDecrease; // Speed decrease rate

    private Coroutine spinCoroutine; // Reference to the current spin coroutine
    private List<int> matchedIndices = new List<int>(); // List of matched symbol indices

    private void Start()
    {
        // Назначение слушателей кнопок
        if (dropButton != null)
        {
            dropButton.onClick.AddListener(OnDropButtonClicked);
        }
        else
        {
            Debug.LogError("JackpotController: DropButton is not assigned in the Inspector.");
        }

        if (panelMessage == null)
        {
            Debug.LogError("JackpotController: PanelMessage is not assigned in the Inspector.");
        }

        if (panelWin == null)
        {
            Debug.LogError("JackpotController: PanelWin is not assigned in the Inspector.");
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }
        else
        {
            Debug.LogError("JackpotController: PauseButton is not assigned in the Inspector.");
        }

        if (infoButton != null)
        {
            infoButton.onClick.AddListener(OnInfoButtonClicked);
        }
        else
        {
            Debug.LogError("JackpotController: InfoButton is not assigned in the Inspector.");
        }

        if (panelPause == null)
        {
            Debug.LogError("JackpotController: PanelPause is not assigned in the Inspector.");
        }

        if (panelInfo == null)
        {
            Debug.LogError("JackpotController: PanelInfo is not assigned in the Inspector.");
        }

        // Проверка соответствия размеров списков
        if (symbolImages.Count != panelImages.Count)
        {
            Debug.LogError("JackpotController: symbolImages and panelImages lists must have the same number of elements.");
        }

        // Инициализация UI
        UpdateCoinsUI();
        panelMessage.HideMessage();
        panelWin.HidePanel();
        panelPause.gameObject.SetActive(false); // Убедитесь, что PausePanel скрыт
        panelInfo.gameObject.SetActive(false); // Убедитесь, что InfoPanel скрыт

        // Сброс цветов панелей
        ResetPanelHighlights();
    }

    private void OnDestroy()
    {
        if (dropButton != null)
        {
            dropButton.onClick.RemoveListener(OnDropButtonClicked);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
        }

        if (infoButton != null)
        {
            infoButton.onClick.RemoveListener(OnInfoButtonClicked);
        }
    }

    /// <summary>
    /// Метод вызывается при нажатии кнопки "Drop"
    /// </summary>
    private void OnDropButtonClicked()
    {
        if (isSpinning)
        {
            Debug.LogWarning("JackpotController: Spin is already in progress.");
            return;
        }

        int currentCoins = GameData.Instance.GetTotalCoins();
        if (currentCoins < spinCost)
        {
            ShowTemporaryMessage("Not enough coins!", 2f);
            return;
        }

        // Списание монет за вращение
        if (GameData.Instance.SpendCoins(spinCost))
        {
            UpdateCoinsUI();
            // Начинаем вращение барабанов
            spinCoroutine = StartCoroutine(SpinReels());
        }
        else
        {
            ShowTemporaryMessage("Not enough coins!", 2f);
        }
    }

    /// <summary>
    /// Метод вызывается при нажатии кнопки "Pause"
    /// </summary>
    private void OnPauseButtonClicked()
    {
        AudioManager.Instance.PlayButtonClick();
        // Открываем панель паузы
        panelPause.gameObject.SetActive(true);
        panelPause.ShowPanel();

        // Приостанавливаем игру
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Метод вызывается при нажатии кнопки "Info"
    /// </summary>
    private void OnInfoButtonClicked()
    {
        AudioManager.Instance.PlayButtonClick();
        // Открываем панель информации
        panelInfo.gameObject.SetActive(true);
        panelInfo.ShowPanel();
    }

    /// <summary>
    /// Корутина для управления вращением барабанов с постепенным замедлением
    /// </summary>
    private IEnumerator SpinReels()
    {
        isSpinning = true;
        panelMessage.HideMessage();

        // Скрываем панель выигрыша, если она активна
        if (panelWin.gameObject.activeSelf)
        {
            panelWin.HidePanel();
        }

        // Скрываем другие панели
        panelPause.gameObject.SetActive(false); // Скрываем панель паузы, если она открыта
        panelInfo.gameObject.SetActive(false); // Скрываем панель информации, если она открыта

        // Сброс подсветки панелей
        ResetPanelHighlights();

        // Инициализация скорости вращения и скорости замедления
        currentSpinSpeed = initialSpinSpeed;
        spinSpeedDecrease = initialSpinSpeed / spinDuration;

        float elapsedTime = 0f;
        float startTime = Time.time;

        // Основной цикл вращения
        while (elapsedTime < spinDuration)
        {
            // Присваиваем случайные символы каждому барабану
            foreach (var reel in symbolImages)
            {
                int randomIndex = Random.Range(0, symbolSprites.Count);
                reel.sprite = symbolSprites[randomIndex];
            }

            // Обновляем прошедшее время
            elapsedTime = Time.time - startTime;

            // Обновляем текущую скорость вращения
            currentSpinSpeed = Mathf.Max(initialSpinSpeed - spinSpeedDecrease * elapsedTime, 0f);

            // Вычисляем время ожидания перед следующим обновлением символов
            float waitTime = (currentSpinSpeed > 0f) ? (1f / currentSpinSpeed) : (spinDuration - elapsedTime);

            // Корректируем waitTime, чтобы не превышало spinDuration
            if (elapsedTime + waitTime > spinDuration)
            {
                waitTime = spinDuration - elapsedTime;
            }

            // Ждем перед следующим обновлением символов
            if (waitTime > 0f)
            {
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                // Если времени осталось мало, выходим из цикла
                break;
            }
        }

        // Устанавливаем финальные символы после завершения вращения
        foreach (var reel in symbolImages)
        {
            int finalIndex = Random.Range(0, symbolSprites.Count);
            reel.sprite = symbolSprites[finalIndex];
        }

        // Проверяем выигрышные комбинации
        CheckForWin();

        isSpinning = false;
    }

    /// <summary>
    /// Метод для проверки выигрышных комбинаций
    /// </summary>
    private void CheckForWin()
    {
        // Сбор текущих символов из всех барабанов
        List<string> currentSymbols = new List<string>();
        foreach (var img in symbolImages)
        {
            currentSymbols.Add(img.sprite.name);
        }

        matchedIndices.Clear(); // Очищаем предыдущие совпадения

        bool isWin = false;

        // Пример для сетки 3x3
        // Индексы:
        // 0 1 2
        // 3 4 5
        // 6 7 8

        // Проверка горизонтальных линий
        for (int row = 0; row < 3; row++)
        {
            int startIdx = row * 3;
            if (currentSymbols[startIdx] == currentSymbols[startIdx + 1] &&
                currentSymbols[startIdx + 1] == currentSymbols[startIdx + 2])
            {
                isWin = true;
                matchedIndices.Add(startIdx);
                matchedIndices.Add(startIdx + 1);
                matchedIndices.Add(startIdx + 2);
                break;
            }
        }

        // Проверка вертикальных линий
        if (!isWin)
        {
            for (int col = 0; col < 3; col++)
            {
                if (currentSymbols[col] == currentSymbols[col + 3] &&
                    currentSymbols[col + 3] == currentSymbols[col + 6])
                {
                    isWin = true;
                    matchedIndices.Add(col);
                    matchedIndices.Add(col + 3);
                    matchedIndices.Add(col + 6);
                    break;
                }
            }
        }

        // Проверка диагоналей
        if (!isWin)
        {
            // Диагональ 0,4,8
            if (currentSymbols[0] == currentSymbols[4] &&
                currentSymbols[4] == currentSymbols[8])
            {
                isWin = true;
                matchedIndices.Add(0);
                matchedIndices.Add(4);
                matchedIndices.Add(8);
            }
            // Диагональ 2,4,6
            else if (currentSymbols[2] == currentSymbols[4] &&
                     currentSymbols[4] == currentSymbols[6])
            {
                isWin = true;
                matchedIndices.Add(2);
                matchedIndices.Add(4);
                matchedIndices.Add(6);
            }
        }

        if (isWin)
        {
            // Выигрыш: добавляем монеты и отображаем панель выигрыша
            GameData.Instance.AddCoins(jackpotReward);
            UpdateCoinsUI();
            HighlightMatchedSymbols();
            ShowWinPanel(jackpotReward);
        }
        else
        {
            // Нет выигрыша: отображаем сообщение "Try Again!" на 2 секунды
            ShowTemporaryMessage("Try Again!", 2f);
        }
    }

    /// <summary>
    /// Метод для обновления отображения монет
    /// </summary>
    private void UpdateCoinsUI()
    {
        if (coinsText != null && GameData.Instance != null)
        {
            coinsText.text = $"{GameData.Instance.GetTotalCoins()}";
        }
    }

    /// <summary>
    /// Метод для отображения временного сообщения игроку
    /// </summary>
    /// <param name="message">Сообщение для отображения</param>
    /// <param name="duration">Продолжительность отображения сообщения в секундах</param>
    private void ShowTemporaryMessage(string message, float duration)
    {
        if (panelMessage != null)
        {
            panelMessage.ShowMessage(message);
            StartCoroutine(HideMessageAfterDelay(duration));
        }
    }

    /// <summary>
    /// Корутина для скрытия сообщения после задержки
    /// </summary>
    /// <param name="delay">Задержка в секундах</param>
    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panelMessage != null)
        {
            panelMessage.HideMessage();
        }
    }

    /// <summary>
    /// Метод для отображения панели выигрыша с суммой награды
    /// </summary>
    /// <param name="rewardAmount">Сумма выигранных монет</param>
    private void ShowWinPanel(int rewardAmount)
    {
        if (panelWin != null)
        {
            panelWin.SetRewardAmount(rewardAmount);
            panelWin.ShowPanel();
        }
        else
        {
            Debug.LogError("JackpotController: PanelWin is not assigned.");
        }
    }

    /// <summary>
    /// Метод для подсветки совпавших символов
    /// </summary>
    private void HighlightMatchedSymbols()
    {
        // Сброс цвета всех панелей до стандартного с прозрачностью
        foreach (var panel in panelImages)
        {
            panel.color = defaultPanelColor; // Используем стандартный цвет с прозрачностью
        }

        // Подсветка совпавших панелей ярким цветом без прозрачности
        foreach (var idx in matchedIndices)
        {
            if (idx >= 0 && idx < panelImages.Count)
            {
                panelImages[idx].color = highlightColor; // Используем яркий цвет без прозрачности
            }
        }
    }

    /// <summary>
    /// Метод для сброса подсветки панелей перед новым вращением
    /// </summary>
    private void ResetPanelHighlights()
    {
        foreach (var panel in panelImages)
        {
            panel.color = defaultPanelColor; // Устанавливаем стандартный цвет с прозрачностью
        }
    }
}
