using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        Vector3Int position = new();
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

        Wave _pathfinding = new();
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

            if (!_buildingPrefabs[currentBuildingPrefab].GroundTypeEquals(tiles[i].Type))
                continue;
            if (_buildingManager.GetNearestBuildingDistance(tiles[i].RealPosition, out _) <= _minDistanceBetweenBuildings)
                continue;

            Vector3 position = tiles[i].RealPosition;
            Building building = Instantiate(_buildingPrefabs[currentBuildingPrefab], position, Quaternion.identity, _parent);
            building.Initialize(position, tiles[i].CellPosition);
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
        TextMeshPro t = Instantiate(_debugText, transform);
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
    private Tile _tile;
    public Tile Tile { get => _tile; set => _tile = value; }

    private TileType _type;
    public TileType Type { get => _type; set => _type = value; }

    private Vector3Int _cellPosition;
    public Vector3Int CellPosition { get => _cellPosition; set => _cellPosition = value; }

    private Vector3 _worldPosition;
    public Vector3 WorldPosition { get => _worldPosition; set => _worldPosition = value; }

    private float _movementMultiply;
    public float MovementMultiply { get => _movementMultiply; set => _movementMultiply = value; }

    private float _movementCost;
    public float MovementCost { get => _movementCost; set => _movementCost = value; }

    public Vector3 RealPosition => WorldPosition + new Vector3(0, 0.3f, 0);

    public TileData(Tile tile, TileType type, Vector3Int cellPosition, Vector3 worldPosition, float movementMultiply, float movementCost)
    {
        _tile = tile;
        _type = type;
        _cellPosition = cellPosition;
        _worldPosition = worldPosition;
        _movementMultiply = movementMultiply;
        _movementCost = movementCost;
    }
}
