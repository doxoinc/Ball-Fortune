using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource menuMusicSource;
    public AudioSource gameMusicSource;
    public AudioSource soundEffectsSource;

    [Header("Sound Effects")]
    public AudioClip buttonClickClip; // Звуковой эффект для нажатия кнопки

    private void Awake()
    {
        // Реализация Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Обработка загрузки новой сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusicForScene(scene.name);
    }

    // Инициализация аудио на основе настроек
    private void InitializeAudioSettings()
    {
        // Установите фиксированную громкость
        float fixedVolume = 0.3f;
        Debug.Log("Initializing Audio Settings. Fixed Volume set to " + fixedVolume);

        // Настройка громкости и состояния звука эффектов
        if (soundEffectsSource != null)
        {
            soundEffectsSource.volume = 1f; // Можно настроить при необходимости
            soundEffectsSource.mute = !GameData.Instance.SoundEnabled;
            Debug.Log("Sound Effects Enabled: " + GameData.Instance.SoundEnabled);
        }

        // Настройка громкости и состояния музыки меню
        if (menuMusicSource != null)
        {
            menuMusicSource.volume = fixedVolume;
            menuMusicSource.mute = !GameData.Instance.MusicEnabled;
            Debug.Log("Menu Music Enabled: " + GameData.Instance.MusicEnabled);
            if (GameData.Instance.MusicEnabled && !menuMusicSource.isPlaying && menuMusicSource.clip != null)
            {
                menuMusicSource.Play();
                Debug.Log("Menu Music Started");
            }
        }

        // Настройка громкости и состояния музыки игры
        if (gameMusicSource != null)
        {
            gameMusicSource.volume = fixedVolume;
            gameMusicSource.mute = true; // Изначально музыка игры не играет
            Debug.Log("Game Music Initially Muted");
        }
    }

    // Обновление музыки в зависимости от сцены
    private void UpdateMusicForScene(string sceneName)
    {
        bool isMainMenu = sceneName == "MainMenu"; // Замените "MainMenu" на точное название вашей сцены меню
        Debug.Log("Is Main Menu: " + isMainMenu);

        if (GameData.Instance.MusicEnabled)
        {
            if (isMainMenu)
            {
                // Включить музыку меню и выключить музыку игры
                if (menuMusicSource != null)
                {
                    menuMusicSource.mute = false;
                    if (!menuMusicSource.isPlaying && menuMusicSource.clip != null)
                    {
                        menuMusicSource.Play();
                        Debug.Log("Menu Music Playing");
                    }
                }

                if (gameMusicSource != null)
                {
                    gameMusicSource.mute = true;
                    if (gameMusicSource.isPlaying)
                    {
                        gameMusicSource.Stop();
                        Debug.Log("Game Music Stopped");
                    }
                }
            }
            else
            {
                // Включить музыку игры и выключить музыку меню
                if (gameMusicSource != null)
                {
                    gameMusicSource.mute = false;
                    if (!gameMusicSource.isPlaying && gameMusicSource.clip != null)
                    {
                        gameMusicSource.Play();
                        Debug.Log("Game Music Playing");
                    }
                }

                if (menuMusicSource != null)
                {
                    menuMusicSource.mute = true;
                    if (menuMusicSource.isPlaying)
                    {
                        menuMusicSource.Stop();
                        Debug.Log("Menu Music Stopped");
                    }
                }
            }
        }
        else
        {
            // Если музыка выключена, остановить все источники музыки
            if (menuMusicSource != null && menuMusicSource.isPlaying)
            {
                menuMusicSource.Stop();
                Debug.Log("Menu Music Stopped (Music Disabled)");
            }

            if (gameMusicSource != null && gameMusicSource.isPlaying)
            {
                gameMusicSource.Stop();
                Debug.Log("Game Music Stopped (Music Disabled)");
            }
        }
    }

    // Включение/Выключение звуковых эффектов
    public void ToggleSound(bool isEnabled)
    {
        if (soundEffectsSource != null)
        {
            soundEffectsSource.mute = !isEnabled;
            Debug.Log("Sound Effects " + (isEnabled ? "Enabled" : "Disabled"));
        }
    }

    // Включение/Выключение музыки
    public void ToggleMusic(bool isEnabled)
    {
        if (GameData.Instance.MusicEnabled != isEnabled)
        {
            GameData.Instance.SetMusicEnabled(isEnabled);
        }

        if (isEnabled)
        {
            // Включаем музыку, исходя из текущей сцены
            string currentScene = SceneManager.GetActiveScene().name;
            UpdateMusicForScene(currentScene);
        }
        else
        {
            // Выключаем всю музыку
            if (menuMusicSource != null && menuMusicSource.isPlaying)
            {
                menuMusicSource.Stop();
                Debug.Log("Menu Music Stopped (ToggleMusic)");
            }

            if (gameMusicSource != null && gameMusicSource.isPlaying)
            {
                gameMusicSource.Stop();
                Debug.Log("Game Music Stopped (ToggleMusic)");
            }
        }
    }

    // Метод для воспроизведения звукового эффекта кнопки
    public void PlayButtonClick()
    {
        if (soundEffectsSource != null && buttonClickClip != null && GameData.Instance.SoundEnabled)
        {
            soundEffectsSource.PlayOneShot(buttonClickClip);
            Debug.Log("Button Click Sound Played");
        }
    }
}
