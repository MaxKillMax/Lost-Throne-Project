using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Pathfinding
{
    protected Action<Vector3, string> OnTextRequired;
    protected Map Map;
    protected Tilemap Tilemap;

    protected bool Debug;

    public virtual void Initialize(Map map, Tilemap tilemap, Action<Vector3, string> onTextRequired, bool debug)
    {
        Map = map;
        Tilemap = tilemap;
        OnTextRequired = onTextRequired;
        Debug = debug;
    }

    public abstract TileData[] CreatePath(Vector3 startPosition, Vector3 endPosition);
}
