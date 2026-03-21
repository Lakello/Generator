namespace Generation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Assets.Scripts;
	using Sirenix.OdinInspector;
	using UnityEngine;

	[BoxGroup("Grid")]
	[HideLabel]
	[Serializable]
	public class Grid
	{
		[SerializeField] private Vector2Int _gridSize;
		[SerializeField] private Vector2 _cellSize;
		[SerializeField] private Vector2 _gridSpacing;
		[SerializeField] private Vector2 _origin;
		[SerializeField] private Node[] _nodes;
		
		public Dictionary<Vector2Int, Node> Nodes { get; } = new();

		public void Generate()
		{
			FillGrid(_gridSize);
			AttachNeighbors(_gridSize);
		}

		public bool TryGetNearestEmpty(out Node nearestEmpty)
		{
			nearestEmpty = Nodes.Values.FirstOrDefault(n => n.Creature == null);

			return nearestEmpty != null;
		}

		private void FillGrid(Vector2Int gridSize)
		{
			for (int i = 0; i < gridSize.x; i++)
			{
				for (int y = 0; y < gridSize.y; y++)
				{
					Vector2Int coordinate = new(i, y);
					Nodes.Add(coordinate, new Node(coordinate, GetWorldPosition(coordinate)));
				}
			}
			
			_nodes = Nodes.Values.ToArray();
		}

		private void AttachNeighbors(Vector2Int gridSize)
		{
			foreach (Vector2Int coordinate in Nodes.Keys)
			{
				Node node = Nodes[coordinate];

				if (coordinate.x - 1 >= 0)
				{
					node.Neighbors[CreatureDirection.Left] = Nodes[new(coordinate.x - 1, coordinate.y)];
				}

				if (coordinate.x + 1 < gridSize.x)
				{
					node.Neighbors[CreatureDirection.Right] = Nodes[new(coordinate.x + 1, coordinate.y)];
				}

				if (coordinate.y - 1 >= 0)
				{
					node.Neighbors[CreatureDirection.Down] = Nodes[new(coordinate.x, coordinate.y - 1)];
				}

				if (coordinate.y + 1 < gridSize.y)
				{
					node.Neighbors[CreatureDirection.Up] = Nodes[new(coordinate.x, coordinate.y + 1)];
				}
			}
		}

		private Vector3 GetWorldPosition(Vector2Int coord)
		{
			float stepX = _cellSize.x + _gridSpacing.x;
			float stepY = _cellSize.y + _gridSpacing.y;

			float px = _origin.x + coord.x * stepX;
			float py = _origin.y + coord.y * stepY;
			return new Vector3(px, 0, py);
		}
	}
}