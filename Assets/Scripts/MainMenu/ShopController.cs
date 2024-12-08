// ShopController.cs
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    [Header("Shop Items")]
    public List<ShopItem> shopItems;       // Список всех предметов магазина
    public GameObject shopItemPrefab;      // Префаб для предмета магазина
    public Transform itemsContainer;       // Контейнер для размещения предметов

    private List<ShopItemController> instantiatedItems = new List<ShopItemController>();

    private void Start()
    {
        PopulateShop();
    }

    // Метод для создания и инициализации предметов магазина
    private void PopulateShop()
    {
        if (shopItemPrefab == null)
        {
            Debug.LogError("ShopController: ShopItemPrefab не назначен в инспекторе.");
            return;
        }

        if (itemsContainer == null)
        {
            Debug.LogError("ShopController: ItemsContainer не назначен в инспекторе.");
            return;
        }

        foreach (ShopItem item in shopItems)
        {
            GameObject newItem = Instantiate(shopItemPrefab, itemsContainer);
            ShopItemController itemController = newItem.GetComponent<ShopItemController>();

            if (itemController != null)
            {
                itemController.shopItem = item;
                itemController.InitializeUI();
                instantiatedItems.Add(itemController);
            }
            else
            {
                Debug.LogError("ShopController: ShopItemPrefab не содержит компонента ShopItemController.");
            }
        }
    }

    // Метод для обновления состояния всех кнопок покупки при изменении монет
    private void UpdateAllPurchaseButtons(int totalCoins)
    {
        foreach (ShopItemController itemController in instantiatedItems)
        {
            itemController.UpdatePurchaseButtonState(totalCoins);
        }
    }

    private void OnEnable()
    {
        // Подписка на событие обновления монет
        if (GameData.Instance != null)
        {
            GameData.Instance.OnCoinsUpdated += UpdateAllPurchaseButtons;
        }
        else
        {
            Debug.LogError("ShopController: GameData.Instance не найден.");
        }
    }

    private void OnDisable()
    {
        // Отписка от события обновления монет
        if (GameData.Instance != null)
        {
            GameData.Instance.OnCoinsUpdated -= UpdateAllPurchaseButtons;
        }
    }
}
