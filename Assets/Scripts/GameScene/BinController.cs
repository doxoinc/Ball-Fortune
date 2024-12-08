using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinController : MonoBehaviour
{
    [Header("Difficulty Settings")]
    public List<BinDifficultySettings> difficultySettings; // Список настроек для каждой сложности

    private float currentMultiplier = 1f; // Текущий коэффициент умножения
    private float currentAddition = 0f;   // Текущий коэффициент прибавления

    private GameController gameController; // Ссылка на GameController
    private SpriteRenderer binSpriteRenderer; // Ссылка на компонент SpriteRenderer

    [Header("Animation Settings")]
    public float pressDownDistance = 0.2f; // Расстояние смещения вниз
    public float animationDuration = 0.1f; // Время анимации в секундах

    private bool isAnimating = false; // Флаг для предотвращения одновременной анимации
    private Vector3 originalPosition; // Исходная позиция ящика

    private void Awake()
    {
        // Кэшируем ссылку на GameController
        gameController = FindObjectOfType<GameController>();
        if (gameController == null)
        {
            Debug.LogError($"{gameObject.name}: GameController не найден в сцене.");
        }

        // Получаем компонент SpriteRenderer на этом объекте
        binSpriteRenderer = GetComponent<SpriteRenderer>();
        if (binSpriteRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: Компонент SpriteRenderer не найден. Пожалуйста, добавьте его к этому объекту.");
        }

        // Сохраняем исходную позицию
        originalPosition = transform.position;
    }

    // Метод для установки сложности
    public void SetDifficulty(DifficultyLevel difficulty)
    {
        BinDifficultySettings settings = difficultySettings.Find(s => s.difficultyLevel == difficulty);
        if (settings != null)
        {
            currentMultiplier = settings.multiplier;
            currentAddition = settings.addition;
            Debug.Log($"{gameObject.name}: Установлена сложность {difficulty} - Multiplier: {currentMultiplier}, Addition: {currentAddition}");

            // Обновляем спрайт ящика
            if (binSpriteRenderer != null && settings.binSprite != null)
            {
                binSpriteRenderer.sprite = settings.binSprite;
                Debug.Log($"{gameObject.name}: Спрайт ящика обновлен на {settings.binSprite.name}");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Спрайт не назначен для сложности {difficulty}");
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name}: Нет настроек для сложности {difficulty}");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            // Запускаем анимацию нажатия, если она ещё не запущена
            if (!isAnimating)
            {
                StartCoroutine(AnimatePress());
            }

            // Получаем текущую ставку из GameData
            int currentBet = GameData.Instance.GetCurrentBet();

            // Вычисляем итоговый счёт
            int score = Mathf.RoundToInt(currentBet * currentMultiplier + currentAddition);

            // Добавляем очки через GameController
            if (gameController != null)
            {
                gameController.AddScore(score);
            }
            else
            {
                Debug.LogError($"{gameObject.name}: GameController не доступен.");
            }

            // Возвращаем шарик в пул
            BallController ball = collision.GetComponent<BallController>();
            if (ball != null)
            {
                ball.DeactivateBall();
            }
            else
            {
                collision.gameObject.SetActive(false);
                Debug.LogError($"{gameObject.name}: BallController не найден на шарике.");
            }
        }
    }

    // Корутин для анимации нажатия
    private IEnumerator AnimatePress()
    {
        isAnimating = true;

        Vector3 targetPosition = originalPosition - new Vector3(0, pressDownDistance, 0);
        float elapsedTime = 0f;

        // Плавное смещение вниз
        while (elapsedTime < animationDuration)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPosition, (elapsedTime / animationDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        // Плавное возврат вверх
        elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            transform.position = Vector3.Lerp(targetPosition, originalPosition, (elapsedTime / animationDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;

        isAnimating = false;
    }
}
