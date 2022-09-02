using System.Collections.Generic;
using UnityEngine;

namespace Game.Board
{
    public class AIInput : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private Board _board;

        [SerializeField] private float _delay = 1;
        private float _time = 1;

        private void Update()
        {
            if (_player.State != PlayerState.Attack || !_board.GameInProgress)
                return;

            _time -= Time.deltaTime;

            if (_time > 0)
                return;

            _time = _delay;

            if (TryFindAttackUnitAndEnemyUnit(out UnitCard attackCard, out UnitCard enemyCard))
            {
                new PickupCommand(_board, _player, attackCard).Execute();
                new AttackCommand(_board, _player, attackCard, enemyCard, () => { new DropCommand(_board, _player, attackCard).Execute(); }).Execute();
                return;
            }

            if (TryAttackTowers(out UnitCard towerCard, out TowerCard tower))
            {
                new PickupCommand(_board, _player, towerCard).Execute();
                new AttackCommand(_board, _player, towerCard, tower, () => { new DropCommand(_board, _player, towerCard).Execute(); }).Execute();
                return;
            }

            if (TryFindCardWithMinTurnCost(out UnitCard movementCard) && TryFindMovementCell(movementCard, out Cell movementCell))
            {
                new PickupCommand(_board, _player, movementCard).Execute();
                new MovementCommand(_board, _player, movementCard, movementCell, () => { new DropCommand(_board, _player, movementCard).Execute(); }).Execute();
                return;
            }

            _board.EndTurn();
        }

        #region Attack

        private bool TryFindAttackUnitAndEnemyUnit(out UnitCard unit, out UnitCard enemy)
        {
            float priority = float.MinValue;
            UnitCard findedEnemyUnit = null;
            UnitCard findedUnit = null;

            for (int i = 0; i < _player.Cards.Count; i++)
            {
                if (!CanDo(_player.Cards[i]))
                    continue;

                Cell cell = _board.FindCellOfCard(_player.Cards[i], out int horizontal, out int vertical);
                if (cell == null)
                    continue;

                Line enemyLine = cell.GetLine(_player.Type == PositionType.Top ? PositionType.Bottom : PositionType.Top);
                if (enemyLine.Cards.Count <= 0)
                    continue;

                for (int x = 0; x < enemyLine.Cards.Count; x++)
                {
                    if (CanKillCard(_player.Cards[i], enemyLine.Cards[x]))
                    {
                        unit = _player.Cards[i];
                        enemy = enemyLine.Cards[x];
                        return true;
                    }
                }

                for (int x = 0; x < enemyLine.Cards.Count; x++)
                {
                    float currentPriority = GetCardAttackPriority(enemyLine.Cards[x]);
                    if (CanAttack(findedUnit, _player.Cards[i], priority, currentPriority))
                    {
                        priority = currentPriority;
                        findedUnit = _player.Cards[i];
                        findedEnemyUnit = enemyLine.Cards[x];
                    }
                }
            }

            unit = findedUnit;
            enemy = findedEnemyUnit;
            return unit != null;
        }

        private bool TryAttackTowers(out UnitCard unit, out TowerCard tower)
        {
            float minTowerHealth = float.MaxValue;
            UnitCard findedUnit= null;
            TowerCard findedTower = null;

            for (int i = 0; i < _player.Cards.Count; i++)
            {
                if (!CanDo(_player.Cards[i]))
                    continue;

                Cell cell = _board.FindCellOfCard(_player.Cards[i], out int horizontal, out int vertical);
                if (cell == null || vertical != _board.PlayerLine)
                    continue;

                if (_board.TryGetTower(horizontal, vertical, out TowerCard gettedTower))
                {
                    if (gettedTower.Unit.GetStatistics(StatisticsType.Health).Value < minTowerHealth)
                    {
                        findedUnit = _player.Cards[i];
                        findedTower = gettedTower;
                    }
                }
            }

            unit = findedUnit;
            tower = findedTower;
            return unit != null;
        }

