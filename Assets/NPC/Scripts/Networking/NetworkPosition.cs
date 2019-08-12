using BeardedManStudios.Forge.Networking.Generated;
using UnityEngine;

namespace NPC.Scripts.Networking
{
    public class NetworkPosition : PlayerBehavior
    {
        public Vector2 MoveDirection { get; private set; }
        public void UpdatePosition(Vector2 moveDirection)
        {
            if (networkObject.IsOwner)
            {
                MoveDirection = moveDirection;
                networkObject.moveDirection = moveDirection;
            }
        }
    }
}
