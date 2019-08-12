using BeardedManStudios.Forge.Networking.Generated;
using NPC.Scripts.Characters;
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
            gameObject.name = networkObject.Owner.Ip;
            isOwner = networkObject.IsOwner;

            if (!networkObject.IsOwner)
            {
                transform.GetComponent<Player>().IsNotPlayer();
            }
        }

        public void UpdatePosition(Vector2 moveDirection)
        {
            if (!networkObject.IsOwner) return;
            
            MoveDirection = moveDirection;
            networkObject.moveDirection = moveDirection;
        }
    }
}
