using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanelPauseController : MonoBehaviour
{
    [Header("Buttons")]
    public Button soundButton; // Sound toggle button
    public Button musicButton; // Music toggle button
    public Button finishButton; // Finish button
    public Button exitButton; // Exit button

    [Header("Sprites")]
    public Sprite onSoundSprite; // Sprite for sound on
    public Sprite offSoundSprite; // Sprite for sound off
    public Sprite onMusicSprite; // Sprite for music on
    public Sprite offMusicSprite; // Sprite for music off

    private bool isSoundOn = true;
    private bool isMusicOn = true;

    private void Start()
    {
        // Проверка наличия кнопок
        if (soundButton != null)
            soundButton.onClick.AddListener(ToggleSound);
        else
            Debug.LogError("PanelPauseController: SoundButton is not assigned.");

        if (musicButton != null)
            musicButton.onClick.AddListener(ToggleMusic);
        else
            Debug.LogError("PanelPauseController: MusicButton is not assigned.");

        if (finishButton != null)
            finishButton.onClick.AddListener(FinishGame);
        else
            Debug.LogError("PanelPauseController: FinishButton is not assigned.");

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitPause);
        else
            Debug.LogError("PanelPauseController: ExitButton is not assigned.");

        // Инициализация состояния кнопок на основе настроек
        InitializeButtonStates();
    }

    private void OnDestroy()
    {
        if (soundButton != null)
            soundButton.onClick.RemoveListener(ToggleSound);

        if (musicButton != null)
            musicButton.onClick.RemoveListener(ToggleMusic);

        if (finishButton != null)
            finishButton.onClick.RemoveListener(FinishGame);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitPause);
    }

    /// <summary>
    /// Инициализация состояния кнопок на основе настроек пользователя
    /// </summary>
    private void InitializeButtonStates()
    {
        isSoundOn = GameData.Instance.SoundEnabled;
        isMusicOn = GameData.Instance.MusicEnabled;

        UpdateSoundButtonImage();
        UpdateMusicButtonImage();
    }

    /// <summary>
    /// Переключение звука
    /// </summary>
    private void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        GameData.Instance.SetSoundEnabled(isSoundOn);
        AudioManager.Instance.ToggleSound(isSoundOn);
        UpdateSoundButtonImage();
        AudioManager.Instance.PlayButtonClick();
    }

    /// <summary>
    /// Переключение музыки
    /// </summary>
    private void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        GameData.Instance.SetMusicEnabled(isMusicOn);
        AudioManager.Instance.ToggleMusic(isMusicOn);
        UpdateMusicButtonImage();
        AudioManager.Instance.PlayButtonClick();
    }

    /// <summary>
    /// Завершение игры и выход в главное меню
    /// </summary>
    private void FinishGame()
    {
        AudioManager.Instance.PlayButtonClick();

        // Возвращаем Time.timeScale к нормальному состоянию
        Time.timeScale = 1f;

        // Загружаем главное меню (убедитесь, что имя сцены совпадает)
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Выход из паузы и возобновление игры
    /// </summary>
    private void ExitPause()
    {
        AudioManager.Instance.PlayButtonClick();

        // Закрываем панель с анимацией
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Close");
        }

        // Возобновляем игру после завершения анимации
        StartCoroutine(ResumeGameAfterAnimation(animator));
    }

    /// <summary>
    /// Корутина для возобновления игры после анимации закрытия панели
    /// </summary>
    private System.Collections.IEnumerator ResumeGameAfterAnimation(Animator animator)
    {
        // Ждем завершения анимации закрытия (предполагается, что длительность 0.5 секунд)
        if (animator != null)
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }

        // Возобновляем игру
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Обновление изображения кнопки Sound
    /// </summary>
    private void UpdateSoundButtonImage()
    {
        if (soundButton != null)
        {
            Image buttonImage = soundButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = isSoundOn ? onSoundSprite : offSoundSprite;
            }
            else
            {
                Debug.LogError("PanelPauseController: SoundButton Image component not found.");
            }
        }
    }

    /// <summary>
    /// Обновление изображения кнопки Music
    /// </summary>
    private void UpdateMusicButtonImage()
    {
        if (musicButton != null)
        {
            Image buttonImage = musicButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = isMusicOn ? onMusicSprite : offMusicSprite;
            }
            else
            {
                Debug.LogError("PanelPauseController: MusicButton Image component not found.");
            }
        }
    }

    /// <summary>
    /// Метод для открытия панели с анимацией
    /// </summary>
    public void ShowPanel()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Open");
        }

        // Активируем панель
        gameObject.SetActive(true);

        // Приостанавливаем игру
        Time.timeScale = 0f;
    }
}
