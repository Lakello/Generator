namespace Generation
{
	using System;
	using UnityEngine;

	[Serializable]
	public class Cell : IEquatable<Cell>
	{
		public Creature Creature;
		public Vector3 Position;

		public bool Equals(Cell other) =>
			Equals(Creature, other.Creature);

		public override bool Equals(object obj) =>
			obj is Cell other && Equals(other);

		public override int GetHashCode() => Creature.GetHashCode();
	}
}