using System.Collections.Generic;
using System.Linq;
using BeardedManStudios;
using BeardedManStudios.Forge.Networking.Generated;
using NPC.Scripts.Characters;
using UnityEngine;

namespace NPC.Scripts.Networking
{
    public class SyncedMovementQueue : PlayerBehavior, IMovementQueue
    {
        public Vector2 nextMovement => _moveList[0];
        public List<Vector2> moveList => _moveList;

        private Vector2 _currentMove = Vector2.zero;
        private List<Vector2> _moveList = new List<Vector2> { Vector2.zero };

        void Start()
        {
            gameObject.name = networkObject.Owner.NetworkId.ToString();

            if (!networkObject.IsOwner)
            {
                transform.GetComponent<Player>().DisablePlayerElements();
            }
        }

        private void Update()
        {
            if (!networkObject.IsOwner && networkObject.moveDirection != nextMovement)
            {
                _moveList.Append(networkObject.moveDirection);
            }
        }
        
        public void BroadcastMove(Vector2 moveDirection)
        {
            if (_moveList.Count > 0 && _moveList[0] == Vector2.zero)
            {
                _moveList[0] = moveDirection;
            }
            else
            {
                _moveList.Append(Vector2.zero);
            }


            if (!networkObject.IsOwner) return;
            networkObject.moveDirection = moveDirection;
        }
        
        public void MoveComplete()
        {
            if (_moveList.Count == 1)
            {
                _moveList[0] = Vector2.zero;
            }

            else if (_moveList.Count > 0)
            {
                _moveList.RemoveAt(0);
            }
        }
    }
}
