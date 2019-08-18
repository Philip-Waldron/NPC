using BeardedManStudios.Forge.Networking.Generated;
using NPC.Scripts.Characters;
using UnityEngine;

namespace NPC.Scripts.Networking
{
    public class NetworkedPosition : PlayerBehavior
    {
        private Vector2 _position;
        public Vector2 Position
        {
            get => _position;
            set => UpdatePosition(value);
        }
        
        private void Start()
        {
            gameObject.name = networkObject.Owner.NetworkId.ToString();

            if (!networkObject.IsOwner)
            {
                transform.GetComponent<Player>().DisablePlayerElements();
            }
        }

        private void Update()
        {
            if (!networkObject.IsOwner)
            {
                gameObject.transform.position = networkObject.moveDirection;
            }
        }

        private void UpdatePosition(Vector2 position)
        {
            _position = position;
            
            if (!networkObject.IsOwner) return;
            networkObject.moveDirection = position;
        }
    }
}
