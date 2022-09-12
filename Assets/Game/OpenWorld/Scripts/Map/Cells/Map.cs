using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    private Grid _grid;
    private Tilemap _tilemap;
    private Pathfinding _pathfinding;

    private List<TileData> _tiles;

    public List<TileData> Tiles => _tiles;

    public void InitializeMap(Grid grid, Tilemap tilemap, List<TileData> tiles, Pathfinding pathfinding)
    {
        _grid = grid;
        _tilemap = tilemap;
        _tiles = tiles;
        _pathfinding = pathfinding;
    }

    public TileData GetTileData(Vector3 position) => GetTileData(_tilemap.WorldToCell(position));

    public TileData GetTileData(Vector3Int position)
    {
        TryGetTileData(position, out TileData data);
        return data;
    }

    public bool TryGetTileData(Vector3 position, out TileData data) => TryGetTileData(_tilemap.WorldToCell(position), out data);

    public bool TryGetTileData(Vector3Int position, out TileData data)
    {
        for (int i = 0; i < _tiles.Count; i++)
        {
            if (_tiles[i].CellPosition == position)
            {
                data = _tiles[i];
                return true;
            }
        }

        data = default;
        return false;
    }

    public TileData[] GetTileDatasOfPath(Vector3 startPosition, Vector3 endPosition) => _pathfinding.CreatePath(startPosition, endPosition);
}
