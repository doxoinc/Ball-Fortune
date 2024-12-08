using UnityEngine;
using UnityEngine.UI;

public class BetSelector : MonoBehaviour
{
    [Header("UI Elements")]
    public Button minusButton;     // Кнопка "-"
    public Button plusButton;      // Кнопка "+"
    public Text betDisplayText;    // Текст отображения ставки

    private void Start()
    {
        // Назначение слушателей кнопок
        if (minusButton != null)
        {
            minusButton.onClick.AddListener(OnMinusButtonClicked);
        }
        else
        {
            Debug.LogError("BetSelector: MinusButton is not assigned in the inspector.");
        }

        if (plusButton != null)
        {
            plusButton.onClick.AddListener(OnPlusButtonClicked);
        }
        else
        {
            Debug.LogError("BetSelector: PlusButton is not assigned in the inspector.");
        }

        // Инициализация отображения ставки
        UpdateBetDisplay();
    }

    private void OnDestroy()
    {
        // Отписываемся от событий кнопок
        if (minusButton != null)
        {
            minusButton.onClick.RemoveListener(OnMinusButtonClicked);
        }

        if (plusButton != null)
        {
            plusButton.onClick.RemoveListener(OnPlusButtonClicked);
        }
    }

    // Метод для уменьшения ставки
    private void OnMinusButtonClicked()
    {
        int currentBet = GameData.Instance.GetCurrentBet();
        if (currentBet > 1)
        {
            GameData.Instance.SetCurrentBet(currentBet - 1);
            UpdateBetDisplay();
        }
    }

    // Метод для увеличения ставки
    private void OnPlusButtonClicked()
    {
        int currentBet = GameData.Instance.GetCurrentBet();
        if (currentBet < 50)
        {
            GameData.Instance.SetCurrentBet(currentBet + 1);
            UpdateBetDisplay();
        }
    }

    // Метод для обновления отображения ставки
    private void UpdateBetDisplay()
    {
        if (betDisplayText != null && GameData.Instance != null)
        {
            betDisplayText.text = GameData.Instance.GetCurrentBet().ToString();
        }
    }
}
