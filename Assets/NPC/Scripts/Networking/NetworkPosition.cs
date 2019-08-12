using BeardedManStudios.Forge.Networking.Generated;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Networking
{
    public class NetworkPosition : PlayerBehavior
    {
        public Vector2 MoveDirection { get; private set; }
        public bool isOwner;

        private void Start()
        {
            gameObject.name = networkObject.UniqueIdentity.ToString();
            isOwner = networkObject.IsOwner;
            
            if (!networkObject.IsOwner)
            {
                Destroy(transform.GetComponent<PlayerInput>());
            }
        }

        public void UpdatePosition(Vector2 moveDirection)
        {
            MoveDirection = moveDirection;
            networkObject.moveDirection = moveDirection;
            
            return;
            if (networkObject.IsOwner)
            {
                MoveDirection = moveDirection;
                networkObject.moveDirection = moveDirection;
            }
        }
    }
}
