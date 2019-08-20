using System;
using System.Collections.Generic;
using System.Linq;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using NPC.Scripts.Characters;
using NPC.Scripts.Items;
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
        public List<RuntimeAnimatorController> AnimatorControllers = new List<RuntimeAnimatorController>();
    
        [Header("Zone Manager")]
        public Tilemap Tilemap;
        public Zone[] Zones;

        public List<Vector3> ValidSpawnPositions = new List<Vector3>();
        public List<Vector3> ValidMovePositions = new List<Vector3>();

        private void Awake()
        {
            Tilemap.CompressBounds();
            FindValidPositions();
            SetupItems();

            foreach (Zone zone in Zones)
            {
                Vector3[] tileSpawnPositions = zone.TilePositions.Intersect(ValidSpawnPositions).ToArray();
                for (int i = 0; i < zone.ItemsInZone && tileSpawnPositions.Any(); i++)
                {
                    int roll = Random.Range(0, _rarityTotal);
                    if (roll < (int) Item.ItemRarity.Common)
                    {
                        SpawnItem(tileSpawnPositions, Item.ItemRarity.Common, zone);
                    }
                    else if (roll < (int) Item.ItemRarity.Common + (int) Item.ItemRarity.Uncommon)
                    {
                        SpawnItem(tileSpawnPositions, Item.ItemRarity.Uncommon, zone);
                    }
                    else if (roll < (int) Item.ItemRarity.Common + (int) Item.ItemRarity.Uncommon +
                             (int) Item.ItemRarity.Rare)
                    {
                        SpawnItem(tileSpawnPositions, Item.ItemRarity.Rare, zone);
                    }
                    else if (roll < (int) Item.ItemRarity.Common + (int) Item.ItemRarity.Uncommon +
                             (int) Item.ItemRarity.Uncommon + (int) Item.ItemRarity.UltraRare)
                    {
                        SpawnItem(tileSpawnPositions, Item.ItemRarity.UltraRare, zone);
                    }
                }
            }
        
            for(int i = 0; i < _npcCount; i++)
            {
                SpawnCharacter(NonPlayerCharacterPrefab);
            }
        
            for(int i = 0; i < _otherPlayerCount; i++)
            {
                SpawnCharacter(otherPlayerPrefab);
            }

            if (SceneManager.GetActiveScene().name == "Networking_Scene")
            {
                SpawnPlayer();
            }
        
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
                    Vector3 tilePosition = position + Tilemap.cellSize / 2;
                    if (zone.CharacterCanSpawn)
                    {
                        ValidSpawnPositions.Add(tilePosition);
                    }

                    if (zone.PathingTarget)
                    {
                        ValidMovePositions.Add(tilePosition);
                    }

                    if (zone.CharacterCanSpawn || zone.PathingTarget)
                    {
                        zone.TilePositions.Add(tilePosition);
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
        /// Spawn a Character prefab in a random valid spawn position.
        /// </summary>
        /// <param name="gameObjectToSpawn"></param>
        public void SpawnCharacter(GameObject gameObjectToSpawn)
        {
            if (ValidSpawnPositions.Count > 0)
            {
                int index = Random.Range(0, ValidSpawnPositions.Count);
                GameObject spawnedPlayer = Instantiate(gameObjectToSpawn, ValidSpawnPositions[index], Quaternion.identity, null);
                Character character = spawnedPlayer.GetComponent<Character>();
                character.animator.runtimeAnimatorController = AnimatorControllers[Random.Range(0, AnimatorControllers.Count)];
                
                /*
                if (character is Player)
                {
                    spawnedPlayer.GetComponent<Player>().MakeOtherPlayerCharacter();
                }
                */
                
                ValidSpawnPositions.RemoveAt(index);
            }
            else
            {
                Debug.LogWarning("Attempted to spawn an object with no remaining spawn positions!");
            }
        }

        /// <summary>
        /// Spawn an item in a random valid spawn position.
        /// </summary>
        /// <param name="validPositions"></param>
        /// <param name="itemRarity"></param>
        /// <param name="zone"></param>
        private void SpawnItem(Vector3[] validPositions, Item.ItemRarity itemRarity, Zone zone)
        {
            if (ValidSpawnPositions.Count > 0)
            {
                List<GameObject> items = _itemRarities[itemRarity].FindAll(x => x.GetComponent<Item>().Rarity == itemRarity);
                if (!items.Any())
                {
                    Debug.LogWarning("Tried to spawn an item of rarity '" + itemRarity + "' but there are none!");
                    return;
                }
                
                GameObject gameObjectToSpawn = items[Random.Range(0, items.Count)];
                Vector3 position = validPositions[Random.Range(1, validPositions.Count())];

                Instantiate(gameObjectToSpawn, position, Quaternion.identity, null);
                ValidSpawnPositions.Remove(position);
            }
            else
            {
                Debug.LogWarning("Attempted to spawn an item with no remaining spawn positions in room " + zone.Tile.name + "!");
            }
        }
    }
}
