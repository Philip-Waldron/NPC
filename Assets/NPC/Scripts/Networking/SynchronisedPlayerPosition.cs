using System;
using BeardedManStudios.Forge.Networking.Generated;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Networking
{
    public class SynchronisedPlayerPosition : PlayerBehavior
    {
        private Vector2 _moveDirection;
        public Vector2 MoveDirection
        {
            get => _moveDirection;
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
                _moveDirection = networkObject.moveDirection;
            }
        }

        public void UpdatePosition(Vector2 moveDirection)
        {
            _moveDirection = moveDirection;
            
            if (!networkObject.IsOwner) return;
            networkObject.moveDirection = moveDirection;
        }
    }
}
