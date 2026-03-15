namespace Generation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class Grid
	{
		public Grid(Vector2Int gridSize)
		{
			Nodes = new Dictionary<Vector2Int, Node>();

			for (int i = 0; i < gridSize.x; i++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					var coord = new Vector2Int(i, y);
					Nodes.Add(coord, new Node(coord));
				}
			}

			foreach (var coord in Nodes.Keys)
			{
				var node = Nodes[coord];

				if (coord.x - 1 > 0)
				{
					node.Neighbors[Direction.Left] = Nodes[new Vector2Int(coord.x - 1, coord.y)];
				}

				if (coord.x + 1 < gridSize.x)
				{
					node.Neighbors[Direction.Right] = Nodes[new Vector2Int(coord.x + 1, coord.y)];
				}

				if (coord.y - 1 > 0)
				{
					node.Neighbors[Direction.Up] = Nodes[new Vector2Int(coord.x, coord.y - 1)];
				}

				if (coord.y + 1 < gridSize.y)
				{
					node.Neighbors[Direction.Down] = Nodes[new Vector2Int(coord.x, coord.y + 1)];
				}
			}
		}

		public Dictionary<Vector2Int, Node> Nodes { get; }

		public bool TryGetNearestEmpty(out Node nearestEmpty)
		{
			nearestEmpty = Nodes.Values.FirstOrDefault(n => n.Cell.Creature == null);

			return nearestEmpty != null;
		}

		public class Node
		{
			public readonly Vector2Int Coord;
			public readonly Dictionary<Direction, Node> Neighbors;

			public readonly Cell Cell = new Cell();

			public Node(Vector2Int coord)
			{
				Coord = coord;
				var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>();

				Neighbors = new Dictionary<Direction, Node>();

				foreach (var direction in directions)
				{
					Neighbors[direction] = null;
				}
			}

			public Size CreatureSIze => Cell.Creature?.Size ?? Size.None;
			public Direction CreatureDirection => Cell.Creature?.Direction ?? Direction.None;
			public List<Cell> CreatureUsedCells => Cell.Creature?.UsedCells ?? null;
		}
	}
}