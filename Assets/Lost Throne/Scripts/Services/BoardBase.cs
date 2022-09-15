using System;
using System.Collections.Generic;
using DG.Tweening;
using LostThrone.Board;
using UnityEngine;

namespace LostThrone
{
    public class BoardBase : IService
    {
        private Formulas _formulas;
        private Formulas Formulas => _formulas ??= Services.GetService<Formulas>();

        private Board.Board _board;
        private Cell[,] _grid;
        private Cell[] _cells;

        public void InitializeBoard(Board.Board board)
        {
            _board = board;
            _grid = board.Grid;
            _cells = board.Cells;
        }

        public void RefreshLinePositions(Line line)
        {
            float middle = (line.Cards.Count - 1f) / 2;
            for (int i = 0; i < line.Cards.Count; i++)
                line.Cards[i].transform.DOLocalMove(new Vector3(i - middle, 0, 0), 0.2f);
        }

        #region Get something (positions)

        public List<Cell> GetMovementCells(BoardPlayer player, UnitCard unitCard)
        {
            List<Cell> cells = new(_cells);
            GetUnitCell(unitCard, out int cardHorizontal, out int cardVertical);

            for (int i = cells.Count - 1; i >= 0; i--)
            {
                GetCellCoordinates(cells[i], out int cellHorizontal, out int cellVertical);
                if (HasWrongPosition(player, cardHorizontal, cardVertical, cellHorizontal, cellVertical))
                    cells.RemoveAt(i);
            }

            return cells;
        }

        public Cell GetTowerCell(BoardPlayer playerSide, Card card, out int horizontal, out int vertical)
        {
            vertical = playerSide.Type == _board.PlayerPositionType ? _board.PlayerLine : playerSide.Type == _board.EnemyPositionType ? _board.EnemyLine : -1;

            for (int i = 0; i < playerSide.Towers.Count; i++)
            {
                if (playerSide.Towers[i] == card)
                {
                    horizontal = i;
                    return _grid[vertical, horizontal];
                }
            }

            horizontal = default;
            return default;
        }

        public Cell GetTowerCell(BoardPlayer playerSide, Card card) => GetTowerCell(playerSide, card, out _, out _);

        public Cell GetUnitCell(UnitCard card, out int horizontal, out int vertical)
        {
            PositionType type = card.GetPlayer().Type;
            Cell cell = GetCellWithCondition(out horizontal, out vertical, (findedCell, h, v) => findedCell.GetLine(type).Cards.Contains(card));

            if (horizontal == -1 && vertical == -1)
            {
                horizontal = 1;
                vertical = card.GetPlayer().Type == PositionType.Bottom ? -1 : _grid.GetLength(0);
            }

            return card.GetPlayer().Hand.Cards.Contains(card) ? card.GetPlayer().Hand.Cell : cell;
        }

        public Cell GetUnitCell(UnitCard card) => GetUnitCell(card, out _, out _);

        public void GetCellCoordinates(Cell cell, out int horizontal, out int vertical)
        {
            GetCellWithCondition(out horizontal, out vertical, (findedCell, h, v) => findedCell == cell);
        }

        public Cell GetCellWithCondition(out int horizontal, out int vertical, Func<Cell, float, float, bool> condition)
        {
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                for (int y = 0; y < _grid.GetLength(1); y++)
                {
                    if (condition.Invoke(_grid[x, y], y, x))
                    {
                        horizontal = y;
                        vertical = x;
                        return _grid[x, y];
                    }
                }
            }

            horizontal = -1;
            vertical = -1;
            return default;
        }

        public Cell GetCellWithCondition(Func<Cell, float, float, bool> condition) => GetCellWithCondition(out int h, out int v, condition);

        public List<Cell> GetCells(Func<Cell, float, float, bool> condition)
        {
            List<Cell> cards = new(1);

            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                for (int y = 0; y < _grid.GetLength(1); y++)
                {
                    if (condition.Invoke(_grid[x, y], y, x))
                        cards.Add(_grid[x, y]);
                }
            }

