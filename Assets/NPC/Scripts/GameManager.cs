using System;
using System.Collections.Generic;
using System.Linq;
using NPC.Scripts.Characters;
using NPC.Scripts.Pickups;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Serializable]
    private struct SpawnZone
    {
        public Tile Tile;
        public int ItemsInZone;
        public bool PlayerCanSpawn;
        public bool NpcsCanSpawn;
    }
    
    public List<Player> players = new List<Player>();
    public List<NonPlayerCharacter> NonPlayerCharacters = new List<NonPlayerCharacter>();
    public List<Item> items = new List<Item>();
    
    [Header("Spawn Manager")]
    [SerializeField]
    private Tilemap _tilemap;
    [SerializeField]
    private SpawnZone[] SpawnZones;
    
    private List<KeyValuePair<SpawnZone, Vector3>> _validSpawnPositions = new List<KeyValuePair<SpawnZone, Vector3>>();
    
    private void Start()
    {
        _tilemap.CompressBounds();
        FindValidPositions();
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
            
            if (SpawnZones.Select(x => x.Tile).Contains(tile))
            {
                SpawnZone spawnZone = SpawnZones.First(x => x.Tile == tile);
                _validSpawnPositions.Add(
                    new KeyValuePair<SpawnZone, Vector3>(spawnZone, position + _tilemap.cellSize / 2));
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
