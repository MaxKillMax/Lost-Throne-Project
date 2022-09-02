using UnityEngine;

[CreateAssetMenu(fileName = "Building Data", menuName = "Building Data", order = 52)]
public class BuildingData : ScriptableObject
{
    [SerializeField] public string Name;
    [SerializeField] public Sprite Icon;
    [SerializeField] public BuildingType Type;
    [SerializeField] public TileType[] GroundTypes;
    [SerializeField, Range(0, 1)] public float SpawnChanceChange;
}

public enum BuildingType
{
    Town,
    Dungeon,
    Quest,
    Building
}
