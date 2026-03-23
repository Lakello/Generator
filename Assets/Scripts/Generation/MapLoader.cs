using Assets.Scripts;
using Cysharp.Threading.Tasks;
using Generation;
using R3;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Grid = Generation.Grid;
using Random = UnityEngine.Random;

namespace Generation
{

    [Serializable]
    public class MapLoader
    {
        [BoxGroup("JSON Save and Load")]
        [SerializeField]
        private string _saveFileName = "map_save.json";

        [BoxGroup("JSON Save and Load")]
        [SerializeField]
        private TextAsset _jsonFileToLoad;

        [BoxGroup("JSON Save and Load")]
        [SerializeField]
        private bool _waitValidation;

        private Grid _grid = new();
        private List<Creature> Creatures = new();
        [SerializeField]
        private Validator _validator = new();

        public event Action Saving;

        public void SaveToJson(Grid grid, List<Creature> creatures)
        {
            Debug.Log($"Грид {grid != null}");

            SaveData saveData = new SaveData
            {
                gridSize = grid.GridSize,
                cellSize = grid.CellSize,
                gridSpacing = grid.GridSpacing,
                origin = grid.Origin,
                creatures = new List<SavedCreature>()
            };

            foreach (var creature in creatures)
            {
                var savedCreature = new SavedCreature
                {
                    id = creature.ID,
                    size = creature.Size,
                    direction = creature.Direction.Value,
                    color = creature.CurrentColor.Value,
                    occupiedCells = new List<Vector2Int>(),
                };

                // Сохраняем координаты занятых ячеек
                if (creature.UsedCells != null)
                {
                    foreach (Node cell in creature.UsedCells)
                        savedCreature.occupiedCells.Add(cell.Coord);
                }
                else
                {
                    savedCreature.occupiedCells.Add(creature.OriginNode.Value.Coord);
                }

                saveData.creatures.Add(savedCreature);
            }

            string json = UnityEngine.JsonUtility.ToJson(saveData, true);
            string fullPath = Path.Combine(Application.dataPath, _saveFileName);
            File.WriteAllText(fullPath, json);

            Debug.Log($"Сохранено в: {fullPath}");
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

        public async UniTask<bool> TryLoadFromJson(Action<Node> cellGeneratedCallback)
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

            await _validator.Validate(_grid, _waitValidation);
        }

        public void SetGrid(Grid grid)
        {
            Debug.Log($"Грид До {_grid != null}");
            _grid = grid;
            Debug.Log($"Грид после {_grid != null}");
        }

        public void SetCreatures(List<Creature> creatures)
        {
            Creatures = creatures;
        }
    }
}
