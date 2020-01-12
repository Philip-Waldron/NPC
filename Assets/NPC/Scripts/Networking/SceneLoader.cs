using System;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking.Unity.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NPC.Scripts.Networking
{
    public class SceneLoader: MonoBehaviour
    {
        private void Start()
        {
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;

            var scene = SceneManager.GetActiveScene();
            
            if (scene.name == "Networking_Scene")
                StartGame(scene);
        }

        public void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Networking_Scene")
            {
                StartGame(scene);
            }
        }

        private void StartGame(Scene scene)
        {
            var foundComponent = false;
            foreach (var gameObject in scene.GetRootGameObjects())
            {
                var gameManager = gameObject.GetComponent<GameManager>();
                if (gameManager == null) continue;
                NetworkManager.Instance.Networker.playerAccepted += gameManager.OnPlayerAccepted;
                NetworkManager.Instance.Networker.playerDisconnected += gameManager.OnPlayerDisconnected;

                foundComponent = true;
                break;
            }
            
            if (!foundComponent)
                Debug.LogWarning("Could not find Game Manager component in scene! Networking will not function!!!!");
        }
        

        public void SceneUnloaded(Scene scene)
        {
            if (scene.name == "Networking_Scene" || scene.name == "Lobby")
            {
                NetworkManager.Instance.Networker.Disconnect(true);
            }
        }
    }
}
