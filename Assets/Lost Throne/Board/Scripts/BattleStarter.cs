using System.Collections;
using System.Collections.Generic;
using LostThrone.OpenWorld;
using UnityEngine;

namespace LostThrone.Board
{
    public class BattleStarter : MonoBehaviour
    {
        // TODO: all script is bad

        [SerializeField] private Board _board;

        [Space]

        [SerializeField] private bool _playerIsFirst;
        [SerializeField] private bool _usePlayerData;
        [SerializeField] private PlayerData _player;

        [Space]

        [SerializeField] private bool _enemyHaveMainUnit;
        [SerializeField] private UnitData[] _enemies;
        [SerializeField] private UnitData _tower;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);

            List<Unit> enemyUnits = CreateUnits(_enemies);
            Tower enemyTower = CreateTower(_tower);
            BattleData enemyBattleData = new(_enemyHaveMainUnit ? enemyUnits[0] : null, enemyUnits, enemyTower);

            BattleData playerBattleData;
            if (_usePlayerData)
            {
                playerBattleData = new BattleData(_player.Unit, _player.Units, _player.Tower);
            }
            else
            {
                List<Unit> playerUnits = CreateUnits(_enemies);
                Tower playerTower = CreateTower(_tower);
                playerBattleData = new BattleData(_enemyHaveMainUnit ? playerUnits[0] : null, playerUnits, playerTower);
            }

            _board.StartBattle(_playerIsFirst, playerBattleData, enemyBattleData);
        }

        private List<Unit> CreateUnits(UnitData[] datas)
        {
            List<Unit> units = new(datas.Length);
            for (int i = 0; i < datas.Length; i++)
                units.Add(new Unit(datas[i]));
            return units;
        }

        private Tower CreateTower(UnitData data)
        {
            return new Tower(data);
        }
    }
}
