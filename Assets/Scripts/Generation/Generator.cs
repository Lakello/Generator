namespace Generation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Cysharp.Threading.Tasks;
	using Sirenix.OdinInspector;
	using UnityEngine;
	using Random = UnityEngine.Random;

	public class Generator : MonoBehaviour
	{
		[SerializeField]
		private Vector2Int _gridSize;

		[SerializeField]
		private Vector2 _cellSize;
		[SerializeField]
		private Vector2 _gridSpacing;

		[SerializeField]
		private GameObject _cellPrefab;

		[BoxGroup("Debug")]
		[SerializeField]
		private Direction _currentDirection;
		[BoxGroup("Debug")]
		[SerializeField]
		private Size _startSize;
		[BoxGroup("Debug")]
		[SerializeField]
		private Size _size;
		[BoxGroup("Debug")]
		[SerializeField]
		private bool _isChangeSize;
		[BoxGroup("Debug")]
		[SerializeField]
		private Vector2Int _gridPosition;
		[BoxGroup("Debug")]
		[SerializeField]
		private Vector2Int _invertedGridPosition;
		[BoxGroup("Debug")]
		[SerializeField]
		private bool _isInverted;

		private bool _isNext;

		[Button]
		private void Next()
		{
			_isNext = true;
		}

		public void Generate(Action<Cell> cellGeneratedCallback)
		{
			var grid = new Grid(_gridSize);

			CreateCreatures(grid, node =>
			{
				var cell = GeneratePositions(node, _cellSize, _gridSpacing, Vector2.zero);
				_gridPosition = cell.Creature.OriginNode.Coord;
				_currentDirection = cell.Creature.Direction;

				cellGeneratedCallback?.Invoke(cell);
			}).Forget();

			//return GeneratePositions(grid, _cellSize, _gridSpacing, Vector2.zero);
		}

		private async UniTaskVoid CreateCreatures(Grid grid, Action<Grid.Node> creatureCreatedCallback)
		{
			while (grid.TryGetNearestEmpty(out var node))
			{
				await UniTask.WaitUntil(() => _isNext);
				_isNext = false;

				_isChangeSize = false;

				var size = (Size)Random.Range(1, 3);
				var direction = (Direction)Random.Range(0, 4);

				var currentCreature = new Creature()
				{
					Size = size,
					Direction = direction,
					UsedCells = new List<Cell>((int)size)
					{
						node.Cell
					}
				};

				node.Cell.Creature = currentCreature;

				_startSize = size;
				_size = size;
				if (size == Size.One)
				{
					currentCreature.OriginNode = GetOrigin(node, currentCreature.Direction, currentCreature);
					creatureCreatedCallback?.Invoke(node);
					continue;
				}

				var rootNode = node;
				var currentNode = rootNode;
				var currentDirection = currentNode.CreatureDirection;
				var currentSize = 1;
				_isInverted = false;

				for (int i = 0; i < (int)size - 1; i++)
				{
					var targetNode = currentNode.Neighbors[currentDirection];

					if (targetNode != null && targetNode.Cell.Creature == null)
					{
						targetNode.Cell.Creature = rootNode.Cell.Creature;
						targetNode.CreatureUsedCells.Add(targetNode.Cell);
						currentNode = targetNode;
						currentSize++;
					}
					else if (_isInverted == false)
					{
						currentDirection = currentCreature.InvertDirection(currentDirection);
						currentNode = rootNode;
						_isInverted = true;
						i--;
					}
					else
					{
						break;
					}
				}

				if (currentSize != (int)node.CreatureSIze)
				{
					_isChangeSize = true;
					_size = (Size)currentSize;
					node.Cell.Creature.Size = (Size)currentSize;
				}

				currentCreature.OriginNode = GetOrigin(node, currentCreature.Direction, currentCreature);
				creatureCreatedCallback?.Invoke(node);
			}
		}

		private Grid.Node GetOrigin(Grid.Node node, Direction direction, Creature creature)
		{
			Debug.Log($"Check {node.Coord}");
			var neighborNode = node.Neighbors[direction];

			Debug.Log($"direction {direction.ToString()}");
			Debug.Log($"neighborNode {neighborNode != null}");

			if (neighborNode != null)
			{
				Debug.Log($"neighborNode.Cell.Creature {neighborNode.Cell.Creature == creature}");
			}
			if (neighborNode != null && neighborNode.Cell.Creature == creature)
			{
				return GetOrigin(neighborNode, direction, creature);
			}

			return node;
		}

		private static Cell GeneratePositions(
			Grid.Node node,
			Vector2 cellSize,
			Vector2 spacing,
			Vector2 origin)
		{
			float stepX = cellSize.x + spacing.x;
			float stepY = cellSize.y + spacing.y;

			float px = origin.x + node.Coord.x * stepX;
			float py = origin.y + node.Coord.y * stepY;
			node.Cell.Creature.OriginNode.Cell.Position = new Vector3(px, 0, py);

			return node.Cell.Creature.OriginNode.Cell;
		}

		private static List<Cell> GeneratePositions(
			Grid grid,
			Vector2 cellSize,
			Vector2 spacing,
			Vector2 origin)
		{
			float stepX = cellSize.x + spacing.x;
			float stepY = cellSize.y + spacing.y;

			var result = new HashSet<Creature>();

			foreach (var coord in grid.Nodes.Keys)
			{
				var node = grid.Nodes[coord];

				float px = origin.x + coord.x * stepX;
				float py = origin.y + coord.y * stepY;
				node.Cell.Position = new Vector3(px, 0, py);

				result.Add(node.Cell.Creature);
			}

			return result.Select(c => c.OriginNode.Cell).ToList();
		}
	}
}