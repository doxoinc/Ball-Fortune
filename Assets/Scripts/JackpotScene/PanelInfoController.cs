using UnityEngine;
using UnityEngine.UI;

public class PanelInfoController : MonoBehaviour
{
    public Button exitButton; // Exit button for InfoPanel

    private void Start()
    {
        if (exitButton != null)
            exitButton.onClick.AddListener(CloseInfoPanel);
        else
            Debug.LogError("PanelInfoController: ExitButton is not assigned.");
    }

    private void OnDestroy()
    {
        if (exitButton != null)
            exitButton.onClick.RemoveListener(CloseInfoPanel);
    }

    private void CloseInfoPanel()
    {
        AudioManager.Instance.PlayButtonClick();
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Close");
        }

        StartCoroutine(HideAfterAnimation(animator));
    }

    private System.Collections.IEnumerator HideAfterAnimation(Animator animator)
    {
        // Wait for animation to finish (assume 0.5 seconds)
        if (animator != null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // Hide the panel
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Метод для открытия панели с анимацией
    /// </summary>
    public void ShowPanel()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Open");
        }

        // Активируем панель
        gameObject.SetActive(true);
    }
}
