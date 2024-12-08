using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [System.Serializable]
    public class MenuPanel
    {
        public string panelName;          // Название панели
        public GameObject panelObject;    // Объект панели
        public Animator panelAnimator;    // Animator панели
    }

    [System.Serializable]
    public class UIElement
    {
        public string elementName;        // Название элемента (для удобства)
        public GameObject elementObject;  // Объект UI элемента
        public Animator elementAnimator;  // Animator UI элемента
    }

    public List<MenuPanel> menuPanels;          // Список всех панелей в меню
    public List<UIElement> uiElementsToManage;  // Список UI объектов для скрытия/показа

    // Панель приветствия
    public GameObject welcomePanel;             // Панель приветствия
    public Button getButton;                    // Кнопка "Get"

    // Текст для отображения баланса монет
    public Text coinText;                       // Текстовый элемент для монет

    // Родительский объект для кнопок
    public GameObject buttonContainer;          // Объект ButtonContainer

    private MenuPanel currentPanel;             // Текущая открытая панель
    private Dictionary<MenuPanel, Coroutine> panelCoroutines = new Dictionary<MenuPanel, Coroutine>();

    void Start()
    {
        // Проверка наличия GameData
        if (GameData.Instance == null)
        {
            Debug.LogError("GameData instance not found in the scene. Please add a GameData object.");
            return;
        }

        // Инициализируем все панели как закрытые при старте
        foreach (var panel in menuPanels)
        {
            if (panel.panelAnimator != null)
            {
                panel.panelAnimator.Play("Closed");
            }
            panel.panelObject.SetActive(false);
        }

        // Инициализируем все UI элементы как активные
        foreach (var uiElement in uiElementsToManage)
        {
            if (uiElement.elementAnimator != null)
            {
                // Предполагается, что начальное состояние UI элементов — видимое
                uiElement.elementAnimator.Play("Show");
            }
            uiElement.elementObject.SetActive(true);
        }

        // Подписываемся на событие изменения монет для обновления UI
        CoinManager.Instance.OnCoinChanged += UpdateCoinUI;
        // Инициализируем UI с текущим балансом монет
        UpdateCoinUI(CoinManager.Instance.GetTotalCoins());

        // Проверяем, первый ли запуск игры
        if (GameData.Instance.GetFirstRun())
        {
            ShowWelcomePanel();
        }
        else
        {
            // Убедитесь, что ButtonContainer активен, если это не первый запуск
            if (buttonContainer != null)
            {
                buttonContainer.SetActive(true);
            }
        }
    }

    void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnCoinChanged -= UpdateCoinUI;
        }
    }

    // Метод для открытия панели по имени
    public void OpenPanel(string panelName)
    {
        Debug.Log("Attempting to open panel: " + panelName);
        // Находим панель по имени
        MenuPanel targetPanel = menuPanels.Find(p => p.panelName == panelName);
        if (targetPanel == null)
        {
            Debug.LogError("Panel " + panelName + " не найдена!");
            return;
        }

        // Если текущая панель не та, что нужно открыть, закрываем её
        if (currentPanel != null && currentPanel != targetPanel)
        {
            ClosePanel(currentPanel);
        }

        // Если панель уже открыта, ничего не делаем
        if (currentPanel == targetPanel)
        {
            return;
        }

        // Активируем и открываем целевую панель
        targetPanel.panelObject.SetActive(true);
        if (targetPanel.panelAnimator != null)
        {
            // Сброс триггеров, чтобы избежать конфликтов
            targetPanel.panelAnimator.ResetTrigger("ClosePanel");
            targetPanel.panelAnimator.SetTrigger("OpenPanel");
        }

        // Скрываем UI элементы
        HideUIElements();

        // Останавливаем любую корутину закрытия, если есть
        if (panelCoroutines.ContainsKey(targetPanel))
        {
            StopCoroutine(panelCoroutines[targetPanel]);
            panelCoroutines.Remove(targetPanel);
        }

        // Устанавливаем текущую панель
        currentPanel = targetPanel;
    }

    // Метод для закрытия текущей панели
    public void CloseCurrentPanel()
    {
        if (currentPanel != null)
        {
            ClosePanel(currentPanel);
        }
    }

    // Метод для закрытия конкретной панели
    public void ClosePanel(MenuPanel panel)
    {
        Debug.Log("Closing panel: " + panel.panelName);
        if (panel != null && panel.panelAnimator != null)
        {
            panel.panelAnimator.SetTrigger("ClosePanel");

            // Запускаем корутину деактивации панели после завершения анимации закрытия
            Coroutine deactivateCoroutine = StartCoroutine(DisablePanelAfterAnimation(panel));
            panelCoroutines[panel] = deactivateCoroutine;
        }

        // Проверяем, нужно ли показать UI элементы обратно
        // Если нет других открытых панелей, показываем UI элементы
        if (!IsAnyPanelOpenExcept(panel))
        {
            ShowUIElements();
        }
    }

    // Корутина для деактивации панели после завершения анимации закрытия
    private IEnumerator DisablePanelAfterAnimation(MenuPanel panel)
    {
        // Получаем длину анимации закрытия
        float waitTime = GetAnimationLength(panel.panelAnimator, "Close");
        yield return new WaitForSeconds(waitTime);

        panel.panelObject.SetActive(false);
        panelCoroutines.Remove(panel);

        // Сбрасываем currentPanel, если закрытая панель была текущей
        if (currentPanel == panel)
        {
            currentPanel = null;
        }
    }

    // Вспомогательный метод для получения длины анимации
    private float GetAnimationLength(Animator animator, string stateName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == stateName)
            {
                return clip.length;
            }
        }
        return 0.5f; // Значение по умолчанию
    }

    // Метод для скрытия UI элементов
    private void HideUIElements()
    {
        Debug.Log("Hiding UI Elements");
        foreach (var uiElement in uiElementsToManage)
        {
            if (uiElement.elementAnimator != null)
            {
                uiElement.elementAnimator.ResetTrigger("Show");
                uiElement.elementAnimator.SetTrigger("Hide");
            }
            // Отключаем интерактивность
            var button = uiElement.elementObject.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }

    // Метод для показа UI элементов
    private void ShowUIElements()
    {
        Debug.Log("Showing UI Elements");
        foreach (var uiElement in uiElementsToManage)
        {
            if (uiElement.elementAnimator != null)
            {
                uiElement.elementAnimator.ResetTrigger("Hide");
                uiElement.elementAnimator.SetTrigger("Show");
            }
            // Включаем интерактивность
            var button = uiElement.elementObject.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = true;
            }
        }
    }

    // Проверка, есть ли открытые панели, кроме указанной
    private bool IsAnyPanelOpenExcept(MenuPanel excludedPanel)
    {
        foreach (var panel in menuPanels)
        {
            if (panel != excludedPanel && panel.panelObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    // Метод для кнопки "Закрыть" на панели
    public void OnCloseButton()
    {
        CloseCurrentPanel();
    }

    // Метод, вызываемый событием Animator по завершении анимации закрытия
    public void OnCloseAnimationComplete(string panelName)
    {
        Debug.Log("Close animation completed for panel: " + panelName);
        MenuPanel panel = menuPanels.Find(p => p.panelName == panelName);
        if (panel != null)
        {
            panel.panelObject.SetActive(false);
            if (currentPanel == panel)
            {
                currentPanel = null;
            }

            // Показать UI элементы, если нет других открытых панелей
            if (!IsAnyPanelOpenExcept(panel))
            {
                ShowUIElements();
            }

            // Удаляем корутину из словаря, если она всё ещё там
            if (panelCoroutines.ContainsKey(panel))
            {
                panelCoroutines.Remove(panel);
            }
        }
    }

    // Метод для отображения панели приветствия
    private void ShowWelcomePanel()
    {
        welcomePanel.SetActive(true);
        // Скрываем ButtonContainer
        if (buttonContainer != null)
        {
            buttonContainer.SetActive(false);
        }

        // Добавляем слушатель на кнопку "Get"
        if (getButton != null)
        {
            getButton.onClick.AddListener(OnGetButtonClicked);
        }
    }

    // Метод, вызываемый при нажатии на кнопку "Get" в приветственной панели
    private void OnGetButtonClicked()
    {
        // Добавляем 500 монет через CoinManager
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoins(500);
        }

        // Обновляем UI монет
        UpdateCoinUI(CoinManager.Instance.GetTotalCoins());

        // Сохраняем, что пользователь видел приветствие
        GameData.Instance.SetFirstRun(false);

        // Закрываем приветственную панель
        welcomePanel.SetActive(false);

        // Показываем ButtonContainer
        if (buttonContainer != null)
        {
            buttonContainer.SetActive(true);
        }

        // Удаляем слушатель, чтобы избежать повторных вызовов
        if (getButton != null)
        {
            getButton.onClick.RemoveListener(OnGetButtonClicked);
        }
    }

    // Метод для обновления UI элемента монет
    private void UpdateCoinUI(int newTotal)
    {
        if (coinText != null)
        {
            coinText.text = newTotal.ToString();
        }
    }
}
