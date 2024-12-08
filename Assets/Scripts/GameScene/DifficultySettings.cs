// DifficultySettings.cs
using UnityEngine;

[System.Serializable]
public class DifficultySettings
{
    public string difficultyName;           // Название сложности (например, "Easy")
    public Sprite defaultButtonImage;       // Изображение кнопки в состоянии Default
    public Sprite selectedButtonImage;      // Изображение кнопки в состоянии Selected
}
