using System;
using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.Board
{
    public class AIInput : BoardInput
    {
        public Action<AIState> OnStateChanged;

        private UnitCard _selectedCard;

        private readonly float _delayBeforeAction = 1;
        private float _time = 1;

        private AIState _state = AIState.Nothing;

        public AIState State => _state;

        private void Update()
        {
            if (!Base.PlayerCanMove(Player))
                return;

            _time -= Time.deltaTime;

            if (_time > 0)
                return;

            _time = _delayBeforeAction;

            if (TryGetEnemyUnitCard(ref _selectedCard, out UnitCard enemyUnitCard))
            {
                SetAIState(AIState.Attacks);
                AttackUnitCard(enemyUnitCard);
                return;
            }
            else if (TryGetEnemyTowerCard(ref _selectedCard, out TowerCard enemyTowerCard))
            {
                SetAIState(AIState.AttackTower);
                AttackTowerCard(enemyTowerCard);
                return;
            }
            else if (CanMove(ref _selectedCard, out Cell cell))
            {
                SetAIState(AIState.Moving);
                MoveToCell(cell);
                return;
            }

            SetAIState(AIState.Nothing);

            Board.SwitchTurn();
        }

        private void SetAIState(AIState state)
        {
            _state = state;
            OnStateChanged?.Invoke(_state);
        }

        #region Checks

        private bool TryGetEnemyUnitCard(ref UnitCard unitCard, out UnitCard enemyUnitCard)
        {
            enemyUnitCard = default;

            List<UnitCard> cards = GetCards((unitCard) => Base.UnitCanAttack(unitCard, Player));

            if (cards.Count == 0)
                return false;

            UnitCard card = Base.GetUnitWithBestCondition(cards, true, GetUnitAttackImportance);
            UnitCard enemyCard = GetEnemyUnitWithBestAttackImportance(card);

            unitCard = card;
            enemyUnitCard = enemyCard;
            return true;
        }

        private float GetUnitAttackImportance(UnitCard unitCard)
            => Formulas.GetUnitAttackImportance(Base.GetUnitWithBestAttackImportance(Base.GetUnitCell(unitCard).GetLine(Player.Type).Cards, out _).GetUnit());

        private UnitCard GetEnemyUnitWithBestAttackImportance(UnitCard unitCard)
            => (UnitCard)Base.GetUnitWithBestAttackImportance(Base.GetUnitCell(unitCard).GetLine(Enemy.Type).Cards, out _);

        private bool TryGetEnemyTowerCard(ref UnitCard unitCard, out TowerCard enemyTowerCard)
        {
            enemyTowerCard = default;

            List<UnitCard> cards = GetCards((unitCard) => Base.UnitCanAttackTower(unitCard, Player));

            if (cards.Count == 0)
                return false;

            List<UnitCard> towerCards = new(1);
            for (int i = 0; i < Enemy.Towers.Count; i++)
            {
                Cell towerCell = Base.GetTowerCell(Enemy, Enemy.Towers[i]);
                towerCards.Add(Base.GetUnitWithCondition(cards, (unitCard) => Base.GetUnitCell(unitCard) == towerCell));
            }

            if (towerCards.Count == 0)
                return false;

            float maxPriority = float.MinValue;

            for (int i = 0; i < Enemy.Towers.Count; i++)
            {
                if (towerCards[i] != null)
                {
                    if (Base.CardCanDie(Enemy.Towers[i], towerCards[i]))
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

            List<UnitCard> cards = GetCards((unitCard) => Base.UnitCanMove(unitCard, Player));
            cards = Base.GetUnitsWithBestCondition(cards, false, (unitCard) => unitCard.TurnCost);

            if (cards.Count == 0)
                return false;

            UnitCard card = cards[UnityEngine.Random.Range(0, cards.Count)];

            List<Cell> cells = Base.GetMovementCells(Player, card);
            List<UnitCard> enemies = new(1);

            float maxPriority = float.MinValue;
            Cell priorityCell = default;

            for (int i = 0; i < cells.Count; i++)
            {
                if (!Board.Base.CellHaveEnemies(Player, cells[i]))
                    continue;

                Line enemyLine = cells[i].GetLine(Enemy.Type);
                Line playerLine = cells[i].GetLine(Player.Type);

                float enemyPriority = Base.GetSumOfUnitsCondition(enemyLine.Cards, (unit) => Formulas.GetUnitAttackImportance(unit.GetUnit()));
                float playerPriority = Base.GetSumOfUnitsCondition(playerLine.Cards, (unit) => Formulas.GetUnitAttackImportance(unit.GetUnit()));
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
            UnitCard cachedCard = _selectedCard;

            new PickupCommand(Board, Player, cachedCard, () =>
            new AttackUnitCommand(Board, Player, cachedCard, enemyUnitCard, () =>
            new DropCommand(Board, Player, cachedCard).Execute()).Execute()).Execute();
        }

        private void AttackTowerCard(TowerCard enemyTowerCard)
        {
            UnitCard cachedCard = _selectedCard;

            new PickupCommand(Board, Player, cachedCard, () =>
            new AttackTowerCommand(Board, Player, cachedCard, Enemy, enemyTowerCard, () =>
            new DropCommand(Board, Player, cachedCard).Execute()).Execute()).Execute();
        }

        private void MoveToCell(Cell cell)
        {
            UnitCard cachedCard = _selectedCard;

            new PickupCommand(Board, Player, cachedCard, () =>
            new MovementCommand(Board, Player, cachedCard, cell, () =>
            new DropCommand(Board, Player, cachedCard).Execute()).Execute()).Execute();
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
