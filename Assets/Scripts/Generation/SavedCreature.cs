using System;
using System.Collections.Generic;
using UnityEngine;
using Generation;
using Assets.Scripts;

[Serializable]
public class SavedCreature
{
    public int Id;
    public Size Size;
    public CreatureDirection Direction;
    public List<Vector2Int> OccupiedCells;
    public Color Color;
}