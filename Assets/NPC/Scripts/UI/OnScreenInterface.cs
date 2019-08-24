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
        public TextMeshProUGUI spectatingText;
        public GameObject winScreen;

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

        private void SetPlayerCount(int players)
        {
            string s = players + " | " + maxPlayerCount;
            playerCount.SetText(s);
        }
        
        public void SetSpectatingText(string playerName)
        {
            string s = "You're  Spectating  " + playerName;
            spectatingText.SetText(s);
        }

        public void WinScreen()
        {
            winScreen.SetActive(true);
        }
    }
}
