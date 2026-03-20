namespace Generation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Assets.Scripts;
	using UnityEngine;

	public class Node
	{
		public readonly Vector2Int Coord;
		public readonly Vector3 Position;
		public readonly Dictionary<Direction, Node> Neighbors;

		public Node(Vector2Int coordinate, Vector3 position)
		{
			Coord = coordinate;
			Position = position;

			IEnumerable<Direction> directions = Enum.GetValues(typeof(Direction)).Cast<Direction>();

			Neighbors = new Dictionary<Direction, Node>();

			foreach (Direction direction in directions)
			{
				Neighbors[direction] = null;
			}
		}

		public Creature Creature { get; set; }
		public Size CreatureSIze => Creature?.Size ?? Size.None;
		public Direction CreatureDirection => Creature?.Direction ?? Direction.None;
		public List<Node> CreatureUsedCells => Creature?.UsedCells ?? null;
	}
}