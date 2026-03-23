using System;
using System.Collections.Generic;
using UnityEngine;
using Generation;
using Assets.Scripts;

[Serializable]
public class SaveData
{
    public Vector2Int GridSize;
    public Vector2 CellSize;
    public Vector2 GridSpacing;
    public Vector2 Oorigin;
    public List<SavedCreature> Creatures;
}
