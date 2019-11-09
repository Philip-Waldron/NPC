using System;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NPC.Scripts.Networking
{
    public class SceneLoader
    {
        private NetWorker _netWorker;
        public void ServerLoaded(NetWorker netWorker)
        {
            _netWorker = netWorker;
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;
            SceneManager.LoadScene("Networking_Scene");
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
                _netWorker.playerAccepted += gameManager.OnPlayerAccepted;
                _netWorker.playerDisconnected += gameManager.OnPlayerDisconnected;

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
                _netWorker.Disconnect(false);
            }
        }
    }
}
