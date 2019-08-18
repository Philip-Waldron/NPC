using BeardedManStudios.Forge.Networking;
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
            SceneManager.LoadScene("Networking_Scene");
        }

        public void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool foundComponent = false;
            foreach (var gameObject in scene.GetRootGameObjects())
            {
                GameManager gameManager = gameObject.GetComponent<GameManager>();
                if (gameManager != null)
                {
                    _netWorker.playerAccepted += gameManager.OnPlayerAccepted;
                    foundComponent = true;
                    
                    if (_netWorker.IsServer)
                    {
                        gameManager.SpawnPlayer();
                    }
                    
                    break;
                }
            }
            
            if (!foundComponent)
                Debug.LogWarning("Could not find Game Manager component in scene! Networking will not function!!!!");
        }
    }
}
