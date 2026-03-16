using System;
using System.Collections.Generic;
using Assets.Scripts;

namespace Generation
{
    [Serializable]
    public class Creature
    {
        public Size Size;
        public Direction Direction;
        public List<Cell> UsedCells;
        public Node OriginNode;

        public void InvertDirection()
        {
            Direction = InvertDirection(Direction);
        }

        public Direction InvertDirection(Direction dir)
        {
            return dir switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
        }
    }
}
