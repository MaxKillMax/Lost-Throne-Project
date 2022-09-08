using System;
using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.Board
{
    public class AIInput : BoardInput
    {
        private float _delayBeforeAction = 1;
        private float _time = 1;

        private void Update()
        {
            if (!_board.Base.PlayerCanMove(_board, _player))
                return;

            _time -= Time.deltaTime;

            if (_time > 0)
                return;

            _time = _delayBeforeAction;

            if (TryGetEnemyUnitCard(ref _selectedCard, out UnitCard enemyUnitCard))
                AttackUnitCard(enemyUnitCard);
            else if (TryGetEnemyTowerCard(ref _selectedCard, out TowerCard enemyTowerCard))
                AttackTowerCard(enemyTowerCard);
            else if (CanMove(ref _selectedCard, out Cell cell))
                Move(cell);

            _board.SwitchTurn();
        }

        #region Checks

        private bool TryGetEnemyUnitCard(ref UnitCard unitCard, out UnitCard enemyUnitCard)
        {
            enemyUnitCard = default;

            List<UnitCard> cards = GetCards((unitCard) => _board.Base.UnitCanAttack(_board, unitCard, _player));

            if (cards.Count == 0)
                return false;

            UnitCard card = _board.Base.GetUnitWithBestCondition(cards, true, GetUnitAttackImportance);
            UnitCard enemyCard = GetEnemyUnitWithBestAttackImportance(card);

            unitCard = card;
            enemyUnitCard = enemyCard;
            return true;
        }

        private float GetUnitAttackImportance(UnitCard unitCard)
            => _formulas.GetUnitAttackImportance(_board.Base.GetUnitWithBestAttackImportance(_board.Base.GetUnitCell(_board, unitCard).GetLine(_player.Type).Cards, out float importance).Unit);

        private UnitCard GetEnemyUnitWithBestAttackImportance(UnitCard unitCard)
            => (UnitCard)_board.Base.GetUnitWithBestAttackImportance(_board.Base.GetUnitCell(_board, unitCard).GetLine(_enemy.Type).Cards, out float importance);

        private bool TryGetEnemyTowerCard(ref UnitCard unitCard, out TowerCard enemyTowerCard)
        {
            enemyTowerCard = default;

            List<UnitCard> cards = GetCards((unitCard) => _board.Base.UnitCanAttack(_board, unitCard, _player));

            if (cards.Count == 0)
                return false;

            List<UnitCard> towerCards = new List<UnitCard>(1);
            for (int i = 0; i < _enemy.Towers.Count; i++)
            {
                Cell towerCell = _board.Base.GetTowerCell(_board, _enemy, _enemy.Towers[i]);
                towerCards.Add(_board.Base.GetUnitWithCondition(cards, (unitCard) => _board.Base.GetUnitCell(_board, unitCard) == towerCell));
            }

            if (towerCards.Count == 0)
                return false;

            float maxPriority = float.MinValue;

            for (int i = 0; i < _enemy.Towers.Count; i++)
            {
                if (towerCards[i] != null)
                {
                    if (_board.Base.UnitCanDie(_enemy.Towers[i], towerCards[i]))
                    {
                        unitCard = towerCards[i];
                        enemyTowerCard = _enemy.Towers[i];
                        return true;
                    }

                    float towerPriority = _formulas.GetUnitAttackImportance(_enemy.Towers[i].Unit);
                    float playerPriority = _formulas.GetUnitAttackImportance(towerCards[i].Unit);
                    float linePriority = towerPriority - playerPriority;

                    if (linePriority > maxPriority)
                    {
                        maxPriority = linePriority;
                        enemyTowerCard = _enemy.Towers[i];
                        unitCard = towerCards[i];
                    }
                }
            }

            return true;
        }

        private bool CanMove(ref UnitCard unitCard, out Cell cell)
        {
            cell = default;

            List<UnitCard> cards = GetCards((unitCard) => _board.Base.UnitCanMove(_board, unitCard, _player));

            cards = _board.Base.GetUnitsWithBestCondition(cards, false, (unitCard) => unitCard.TurnCost);
            UnitCard card = cards[UnityEngine.Random.Range(0, cards.Count)];

            List<Cell> cells = _board.Base.GetMovementCells(_board, _player, card);
            List<Card> enemies = new List<Card>(1);

            float maxPriority = float.MinValue;
            Cell priorityCell = default;

            for (int i = 0; i < cells.Count; i++)
            {
                if (!_board.Base.CellHaveEnemies(_player, cells[i]))
                    continue;

                Line enemyLine = cells[i].GetLine(_enemy.Type);
                Line playerLine = cells[i].GetLine(_player.Type);

                float enemyPriority = _board.Base.GetSumOfUnitsCondition(enemyLine.Cards, (unit) => _formulas.GetUnitAttackImportance(unit.Unit));
                float playerPriority = _board.Base.GetSumOfUnitsCondition(playerLine.Cards, (unit) => _formulas.GetUnitAttackImportance(unit.Unit));
                float linePriority = enemyPriority - playerPriority;

                if (linePriority > maxPriority)
                {
                    maxPriority = linePriority;
                    priorityCell = cells[i];
                }
            }
            
            if (cards.Count != 0 && cells.Count != 0)
            {
                unitCard = card;
                cell = cells[UnityEngine.Random.Range(0, cells.Count)];
                return true;
            }

            return false;
        }

        #endregion

        #region Actions

        private void AttackUnitCard(UnitCard enemyUnitCard)
        {
            Debug.Log("Attack units: " + _selectedCard + " : " + enemyUnitCard);
            new PickupCommand(_board, _player, _selectedCard, () =>
            new AttackUnitCommand(_board, _player, _selectedCard, enemyUnitCard, () =>
            new DropCommand(_board, _player, _selectedCard).Execute()).Execute()).Execute();
        }

        private void AttackTowerCard(TowerCard enemyTowerCard)
        {
            Debug.Log("Attack towers: " + _selectedCard + " : " + enemyTowerCard);
            new PickupCommand(_board, _player, _selectedCard, () =>
            new AttackTowerCommand(_board, _player, _selectedCard, _enemy, enemyTowerCard, () =>
            new DropCommand(_board, _player, _selectedCard).Execute()).Execute()).Execute();
        }

        private void Move(Cell cell)
        {
            Debug.Log("Move units: " + _selectedCard + " : " + cell);
            new PickupCommand(_board, _player, _selectedCard, () =>
            new MovementCommand(_board, _player, _selectedCard, cell, () =>
            new DropCommand(_board, _player, _selectedCard).Execute()).Execute()).Execute();
        }

        #endregion

        private List<UnitCard> GetCards(Func<UnitCard, bool> condition)
        {
            List<UnitCard> cards = new List<UnitCard>(_player.Cards);

            for (int i = cards.Count - 1; i >= 0; i--)
                if (!condition.Invoke(cards[i]))
                    cards.RemoveAt(i);

            return cards;
        }

        // OLD

        private bool CanAttack(UnitCard findedCard, UnitCard currentCard, float priority, float currentPriority) => findedCard == null || currentCard.TurnCost < findedCard.TurnCost || (currentCard.TurnCost == findedCard.TurnCost && currentPriority > priority);
    }
}