        private bool CanKillCard(UnitCard attackCard, UnitCard card)
        {
            float damage = attackCard.Unit.GetStatistics(StatisticsType.Damage).Value;
            float enemyArmor = card.Unit.GetStatistics(StatisticsType.Armor).Value;
            float enemyHealth = card.Unit.GetStatistics(StatisticsType.Health).Value;

            enemyHealth = enemyHealth - damage * (100 - enemyArmor) / 100;
            return enemyHealth <= 0;
        }

        private bool CanDo(UnitCard unit) => _player.TurnPoints >= unit.TurnCost && unit.TurnCost < 4;

        private bool CanAttack(UnitCard findedCard, UnitCard currentCard, float priority, float currentPriority) => findedCard == null || currentCard.TurnCost < findedCard.TurnCost || (currentCard.TurnCost == findedCard.TurnCost && currentPriority > priority);

        private float GetCardAttackPriority(UnitCard card) => card.Unit.Level + (100 - card.Unit.GetStatistics(StatisticsType.Health).Value) + (card.Unit.GetStatistics(StatisticsType.Damage).Value * 2) + (100 - card.Unit.GetStatistics(StatisticsType.Armor).Value);

        #endregion

        #region Movement

        private bool TryFindCardWithMinTurnCost(out UnitCard card)
        {
            int minCost = int.MaxValue;
            UnitCard minCostCard = null;

            for (int i = 0; i < _player.Cards.Count; i++)
            {
                if (_player.TurnPoints >= _player.Cards[i].TurnCost)
                {
                    if (_player.Cards[i].TurnCost < minCost || (_player.Cards[i].TurnCost == minCost && Random.Range(0, 3) > 0))
                    {
                        minCost = _player.Cards[i].TurnCost;
                        minCostCard = _player.Cards[i];
                    }
                }
            }

            card = minCostCard;
            return minCostCard != null;
        }

        private bool TryFindMovementCell(UnitCard card, out Cell movementCell)
        {
            List<Cell> cells = new List<Cell>(9);
            List<Cell> cellsWithEnemies = new List<Cell>(1);

            for (int vertical = 0; vertical < _board.Grid.GetLength(0); vertical++)
                for (int horizontal = 0; horizontal < _board.Grid.GetLength(1); horizontal++)
                    cells.Add(_board.Grid[vertical, horizontal]);

            _board.FindCellOfCard(card, out int cardHorizontal, out int cardVertical);

            for (int i = cells.Count - 1; i >= 0; i--)
            {
                _board.FindCellCoordinates(cells[i], out int cellHorizontal, out int cellVertical);
                if (_board.HasWrongPosition(_player, cardHorizontal, cardVertical, cellHorizontal, cellVertical))
                    cells.RemoveAt(i);
            }

            for (int i = 0; i < cells.Count; i++)
            {
                Line enemyLine = cells[i].GetLine(_player.Type == PositionType.Top ? PositionType.Bottom : PositionType.Top);
                if (enemyLine.Cards.Count > 0)
                    cellsWithEnemies.Add(cells[i]);
            }

            if (cellsWithEnemies.Count > 0)
            {
                float priority = float.MinValue;
                Cell cell = null;

                for (int i = 0; i < cellsWithEnemies.Count; i++)
                {
                    Line enemyLine = cells[i].GetLine(_player.Type == PositionType.Top ? PositionType.Bottom : PositionType.Top);
                    Line playerLine = cells[i].GetLine(_player.Type);

                    float currentPriority = 0;

                    for (int x = 0; x < playerLine.Cards.Count; x++)
                        currentPriority -= GetCardAttackPriority(playerLine.Cards[x]);
                    for (int x = 0; x < enemyLine.Cards.Count; x++)
                        currentPriority += GetCardAttackPriority(enemyLine.Cards[x]);

                    if (currentPriority > priority)
                    {
                        priority = currentPriority;
                        cell = cellsWithEnemies[i];
                    }
                }

                movementCell = cell;
                return true;
            }

            if (cells.Count == 0)
            {
                movementCell = default;
                return false;
            }

            movementCell = cells[Random.Range(0, cells.Count)];
            return true;
        }

        #endregion
    }
}
