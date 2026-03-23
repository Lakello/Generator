namespace Generation
{
    using Assets.Scripts;
    using Cysharp.Threading.Tasks;
    using R3;
    using Sirenix.OdinInspector;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEngine;
    using Random = UnityEngine.Random;

    public class Generator : MonoBehaviour
    {
        [SerializeField]
        private Grid _grid;

        [SerializeField]
        private Validator _validator;

        #region Debug

        [BoxGroup("Debug")]
        [SerializeField]
        private CreatureDirection _currentCreatureDirection;

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

        [BoxGroup("Debug")]
        [SerializeField]
        private bool _canWaitNext;

        #endregion Debug

        [SerializeField]
        private MapLoader _mapLoader = new MapLoader();
       

        private bool _isNext;

        public readonly List<Creature> Creatures = new List<Creature>();

        public async UniTaskVoid Generate(Action<Node> cellGeneratedCallback, bool loadFromJson)
        {
            if (loadFromJson && (_mapLoader is null == false) && await _mapLoader.TryLoadFromJson(cellGeneratedCallback))
            {
                Debug.Log("Карта загружена из JSON");
                return;
            }

            await GenerateNewMap(cellGeneratedCallback);
        }

        public async UniTask GenerateNewMap(Action<Node> cellGeneratedCallback)
        {
            _grid.Generate();
            Creatures.Clear();

            await CreateCreatures(_grid, node =>
            {
                _gridPosition = node.Coord;
                _currentCreatureDirection = node.Creature.Direction.Value;

                cellGeneratedCallback?.Invoke(node);
            });

            foreach (var node in _grid.Nodes.Values)
            {
                if (node == null) 
                    continue;

                node.NeighborsEditor = node.Neighbors
                    .Select(kvp =>
                    {
                        return new Node.Data
                        {
                            ID = kvp.Value?.Creature.ID ?? -1,
                            _creatureDirection = kvp.Key,
                        };
                    })
                    .ToArray();

                node.ID = node.Creature.ID;
            }

            await _validator.Validate(_grid);
        }

        private void OnDestroy()
        {
            Creatures.ForEach(c => c.Dispose());
        }

        [Button]
        private void Next()
        {
            _isNext = true;
        }

        [Button]
        private void SaveMap()
        {
            _mapLoader.SaveToJson(_grid, Creatures);
        }

        private async UniTask CreateCreatures(Grid grid, Action<Node> creatureCreatedCallback)
        {
            int id = 0;

            while (grid.TryGetNearestEmpty(out Node node))
            {
                if (_canWaitNext)
                {
                    await UniTask.WaitUntil(() => _isNext);
                    _isNext = false;
                }

                _isChangeSize = false;

                Size size = (Size)Random.Range(1, 3);
                CreatureDirection creatureDirection = (CreatureDirection)Random.Range(0, 4);

                Creature currentCreature = new()
                {
                    ID = id++,
                    Size = size,
                    UsedCells = new((int)size)
                    {
                        node
                    },
                };

                Creatures.Add(currentCreature);

                currentCreature.Direction.Value = creatureDirection;

                node.Creature = currentCreature;

                _startSize = size;
                _size = size;

                if (size == Size.One)
                {
                    currentCreature.UpdateOrigin(node);
                    creatureCreatedCallback?.Invoke(node);

                    continue;
                }

                Node rootNode = node;
                Node currentNode = rootNode;
                CreatureDirection currentCreatureDirection = currentNode.CreatureDirection;
                int currentSize = 1;
                _isInverted = false;

                for (int i = 0; i < (int)size - 1; i++)
                {
                    Node targetNode = currentNode.Neighbors[currentCreatureDirection];

                    if (targetNode != null && targetNode.Creature == null)
                    {
                        targetNode.Creature = rootNode.Creature;

                        targetNode.CreatureUsedCells.Add(targetNode);
                        currentNode = targetNode;
                        currentSize++;
                    }
                    else if (_isInverted == false)
                    {
                        currentCreatureDirection = currentCreature.InvertDirection(currentCreatureDirection);
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

                currentCreature.UpdateOrigin(node);
                creatureCreatedCallback?.Invoke(currentCreature.OriginNode.Value);
            }
        }
    }
}