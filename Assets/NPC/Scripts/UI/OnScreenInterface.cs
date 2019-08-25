using NPC.Scripts.Characters;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NPC.Scripts.UI
{
    public class OnScreenInterface : MonoBehaviour
    {
        [Header("UI References")]
        public Transform inventoryBar;
        public TextMeshProUGUI playerCount;
        public TextMeshProUGUI spectatingText;
        public GameObject winScreen;
        public GameObject menuScreen;
        public string MultiplayerMenuSceneName;

        [Header("Menu Buttons")] 
        public Button settingsButton;
        public Button backToLobbyButton;
        public Button quitButton;
        public Button closeMenuButton;

        private GameManager gameManager;
        private int maxPlayerCount;

        public Player Player { get; set; }

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            settingsButton.onClick.AddListener(OpenSettings);
            backToLobbyButton.onClick.AddListener(BackToLobby);
            quitButton.onClick.AddListener(QuitApplication);
            closeMenuButton.onClick.AddListener(CloseMenu);
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
        public void MenuScreen(bool toggle, bool state = false)
        {
            if (toggle)
            {
                menuScreen.SetActive(!menuScreen.activeSelf);
            }
            else
            {
                menuScreen.SetActive(state);
            }
            Player.DisableInput = menuScreen.activeSelf;
        }
        private static void OpenSettings()
        {
            Debug.Log("Sorry TotalBiscuit");
        }
        public void BackToLobby()
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }
        private static void QuitApplication()
        {
            Application.Quit();
        }
        private void CloseMenu()
        {
            MenuScreen(false, false);
        }
    }
}
