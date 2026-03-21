namespace Generation
{
	using Assets.Scripts;
	using R3;
	using TMPro;
	using UnityEngine;

	public class CreatureView : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _idOut;

		[SerializeField]
		private MeshRenderer _meshRenderer;

		private CompositeDisposable _disposable;
		private Creature _creature;

		private void OnDestroy()
		{
			_disposable?.Dispose();
		}

		public void Init(Creature creature)
		{
			_disposable?.Dispose();

			_disposable = new CompositeDisposable();

			_creature = creature;
			_creature.CurrentColor
				.Subscribe(v =>
				{
					_meshRenderer.material.color = v;
				})
				.AddTo(_disposable);

			_creature.OriginNode
				.Subscribe(n =>
				{
					transform.position = n.Position;
					transform.rotation = GetRotation(n.CreatureDirection);
				})
				.AddTo(_disposable);

			transform.position = creature.OriginNode.Value.Position;
			transform.rotation = GetRotation(creature.Direction.Value);

			_meshRenderer.material.color = _creature.CurrentColor.Value;
			_idOut.text = creature.ID.ToString();
		}

		private Quaternion GetRotation(CreatureDirection creatureDirection)
		{
			const int Rotation = 90;

			return Quaternion.Euler(new Vector3(0, Rotation * (int)creatureDirection, 0));
		}
	}
}