using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using NaughtyAttributes;

[DisallowMultipleComponent]
public class Generator : MonoBehaviour
{
    [SerializeField, Foldout("Components"), Required] 
    private BuildingManager _buildingManager;
    [SerializeField, Foldout("Components"), Required] 
    private Map _map;
    [SerializeField, Foldout("Components"), Required]
    private Grid _grid;
    [SerializeField, Foldout("Components"), Required]
    private Tilemap _tilemap;

    [SerializeField, Foldout("Debug Pathfinding")] 
    private TextMeshPro _debugText;
    [SerializeField, Foldout("Debug Pathfinding")] 
    private bool _debugPathfinding;

    [SerializeField, Foldout("Map generation")] 
    private Tile[] _easyTiles;
    [SerializeField, Foldout("Map generation")] 
    private Tile[] _usuallyTiles;
    [SerializeField, Foldout("Map generation")] 
    private Tile[] _hardlyTiles;
    [SerializeField, Foldout("Map generation")] 
    private Tile[] _impassableTiles;
    [SerializeField, Foldout("Map generation")] 
    private float[] _tileMovementMultiplies;
    [SerializeField, Foldout("Map generation")] 
    private float[] _tileMovementCosts;

    [SerializeField, Foldout("Building generation")] 
    private Transform _parent;
    [SerializeField, Foldout("Building generation")] 
    private float _minDistanceBetweenBuildings = 2;
    [SerializeField, Foldout("Building generation")] 
    private Building[] _buildingPrefabs;
    [SerializeField, Foldout("Building generation"), Range(0, 1)] 
    private float _additionalSpawnChance = 0.0f;

    private Tile[][] _tilesTypes;

    private List<TileData> _findedTiles;

    private List<Building> _createdBuildings;

    private void Start()
    {
        InitializeMap();
        GenerateBuildings();
    }

    private void InitializeMap()
    {
        _findedTiles = new List<TileData>(5000);
        _tilesTypes = new Tile[][] { _easyTiles, _usuallyTiles, _hardlyTiles, _impassableTiles };

        Vector3Int position = new Vector3Int();
        for (int x = -1000; x < 1000; x++)
        {
            for (int y = -1000; y < 1000; y++)
            {
                position.x = x;
                position.y = y;

                Tile tile = _tilemap.GetTile(position) as Tile;

                if (tile != null)
                    _findedTiles.Add(new TileData(tile, GetTileType(tile), position, _tilemap.CellToWorld(position), _tileMovementMultiplies[(int)GetTileType(tile)], _tileMovementCosts[(int)GetTileType(tile)]));
            }
        }

        Wave _pathfinding = new Wave();
        _pathfinding.Initialize(_map, _tilemap, DebugCreateText, _debugPathfinding);

        _map.InitializeMap(_grid, _tilemap, _findedTiles, _pathfinding);
    }

    private void GenerateBuildings()
    {
        List<TileData> tiles = _map.Tiles;
        _createdBuildings = new List<Building>(Mathf.RoundToInt(tiles.Count / 10) + 10);

        float chance;
        int currentBuildingPrefab = -1;

        for (int i = 0; i < tiles.Count; i++)
        {
            chance = Random.Range(0f, 1f);

            currentBuildingPrefab++;

            if (currentBuildingPrefab >= _buildingPrefabs.Length)
                currentBuildingPrefab = 0;

            if (chance <= 1 - _buildingPrefabs[currentBuildingPrefab].SpawnChanceChange - _additionalSpawnChance)
                continue;

            if (!_buildingPrefabs[currentBuildingPrefab].GroundTypeEquals(tiles[i].type))
                continue;

            if (_buildingManager.GetNearestBuildingDistance(tiles[i].realPosition, out Building nearestPoint) <= _minDistanceBetweenBuildings)
                continue;

            Vector3 position = tiles[i].realPosition;
            Building building = Instantiate(_buildingPrefabs[currentBuildingPrefab], position, Quaternion.identity, _parent);
            building.Initialize(position, tiles[i].cellPosition);
            _createdBuildings.Add(building);
            _buildingManager.SetBuildingsList(_createdBuildings);
        }
    }

    private TileType GetTileType(Tile tile)
    {
        for (int i = 0; i < _tilesTypes.Length; i++)
        {
            for (int x = 0; x < _tilesTypes[i].Length; x++)
            {
                if (_tilesTypes[i][x] == tile)
                    return (TileType)i;
            }
        }

        return default;
    }

    public void DebugCreateText(Vector3 position, string cost)
    {
        var t = Instantiate(_debugText, transform);
        t.transform.position = position;
        t.text = cost;
    }
}

public enum TileType
{
    EasilyPassable,
    UsuallyPassable,
    HardlyPassable,
    Impassable
}

public struct TileData
{
    public Tile tile;
    public TileType type;
    public Vector3Int cellPosition;
    public Vector3 worldPosition;
    public float movementMultiply;
    public float movementCost;
    public Vector3 realPosition => worldPosition + new Vector3(0, 0.3f, 0);

    public TileData(Tile tile, TileType type, Vector3Int cellPosition, Vector3 worldPosition, float movementMultiply, float movementCost)
    {
        this.tile = tile;
        this.type = type;
        this.cellPosition = cellPosition;
        this.worldPosition = worldPosition;
        this.movementMultiply = movementMultiply;
        this.movementCost = movementCost;
    }
}
