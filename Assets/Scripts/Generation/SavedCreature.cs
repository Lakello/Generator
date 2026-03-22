using System;
using System.Collections.Generic;
using UnityEngine;
using Generation;
using Assets.Scripts;

[Serializable]
public class SavedCreature
{
    public int id;
    public Size size;
    public CreatureDirection direction;
    public List<Vector2Int> occupiedCells;
    public Color color;
}