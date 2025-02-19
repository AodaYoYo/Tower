using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public LevelConfig[] levels;

    void Start()
    {
        GenerateLevels();
    }

    public void GenerateLevels()
    {
        levels = new LevelConfig[]
        {
        // Уровень 1: 3 шара
        new LevelConfig
        {
            startState = new List<Color> { Color.blue, Color.green, Color.red },
            targetState = new List<Color> { Color.red, Color.green, Color.blue }, // Порядок: красный, зеленый, синий
            moveLimit = 10,
            timeLimit = 60
        },
        // Уровень 2: 4 шара
        new LevelConfig
        {
            startState = new List<Color> { Color.blue, Color.green, Color.red, Color.yellow },
            targetState = new List<Color> { Color.red, Color.green, Color.blue, Color.yellow }, // Добавляем желтый
            moveLimit = 15,
            timeLimit = 90
        },
        // Уровень 3: 5 шаров
        new LevelConfig
        {
            startState = new List<Color> { Color.blue, Color.green, Color.red, Color.yellow, Color.magenta },
            targetState = new List<Color> { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta }, // Добавляем фиолетовый
            moveLimit = 20,
            timeLimit = 120
        }
        };
    }
}