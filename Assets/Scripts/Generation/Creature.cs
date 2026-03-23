namespace Generation
{
    using System;
    using System.Collections.Generic;
    using Assets.Scripts;
    using R3;
    using UnityEngine;

    [Serializable]
    public class Creature : IDisposable
    {
        public int ID;
        public Size Size;
        [NonSerialized]
        public List<Node> UsedCells;
        [NonSerialized]
        public ReactiveProperty<Node> OriginNode = new ReactiveProperty<Node>(null);

        private CompositeDisposable _disposable = new CompositeDisposable();
        
        public SerializableReactiveProperty<CreatureDirection> Direction =  new SerializableReactiveProperty<CreatureDirection>(CreatureDirection.None);
        public SerializableReactiveProperty<Color> CurrentColor = new SerializableReactiveProperty<Color>(Color.gray);

        public Creature()
        {
            Direction.Subscribe(_ =>
            {
                UpdateOrigin(OriginNode.Value);
            })
            .AddTo(_disposable);
        }

        public void Dispose()
        {
            OriginNode?.Dispose();
            Direction?.Dispose();
            CurrentColor?.Dispose();
            _disposable.Dispose();
        }

        public void SetDirection(CreatureDirection dir)
        {
            Direction.Value = dir;
        }
        
        public void InvertDirection()
        {
            Direction.Value = InvertDirection(Direction.Value);
        }

        public CreatureDirection InvertDirection(CreatureDirection dir)
        {
            return dir switch
            {
                CreatureDirection.Up => CreatureDirection.Down,
                CreatureDirection.Down => CreatureDirection.Up,
                CreatureDirection.Left => CreatureDirection.Right,
                CreatureDirection.Right => CreatureDirection.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
        }

        public void UpdateOrigin(Node node)
        {
            if (node == null)
            {
                return;
            }
            
            OriginNode.Value = GetOrigin(node);
            OriginNode.ForceNotify();
            
            return;
            
            Node GetOrigin(Node targetNode)
            {
                Node neighborNode = targetNode.Neighbors[Direction.Value];

                if (neighborNode != null && neighborNode.Creature == this)
                {
                    return GetOrigin(neighborNode);
                }

                return targetNode;
            }
        }
    }
}
