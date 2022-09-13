using UnityEngine;

[CreateAssetMenu(fileName = "Building Data", menuName = "Building Data", order = 52)]
public class BuildingData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private Sprite _icon;
    [SerializeField] private BuildingType _type;
    [SerializeField] private TileType[] _groundTypes;
    [SerializeField, Range(0, 1)] private float _spawnChanceChange;

    public string Name { get => _name; set => _name = value; }
    public Sprite Icon { get => _icon; set => _icon = value; }
    public BuildingType Type { get => _type; set => _type = value; }
    public TileType[] GroundTypes { get => _groundTypes; set => _groundTypes = value; }
    public float SpawnChanceChange { get => _spawnChanceChange; set => _spawnChanceChange = value; }
}

public enum BuildingType
{
    Town,
    Dungeon,
    Quest,
    Building
}
