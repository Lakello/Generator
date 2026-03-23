namespace Generation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Cysharp.Threading.Tasks;
	using Sirenix.OdinInspector;
	using UnityEngine;

	[BoxGroup(nameof(Validator))]
	[HideLabel]
	[Serializable]
	public class Validator
	{
		[BoxGroup("Debug")]
		[SerializeField]
		private bool _canWaitNext;

		private bool _isNext;

		[Button]
		private void Next()
		{
			_isNext = true;
		}

		public async UniTask Validate(Grid grid, bool? canWaitNext = null)
		{
			if(canWaitNext is not null)
			{
				_canWaitNext = canWaitNext.Value;

            }

			Dictionary<Creature, bool> allCreatures = new();

			foreach (var node in grid.Nodes.Values)
			{
				if (node?.Creature != null)
					allCreatures.TryAdd(node.Creature, false);
			}

			LinkedList<Creature> chain = new LinkedList<Creature>();

			while (TryGetNonCheckedCreature(out Creature creature))
			{
				chain.Clear();

				chain.AddFirst(creature);

				await FillChain(creature);
			}

			return;

			bool TryGetNonCheckedCreature(out Creature creature)
			{
				creature = allCreatures.Keys.FirstOrDefault(c => allCreatures[c] == false);

				return creature != null;
			}

			async UniTask<bool> FillChain(Creature root)
			{
				root.CurrentColor.Value = Color.yellow;

				if (_canWaitNext)
				{
					await UniTask.WaitUntil(() => _isNext);
					_isNext = false;
				}

				if (root.OriginNode.Value.Neighbors.TryGetValue(root.Direction.Value, out var neighborNode)
					&& neighborNode != null
					&& allCreatures[neighborNode.Creature] == false)
				{
					if (TryInvertDirection(neighborNode))
					{
						if (TryInvertDirection(neighborNode))
						{
							return false;
						}
					}

					chain.AddLast(neighborNode.Creature);

					if (await FillChain(neighborNode.Creature) == false)
					{
						neighborNode.Creature.InvertDirection();

						var isChecked = FillChain(neighborNode.Creature);
						Debug.LogError($"{isChecked}");
					}
				}

				await ValidateDirection(root);

				allCreatures[root] = true;

				root.CurrentColor.Value = Color.green;

				return true;
			}

			async UniTask ValidateDirection(Creature root)
			{
				Node current = root.OriginNode.Value;

				while (current.Neighbors.TryGetValue(root.Direction.Value, out var targetNode) && targetNode != null)
				{
					if (allCreatures[targetNode.Creature])
					{
						current = targetNode;
						continue;
					}

					var prevColor = current.Creature.CurrentColor.Value;
					current.Creature.CurrentColor.Value = Color.blue;

					if (_canWaitNext)
					{
						await UniTask.WaitUntil(() => _isNext);
						_isNext = false;
					}

					var invertedDirection = targetNode.Creature.InvertDirection(targetNode.CreatureDirection);
					if (invertedDirection == root.Direction.Value)
					{
						targetNode.Creature.InvertDirection();
					}

					current.Creature.CurrentColor.Value = prevColor;
					current = targetNode;
				}
			}

			bool TryInvertDirection(Node node)
			{
				if (node != null && node.Neighbors.TryGetValue(node.CreatureDirection, out var targetNode) && targetNode != null)
				{
					if (chain.Last.Value == targetNode.Creature || chain.Contains(targetNode.Creature))
					{
						node.Creature.InvertDirection();
						return true;
					}
				}

				return false;
			}
		}
	}
}