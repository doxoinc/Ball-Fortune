using UnityEngine;
using UnityEngine.UI;

public class ItemUsageController : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName; // Название предмета, например, "Triple Ball"

    [Header("UI Elements")]
    public Button useButton; // Кнопка использования предмета
    public Text countText;   // Текст отображения количества доступных предметов

    [Header("Game Controller")]
    public GameController gameController; // Ссылка на GameController (назначьте через инспектор)

    private void Start()
    {
        if (useButton != null)
        {
            useButton.onClick.AddListener(OnUseButtonClicked);
            Debug.Log($"ItemUsageController: Listener добавлен для кнопки использования предмета {itemName}.");
        }
        else
        {
            Debug.LogError("ItemUsageController: UseButton не назначен в инспекторе.");
        }

        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
            if (gameController == null)
            {
                Debug.LogError("ItemUsageController: GameController не найден в сцене. Назначьте его через инспектор.");
            }
            else
            {
                Debug.Log("ItemUsageController: GameController найден через FindObjectOfType.");
            }
        }
        else
        {
            Debug.Log("ItemUsageController: GameController назначен через инспектор.");
        }

        UpdateCountText();
    }

    private void OnDestroy()
    {
        if (useButton != null)
        {
            useButton.onClick.RemoveListener(OnUseButtonClicked);
            Debug.Log($"ItemUsageController: Listener удален для кнопки использования предмета {itemName}.");
        }
    }

    /// <summary>
    /// Метод, вызываемый при нажатии на кнопку использования предмета
    /// </summary>
    private void OnUseButtonClicked()
    {
        Debug.Log($"ItemUsageController: Нажата кнопка использования предмета {itemName}.");

        if (GameData.Instance.UsePurchasedItem(itemName))
        {
            Debug.Log($"ItemUsageController: Использован предмет {itemName}.");
            ApplyItemEffect();
            UpdateCountText();
            // Дополнительная логика, например, воспроизведение звука или отображение уведомления
        }
        else
        {
            Debug.LogWarning($"ItemUsageController: Невозможно использовать {itemName}. Недостаточно предметов.");
            // Здесь можно добавить визуальное уведомление игроку
        }
    }

    /// <summary>
    /// Метод для применения эффекта предмета
    /// </summary>
    private void ApplyItemEffect()
    {
        if (itemName.Equals("TripleBall", System.StringComparison.OrdinalIgnoreCase) ||
            itemName.Equals("Triple Ball", System.StringComparison.OrdinalIgnoreCase))
        {
            if (gameController != null)
            {
                Debug.Log("ItemUsageController: Применяем эффект TripleBall.");
                gameController.ActivateTripleBall();
            }
            else
            {
                Debug.LogError("ItemUsageController: Ссылка на GameController отсутствует.");
            }
        }
        else if (itemName.Equals("Luck", System.StringComparison.OrdinalIgnoreCase))
        {
            // Реализуйте эффект для предмета "Luck" здесь
            Debug.Log("ItemUsageController: Эффект Luck пока не реализован.");
        }
        else if (itemName.Equals("Lightning", System.StringComparison.OrdinalIgnoreCase))
        {
            // Реализуйте эффект для предмета "Lightning" здесь
            Debug.Log("ItemUsageController: Эффект Lightning пока не реализован.");
        }
        else if (itemName.Equals("Drop", System.StringComparison.OrdinalIgnoreCase))
        {
            // Логика для кнопки "Drop" уже реализована в GameController
            Debug.Log("ItemUsageController: Эффект Drop уже обрабатывается в GameController.");
        }
        else
        {
            Debug.LogWarning($"ItemUsageController: Неизвестное название предмета '{itemName}'.");
        }
    }

    /// <summary>
    /// Метод для обновления текста с количеством доступных предметов
    /// </summary>
    public void UpdateCountText()
    {
        if (GameData.Instance != null && countText != null)
        {
            int count = GameData.Instance.GetPurchasedItemQuantity(itemName);
            countText.text = count.ToString();
            useButton.interactable = count > 0;
            Debug.Log($"ItemUsageController: Обновлено количество предметов {itemName}: {count}");
        }
    }
}
