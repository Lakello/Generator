using System;
using System.Collections.Generic;
using System.Linq;
using Generation;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Spawner : MonoBehaviour
{
	[SerializeField]
	private Generator _generator;
	[SerializeField]
	private Data[] _data;

	private Dictionary<Size, GameObject> _prefabs;
		
	[Serializable]
	private struct Data
	{
		public Size Size;
		public GameObject Prefab;
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		_generator ??= FindObjectOfType<Generator>();
		EditorUtility.SetDirty(this);
	}
#endif

	private void Start()
	{
		_prefabs ??= _data.ToDictionary(d => d.Size, d => d.Prefab);

		_generator.Generate(cell =>
		{
			var prefab = _prefabs[cell.Creature.Size];
			Instantiate(prefab, cell.Position, GetRotation(cell.Creature.Direction));
		});
	}

	private Quaternion GetRotation(Direction direction)
	{
		return Quaternion.Euler(new Vector3(0, 90 * (int)direction, 0));
	}
}