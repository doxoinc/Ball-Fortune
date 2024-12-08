// BinDifficultySettings.cs
using UnityEngine;

[System.Serializable]
public class BinDifficultySettings
{
    public DifficultyLevel difficultyLevel; // Уровень сложности
    public float multiplier;               // Коэффициент умножения
    public float addition;                 // Коэффициент прибавления
    public Sprite binSprite;               // Спрайт для данного уровня сложности
}
