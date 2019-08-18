using BeardedManStudios.Forge.Networking.Generated;
using UnityEngine;

namespace NPC.Scripts.Networking
{
    public class NetworkPosition : PlayerBehavior
    {
        public Vector2 MoveDirection
        {
            get => _moveDirection;
            private set => UpdatePosition(value);
        }

        public Vector2 SyncedTransform
        {
            get => _transform;
            set => 
        }

        private Vector2 _moveDirection;
        private Vector2 _transform;
        public void UpdatePosition(Vector2 position)
        {
            if (networkObject.IsOwner)
            {
                
            }
        }
        
        public void UpdateDirection(Vector2 moveDirection)
        {
            if (networkObject.IsOwner)
            {
                MoveDirection = moveDirection;
                networkObject.moveDirection = moveDirection;
            }
        }
    }
}
