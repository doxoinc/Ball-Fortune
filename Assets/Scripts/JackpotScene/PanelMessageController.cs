using UnityEngine;
using UnityEngine.UI;

public class PanelMessageController : MonoBehaviour
{
    public Text messageText; // Text component to display the message

    /// <summary>
    /// Method to show a message on the panel
    /// </summary>
    /// <param name="message">The message to display</param>
    public void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Method to hide the message panel
    /// </summary>
    public void HideMessage()
    {
        messageText.text = "";
        gameObject.SetActive(false);
    }
}
