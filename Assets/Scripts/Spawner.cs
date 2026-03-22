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
    using R3;

    public class Spawner : MonoBehaviour
    {
        [SerializeField]
        private Generator _generator;

        [SerializeField]
        private Data[] _data;

        private Dictionary<Size, GameObject> _prefabs;

        [SerializeField]
        private bool _useJsonLoad = false;

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

            _generator.Generate(originNode =>
            {
                GameObject prefab = _prefabs[originNode.Creature.Size];
                var instance = Instantiate(prefab);
                instance.GetComponentInChildren<CreatureView>().Init(originNode.Creature);
            }, _useJsonLoad).Forget(); // Добавляем параметр
        }

        [Serializable]
        private struct Data
        {
            public Size Size;
            public GameObject Prefab;
        }
    }
}