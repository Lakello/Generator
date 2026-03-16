using System;
using Assets.Scripts;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generation
{
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

        public void Generate(Action<Cell> cellGeneratedCallback)
        {
            Grid grid = new(_gridSize);

            CreateCreatures(grid, node =>
            {
                Cell cell = GeneratePositions(node, _cellSize, _gridSpacing, Vector2.zero);
                _gridPosition = cell.Creature.OriginNode.Coord;
                _currentDirection = cell.Creature.Direction;

                cellGeneratedCallback?.Invoke(cell);
            }).Forget();

            //return GeneratePositions(grid, _cellSize, _gridSpacing, Vector2.zero);
        }

        private async UniTaskVoid CreateCreatures(Grid grid, Action<Node> creatureCreatedCallback)
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
                    UsedCells = new((int)size) { node.Cell },
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

                Node rootNode = node;
                Node currentNode = rootNode;
                Direction currentDirection = currentNode.CreatureDirection;
                int currentSize = 1;
                _isInverted = false;

                for (int i = 0; i < (int)size - 1; i++)
                {
                    Node targetNode = currentNode.Neighbors[currentDirection];

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
                creatureCreatedCallback?.Invoke(currentCreature.OriginNode);
            }
        }

        private Node GetOrigin(Node node, Direction direction, Creature creature)
        {
            Debug.Log($"Check {node.Coord}");
            Node neighborNode = node.Neighbors[direction];

            Debug.Log($"direction {direction}");
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
            Node node,
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

        [Button]
        private void Next()
        {
            _isNext = true;
        }
    }
}
