using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using NPC.Scripts.Characters;
using TMPro;
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
        public GameObject rootMenuScreen;
        public GameObject settingsScreen;

        [Header("Menu Buttons")] 
        public Button settingsButton;
        public Button backToLobbyButton;
        public Button quitButton;
        public Button closeMenuButton;
        
        [Header("Settings Buttons")] 
        public TMP_InputField nameField;
        public Button backToMenuButton;

        [Header("Audio")] 
        public AudioSource audioSource;
        public List<AudioClip> backgroundMusic = new List<AudioClip>();
        private GameManager gameManager;
        private int maxPlayerCount;
        private NetWorker server;
        public Player Player { private get; set; }

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            
            // Button Listeners
            settingsButton.onClick.AddListener(OpenSettings);
            backToLobbyButton.onClick.AddListener(BackToLobby);
            quitButton.onClick.AddListener(QuitApplication);
            closeMenuButton.onClick.AddListener(CloseMenu);
            backToMenuButton.onClick.AddListener(BackToMenu);
            nameField.onEndEdit.AddListener(SetPlayerName);
            nameField.onSelect.AddListener(DisableUserInput);
            
            // Music
            audioSource.clip = backgroundMusic[Random.Range(0, backgroundMusic.Count)];
            audioSource.Play();
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
        private void OpenSettings()
        {
            settingsScreen.SetActive(true);
            rootMenuScreen.SetActive(false);
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
        private void BackToMenu()
        {
            settingsScreen.SetActive(false);
            rootMenuScreen.SetActive(true);
        }
        private void DisableUserInput(string n)
        {
            Player.DisableInput = true;
        }
        private void SetPlayerName(string n)
        {
            Player.SetCharacterName(n);
            Player.DisableInput = false;
        }
    }
}
