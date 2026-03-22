using System;
using System.Collections.Generic;
using UnityEngine;
using Generation;
using Assets.Scripts;

[Serializable]
public class SaveData
{
    public Vector2Int gridSize;
    public Vector2 cellSize;
    public Vector2 gridSpacing;
    public Vector2 origin;
    public List<SavedCreature> creatures;
}
