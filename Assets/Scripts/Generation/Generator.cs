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

        [BoxGroup("JSON Save and Load")]
        [SerializeField]
        private string _saveFileName = "map_save.json";

        [BoxGroup("JSON Save and Load")]
        [SerializeField]
        private TextAsset _jsonFileToLoad;

        private bool _isNext;

        public readonly List<Creature> Creatures = new List<Creature>();

        public async UniTaskVoid Generate(Action<Node> cellGeneratedCallback, bool loadFromJson)
        {
            if (loadFromJson && await TryLoadFromJson(cellGeneratedCallback))
            {
                Debug.Log("Карта загружена из JSON");
                return;
            }

            await GenerateNewMap(cellGeneratedCallback);
        }

        private void SetupGridFromSave(SaveData saveData)
        {
            var gridType = _grid.GetType();

            var gridSizeField = gridType.GetField("_gridSize",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var cellSizeField = gridType.GetField("_cellSize",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var gridSpacingField = gridType.GetField("_gridSpacing",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var originField = gridType.GetField("_origin",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (gridSizeField != null) 
                gridSizeField.SetValue(_grid, saveData.gridSize);

            if (cellSizeField != null) 
                cellSizeField.SetValue(_grid, saveData.cellSize);

            if (gridSpacingField != null) 
                gridSpacingField.SetValue(_grid, saveData.gridSpacing);

            if (originField != null) 
                originField.SetValue(_grid, saveData.origin);

            _grid.Generate();
        }

        private async UniTask LoadFromSaveData(SaveData saveData, Action<Node> cellGeneratedCallback)
        {
            Creatures.Clear();

            SetupGridFromSave(saveData);

            var creaturesDict = new Dictionary<int, Creature>();

            foreach (var savedCreature in saveData.creatures)
            {
                var creature = new Creature
                {
                    ID = savedCreature.id,
                    Size = savedCreature.size,
                    UsedCells = new List<Node>()
                };

                creature.Direction.Value = savedCreature.direction;
                creature.CurrentColor.Value = savedCreature.color;

                Creatures.Add(creature);
                creaturesDict[creature.ID] = creature;
            }

            foreach (var savedCreature in saveData.creatures)
            {
                var creature = creaturesDict[savedCreature.id];

                foreach (var coord in savedCreature.occupiedCells)
                {
                    if (_grid.Nodes.TryGetValue(coord, out Node node))
                    {
                        node.Creature = creature;
                        creature.UsedCells.Add(node);
                    }
                }
            }

            foreach (var creature in Creatures)
            {
                if (creature.UsedCells.Count > 0)
                {
                    creature.UpdateOrigin(creature.UsedCells[0]);
                    cellGeneratedCallback?.Invoke(creature.OriginNode.Value);
                }
            }

            foreach (var node in _grid.Nodes.Values)
            {
                if (node?.Creature != null)
                {
                    node.NeighborsEditor = node.Neighbors
                        .Select(kvp => new Node.Data
                        {
                            ID = kvp.Value?.Creature.ID ?? -1,
                            _creatureDirection = kvp.Key,
                        })
                        .ToArray();

                    node.ID = node.Creature.ID;
                }
            }

            await _validator.Validate(_grid);
        }

        private async UniTask<bool> TryLoadFromJson(Action<Node> cellGeneratedCallback)
        {
            if (_jsonFileToLoad == null)
            {
                Debug.Log("Нет JSON файла для загрузки");

                return false;
            }

            Debug.Log($"Загрузка из: {_jsonFileToLoad.name}");

            SaveData saveData = JsonUtility.FromJson<SaveData>(_jsonFileToLoad.text);

            if (saveData == null)
            {
                Debug.LogError("Ошибка десериализации JSON файла");

                return false;
            }

            await LoadFromSaveData(saveData, cellGeneratedCallback);

            return true;
        }

        public async UniTask GenerateNewMap(Action<Node> cellGeneratedCallback) // Заменил UniTaskVoid на Unitask
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
        private void SaveToJson()
        {
            var saveData = new SaveData
            {
                gridSize = _grid.GetGridSize(),
                cellSize = _grid.GetCellSize(),
                gridSpacing = _grid.GetGridSpacing(),
                origin = _grid.GetOrigin(),
                creatures = new List<SavedCreature>()
            };

            foreach (var creature in Creatures)
            {
                var savedCreature = new SavedCreature
                {
                    id = creature.ID,
                    size = creature.Size,
                    direction = creature.Direction.Value,
                    occupiedCells = new List<Vector2Int>(), //TODO удалить т.к. ячейки больше не нужны
                    color = creature.CurrentColor.Value
                };

                // Сохраняем координаты занятых ячеек
                if (creature.UsedCells != null)
                {
                    foreach (var cell in creature.UsedCells)
                        savedCreature.occupiedCells.Add(cell.Coord);
                }
                else
                {
                    savedCreature.occupiedCells.Add(creature.OriginNode.Value.Coord);
                }

                saveData.creatures.Add(savedCreature);
            }

            string json = JsonUtility.ToJson(saveData, true);
            string fullPath = Path.Combine(Application.dataPath, _saveFileName);
            File.WriteAllText(fullPath, json);

            Debug.Log($"Сохранено в: {fullPath}");
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