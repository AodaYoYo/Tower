using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelConfig
{
    public List<Color> startState;    
    public List<Color> targetState;   
    public int moveLimit;             
    public float timeLimit;           

}