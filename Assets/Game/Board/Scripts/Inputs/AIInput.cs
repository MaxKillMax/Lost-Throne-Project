using System;
using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.Board
{
    public class AIInput : BoardInput
    {
        private readonly float _delayBeforeAction = 1;
        private float _time = 1;

        private void Update()
        {
            if (!Base.PlayerCanMove(Player))
                return;

            _time -= Time.deltaTime;

            if (_time > 0)
                return;

            _time = _delayBeforeAction;

            if (TryGetEnemyUnitCard(ref SelectedCard, out UnitCard enemyUnitCard))
                AttackUnitCard(enemyUnitCard);
            else if (TryGetEnemyTowerCard(ref SelectedCard, out TowerCard enemyTowerCard))
                AttackTowerCard(enemyTowerCard);
            else if (CanMove(ref SelectedCard, out Cell cell))
                Move(cell);

            Board.SwitchTurn();
        }

        #region Checks

        private bool TryGetEnemyUnitCard(ref UnitCard unitCard, out UnitCard enemyUnitCard)
        {
            enemyUnitCard = default;

            List<UnitCard> cards = GetCards((unitCard) => Board.Base.UnitCanAttack(unitCard, Player));

            if (cards.Count == 0)
                return false;

            UnitCard card = Board.Base.GetUnitWithBestCondition(cards, true, GetUnitAttackImportance);
            UnitCard enemyCard = GetEnemyUnitWithBestAttackImportance(card);

            unitCard = card;
            enemyUnitCard = enemyCard;
            return true;
        }

        private float GetUnitAttackImportance(UnitCard unitCard)
            => Formulas.GetUnitAttackImportance(Board.Base.GetUnitWithBestAttackImportance(Board.Base.GetUnitCell(unitCard).GetLine(Player.Type).Cards, out _).GetUnit());

        private UnitCard GetEnemyUnitWithBestAttackImportance(UnitCard unitCard)
            => (UnitCard)Board.Base.GetUnitWithBestAttackImportance(Board.Base.GetUnitCell(unitCard).GetLine(Enemy.Type).Cards, out _);

        private bool TryGetEnemyTowerCard(ref UnitCard unitCard, out TowerCard enemyTowerCard)
        {
            enemyTowerCard = default;

            List<UnitCard> cards = GetCards((unitCard) => Board.Base.UnitCanAttack(unitCard, Player));

            if (cards.Count == 0)
                return false;

            List<UnitCard> towerCards = new(1);
            for (int i = 0; i < Enemy.Towers.Count; i++)
            {
                Cell towerCell = Board.Base.GetTowerCell(Enemy, Enemy.Towers[i]);
                towerCards.Add(Board.Base.GetUnitWithCondition(cards, (unitCard) => Board.Base.GetUnitCell(unitCard) == towerCell));
            }

            if (towerCards.Count == 0)
                return false;

            float maxPriority = float.MinValue;

            for (int i = 0; i < Enemy.Towers.Count; i++)
            {
                if (towerCards[i] != null)
                {
                    if (Board.Base.UnitCanDie(Enemy.Towers[i], towerCards[i]))
                    {
                        unitCard = towerCards[i];
                        enemyTowerCard = Enemy.Towers[i];
                        return true;
                    }

                    float towerPriority = Formulas.GetUnitAttackImportance(Enemy.Towers[i].GetUnit());
                    float playerPriority = Formulas.GetUnitAttackImportance(towerCards[i].GetUnit());
                    float linePriority = towerPriority - playerPriority;

                    if (linePriority > maxPriority)
                    {
                        maxPriority = linePriority;
                        enemyTowerCard = Enemy.Towers[i];
                        unitCard = towerCards[i];
                    }
                }
            }

            return true;
        }

        private bool CanMove(ref UnitCard unitCard, out Cell cell)
        {
            cell = default;

            List<UnitCard> cards = GetCards((unitCard) => Board.Base.UnitCanMove(unitCard, Player));

            cards = Board.Base.GetUnitsWithBestCondition(cards, false, (unitCard) => unitCard.TurnCost);
            UnitCard card = cards[UnityEngine.Random.Range(0, cards.Count)];

            List<Cell> cells = Board.Base.GetMovementCells(Player, card);
            List<Card> enemies = new(1);

            float maxPriority = float.MinValue;
            Cell priorityCell = default;

            for (int i = 0; i < cells.Count; i++)
            {
                if (!Board.Base.CellHaveEnemies(Player, cells[i]))
                    continue;

                Line enemyLine = cells[i].GetLine(Enemy.Type);
                Line playerLine = cells[i].GetLine(Player.Type);

                float enemyPriority = Board.Base.GetSumOfUnitsCondition(enemyLine.Cards, (unit) => Formulas.GetUnitAttackImportance(unit.GetUnit()));
                float playerPriority = Board.Base.GetSumOfUnitsCondition(playerLine.Cards, (unit) => Formulas.GetUnitAttackImportance(unit.GetUnit()));
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
            Debug.Log("Attack units: " + SelectedCard + " : " + enemyUnitCard);
            new PickupCommand(Board, Player, SelectedCard, () =>
            new AttackUnitCommand(Board, Player, SelectedCard, enemyUnitCard, () =>
            new DropCommand(Board, Player, SelectedCard).Execute()).Execute()).Execute();
        }

        private void AttackTowerCard(TowerCard enemyTowerCard)
        {
            Debug.Log("Attack towers: " + SelectedCard + " : " + enemyTowerCard);
            new PickupCommand(Board, Player, SelectedCard, () =>
            new AttackTowerCommand(Board, Player, SelectedCard, Enemy, enemyTowerCard, () =>
            new DropCommand(Board, Player, SelectedCard).Execute()).Execute()).Execute();
        }

        private void Move(Cell cell)
        {
            Debug.Log("Move units: " + SelectedCard + " : " + cell);
            new PickupCommand(Board, Player, SelectedCard, () =>
            new MovementCommand(Board, Player, SelectedCard, cell, () =>
            new DropCommand(Board, Player, SelectedCard).Execute()).Execute()).Execute();
        }

        #endregion

        private List<UnitCard> GetCards(Func<UnitCard, bool> condition)
        {
            List<UnitCard> cards = new(Player.Cards);

            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (!condition.Invoke(cards[i]))
                    cards.RemoveAt(i);
            }

            return cards;
        }
    }
}
