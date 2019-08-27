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
        public TextMeshProUGUI playerName;
        public TextMeshProUGUI songName;
        public GameObject winScreen;
        public GameObject menuScreen;
        public GameObject rootMenuScreen;
        public GameObject settingsScreen;
        public GameObject audioSettings;
        public GameObject graphicsSettings;

        [Header("Menu Buttons")] 
        public Button settingsButton;
        public Button backToLobbyButton;
        public Button quitButton;
        public Button closeMenuButton;
        
        [Header("Settings")] 
        public TMP_InputField nameField;
        public Button backToMenuButton;
        public Button audioTab;
        public Button graphicsTab;
        [Space(10)] 
        public Slider masterVolume;
        public Slider musicVolume;
        public Slider sfxVolume;

        [Header("Audio")] 
        public AudioSource audioSource;
        public List<AudioClip> backgroundMusic = new List<AudioClip>();
        private GameManager gameManager;
        private NetWorker server;
        public Player Player { private get; set; }
        
        // Player Prefs
        private const string MASTER_VOLUME_PREF = "MasterVolume";
        private const string MUSIC_VOLUME_PREF = "MusicVolume";
        private const string SFX_VOLUME_PREF = "SFXVolume";
            
        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            
            // Button Listeners
            settingsButton.onClick.AddListener(OpenSettings);
            audioTab.onClick.AddListener(AudioSettings);
            graphicsTab.onClick.AddListener(GraphicsSettings);
            backToLobbyButton.onClick.AddListener(BackToLobby);
            quitButton.onClick.AddListener(QuitApplication);
            closeMenuButton.onClick.AddListener(CloseMenu);
            backToMenuButton.onClick.AddListener(BackToMenu);
            
            // Slider Listeners
            masterVolume.onValueChanged.AddListener(MasterVolume);
            musicVolume.onValueChanged.AddListener(MusicVolume);
            sfxVolume.onValueChanged.AddListener(SFXVolume);
            // Set Values
            MasterVolume(PlayerPrefs.GetFloat(MASTER_VOLUME_PREF));
            MusicVolume(PlayerPrefs.GetFloat(MUSIC_VOLUME_PREF));
            SFXVolume(PlayerPrefs.GetFloat(SFX_VOLUME_PREF));
            masterVolume.value = PlayerPrefs.GetFloat(MASTER_VOLUME_PREF);
            musicVolume.value = PlayerPrefs.GetFloat(MUSIC_VOLUME_PREF);
            sfxVolume.value = PlayerPrefs.GetFloat(SFX_VOLUME_PREF);

            Debug.Log(PlayerPrefs.GetFloat(MASTER_VOLUME_PREF));
            
            // Text Field Listeners
            nameField.onEndEdit.AddListener(SetPlayerName);
            nameField.onSelect.AddListener(DisableUserInput);
            
            // Music
            audioSource.clip = backgroundMusic[Random.Range(0, backgroundMusic.Count)];
            songName.SetText("Song:  " + audioSource.clip.name + "  by  Ahsan  Iqbal");
            audioSource.Play();
        }
        private void Update()
        {
            int livePlayersCount = gameManager.LivePlayers.Count;
            int allPlayerCount = gameManager.AllPlayers.Count;
            SetPlayerCount(livePlayersCount, allPlayerCount);
        }
        private void SetPlayerCount(int players, int allPlayers)
        {
            string s = players + " | " + allPlayers;
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
            
            // Reset Menu
            BackToMenu();
            AudioSettings();
        }
        private void OpenSettings()
        {
            settingsScreen.SetActive(true);
            rootMenuScreen.SetActive(false);
        }
        private void AudioSettings()
        {
            audioSettings.SetActive(true);
            graphicsSettings.SetActive(false);
        }
        private void GraphicsSettings()
        {
            graphicsSettings.SetActive(true);
            audioSettings.SetActive(false);
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
        private static void MasterVolume(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(MASTER_VOLUME_PREF, value);
        }
        private void MusicVolume(float value)
        {
            audioSource.volume = value;
            PlayerPrefs.SetFloat(MUSIC_VOLUME_PREF, value);
        }
        private void SFXVolume(float value)
        {
            if (Player == null)
            {
                return;
            }
            
            Player.audioSource.volume = value;
            PlayerPrefs.SetFloat(SFX_VOLUME_PREF, value);
            
            // This is messy, but hey how are ya
            foreach (Character nonPlayer in gameManager.NonPlayers)
            {
                nonPlayer.audioSource.volume = value;
            }
            foreach (Player player in gameManager.AllPlayers)
            {
                player.audioSource.volume = value;
            }
        }
    }
}
