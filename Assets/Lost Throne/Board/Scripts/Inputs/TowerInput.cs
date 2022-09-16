using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.Board
{
    public class TowerInput : BoardInput
    {
        public event Action OnTowersAttacked;

        public void InitializePlayer(BoardPlayer player, BoardPlayer enemy)
        {
            Player = player;
            Enemy = enemy;
        }

        public void AttackEnemies()
        {
            GetTowersWithEnemiesOnCell(out List<TowerCard> towers, out List<UnitCard> enemies);
            StartCoroutine(WaitForAttack(towers, enemies));
        }

        private IEnumerator WaitForAttack(List<TowerCard> towers, List<UnitCard> enemies)
        {
            int index = 0;

            while (index < towers.Count)
            {
                AttackEnemy(towers[index], enemies[index]);
                index++;
                yield return new WaitForSeconds(1);
            }

            OnTowersAttacked?.Invoke();
        }

        private void GetTowersWithEnemiesOnCell(out List<TowerCard> towerCards, out List<UnitCard> enemyUnits)
        {
            List<TowerCard> towers = new(1);
            List<UnitCard> enemies = new(1);

            for (int i = 0; i < Player.Towers.Count; i++)
            {
                Base.GetTowerCell(Player, Player.Towers[i], out int horizontal, out int vertical);

                List<UnitCard> enemyCards = Base.GetUnitsWithCondition(Enemy.Cards, (unitCard) => CheckEnemyUnitCard(unitCard, horizontal, vertical));

                if (enemyCards.Count == 0)
                    continue;

                towers.Add(Player.Towers[i]);
                enemies.Add((UnitCard)Base.GetUnitWithBestAttackImportance(enemyCards, out _));
            }

            towerCards = towers;
            enemyUnits = enemies;
        }

        private bool CheckEnemyUnitCard(UnitCard unitCard, int towerHorizontal, int towerVertical)
        {
            Base.GetUnitCell(unitCard, out int enemyHorizontal, out int enemyVertical);
            return enemyHorizontal == towerHorizontal && enemyVertical == towerVertical;
        }

        private void AttackEnemy(TowerCard tower, UnitCard enemy)
        {
            new AttackUnitWithTowerCommand(Board, Player, tower, enemy).Execute();
        }
    }
}
