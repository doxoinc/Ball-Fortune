using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyManager : MonoBehaviour
{
    [Header("Difficulty Settings")]
    public List<DifficultySettings> difficulties;    // Список настроек для всех сложностей

    [Header("Buttons")]
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;

    private Button selectedButton;

    public DifficultyLevel CurrentDifficulty { get; private set; } // Текущее выбранное значение сложности

    private void Start()
    {
        // Назначаем слушатели для кнопок
        easyButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Easy));
        normalButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Normal));
        hardButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Hard));

        // Загружаем сохранённую сложность или устанавливаем начальную
        DifficultyLevel savedDifficulty = LoadSavedDifficulty();
        OnDifficultySelected(savedDifficulty);
    }

    private DifficultyLevel LoadSavedDifficulty()
    {
        if (PlayerPrefs.HasKey("SelectedDifficulty"))
        {
            string difficultyName = PlayerPrefs.GetString("SelectedDifficulty");
            if (System.Enum.TryParse(difficultyName, out DifficultyLevel difficulty))
            {
                return difficulty;
            }
            else
            {
                Debug.LogError($"DifficultyManager: Не удалось распознать сохранённую сложность '{difficultyName}'. Устанавливаем Normal.");
                return DifficultyLevel.Normal;
            }
        }
        else
        {
            // Если сложность не сохранена, устанавливаем Normal по умолчанию
            return DifficultyLevel.Normal;
        }
    }

    private void OnDifficultySelected(DifficultyLevel difficulty)
    {
        CurrentDifficulty = difficulty; // Устанавливаем текущее значение сложности

        // Сохранение выбранной сложности
        PlayerPrefs.SetString("SelectedDifficulty", difficulty.ToString());
        PlayerPrefs.Save();

        // Обновляем коэффициенты всех ящиков
        UpdateBinCoefficients(difficulty);

        // Обновляем иконки кнопок
        UpdateButtonImages(difficulty);
    }

    private void UpdateBinCoefficients(DifficultyLevel difficulty)
    {
        // Найти все ящики в сцене и обновить их коэффициенты
        BinController[] bins = FindObjectsOfType<BinController>();
        foreach (BinController bin in bins)
        {
            bin.SetDifficulty(difficulty);
        }

        Debug.Log($"DifficultyManager: Коэффициенты ящиков обновлены до {difficulty}");
    }

    private void UpdateButtonImages(DifficultyLevel selectedDifficulty)
    {
        // Сброс иконок всех кнопок до Default
        SetButtonImage(easyButton, DifficultyLevel.Easy == selectedDifficulty);
        SetButtonImage(normalButton, DifficultyLevel.Normal == selectedDifficulty);
        SetButtonImage(hardButton, DifficultyLevel.Hard == selectedDifficulty);
    }

    private void SetButtonImage(Button button, bool isSelected)
    {
        Sprite selectedSprite = null;
        Sprite defaultSprite = null;

        // Определяем, какой уровень сложности соответствует этой кнопке
        DifficultySettings difficultySettings = null;
        if (button == easyButton)
            difficultySettings = difficulties.Find(d => d.difficultyName == "Easy");
        else if (button == normalButton)
            difficultySettings = difficulties.Find(d => d.difficultyName == "Normal");
        else if (button == hardButton)
            difficultySettings = difficulties.Find(d => d.difficultyName == "Hard");

        if (difficultySettings == null)
        {
            Debug.LogError($"DifficultyManager: Не найдены настройки сложности для кнопки {button.name}.");
            return;
        }

        defaultSprite = difficultySettings.defaultButtonImage;
        selectedSprite = difficultySettings.selectedButtonImage;

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage == null)
        {
            Debug.LogError($"DifficultyManager: Кнопка {button.name} не имеет компонента Image.");
            return;
        }

        // Устанавливаем соответствующее изображение
        if (isSelected)
        {
            buttonImage.sprite = selectedSprite;
        }
        else
        {
            buttonImage.sprite = defaultSprite;
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от слушателей кнопок
        easyButton.onClick.RemoveListener(() => OnDifficultySelected(DifficultyLevel.Easy));
        normalButton.onClick.RemoveListener(() => OnDifficultySelected(DifficultyLevel.Normal));
        hardButton.onClick.RemoveListener(() => OnDifficultySelected(DifficultyLevel.Hard));
    }
}
