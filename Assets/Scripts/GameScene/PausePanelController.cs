using UnityEngine;
using UnityEngine.UI;

public class PausePanelController : MonoBehaviour
{
    [Header("Buttons")]
    public Button finishButton;   // Кнопка "Finish"
    public Button exitButton;     // Кнопка "Exit"
    public Button musicButton;    // Кнопка "Music"
    public Button soundsButton;   // Кнопка "Sounds"

    [Header("Music Button Sprites")]
    public Sprite musicOnSprite;  // Спрайт для включенной музыки
    public Sprite musicOffSprite; // Спрайт для выключенной музыки

    [Header("Sounds Button Sprites")]
    public Sprite soundsOnSprite;  // Спрайт для включенных звуков
    public Sprite soundsOffSprite; // Спрайт для выключенных звуков

    private GameController gameController;

    private void Start()
    {
        gameController = FindObjectOfType<GameController>();

        // Назначение слушателей кнопок
        if (finishButton != null)
        {
            finishButton.onClick.AddListener(OnFinishButtonClicked);
        }
        else
        {
            Debug.LogError("PausePanelController: FinishButton is not assigned in the inspector.");
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
        else
        {
            Debug.LogError("PausePanelController: ExitButton is not assigned in the inspector.");
        }

        if (musicButton != null)
        {
            musicButton.onClick.AddListener(OnMusicButtonClicked);
            UpdateMusicButtonImage();
        }
        else
        {
            Debug.LogError("PausePanelController: MusicButton is not assigned in the inspector.");
        }

        if (soundsButton != null)
        {
            soundsButton.onClick.AddListener(OnSoundsButtonClicked);
            UpdateSoundsButtonImage();
        }
        else
        {
            Debug.LogError("PausePanelController: SoundsButton is not assigned in the inspector.");
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий кнопок при уничтожении объекта
        if (finishButton != null)
        {
            finishButton.onClick.RemoveListener(OnFinishButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }

        if (musicButton != null)
        {
            musicButton.onClick.RemoveListener(OnMusicButtonClicked);
        }

        if (soundsButton != null)
        {
            soundsButton.onClick.RemoveListener(OnSoundsButtonClicked);
        }
    }

    private void OnFinishButtonClicked()
    {
        if (gameController != null)
        {
            gameController.EndGame();
        }

        // Закрываем PausePanel и возобновляем игру
        if (gameController != null)
        {
            gameController.ResumeGame();
        }

        // Воспроизводим звуковой эффект нажатия кнопки
        AudioManager.Instance?.PlayButtonClick();
    }

    private void OnExitButtonClicked()
    {
        if (gameController != null)
        {
            gameController.ResumeGame();
        }

        // Воспроизводим звуковой эффект нажатия кнопки
        AudioManager.Instance?.PlayButtonClick();
    }

    private void OnMusicButtonClicked()
    {
        bool isMusicEnabled = GameData.Instance.MusicEnabled;
        // Переключаем состояние музыки
        AudioManager.Instance.ToggleMusic(!isMusicEnabled);
        // Обновляем изображение кнопки
        UpdateMusicButtonImage();

        // Воспроизводим звуковой эффект нажатия кнопки
        AudioManager.Instance?.PlayButtonClick();
    }

    private void OnSoundsButtonClicked()
    {
        bool isSoundsEnabled = GameData.Instance.SoundEnabled;
        // Переключаем состояние звуков
        AudioManager.Instance.ToggleSound(!isSoundsEnabled);
        // Обновляем изображение кнопки
        UpdateSoundsButtonImage();

        // Воспроизводим звуковой эффект нажатия кнопки
        AudioManager.Instance?.PlayButtonClick();
    }

    private void UpdateMusicButtonImage()
    {
        if (musicButton != null && musicButton.image != null)
        {
            if (GameData.Instance.MusicEnabled)
            {
                musicButton.image.sprite = musicOnSprite;
            }
            else
            {
                musicButton.image.sprite = musicOffSprite;
            }
        }
    }

    private void UpdateSoundsButtonImage()
    {
        if (soundsButton != null && soundsButton.image != null)
        {
            if (GameData.Instance.SoundEnabled)
            {
                soundsButton.image.sprite = soundsOnSprite;
            }
            else
            {
                soundsButton.image.sprite = soundsOffSprite;
            }
        }
    }
}
