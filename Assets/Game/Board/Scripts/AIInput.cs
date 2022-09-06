using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.Board
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

            if (_player.Cards.Count == 0)
                _board.EndTurn();

            _time -= Time.deltaTime;

            if (_time > 0)
                return;

            _time = _delay;

            if (TryFindAttackUnitAndEnemyUnit(out UnitCard attackCard, out UnitCard enemyCard))
            {
                //Debug.Log("Attack units: " + attackCard + " : " + enemyCard);
                new PickupCommand(_board, _player, attackCard, () =>
                new AttackUnitCommand(_board, _player, attackCard, enemyCard, () => 
                new DropCommand(_board, _player, attackCard).Execute()).Execute()).Execute();
                return;
            }

            if (TryAttackTowers(out UnitCard towerCard, out TowerCard tower))
            {
                //Debug.Log("Attack towers: " + towerCard + " : " + tower);
                new PickupCommand(_board, _player, towerCard, () =>
                new AttackTowerCommand(_board, _player, towerCard, tower, () => 
                new DropCommand(_board, _player, towerCard).Execute()).Execute()).Execute();
                return;
            }

            UnitCard movementCard = Services.GetService<BoardBase>().GetUnitWithBestCondition(_player.Cards, false, (unit) => unit.TurnCost, out float value);
            if (Services.GetService<BoardBase>().UnitCanMove(movementCard, _player) && TryFindMovementCell(movementCard, out Cell movementCell))
            {
                //Debug.Log("Move units: " + movementCard + " : " + movementCell);
                new PickupCommand(_board, _player, movementCard, () =>
                new MovementCommand(_board, _player, movementCard, movementCell, () => 
                new DropCommand(_board, _player, movementCard).Execute()).Execute()).Execute();
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
                if (!Services.GetService<BoardBase>().UnitCanMove(_player.Cards[i], _player))
                    continue;

                Cell cell = Services.GetService<BoardBase>().GetCardCell(_board, _player.Cards[i], out int horizontal, out int vertical);
                if (cell == null)
                    continue;

                Line enemyLine = cell.GetLine(_player.Type == PositionType.Top ? PositionType.Bottom : PositionType.Top);
                if (enemyLine.Cards.Count <= 0)
                    continue;

                for (int x = 0; x < enemyLine.Cards.Count; x++)
                {
                    if (Services.GetService<BoardBase>().UnitCanDie(enemyLine.Cards[x], _player.Cards[i]))
                    {
                        unit = _player.Cards[i];
                        enemy = enemyLine.Cards[x];
                        return true;
                    }
                }

                for (int x = 0; x < enemyLine.Cards.Count; x++)
                {
                    float currentPriority = Services.GetService<Formulas>().GetUnitAttackImportance(enemyLine.Cards[x].Unit);
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
            unit = default;
            tower = default;

            List<TowerCard> findedTowers = new List<TowerCard>();
            List<UnitCard> units = new List<UnitCard>();

            for (int i = 0; i < _player.Cards.Count; i++)
            {
                if (!Services.GetService<BoardBase>().UnitCanMove(_player.Cards[i], _player))
                    continue;

                Cell cell = Services.GetService<BoardBase>().GetCardCell(_board, _player.Cards[i], out int horizontal, out int vertical);
                if (cell == null || vertical != _board.PlayerLine)
                    continue;

                if (Services.GetService<BoardBase>().TryGetTower(_board, horizontal, vertical, out tower))
                {
                    findedTowers.Add(tower);
                    units.Add(_player.Cards[i]);
                }
            }

            
            for (int i = 0; i < findedTowers.Count; i++)
            {
                if (Services.GetService<BoardBase>().UnitCanDie(findedTowers[i], units[i]))
                {
                    unit = units[i];
                    tower = findedTowers[i];
                    return true;
                }
            }

            if (findedTowers.Count > 0)
            {
                unit = (UnitCard)Services.GetService<BoardBase>().GetUnitWithBestAttackImportance(units, out float importance);
                for (int i = 0; i < units.Count; i++)
                    if (unit == units[i])
                        tower = findedTowers[i];
            }

            return unit != null;
        }

        private bool CanAttack(UnitCard findedCard, UnitCard currentCard, float priority, float currentPriority) => findedCard == null || currentCard.TurnCost < findedCard.TurnCost || (currentCard.TurnCost == findedCard.TurnCost && currentPriority > priority);

        #endregion

        #region Movement

        private bool TryFindMovementCell(UnitCard card, out Cell movementCell)
        {
            List<Cell> cells = Services.GetService<BoardBase>().GetMovementCells(_board, _player, card);
            List<Cell> cellsWithEnemies = new List<Cell>(1);

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

                    float enemyPriority = Services.GetService<BoardBase>().GetSumOfUnitsCondition(enemyLine.Cards, (unit) => Services.GetService<Formulas>().GetUnitAttackImportance(unit.Unit));
                    float playerPriority = Services.GetService<BoardBase>().GetSumOfUnitsCondition(playerLine.Cards, (unit) => Services.GetService<Formulas>().GetUnitAttackImportance(unit.Unit));
                    float linePriority = enemyPriority - playerPriority;

                    if (linePriority > priority)
                    {
                        priority = linePriority;
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
