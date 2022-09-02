using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Pathfinding
{
    protected Action<Vector3, string> onTextRequired;
    protected Map _map;
    protected Tilemap _tilemap;

    protected bool _debug;

    public virtual void Initialize(Map map, Tilemap tilemap, Action<Vector3, string> onTextRequired, bool debug)
    {
        _map = map;
        _tilemap = tilemap;
        this.onTextRequired = onTextRequired;
        _debug = debug;
    }

    public abstract TileData[] CreatePath(Vector3 startPosition, Vector3 endPosition);
}
