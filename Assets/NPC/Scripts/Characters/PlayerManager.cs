using System;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;
using NPC.Scripts.Networking;

namespace NPC.Scripts.Characters
{
    public class PlayerManager : MonoBehaviour
    {
        public List<Player> players = new List<Player>();
        public List<NonPlayer> nonPlayers = new List<NonPlayer>();

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject nonPlayerPrefab;
        [SerializeField, Range(0, 30)] private int nonPlayerCount;

        private void Awake()
        {
            var player = NetworkManager.Instance.InstantiatePlayer();
            return;
        }

        private void Update()
        {
            
        }
    }
}
