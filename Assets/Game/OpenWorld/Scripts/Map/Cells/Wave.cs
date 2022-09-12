using System.Collections.Generic;
using UnityEngine;

public class Wave : Pathfinding
{
    private List<float> _tileCosts;
    private List<Vector3Int> _checkedTiles;
    private List<Vector3Int> _allTiles;

    private Vector3Int _start;
    private Vector3Int _end;

    public override TileData[] CreatePath(Vector3 startPosition, Vector3 endPosition)
    {
        if (Tilemap.WorldToCell(startPosition) == Tilemap.WorldToCell(endPosition))
            return new TileData[] { Map.GetTileData(startPosition) };

        InitializeAlgorythm(startPosition, endPosition);
        CheckTile(_start);

        if (_allTiles.Count == _checkedTiles.Count)
            return default;

        if (!FindEndTile())
            return default;

        if (Debug)
            CreateMoneyTexts();

        return GetCompletedPath();
    }

    private void CreateMoneyTexts()
    {
        for (int i = 0; i < _allTiles.Count; i++)
        {
            string cost = _tileCosts[GetPositionIndex(_allTiles[i])].ToString();
            OnTextRequired?.Invoke(Map.GetTileData(_allTiles[i]).RealPosition, cost);
        }
    }

    private void InitializeAlgorythm(Vector3 startPosition, Vector3 endPosition)
    {
        _start = Tilemap.WorldToCell(startPosition);
        _end = Tilemap.WorldToCell(endPosition);

        _tileCosts = new List<float>(200);
        _checkedTiles = new List<Vector3Int>(200);
        _allTiles = new List<Vector3Int>(300);

        _tileCosts.Add(0);
        _allTiles.Add(_start);
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
                if (Debug)
                    UnityEngine.Debug.LogWarning("attempt limit exceeded");
                break;
            }

            if (_allTiles.Count == _checkedTiles.Count)
            {
                if (Debug)
                    UnityEngine.Debug.LogWarning("all tiles equals checked tiles");
                break;
            }

            if (HaveEndTile())
            {
                if (Debug)
                {
                    UnityEngine.Debug.LogWarning("end finded");
                    UnityEngine.Debug.LogWarning("cost: " + _tileCosts[GetPositionIndex(_end)]);
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
        List<Vector3Int> neighbors = new()
        {
            new Vector3Int(position.x + 1, position.y, 0),
            new Vector3Int(position.x, position.y + 1, 0),
            new Vector3Int(position.x - 1, position.y, 0),
            new Vector3Int(position.x, position.y - 1, 0)
        };

        for (int i = neighbors.Count - 1; i >= 0; i--)
        {
            if (_allTiles.Contains(neighbors[i]) || !Map.TryGetTileData(neighbors[i], out TileData data) || data.Type == TileType.Impassable)
                neighbors.RemoveAt(i);
        }

        return neighbors;
    }

    private void SetCostsToTiles(Vector3Int parent, ref List<Vector3Int> tiles)
    {
        float parentCost = 0;

        for (int i = 0; i < _allTiles.Count; i++)
        {
            if (_allTiles[i] == parent)
                parentCost = _tileCosts[i];
        }

        for (int i = 0; i < tiles.Count; i++)
            _tileCosts.Add(parentCost + Map.GetTileData(tiles[i]).MovementCost);
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

    private bool HaveEndTile() => _allTiles.Contains(_end);

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
        List<Vector3Int> path = new(15)
        {
            _end
        };

        int attempt = 0;
        int maxAttempt = 25;

        while (path[^1] != _start)
        {
            path.Add(FindEasiestNeighbor(path[^1]));
            attempt++;

            if (attempt > maxAttempt)
            {
                if (Debug)
                    UnityEngine.Debug.LogError("path not completed");
                path.Add(_start);
                break;
            }
        }

        path.Remove(_start);
        path.Reverse();

        TileData[] datas = new TileData[path.Count];
        for (int i = 0; i < path.Count; i++)
            datas[i] = Map.GetTileData(path[i]);

        return datas;
    }

    private Vector3Int FindEasiestNeighbor(Vector3Int position)
    {
        List<Vector3Int> neighbors = new()
        {
            new Vector3Int(position.x + 1, position.y, 0),
            new Vector3Int(position.x, position.y + 1, 0),
            new Vector3Int(position.x - 1, position.y, 0),
            new Vector3Int(position.x, position.y - 1, 0)
        };

        for (int i = neighbors.Count - 1; i >= 0; i--)
        {
            if (!_allTiles.Contains(neighbors[i]))
                neighbors.RemoveAt(i);
        }

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
