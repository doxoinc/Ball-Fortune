// ShopItemController.cs
using UnityEngine;
using UnityEngine.UI;

public class ShopItemController : MonoBehaviour
{
    [Header("UI Elements")]
    public Image itemImage;               // Изображение предмета
    public Button purchaseButton;        // Кнопка покупки
    public Button plusButton;            // Кнопка "+"
    public Button minusButton;           // Кнопка "-"
    public Text quantityText;            // Текст отображения количества
    public Text totalCostText;           // Текст общей стоимости

    [Header("Shop Item Data")]
    public ShopItem shopItem;            // Данные предмета

    private int currentQuantity = 1;     // Текущее количество для покупки

    private void Start()
    {
        // Инициализация UI элементов
        InitializeUI();

        // Назначение слушателей кнопок
        purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
        plusButton.onClick.AddListener(OnPlusButtonClicked);
        minusButton.onClick.AddListener(OnMinusButtonClicked);

        // Подписка на событие обновления монет
        GameData.Instance.OnCoinsUpdated += UpdatePurchaseButtonState;

        // Обновление состояния кнопки покупки при запуске
        UpdatePurchaseButtonState(GameData.Instance.GetTotalCoins());
    }

    private void OnDestroy()
    {
        // Отписываемся от слушателей кнопок при уничтожении объекта
        purchaseButton.onClick.RemoveListener(OnPurchaseButtonClicked);
        plusButton.onClick.RemoveListener(OnPlusButtonClicked);
        minusButton.onClick.RemoveListener(OnMinusButtonClicked);

        // Отписываемся от события обновления монет
        if (GameData.Instance != null)
        {
            GameData.Instance.OnCoinsUpdated -= UpdatePurchaseButtonState;
        }
    }

    // Публичный метод инициализации UI элементов
    public void InitializeUI()
    {
        if (itemImage != null && shopItem.itemImage != null)
        {
            itemImage.sprite = shopItem.itemImage;
        }

        if (quantityText != null)
        {
            quantityText.text = currentQuantity.ToString();
        }

        UpdateTotalCost();
    }

    // Метод обновления общей стоимости
    private void UpdateTotalCost()
    {
        if (totalCostText != null)
        {
            int totalCost = shopItem.itemCost * currentQuantity;
            totalCostText.text = $"{totalCost}";
        }
    }

    // Метод обработки нажатия на кнопку "+"
    private void OnPlusButtonClicked()
    {
        if (currentQuantity < shopItem.maxPurchaseQuantity)
        {
            currentQuantity++;
            quantityText.text = currentQuantity.ToString();
            UpdateTotalCost();
            UpdatePurchaseButtonState(GameData.Instance.GetTotalCoins());
        }
    }

    // Метод обработки нажатия на кнопку "-"
    private void OnMinusButtonClicked()
    {
        if (currentQuantity > 1)
        {
            currentQuantity--;
            quantityText.text = currentQuantity.ToString();
            UpdateTotalCost();
            UpdatePurchaseButtonState(GameData.Instance.GetTotalCoins());
        }
    }

    // Метод обработки нажатия на кнопку "Purchase"
    private void OnPurchaseButtonClicked()
    {
        int totalCost = shopItem.itemCost * currentQuantity;
        int playerCoins = GameData.Instance.GetTotalCoins();

        if (playerCoins >= totalCost)
        {
            // Списание монет
            bool success = GameData.Instance.SpendCoins(totalCost);
            if (success)
            {
                Debug.Log($"Покупка успешна: {shopItem.itemName} x{currentQuantity} за {totalCost} монет.");
                
                // Сохранение купленного предмета в GameData
                GameData.Instance.AddPurchasedItem(shopItem.itemName, currentQuantity);
            }
            else
            {
                Debug.LogWarning("Не удалось списать монеты. Возможно, недостаточно монет.");
            }
        }
        else
        {
            Debug.Log("Недостаточно монет для покупки.");
            // Здесь можно добавить визуальное уведомление игроку (например, всплывающее сообщение)
        }
    }

    // Публичный метод обновления состояния кнопки покупки
    public void UpdatePurchaseButtonState(int totalCoins)
    {
        if (purchaseButton != null)
        {
            purchaseButton.interactable = totalCoins >= (shopItem.itemCost * currentQuantity);
        }
    }
}
