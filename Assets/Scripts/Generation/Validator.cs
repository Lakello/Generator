namespace Generation
{
	using System.Collections.Generic;

	public class Validator
	{
		public void Validate(Grid grid)
		{
			Dictionary<Creature, bool> allCreatures = new();

			foreach (var node in grid.Nodes.Values)
			{
				if (node?.Creature != null)
					allCreatures.Add(node.Creature, false);
			}

			LinkedList<Creature> chain = new LinkedList<Creature>();

			foreach (var creature in allCreatures.Keys)
			{
				chain.Clear();

				chain.AddFirst(creature);

				if (creature.OriginNode.Neighbors.TryGetValue(creature.Direction, out var neighborNode))
				{
					if (neighborNode.Neighbors.TryGetValue(neighborNode.CreatureDirection, out var targetNode))
					{
						if (chain.Last.Value == targetNode.Creature || chain.Contains(targetNode.Creature))
						{
							neighborNode.Creature.InvertDirection();
						}
					}

					chain.AddLast(neighborNode.Creature);
				}
			}
		}
	}
}