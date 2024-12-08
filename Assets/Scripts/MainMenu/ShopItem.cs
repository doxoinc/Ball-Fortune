// ShopItem.cs
using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string itemName;            // Название предмета
    public Sprite itemImage;           // Изображение предмета
    public int itemCost;               // Стоимость одного предмета
    public int maxPurchaseQuantity = 99; // Максимальное количество для покупки
}
