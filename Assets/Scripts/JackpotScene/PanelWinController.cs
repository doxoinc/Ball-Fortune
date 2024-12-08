using UnityEngine;
using UnityEngine.UI;

public class PanelWinController : MonoBehaviour
{
    public Text winText; // Text component to display the reward amount
    public Button getButton; // "Get" Button
    private Animator animator; // Animator component for animations

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (getButton != null)
        {
            getButton.onClick.AddListener(OnGetButtonClicked);
        }
        else
        {
            Debug.LogError("PanelWinController: GetButton is not assigned in the Inspector.");
        }
    }

    private void OnDestroy()
    {
        if (getButton != null)
        {
            getButton.onClick.RemoveListener(OnGetButtonClicked);
        }
    }

    /// <summary>
    /// Метод для установки суммы выигрыша на панели
    /// </summary>
    /// <param name="amount">Сумма выигранных монет</param>
    public void SetRewardAmount(int amount)
    {
        if (winText != null)
        {
            winText.text = $"{amount}";
        }
    }

    /// <summary>
    /// Метод для отображения панели выигрыша с анимацией
    /// </summary>
    public void ShowPanel()
    {
        // Активируем панель сначала
        gameObject.SetActive(true);
        Debug.Log("PanelWinController: WinPanel activated.");

        if (animator != null)
        {
            animator.SetTrigger("Open"); // Используйте точное название триггера
            Debug.Log("PanelWinController: OpenTrigger set.");
        }
        else
        {
            Debug.LogError("PanelWinController: Animator is not assigned.");
        }
    }

    /// <summary>
    /// Метод для скрытия панели выигрыша с анимацией
    /// </summary>
    public void HidePanel()
    {
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("PanelWinController: HidePanel called on inactive GameObject.");
            return;
        }

        if (animator != null)
        {
            animator.SetTrigger("Close"); // Используйте точное название триггера
            Debug.Log("PanelWinController: CloseTrigger set.");
        }

        // Теперь панель будет скрыта через Animation Event
    }

    /// <summary>
    /// Метод, вызываемый через Animation Event после завершения анимации закрытия
    /// </summary>
    public void OnCloseAnimationComplete()
    {
        gameObject.SetActive(false);
        Debug.Log("PanelWinController: WinPanel deactivated.");
    }

    /// <summary>
    /// Callback метод при нажатии кнопки "Get"
    /// </summary>
    private void OnGetButtonClicked()
    {
        HidePanel();
    }
}
