using System;
using System.Collections.Generic;
using System.Linq;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using NPC.Scripts.Characters;
using NPC.Scripts.Items;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace NPC.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [Serializable]
        public struct Zone
        {
            public Tile Tile;
            public int ItemsInZone;
            public bool CharacterCanSpawn;
            public bool PathingTarget;
            [HideInInspector]
            public List<Vector3> TilePositions;
        }

        [Header("Player")]
        public GameObject playerPrefab;
    
        [Header("Other Players")]
        [SerializeField]
        private int _otherPlayerCount;
        public GameObject otherPlayerPrefab;
    
        [Header("Non Player Characters")]
        [SerializeField]
        private int _npcCount;
        public GameObject NonPlayerCharacterPrefab;
    
        [Header("Items")]
        public List<GameObject> Items = new List<GameObject>();
        private Dictionary<Item.ItemRarity, List<GameObject>> _itemRarities = new Dictionary<Item.ItemRarity, List<GameObject>>();
        private int _rarityTotal;

        [Header("Player Animations")]
        public List<AnimatorController> AnimatorControllers = new List<AnimatorController>();
    
        [Header("Zone Manager")]
        public Tilemap Tilemap;
        public Zone[] Zones;

        public List<Vector3> ValidSpawnPositions = new List< Vector3>();
        public List<Vector3> ValidMovePositions = new List<Vector3>();

        private void Awake()
        {
            Tilemap.CompressBounds();
            FindValidPositions();
            SetupItems();

            foreach (Zone zone in Zones)
            {
                if (zone.TilePositions.Count == 0)
                {
                    continue;
                }
            
                for (int i = 0; i < zone.ItemsInZone; i++)
                {
                    int roll = Random.Range(0, _rarityTotal);
                    if (roll < (int) Item.ItemRarity.Common)
                    {
                        SpawnItem(zone, Item.ItemRarity.Common);
                    }
                    else if (roll < (int) Item.ItemRarity.Common + (int) Item.ItemRarity.Uncommon)
                    {
                        SpawnItem(zone, Item.ItemRarity.Uncommon);
                    }
                    else if (roll < (int) Item.ItemRarity.Common + (int) Item.ItemRarity.Uncommon  + (int) Item.ItemRarity.Rare)
                    {
                        SpawnItem(zone, Item.ItemRarity.Rare);
                    }
                    else  if (roll < (int) Item.ItemRarity.Common + (int) Item.ItemRarity.Uncommon  + (int) Item.ItemRarity.Uncommon + (int) Item.ItemRarity.UltraRare)
                    {
                        SpawnItem(zone, Item.ItemRarity.UltraRare);
                    }
                }
            }
        
            for(int i = 0; i < _npcCount; i++)
            {
                Spawn(NonPlayerCharacterPrefab, true);
            }
        
            for(int i = 0; i < _otherPlayerCount; i++)
            {
                Spawn(otherPlayerPrefab, true);
            }
            
            if (SceneManager.GetActiveScene().name == "Networking_Scene")
                SpawnPlayer();
        
            // Spawn(playerPrefab);
        }

        private void SetupItems()
        {
            _itemRarities.Add(Item.ItemRarity.Common, new List<GameObject>());
            _itemRarities.Add(Item.ItemRarity.Uncommon, new List<GameObject>());
            _itemRarities.Add(Item.ItemRarity.Rare, new List<GameObject>());
            _itemRarities.Add(Item.ItemRarity.UltraRare, new List<GameObject>());
            _rarityTotal = (int) Item.ItemRarity.Common + (int) Item.ItemRarity.Uncommon +
                           (int) Item.ItemRarity.Rare + (int) Item.ItemRarity.UltraRare;
        
            foreach (GameObject item in Items)
            {
                _itemRarities[item.GetComponent<Item>().Rarity].Add(item);
            }
        }

        public void FindValidPositions()
        {
            foreach (Vector3Int position in Tilemap.cellBounds.allPositionsWithin)
            {
                TileBase tile = Tilemap.GetTile(position);
                if (tile == null)
                {
                    continue;
                }
            
                if (Zones.Select(x => x.Tile).Contains(tile))
                {
                    Zone zone = Zones.First(x => x.Tile == tile);
                    if (zone.CharacterCanSpawn)
                    {
                        ValidSpawnPositions.Add(position + Tilemap.cellSize / 2);
                    }

                    if (zone.PathingTarget)
                    {
                        Vector3 pathingTarget = position + Tilemap.cellSize / 2;
                        ValidMovePositions.Add(pathingTarget);
                        zone.TilePositions.Add(pathingTarget);
                    }
                }
            }
        }
    
        public void SpawnPlayer()
        {
            Vector3 position = RetrieveRandomValidPosition();
            NetworkManager.Instance.InstantiatePlayer(0, position);
        }

        public void OnPlayerAccepted(NetworkingPlayer player, NetWorker netWorker)
        {
            MainThreadManager.Run(() =>
            {
                Vector3 position = new Vector3(0.5f, 0.5f, 0);
                PlayerBehavior playerScript = NetworkManager.Instance.InstantiatePlayer(0, position);
                playerScript.networkObject.AssignOwnership(player);
            });
        }
    
        public Vector3 RetrieveRandomValidPosition()
        {
            if (ValidSpawnPositions.Count > 0)
            {
                int index = Random.Range(0, ValidSpawnPositions.Count);
                ValidSpawnPositions.RemoveAt(index);
                return ValidSpawnPositions[index];
            }
            else
            {
                Debug.LogWarning("Attempted to spawn an object with no remaining spawn positions!");
                return new Vector3(0.5f, 0.5f, 0);
            }
        }
        /// <summary>
        /// Use this to spawn any GameObject that doesn't need anything fed to it
        /// </summary>
        /// <param name="gameObjectToSpawn"></param>
        public void Spawn(GameObject gameObjectToSpawn)
        {
            if (ValidSpawnPositions.Count > 0)
            {
                int index = Random.Range(0, ValidSpawnPositions.Count);
                Instantiate(gameObjectToSpawn, ValidSpawnPositions[index], Quaternion.identity, null);
                ValidSpawnPositions.RemoveAt(index);
            }
            else
            {
                Debug.LogWarning("Attempted to spawn an object with no remaining spawn positions!");
            }
        }
        /// <summary>
        /// Use this to spawn Player Prefabs
        /// </summary>
        /// <param name="gameObjectToSpawn"></param>
        /// <param name="player"></param>
        public void Spawn(GameObject gameObjectToSpawn, bool player)
        {
            if (ValidSpawnPositions.Count > 0 && player)
            {
                int index = Random.Range(0, ValidSpawnPositions.Count);
                GameObject spawnedPlayer = Instantiate(gameObjectToSpawn, ValidSpawnPositions[index], Quaternion.identity, null);
                spawnedPlayer.GetComponent<Character>().animator.runtimeAnimatorController = AnimatorControllers[Random.Range(0, AnimatorControllers.Count)];
                ValidSpawnPositions.RemoveAt(index);
            }
            else
            {
                Debug.LogWarning("Attempted to spawn an object with no remaining spawn positions!");
            }
        }
        /// <summary>
        /// Use this to spawn Items
        /// </summary>
        /// <param name="gameObjectToSpawn"></param>
        /// <param name="position"></param>
        public void Spawn(GameObject gameObjectToSpawn, Vector3 position)
        {
            if (ValidSpawnPositions.Count > 0)
            {
                Instantiate(gameObjectToSpawn, position, Quaternion.identity, null);
                ValidSpawnPositions.Remove(position);
            }
            else
            {
                Debug.LogWarning("Attempted to spawn an object with no remaining spawn positions!");
            }
        }
    
        private void SpawnItem(Zone zone, Item.ItemRarity itemRarity)
        {
            List<GameObject> items = _itemRarities[itemRarity].FindAll(x => x.GetComponent<Item>().Rarity == itemRarity);
            Spawn(items[Random.Range(0, items.Count)], zone.TilePositions[Random.Range(1, zone.TilePositions.Count)]);
        }
    }
}
