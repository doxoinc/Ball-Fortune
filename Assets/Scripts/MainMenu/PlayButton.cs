using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PlayButton : MonoBehaviour
{
    [Header("Play Button Settings")]
    [SerializeField] private Button playButton; // Ссылка на кнопку Play
    [SerializeField] private string gameSceneName = "GameScene"; // Имя сцены для загрузки при нажатии Play

    [Header("Jackpot Button Settings")]
    [SerializeField] private Button jackpotButton; // Ссылка на кнопку Jackpot
    [SerializeField] private string jackpotSceneName = "JackpotScene"; // Имя сцены для загрузки при нажатии Jackpot

    [Header("Loading Screen Settings")]
    [SerializeField] private GameObject loadingScreen; // Ссылка на объект экрана загрузки (опционально)
    [SerializeField] private Slider loadingSlider; // Ссылка на слайдер загрузки (опционально)
    [SerializeField] private Text loadingText; // Ссылка на текст загрузки (например, "Loading...")

    private void Start()
    {
        // Назначение слушателей кнопок Play и Jackpot
        if (playButton != null)
        {
            playButton.onClick.AddListener(() => OnPlayButtonClicked(gameSceneName));
            Debug.Log("PlayButton: PlayButton слушатель добавлен.");
        }
        else
        {
            Debug.LogError("PlayButton: PlayButton не назначен в инспекторе.");
        }

        if (jackpotButton != null)
        {
            jackpotButton.onClick.AddListener(() => OnPlayButtonClicked(jackpotSceneName));
            Debug.Log("PlayButton: JackpotButton слушатель добавлен.");
        }
        else
        {
            Debug.LogError("PlayButton: JackpotButton не назначен в инспекторе.");
        }

        // Скрыть экран загрузки при старте
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
            Debug.Log("PlayButton: LoadingScreen скрыт при старте.");
        }

        // Проверка наличия слайдера загрузки и текста
        if (loadingSlider == null)
        {
            Debug.LogWarning("PlayButton: LoadingSlider не назначен в инспекторе.");
        }

        if (loadingText == null)
        {
            Debug.LogWarning("PlayButton: LoadingText не назначен в инспекторе.");
        }
    }

    private void OnDestroy()
    {
        // Отписка от событий кнопок при уничтожении объекта
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            Debug.Log("PlayButton: PlayButton слушатель удален.");
        }

        if (jackpotButton != null)
        {
            jackpotButton.onClick.RemoveAllListeners();
            Debug.Log("PlayButton: JackpotButton слушатель удален.");
        }
    }

    /// <summary>
    /// Метод, вызываемый при нажатии на любую из кнопок загрузки сцен
    /// </summary>
    /// <param name="sceneName">Имя сцены для загрузки</param>
    private void OnPlayButtonClicked(string sceneName)
    {
        Debug.Log($"PlayButton: Кнопка для загрузки сцены '{sceneName}' нажата.");

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true); // Показать экран загрузки
            Debug.Log("PlayButton: LoadingScreen показан.");
        }

        StartCoroutine(LoadSceneAsync(sceneName));
    }

    /// <summary>
    /// Корутин для асинхронной загрузки сцены с отображением прогресса
    /// </summary>
    /// <param name="sceneName">Имя сцены для загрузки</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        Debug.Log($"PlayButton: Начата загрузка сцены '{sceneName}'.");

        while (!asyncLoad.isDone)
        {
            // Обновление слайдера загрузки
            if (loadingSlider != null)
            {
                // asyncLoad.progress достигает 0.9 при завершении загрузки
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                loadingSlider.value = progress;
                Debug.Log($"PlayButton: Прогресс загрузки: {progress * 100}%");
            }

            // Обновление текста загрузки
            if (loadingText != null)
            {
                loadingText.text = "Loading...";
            }

            // Проверка, достигнута ли загрузка 90%
            if (asyncLoad.progress >= 0.9f)
            {
                // Можно добавить задержку или ожидание действия пользователя
                // Например, автоматическое переключение после небольшой задержки
                yield return new WaitForSeconds(1f);
                asyncLoad.allowSceneActivation = true;
                Debug.Log($"PlayButton: Разрешено переключение на сцену '{sceneName}'.");
            }

            yield return null;
        }

        Debug.Log($"PlayButton: Загрузка сцены '{sceneName}' завершена.");
    }
}
