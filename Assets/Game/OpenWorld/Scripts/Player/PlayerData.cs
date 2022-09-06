using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.OpenWorld
{
    public class PlayerData : MonoBehaviour
    {
        [SerializeField] private Map _map;
        [SerializeField] private BuildingManager _buildingManager;
        [SerializeField] private Zoom _zoom;

        [SerializeField] private Transform _transform;
        [SerializeField] private GameObject _gameObject;

        [SerializeField] private UnitData _playerUnitData;

        private List<Unit> _units;
        private Tower _tower;

        public Map Map => _map;
        public BuildingManager BuildingManager => _buildingManager;
        public Zoom Zoom => _zoom;

        public GameObject GameObject => _gameObject;
        public Transform Transform => _transform;

        public int Level => _units[0].Level;
        public Unit Unit => _units[0];
        public List<Unit> Units => _units;
        public Tower Tower => _tower;

        public UnitData PlayerUnitData => _playerUnitData;

        public void Initialize(Unit player)
        {
            _units = new List<Unit>(5);
            _units.Add(player);
        }
    }

    [System.Serializable]
    public struct BattleData
    {
        public bool HaveMainUnit => MainUnit != null;

        public Unit MainUnit;
        public List<Unit> Units;
        public Tower Tower;

        public BattleData(Unit mainUnit, List<Unit> units, Tower tower)
        {
            MainUnit = mainUnit;
            Units = units;
            Tower = tower;
        }
    }
}
