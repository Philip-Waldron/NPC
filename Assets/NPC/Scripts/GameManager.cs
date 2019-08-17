using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField]
    private Tilemap _tilemap;
    [SerializeField]
    private Zone[] Zones;
    
    public List<KeyValuePair<Zone, Vector3>> _validSpawnPositions = new List<KeyValuePair<Zone, Vector3>>();
    public List<KeyValuePair<Zone, Vector3>> _validMovePositions = new List<KeyValuePair<Zone, Vector3>>();
    
    private void Start()
    {
        _tilemap.CompressBounds();
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
        foreach (Vector3Int position in _tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = _tilemap.GetTile(position);
            if (tile == null)
            {
                continue;
            }
            
            if (Zones.Select(x => x.Tile).Contains(tile))
            {
                Zone zone = Zones.First(x => x.Tile == tile);
                if (zone.CharacterCanSpawn)
                {
                    _validSpawnPositions.Add(
                        new KeyValuePair<Zone, Vector3>(zone, position + _tilemap.cellSize / 2));
                }

                if (zone.PathingTarget)
                {
                    _validMovePositions.Add(
                        new KeyValuePair<Zone, Vector3>(zone, position + _tilemap.cellSize / 2));
                }
            }
        }
    }

    public void Spawn(GameObject gameObjectToSpawn)
    {
        if (_validSpawnPositions.Count > 0)
        {
            int index = Random.Range(0, _validSpawnPositions.Count);
            Instantiate(gameObjectToSpawn, _validSpawnPositions[index].Value, Quaternion.identity, null);
            _validSpawnPositions.RemoveAt(index);
        }
        else
        {
            Debug.LogWarning("Attempted to spawn an object with no remaining spawn positions!");
        }
    }
}
