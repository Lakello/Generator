using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;

namespace Generation
{
    public class Node
    {
        public readonly Vector2Int Coord;
        public readonly Dictionary<Direction, Node> Neighbors;
        public readonly Cell Cell = new();

        public Node(Vector2Int coordinate)
        {
            Coord = coordinate;
            IEnumerable<Direction> directions = Enum.GetValues(typeof(Direction)).Cast<Direction>();

            Neighbors = new Dictionary<Direction, Node>();

            foreach (Direction direction in directions)
            {
                Neighbors[direction] = null;
            }
        }

        public Size CreatureSIze => Cell.Creature?.Size ?? Size.None;
        public Direction CreatureDirection => Cell.Creature?.Direction ?? Direction.None;
        public List<Cell> CreatureUsedCells => Cell.Creature?.UsedCells ?? null;
    }
}
