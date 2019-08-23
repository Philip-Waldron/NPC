using System;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Networking
{
    public class NetworkCharacterParameters : PlayerBehavior
    {
        public Vector2 GridPosition
        {
            get => _gridPosition;
            set => UpdatePosition(value);
        }

        private Vector2 _gridPosition;

        private Player _playerScript;
        private NonPlayerCharacter _nonPlayerCharacterScript;

        private void Start()
        {
            _playerScript = gameObject.GetComponent<Player>();
            _nonPlayerCharacterScript = gameObject.GetComponent<NonPlayerCharacter>();
        }

        public override void NetworkStart()
        {
            base.NetworkStart();
            
            if (!networkObject.IsOwner)
            {
                if (_playerScript != null)
                {
                    _playerScript.MakeOtherPlayerCharacter();
                }
            }
            
            else if (_nonPlayerCharacterScript != null)
            {
                _nonPlayerCharacterScript.UsePathfinding = true;
                _nonPlayerCharacterScript.RollState();
            }
        }

        private void Update()
        {
            if (networkObject == null)
                return;
            
            if (_playerScript != null)
            {
                if (_gridPosition != networkObject.gridPosition && !networkObject.IsOwner)
                {
                    StartCoroutine(_playerScript.MoveToPosition(_playerScript.gameObject.transform, networkObject.gridPosition, _playerScript._timeToMove));
                }
            }

            if (_nonPlayerCharacterScript != null)
            {
                if (_gridPosition != networkObject.gridPosition && !networkObject.IsOwner)
                {
                    StartCoroutine(_nonPlayerCharacterScript.MoveToPosition(_nonPlayerCharacterScript.gameObject.transform, networkObject.gridPosition, _nonPlayerCharacterScript._timeToMove));
                }
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

        public void CommunicateShot(Vector2 target, Vector2 hitPoint)
        {
            networkObject.SendRpc(RPC_SHOTS_FIRED, Receivers.Others, target, hitPoint);
        }

        public override void ShotsFired(RpcArgs args)
        {
            MainThreadManager.Run(() =>
                {
                    var target = args.GetAt<Vector2>(0);
                    var hitPoint = args.GetAt<Vector2>(1);
                    
                    if (_playerScript != null)
                    {
                        _playerScript.Damage(target, hitPoint, false);
                    }
                    
                    if (_nonPlayerCharacterScript != null)
                    {
                        _nonPlayerCharacterScript.Damage(target, hitPoint, false);
                    }  
                }
                );
        }
    }
}
