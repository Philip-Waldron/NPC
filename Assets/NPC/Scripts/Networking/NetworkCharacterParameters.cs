using System;
using System.Linq;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using NPC.Scripts.Characters;
using UnityEngine;

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
            
            if (!(NetworkManager.Instance.Networker is IServer) && _playerScript != null)
            {
                //setup the disconnected event
                NetworkManager.Instance.Networker.disconnected += DisconnectedFromServer;
                networkObject.onDestroy += WasDestroyed;
            }
        }
        
        private void DisconnectedFromServer(NetWorker sender)
        {
            MainThreadManager.Run(() =>
            {
                //Loop through the network objects to see if the disconnected player is the host
                if (sender.NetworkObjectList.Any(senderNetworkObject => senderNetworkObject.Owner.IsHost))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                }

                NetworkManager.Instance.Disconnect();
            });
        }

        public void GenerateName()
        {
            if (networkObject.IsServer)
            {
                BroadcastName(_playerScript.characterName + "_" + networkObject.Networker.ServerPlayerCounter);
            }
        }

        void WasDestroyed(NetWorker sender)
        {
            if (_playerScript != null)
            {
                _playerScript.RemoveFromLivePlayersList();
            }
        }

        private void Update()
        {
            if (networkObject == null) return;

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

        private void UpdatePosition(Vector2 position)
        {
            if (networkObject == null) return;

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
            });
        }

        public void BroadcastEmote(int index)
        {
            networkObject.SendRpc(RPC_EMOTE, Receivers.Others, index);
        }

        public override void Emote(RpcArgs args)
        {
            var index = args.GetNext<int>();

            if (_playerScript != null)
            {
                _playerScript.Emote(index, _playerScript._emoteDuration);
            }
        }

        public override void Name(RpcArgs args)
        {
            if (_playerScript != null)
            {
                _playerScript.SetCharacterName(args.GetAt<string>(0));
            }
        }

        public void BroadcastName(string n)
        {
            if (networkObject.IsOwner)
            {
                networkObject.SendRpc(RPC_NAME, Receivers.AllBuffered, n);
            }
        }
    }
}
