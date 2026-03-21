namespace Generation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Assets.Scripts;
	using UnityEngine;

	[Serializable]
	public class Node
	{
		[SerializeField]
		public Data[] NeighborsEditor;
		public int ID;
		
		public Vector2Int Coord;
		public Vector3 Position;

		[NonSerialized]
		public Dictionary<CreatureDirection, Node> Neighbors;

		public Node(Vector2Int coordinate, Vector3 position)
		{
			Coord = coordinate;
			Position = position;

			IEnumerable<CreatureDirection> directions = Enum.GetValues(typeof(CreatureDirection)).Cast<CreatureDirection>();

			Neighbors = new Dictionary<CreatureDirection, Node>();

			foreach (CreatureDirection direction in directions)
			{
				if (direction == Assets.Scripts.CreatureDirection.None)
				{
					continue;
				}

				Neighbors[direction] = null;
			}
		}

		public Creature Creature { get; set; }
		public Size CreatureSIze => Creature?.Size ?? Size.None;
		public CreatureDirection CreatureDirection => Creature?.Direction.Value ?? CreatureDirection.None;
		public List<Node> CreatureUsedCells => Creature?.UsedCells ?? null;
		
		[Serializable]
		public struct Data
		{
			public CreatureDirection _creatureDirection;
			public int ID;
		}
	}
}