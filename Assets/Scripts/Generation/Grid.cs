using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;

namespace Generation
{
    public class Grid
    {
        public Grid(Vector2Int gridSize)
        {
            FillGrid(gridSize);
            AttachNeighbors(gridSize);
        }

        public Dictionary<Vector2Int, Node> Nodes { get; } = new();

        public bool TryGetNearestEmpty(out Node nearestEmpty)
        {
            nearestEmpty = Nodes.Values.FirstOrDefault(n => n.Cell.Creature == null);

            return nearestEmpty != null;
        }

        private void FillGrid(Vector2Int gridSize)
        {
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2Int coordinate = new(i, y);
                    Nodes.Add(coordinate, new Node(coordinate));
                }
            }
        }

        private void AttachNeighbors(Vector2Int gridSize)
        {
            foreach (Vector2Int coordinate in Nodes.Keys)
            {
                Node node = Nodes[coordinate];

                if (coordinate.x - 1 > 0)
                {
                    node.Neighbors[Direction.Left] = Nodes[new(coordinate.x - 1, coordinate.y)];
                }

                if (coordinate.x + 1 < gridSize.x)
                {
                    node.Neighbors[Direction.Right] = Nodes[new(coordinate.x + 1, coordinate.y)];
                }

                if (coordinate.y - 1 > 0)
                {
                    node.Neighbors[Direction.Down] = Nodes[new(coordinate.x, coordinate.y - 1)];
                }

                if (coordinate.y + 1 < gridSize.y)
                {
                    node.Neighbors[Direction.Up] = Nodes[new(coordinate.x, coordinate.y + 1)];
                }
            }
        }
    }
}
