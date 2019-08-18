using System;
using System.Collections.Generic;
using System.Linq;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Serializable]
    public struct Zone
    {
        public Tile Tile;
        public int ItemsInZone;
        public bool CharacterCanSpawn;
        public bool PathingTarget;
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

    [Header("Zone Manager")]
    public Tilemap Tilemap;
    [SerializeField]
    private Zone[] Zones;

    public List<KeyValuePair<Zone, Vector3>> ValidSpawnPositions = new List<KeyValuePair<Zone, Vector3>>();
    public List<KeyValuePair<Zone, Vector3>> ValidMovePositions = new List<KeyValuePair<Zone, Vector3>>();
    public Dictionary<Tile, List<Vector3>> Rooms = new Dictionary<Tile, List<Vector3>>();
    
    private void Awake()
    {
        Tilemap.CompressBounds();
        FindValidPositions();
        
        for(int i = 0; i < _npcCount; i++)
        {
            Spawn(NonPlayerCharacterPrefab);
        }
        
        for(int i = 0; i < _otherPlayerCount; i++)
        {
            Spawn(otherPlayerPrefab);
        }
        
        // Spawn(playerPrefab);
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
                    ValidSpawnPositions.Add(
                        new KeyValuePair<Zone, Vector3>(zone, position + Tilemap.cellSize / 2));
                }

                if (zone.PathingTarget)
                {
                    ValidMovePositions.Add(
                        new KeyValuePair<Zone, Vector3>(zone, position + Tilemap.cellSize / 2));
                    if (!Rooms.ContainsKey(zone.Tile))
                    {
                        Rooms.Add(zone.Tile, new List<Vector3> { position + Tilemap.cellSize / 2 });
                    }
                    else
                    {
                        Rooms[zone.Tile].Add(position + Tilemap.cellSize / 2);
                    }
                }
            }
        }
    }

    public void OnPlayerAccepted(NetworkingPlayer player, NetWorker netWorker)
    {
        var playerScript = NetworkManager.Instance.InstantiatePlayer();
        playerScript.networkObject.AssignOwnership(player);
    }

    public void Spawn(GameObject gameObjectToSpawn)
    {
        if (ValidSpawnPositions.Count > 0)
        {
            int index = Random.Range(0, ValidSpawnPositions.Count);
            Instantiate(gameObjectToSpawn, ValidSpawnPositions[index].Value, Quaternion.identity, null);
            ValidSpawnPositions.RemoveAt(index);
        }
        else
        {
            Debug.LogWarning("Attempted to spawn an object with no remaining spawn positions!");
        }
    }
}