            return cards;
        }

        #endregion

        #region Get something (units)

        public List<UnitCard> GetUnitsThatCanBeKilled(List<UnitCard> units, UnitCard attacking)
        {
            List<UnitCard> unitCards = units;

            for (int i = unitCards.Count - 1; i >= 0; i--)
            {
                if (!CardCanDie(unitCards[i], attacking))
                    unitCards.RemoveAt(i);
            }

            return units;
        }

        public bool CardCanDie(Card unitCard, Card attacking)
        {
            float health = unitCard.GetUnit().GetStatistics(StatisticsType.Health).Value;
            float armor = unitCard.GetUnit().GetStatistics(StatisticsType.Armor).Value;
            float damage = attacking.GetUnit().GetStatistics(StatisticsType.Damage).Value;

            float decrease = Services.GetService<Formulas>().DamageReducedByArmor(damage, armor);

            return health - decrease <= 0;
        }

        public Card GetUnitWithBestStatistics(List<UnitCard> units, StatisticsType type) => GetUnitWithBestCondition(units, true, (unit) => unit.GetUnit().GetStatistics(type).Value, out float value);

        public Card GetUnitWithBestImportance(List<UnitCard> units, out float importance) => GetUnitWithBestCondition(units, true, (unit) => Formulas.GetUnitImportance(unit.GetUnit()), out importance);

        public Card GetUnitWithBestAttackImportance(List<UnitCard> units, out float importance) => GetUnitWithBestCondition(units, true, (unit) => Formulas.GetUnitAttackImportance(unit.GetUnit()), out importance);

        public float GetUnitAttackImportance(BoardPlayer player, UnitCard unitCard)
            => Formulas.GetUnitAttackImportance(GetUnitWithBestAttackImportance(GetUnitCell(unitCard).GetLine(player.Type).Cards, out _).GetUnit());

        public float GetSumOfUnitsCondition(List<UnitCard> units, Func<UnitCard, float> condition)
        {
            float value = 0;
            for (int i = 0; i < units.Count; i++)
                value += condition.Invoke(units[i]);

            return value;
        }

        public UnitCard GetUnitWithBestCondition(List<UnitCard> units, bool moreIsBetter, Func<UnitCard, float> condition, out float value)
        {
            UnitCard unitCard = default;
            float maxValue = moreIsBetter ? float.MinValue : float.MaxValue;

            for (int i = 0; i < units.Count; i++)
            {
                float currentValue = condition.Invoke(units[i]);

                if (moreIsBetter)
                {
                    if (currentValue > maxValue)
                    {
                        maxValue = currentValue;
                        unitCard = units[i];
                    }
                }
                else
                {
                    if (currentValue < maxValue)
                    {
                        maxValue = currentValue;
                        unitCard = units[i];
                    }
                }
            }

            value = maxValue;
            return unitCard;
        }

        public UnitCard GetUnitWithBestCondition(List<UnitCard> units, bool moreIsBetter, Func<UnitCard, float> condition) => GetUnitWithBestCondition(units, moreIsBetter, condition, out _);

        public List<UnitCard> GetUnitsWithBestCondition(List<UnitCard> units, bool moreIsBetter, Func<UnitCard, float> condition)
        {
            List<UnitCard> unitCards = new(1);
            float maxValue = moreIsBetter ? float.MinValue : float.MaxValue;

            for (int i = 0; i < units.Count; i++)
            {
                float currentValue = condition.Invoke(units[i]);

                if (currentValue == maxValue)
                {
                    unitCards.Add(units[i]);
                }
                else if (moreIsBetter)
                {
                    if (currentValue > maxValue)
                    {
                        maxValue = currentValue;
                        unitCards.Clear();
                        unitCards.Add(units[i]);
                    }
                }
                else
                {
                    if (currentValue < maxValue)
                    {
                        maxValue = currentValue;
                        unitCards.Clear();
                        unitCards.Add(units[i]);
                    }
                }
            }

            return unitCards;
        }

        public UnitCard GetUnitWithCondition(List<UnitCard> units, Func<UnitCard, bool> condition)
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (condition.Invoke(units[i]))
                    return units[i];
            }

            return default;
        }

        public List<UnitCard> GetUnitsWithCondition(List<UnitCard> units, Func<UnitCard, bool> condition)
        {
            List<UnitCard> unitCards = new(1);

            for (int i = 0; i < units.Count; i++)
            {
                if (condition.Invoke(units[i]))
                    unitCards.Add(units[i]);
            }

            return unitCards;
        }

        #endregion

        #region Checks

        public bool PlayerLost(BoardPlayer player) => player.Cards.Count == 0 || player.Towers.Count != _board.StandardTowersCount || (player.BattleData.HaveMainUnit && !player.BattleData.Units.Contains(player.BattleData.MainUnit));

        public bool PlayerCanMove(BoardPlayer player) => player.Type == _board.TurnPosition && _board.GameState == BoardState.InProcess;

        public bool UnitCanAttack(UnitCard unit, BoardPlayer player) => player.TurnPoints >= unit.TurnCost && !player.Hand.Cards.Contains(unit) && GetUnitCell(unit).GetLine(player.Type == PositionType.Bottom ? PositionType.Top : PositionType.Bottom).Cards.Count > 0;

        public bool UnitCanAttackTower(UnitCard unit, BoardPlayer player) => player.TurnPoints >= unit.TurnCost && GetUnitCell(unit, out _, out int vertical) && vertical == (player.Type == _board.PlayerPositionType ? _board.EnemyLine : _board.PlayerLine);

        public bool UnitCanMove(UnitCard unit, BoardPlayer player) => player.TurnPoints >= unit.TurnCost && (GetUnitCell(unit).GetLine(player.Type == PositionType.Bottom ? PositionType.Top : PositionType.Bottom).Cards.Count == 0 || player.Hand.Cards.Contains(unit));

        public bool LineCanAcceptCard(Line line, UnitCard card)
            => !line.Cards.Contains(card) && line.Cards.Count < _board.CardsLimitInLine;

        public bool CellHaveEnemies(BoardPlayer invoker, Cell cell)
            => cell.GetLine(invoker.Type == PositionType.Top ? PositionType.Bottom : PositionType.Top).Cards.Count != 0;

        public bool HasWrongPosition(BoardPlayer invoker, int startHorizontal, int startVertical, int endHorizontal, int endVertical)
            => startVertical == endVertical || startVertical != endVertical + (invoker.Type == PositionType.Top ? _board.RequiredVerticalOffset : -_board.RequiredVerticalOffset) || Mathf.Abs(endHorizontal - startHorizontal) > _board.MaximumHorizontalOffset;

        public bool HasWrongPosition(BoardPlayer invoker, Cell startCell, Cell endCell)
        {
            GetCellCoordinates(startCell, out int startHorizontal, out int startVertical);
            GetCellCoordinates(endCell, out int endHorizontal, out int endVertical);
            return HasWrongPosition(invoker, startHorizontal, startVertical, endHorizontal, endVertical);
        }

        public bool CardTypesEquals(Card card1, Card card2)
            => card1.GetPlayer().Type == card2.GetPlayer().Type;

        #endregion
    }
}
