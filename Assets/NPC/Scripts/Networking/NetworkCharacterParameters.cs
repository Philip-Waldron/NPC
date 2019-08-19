using System;
using BeardedManStudios.Forge.Networking.Generated;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Networking
{
    public class NetworkCharacterParameters : PlayerBehavior
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

        private Player _playerScript;

        private void Start()
        {
            _playerScript = gameObject.GetComponent<Player>();
        }

        public override void NetworkStart()
        {
            base.NetworkStart();
            if (!networkObject.IsOwner)
            {
                _playerScript.MakeOtherPlayerCharacter();
            }
        }

        private void Update()
        {
            if (networkObject == null)
                return;
            
            if (!networkObject.IsOwner && _gridPosition != networkObject.gridPosition)
            {
                StartCoroutine(_playerScript.MoveToPosition(gameObject.transform, networkObject.gridPosition, 0.3f));
            }
        }

        public void UpdatePosition(Vector2 position)
        {
            if (networkObject == null)
                return;
            
            _gridPosition = position;
            
            if (networkObject.IsOwner)
            {
                networkObject.gridPosition = position;
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
