using System.Linq;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingData _data;

    private Vector3Int _cellPosition;
    private Vector3 _worldPosition;

    public Vector3Int CellPosition => _cellPosition;
    public Vector3 WorldPosition => _worldPosition;

    public float SpawnChanceChange => _data.SpawnChanceChange;

    public void Initialize(Vector3 worldPosition, Vector3Int cellPosition)
    {
        _cellPosition = cellPosition;
        _worldPosition = worldPosition;
    }

    public bool GroundTypeEquals(TileType tileType) => _data.GroundTypes.Any(t => t == tileType);
}
