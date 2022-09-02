using System.Collections.Generic;
using UnityEngine;

public class Wave : Pathfinding
{
    private List<float> _tileCosts;
    private List<Vector3Int> _checkedTiles;
    private List<Vector3Int> _allTiles;

    private Vector3Int start;
    private Vector3Int end;

    public override TileData[] CreatePath(Vector3 startPosition, Vector3 endPosition)
    {
        if (_tilemap.WorldToCell(startPosition) == _tilemap.WorldToCell(endPosition))
            return new TileData[] { _map.GetTileData(startPosition) };

        InitializeAlgorythm(startPosition, endPosition);
        CheckTile(start);

        if (_allTiles.Count == _checkedTiles.Count)
            return default;

        if (!FindEndTile())
            return default;

        if (_debug)
            CreateMoneyTexts();

        return GetCompletedPath();
    }

    private void CreateMoneyTexts()
    {
        for (int i = 0; i < _allTiles.Count; i++)
        {
            string cost = _tileCosts[GetPositionIndex(_allTiles[i])].ToString();
            onTextRequired?.Invoke(_map.GetTileData(_allTiles[i]).realPosition, cost);
        }
    }

    private void InitializeAlgorythm(Vector3 startPosition, Vector3 endPosition)
    {
        start = _tilemap.WorldToCell(startPosition);
        end = _tilemap.WorldToCell(endPosition);

        _tileCosts = new List<float>(200);
        _checkedTiles = new List<Vector3Int>(200);
        _allTiles = new List<Vector3Int>(300);

        _tileCosts.Add(0);
        _allTiles.Add(start);
    }

    private bool FindEndTile()
    {
        int attempt = 0;
        int maxAttempt = 200;

        while (maxAttempt > attempt)
        {
            CheckTile(FindMinNewTile());
            attempt++;

            if (attempt > maxAttempt)
            {
                if (_debug)
                    Debug.LogWarning("attempt limit exceeded");
                break;
            }

            if (_allTiles.Count == _checkedTiles.Count)
            {
                if (_debug)
                    Debug.LogWarning("all tiles equals checked tiles");
                break;
            }

            if (HaveEndTile())
            {
                if (_debug)
                {
                    Debug.LogWarning("end finded");
                    Debug.LogWarning("cost: " + _tileCosts[GetPositionIndex(end)]);
                }
                return true;
            }
        }

        return false;
    }

    private void CheckTile(Vector3Int parent)
    {
        _checkedTiles.Add(parent);

        List<Vector3Int> newTiles = FindNewNeighbors(parent);
        SetCostsToTiles(parent, ref newTiles);
        SetTilesAsNew(newTiles);
    }

    private List<Vector3Int> FindNewNeighbors(Vector3Int position)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>()
        {
            new Vector3Int(position.x + 1, position.y, 0),
            new Vector3Int(position.x, position.y + 1, 0),
            new Vector3Int(position.x - 1, position.y, 0),
            new Vector3Int(position.x, position.y - 1, 0)
        };

        for (int i = neighbors.Count - 1; i >= 0; i--)
            if (_allTiles.Contains(neighbors[i]) || !_map.TryGetTileData(neighbors[i], out TileData data) || data.type == TileType.Impassable)
                neighbors.RemoveAt(i);

        return neighbors;
    }

    private void SetCostsToTiles(Vector3Int parent, ref List<Vector3Int> tiles)
    {
        float parentCost = 0;

        for (int i = 0; i < _allTiles.Count; i++)
            if (_allTiles[i] == parent)
                parentCost = _tileCosts[i];

        for (int i = 0; i < tiles.Count; i++)
            _tileCosts.Add(parentCost + _map.GetTileData(tiles[i]).movementCost);
    }

    private void SetTilesAsNew(List<Vector3Int> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
            _allTiles.Add(tiles[i]);
    }

    private Vector3Int FindMinNewTile()
    {
        int index = 0;
        float value = float.MaxValue;

        for (int i = 0; i < _allTiles.Count; i++)
        {
            if (_checkedTiles.Contains(_allTiles[i]))
                continue;

            float cost = _tileCosts[GetPositionIndex(_allTiles[i])];
            if (cost < value)
            {
                value = cost;
                index = i;
            }
        }

        return _allTiles[index];
    }

    private bool HaveEndTile() => _allTiles.Contains(end);

    private int GetPositionIndex(Vector3Int position)
    {
        for (int i = 0; i < _allTiles.Count; i++)
        {
            if (_allTiles[i] == position)
                return i;
        }

        return default;
    }

    private TileData[] GetCompletedPath()
    {
        List<Vector3Int> path = new List<Vector3Int>(15);
        path.Add(end);

        int attempt = 0;
        int maxAttempt = 25;

        while (path[path.Count - 1] != start)
        {
            path.Add(FindEasiestNeighbor(path[path.Count - 1]));
            attempt++;

            if (attempt > maxAttempt)
            {
                if (_debug)
                    Debug.LogError("path not completed");
                path.Add(start);
                break;
            }
        }

        path.Remove(start);
        path.Reverse();

        TileData[] datas = new TileData[path.Count];
        for (int i = 0; i < path.Count; i++)
            datas[i] = _map.GetTileData(path[i]);

        return datas;
    }

    private Vector3Int FindEasiestNeighbor(Vector3Int position)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>()
        {
            new Vector3Int(position.x + 1, position.y, 0),
            new Vector3Int(position.x, position.y + 1, 0),
            new Vector3Int(position.x - 1, position.y, 0),
            new Vector3Int(position.x, position.y - 1, 0)
        };

        for (int i = neighbors.Count - 1; i >= 0; i--)
            if (!_allTiles.Contains(neighbors[i]))
                neighbors.RemoveAt(i);

        int index = 0;
        float value = float.MaxValue;

        float currentCost;

        for (int i = 0; i < neighbors.Count; i++)
        {
            currentCost = _tileCosts[GetPositionIndex(neighbors[i])];

            if (currentCost < value)
            {
                value = _tileCosts[GetPositionIndex(neighbors[i])];
                index = i;
            }
        }

        return neighbors[index];
    }
}
