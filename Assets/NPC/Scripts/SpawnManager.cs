using System;
using System.Linq;
using Boo.Lang;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private Transform sprite;
    [SerializeField]
    private List<Vector3> ValidPositions = new List<Vector3>();
    [SerializeField]
    private Tilemap _tilemap;
    [SerializeField]
    private Tile[] ValidSpawnTiles;

    public bool spawn;
    
    private void Start()
    {
        _tilemap.CompressBounds();
        //BoundsInt bounds = _tilemap.cellBounds;
        //TileBase[] allTiles = _tilemap.GetTilesBlock(bounds);
        SetValidPositions();
    }

    private void Update()
    {
//        if (!spawn)
//        {
//            return;
//        }
        
        spawn = false;
        if (ValidPositions.Count > 0)
        {
            Spawn();
        }
        else
        {
            Debug.LogWarning("Attempted to spawn an object with no remaining spawn positions!");
        }
    }

    public void SetValidPositions()
    {
        foreach (var position in _tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = _tilemap.GetTile(position);
            if (tile != null && ValidSpawnTiles.Any(x => x.name == tile.name))
            {
                ValidPositions.Add(position + _tilemap.cellSize / 2);
            }
        }

    }

    public void Spawn()
    {
        int index = Random.Range(0, ValidPositions.Count);
        Instantiate(sprite, ValidPositions[index], Quaternion.identity, transform);
        ValidPositions.RemoveAt(index);
    }

    public void PrintTiles(BoundsInt bounds, TileBase[] allTiles)
    {
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Debug.Log("x:" + x + " y:" + y + " tile:" + tile.name);
                }
                else
                {
                    Debug.Log("x:" + x + " y:" + y + " tile: (null)");
                }
            }
        } 
    }
}
