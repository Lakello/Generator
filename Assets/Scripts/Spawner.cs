using System;
using System.Collections.Generic;
using System.Linq;
using Generation;
#region Validation
#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion Validation
using UnityEngine;

namespace Assets.Scripts
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField]
        private Generator _generator;

        [SerializeField]
        private Data[] _data;

        private Dictionary<Size, GameObject> _prefabs;

        #region Validation
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_generator == null)
            {
                _generator = FindObjectOfType<Generator>();
                EditorUtility.SetDirty(this);
            }
        }
#endif
        #endregion Validation

        private void Start()
        {
            _prefabs ??= _data.ToDictionary(d => d.Size, d => d.Prefab);

            _generator.Generate(cell =>
            {
                GameObject prefab = _prefabs[cell.Creature.Size];
                Instantiate(prefab, cell.Position, GetRotation(cell.Creature.Direction));
            });
        }

        private Quaternion GetRotation(Direction direction)
        {
            const int Rotation = 90;

            return Quaternion.Euler(new Vector3(0, Rotation * (int)direction, 0));
        }

        [Serializable]
        private struct Data
        {
            public Size Size;
            public GameObject Prefab;
        }
    }
}
