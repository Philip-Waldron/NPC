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
        public GameObject winScreen;

        private GameManager gameManager;
        private int maxPlayerCount;

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            gameManager.WinState.AddListener(WinScreen);
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

        private void WinScreen()
        {
            winScreen.SetActive(true);
        }
    }
}
