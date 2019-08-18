using System;
using BeardedManStudios.Forge.Networking.Generated;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Networking
{
    public class NetworkParameters : PlayerBehavior
    {
        public Vector2 MoveDirection
        {
            get => _moveDirection;
            private set => UpdateDirection(value);
        }

        public Vector2 GridPosition
        {
            get => _gridPosition;
            set => UpdatePosition(value);
        }

        private Vector2 _moveDirection;
        private Vector2 _gridPosition;

        private void Update()
        {
            if (!networkObject.IsOwner)
            {
                gameObject.transform.position = networkObject.position;
            }
        }

        public void UpdatePosition(Vector2 position)
        {
            _gridPosition = position;
            
            if (networkObject.IsOwner)
            {
                networkObject.position = position;
            }
        }
        
        public void UpdateDirection(Vector2 moveDirection)
        {
            _moveDirection = moveDirection;
            
            if (networkObject.IsOwner)
            {
                networkObject.moveDirection = moveDirection;
            }
        }
    }
}
