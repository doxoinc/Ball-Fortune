using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("Sound Button")]
    public Button soundButton;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    public Image soundButtonImage;

    [Header("Music Button")]
    public Button musicButton;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;
    public Image musicButtonImage;

    private void Start()
    {
        // Инициализация состояния кнопок на основе сохранённых настроек
        UpdateSoundButton();
        UpdateMusicButton();

        // Добавление слушателей для кнопок
        soundButton.onClick.AddListener(ToggleSound);
        musicButton.onClick.AddListener(ToggleMusic);
    }

    // Метод для переключения звука
    private void ToggleSound()
    {
        bool newSoundState = !GameData.Instance.SoundEnabled;
        GameData.Instance.SetSoundEnabled(newSoundState);
        AudioManager.Instance.ToggleSound(newSoundState);
        UpdateSoundButton();
    }

    // Метод для переключения музыки
    private void ToggleMusic()
    {
        bool newMusicState = !GameData.Instance.MusicEnabled;
        GameData.Instance.SetMusicEnabled(newMusicState);
        AudioManager.Instance.ToggleMusic(newMusicState);
        UpdateMusicButton();
    }

    // Обновление спрайта кнопки звука
    private void UpdateSoundButton()
    {
        if (GameData.Instance.SoundEnabled)
        {
            soundButtonImage.sprite = soundOnSprite;
        }
        else
        {
            soundButtonImage.sprite = soundOffSprite;
        }
    }

    // Обновление спрайта кнопки музыки
    private void UpdateMusicButton()
    {
        if (GameData.Instance.MusicEnabled)
        {
            musicButtonImage.sprite = musicOnSprite;
        }
        else
        {
            musicButtonImage.sprite = musicOffSprite;
        }
    }
}
