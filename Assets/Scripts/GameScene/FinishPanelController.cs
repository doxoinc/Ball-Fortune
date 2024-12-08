using UnityEngine;
using UnityEngine.UI;

public class FinishPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public Text scoreText;               // Текст отображения очков
    public Image difficultyImage;        // Изображение иконки сложности
    public Sprite easyDifficultyIcon;    // Спрайт иконки для Easy
    public Sprite normalDifficultyIcon;  // Спрайт иконки для Normal
    public Sprite hardDifficultyIcon;    // Спрайт иконки для Hard

    [Header("Buttons")]
    public Button exitButton; // Кнопка "Exit"

    private GameController gameController;
    private DifficultyManager difficultyManager;

    private void Awake()
    {
        // Кэшируем ссылки на GameController и DifficultyManager
        gameController = FindObjectOfType<GameController>();
        if (gameController == null)
        {
            Debug.LogError("FinishPanelController: GameController не найден в сцене.");
        }

        difficultyManager = FindObjectOfType<DifficultyManager>();
        if (difficultyManager == null)
        {
            Debug.LogError("FinishPanelController: DifficultyManager не найден в сцене.");
        }
    }

    private void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
        else
        {
            Debug.LogError("FinishPanelController: ExitButton is not assigned in the inspector.");
        }
    }

    private void OnEnable()
    {
        // Обновляем UI элементы при активации панели
        UpdateFinishPanel();
    }

    private void OnDestroy()
    {
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }
    }

    private void OnExitButtonClicked()
    {
        if (gameController != null)
        {
            gameController.ExitToMainMenu();
        }
    }

    private void UpdateFinishPanel()
    {
        // Установка текста с общим счётом
        if (gameController != null && scoreText != null)
        {
            scoreText.text = $"{gameController.TotalScore}";
        }
        else
        {
            Debug.LogError("FinishPanelController: GameController или scoreText не назначены.");
        }

        // Установка иконки сложности
        if (difficultyManager != null && difficultyImage != null)
        {
            DifficultyLevel currentDifficulty = difficultyManager.CurrentDifficulty;
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:
                    difficultyImage.sprite = easyDifficultyIcon;
                    break;
                case DifficultyLevel.Normal:
                    difficultyImage.sprite = normalDifficultyIcon;
                    break;
                case DifficultyLevel.Hard:
                    difficultyImage.sprite = hardDifficultyIcon;
                    break;
                default:
                    Debug.LogWarning("FinishPanelController: Неизвестный уровень сложности.");
                    break;
            }
        }
        else
        {
            Debug.LogError("FinishPanelController: DifficultyManager или difficultyImage не назначены.");
        }
    }
}
