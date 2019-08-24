using System;
using TMPro;
using UnityEngine;

namespace NPC.Scripts.UI
{
    public class OnScreenInterface : MonoBehaviour
    {
        [Header("UI References")]
        public Transform inventoryBar;
        public TextMeshProUGUI playerCount;
        
        private GameManager gameManager;
        private int maxPlayerCount;

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        private void Update()
        {
            int allPlayerCount = gameManager.AllPlayers.Count;
            maxPlayerCount = allPlayerCount > maxPlayerCount ? allPlayerCount : maxPlayerCount;
            SetPlayerCount(allPlayerCount);
        }
        
        public void SetPlayerCount(int players)
        {
            string s = players + " | " + maxPlayerCount;
            playerCount.SetText(s);
        }
    }
}
