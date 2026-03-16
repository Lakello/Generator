using System;
using UnityEngine;

namespace Generation
{
    [Serializable]
    public class Cell : IEquatable<Cell>
    {
        public Creature Creature;
        public Vector3 Position;

        public bool Equals(Cell other)
        {
            return Equals(Creature, other.Creature);
        }

        public override bool Equals(object obj)
        {
            return obj is Cell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Creature.GetHashCode();
        }
    }
}
