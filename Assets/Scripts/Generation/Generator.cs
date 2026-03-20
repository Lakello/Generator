namespace Generation
{
	using System;
	using Assets.Scripts;
	using Cysharp.Threading.Tasks;
	using Sirenix.OdinInspector;
	using UnityEngine;
	using Random = UnityEngine.Random;

	public class Generator : MonoBehaviour
	{
		[SerializeField]
		private Grid _grid;

		[SerializeField]
		private GameObject _cellPrefab;

#region Debug

		[BoxGroup("Debug")]
		[SerializeField]
		private Direction _currentDirection;

		[BoxGroup("Debug")]
		[SerializeField]
		private Size _startSize;

		[BoxGroup("Debug")]
		[SerializeField]
		private Size _size;

#pragma warning disable CS0414 // is assigned but its value is never used
		[BoxGroup("Debug")]
		[SerializeField]
		private bool _isChangeSize;
#pragma warning restore CS0414 // is assigned but its value is never used

		[BoxGroup("Debug")]
		[SerializeField]
		private Vector2Int _gridPosition;

		[BoxGroup("Debug")]
		[SerializeField]
		private Vector2Int _invertedGridPosition;

		[BoxGroup("Debug")]
		[SerializeField]
		private bool _isInverted;

#endregion Debug

		private bool _isNext;

		public async UniTaskVoid Generate(Action<Node> cellGeneratedCallback)
		{
			_grid.Generate();

			await CreateCreatures(_grid, node =>
			{
				_gridPosition = node.Coord;
				_currentDirection = node.Creature.Direction;

				cellGeneratedCallback?.Invoke(node);
			});
			
			//new Validator(true).Fix(_grid);
		}

		[Button]
		private void Next()
		{
			_isNext = true;
		}

		private async UniTask CreateCreatures(Grid grid, Action<Node> creatureCreatedCallback)
		{
			while (grid.TryGetNearestEmpty(out Node node))
			{
				await UniTask.WaitUntil(() => _isNext);

				_isNext = false;
				_isChangeSize = false;

				Size size = (Size)Random.Range(1, 3);
				Direction direction = (Direction)Random.Range(0, 4);

				Creature currentCreature = new()
				{
					Size = size,
					Direction = direction,
					UsedCells = new((int)size)
					{
						node
					},
				};

				node.Creature = currentCreature;

				_startSize = size;
				_size = size;

				if (size == Size.One)
				{
					currentCreature.OriginNode = GetOrigin(node, currentCreature.Direction, currentCreature);
					creatureCreatedCallback?.Invoke(node);

					continue;
				}

				Node rootNode = node;
				Node currentNode = rootNode;
				Direction currentDirection = currentNode.CreatureDirection;
				int currentSize = 1;
				_isInverted = false;

				for (int i = 0; i < (int)size - 1; i++)
				{
					Node targetNode = currentNode.Neighbors[currentDirection];

					if (targetNode != null && targetNode.Creature == null)
					{
						targetNode.Creature = rootNode.Creature;
						targetNode.CreatureUsedCells.Add(targetNode);
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
					node.Creature.Size = (Size)currentSize;
				}

				currentCreature.OriginNode = GetOrigin(node, currentCreature.Direction, currentCreature);
				creatureCreatedCallback?.Invoke(currentCreature.OriginNode);
			}
		}

		private Node GetOrigin(Node node, Direction direction, Creature creature)
		{
			Node neighborNode = node.Neighbors[direction];

			if (neighborNode != null && neighborNode.Creature == creature)
			{
				return GetOrigin(neighborNode, direction, creature);
			}

			return node;
		}
	}
}