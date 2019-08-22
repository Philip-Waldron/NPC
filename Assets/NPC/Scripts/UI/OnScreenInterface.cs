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
        private int originalPlayerCount;

        public void SetupUI(GameManager g, int playerTotal)
        {
            gameManager = g;
            originalPlayerCount = playerTotal;
            SetPlayerCount(originalPlayerCount);
        }
        
        public void SetPlayerCount(int players)
        {
            string s = players + " | " + originalPlayerCount;
            playerCount.SetText(s);
        }
    }
}
