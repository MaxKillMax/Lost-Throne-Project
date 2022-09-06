using System;
using System.Linq;
using System.Collections.Generic;
using LostThrone.Board;
using LostThrone.OpenWorld;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

namespace LostThrone
{
    public class BoardBase : Service
    {
        private Cell _currentCameraCell;
        public Cell CurrentCameraCell => _currentCameraCell;

        #region Do something

        public void RefreshLinePositions(Line line)
        {
            float middle = (line.Cards.Count - 1f) / 2;
            for (int i = 0; i < line.Cards.Count; i++)
                line.Cards[i].transform.DOLocalMove(new Vector3((i - middle), 0, 0), 0.2f);
        }

        public void SetCameraTarget(CinemachineVirtualCamera virtualCamera, Cell cell)
        {
            _currentCameraCell = cell;
            virtualCamera.Follow = _currentCameraCell.transform;
        }

        #endregion

        #region Get something (positions)

        public List<Cell> GetMovementCells(Board.Board board, Board.Player player, UnitCard unitCard)
        {
            List<Cell> cells = new List<Cell>(board.Cells);
            GetCardCell(board, unitCard, out int cardHorizontal, out int cardVertical);

            for (int i = cells.Count - 1; i >= 0; i--)
            {
                GetCellCoordinates(board, cells[i], out int cellHorizontal, out int cellVertical);
                if (HasWrongPosition(board, player, cardHorizontal, cardVertical, cellHorizontal, cellVertical))
                    cells.RemoveAt(i);
            }

            return cells;
        }

        public bool TryGetTower(Board.Board board, int horizontal, int vertical, out TowerCard tower)
        {
            tower = default;

            if (vertical == board.PlayerLine || vertical == board.EnemyLine)
            {
                tower = vertical == board.PlayerLine ? board.PlayerTowers[horizontal] : board.EnemyTowers[horizontal];
                if (tower.IsDestroyed)
                    return false;

                return true;
            }

            return false;
        }

        public Cell GetTowerCell(Board.Board board, Card card, out int horizontal, out int vertical)
        {
            TowerCard[] array;
            if (board.PlayerTowers.Contains(card))
            {
                array = board.PlayerTowers;
                horizontal = board.PlayerLine;
            }
            else if (board.EnemyTowers.Contains(card))
            {
                array = board.EnemyTowers;
                horizontal = board.EnemyLine;
            }
            else
            {
                horizontal = default;
                vertical = default;
                return default;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == card)
                {
                    vertical = i;
                    return board.Grid[horizontal, vertical];
                }
            }

            vertical = default;
            return default;
        }

        public Cell GetCardCell(Board.Board board, Card card, out int horizontal, out int vertical)
        {
            if (board.PlayerTowers.Contains(card) || board.EnemyTowers.Contains(card))
                return GetTowerCell(board, card, out horizontal, out vertical);

            PositionType type = card.Player.Type;
            Cell cell = GetGridCell(board, out horizontal, out vertical, (findedCell, h, v) => findedCell.GetLine(type).Cards.Contains(card));

            if (cell == default)
            {
                horizontal = 1;
                vertical = card.Player.Type == PositionType.Bottom ? -1 : 3;
                return null;
            }
            else
            {
                return cell;
            }
        }

        public void GetCellCoordinates(Board.Board board, Cell cell, out int horizontal, out int vertical)
        {
            GetGridCell(board, out horizontal, out vertical, (findedCell, h, v) => findedCell == cell);
        }

        public Cell GetGridCell(Board.Board board, out int horizontal, out int vertical, Func<Cell, float, float, bool> conditionOfFinding)
        {
            for (int x = 0; x < board.Grid.GetLength(0); x++)
            {
                for (int y = 0; y < board.Grid.GetLength(1); y++)
                {
                    if (conditionOfFinding.Invoke(board.Grid[x, y], y, x))
                    {
                        horizontal = y;
                        vertical = x;
                        return board.Grid[x, y];
                    }
                }
            }

            horizontal = -1;
            vertical = -1;
            return default;
        }

        #endregion

        #region Get something (units)

        public List<UnitCard> GetUnitsThatCanBeKilled(List<UnitCard> units, UnitCard attacking)
        {
            List<UnitCard> unitCards = units;

            for (int i = unitCards.Count - 1; i >= 0; i--)
                if (!UnitCanDie(unitCards[i], attacking))
                    unitCards.RemoveAt(i);

            return units;
        }

        public bool UnitCanDie(Card unitCard, Card attacking)
        {
            Unit unit = unitCard.Unit;

            float health = unit.GetStatistics(StatisticsType.Health).Value;
            float armor = unit.GetStatistics(StatisticsType.Armor).Value;
            float damage = unit.GetStatistics(StatisticsType.Damage).Value;

            float decrease = Services.GetService<Formulas>().DamageReducedByArmor(damage, armor);

            return health - decrease <= 0;
        }

        public Card GetUnitWithBestStatistics(List<UnitCard> units, StatisticsType type) => GetUnitWithBestCondition(units, true, (unit) => unit.Unit.GetStatistics(type).Value, out float value);

        public Card GetUnitWithBestImportance(List<UnitCard> units, out float importance) => GetUnitWithBestCondition(units, true, (unit) => Services.GetService<Formulas>().GetUnitImportance(unit.Unit), out importance);

        public Card GetUnitWithBestAttackImportance(List<UnitCard> units, out float importance) => GetUnitWithBestCondition(units, true, (unit) => Services.GetService<Formulas>().GetUnitAttackImportance(unit.Unit), out importance);

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

        #endregion

        #region Checks

        public bool PlayerCanMove(Board.Board board, Board.Player player) => player.TurnPoints > 0 && player.Type == board.CurrentTurn;

        public bool UnitCanMove(UnitCard unit, Board.Player player) => player.TurnPoints >= unit.TurnCost && (player.Type == PositionType.Bottom || unit.TurnCost < 4);

        public bool LineCanAcceptCard(Board.Board board, Line line, UnitCard card)
            => !line.Cards.Contains(card) && line.Cards.Count < board.CardsLimitInLine;

        public bool CellHaveEnemies(Board.Player invoker, Cell cell)
            => cell.GetLine(invoker.Type == PositionType.Top ? PositionType.Bottom : PositionType.Top).Cards.Count != 0;

        public bool HasWrongPosition(Board.Board board, Board.Player invoker, int startHorizontal, int startVertical, int endHorizontal, int endVertical)
            => startVertical == endVertical || startVertical != endVertical + (invoker.Type == PositionType.Top ? board.RequiredVerticalOffset : -board.RequiredVerticalOffset) || Mathf.Abs(endHorizontal - startHorizontal) > board.MaximumHorizontalOffset;

        public bool HasWrongPosition(Board.Board board, Board.Player invoker, Cell startCell, Cell endCell)
        {
            GetCellCoordinates(board, startCell, out int startHorizontal, out int startVertical);
            GetCellCoordinates(board, endCell, out int endHorizontal, out int endVertical);
            return HasWrongPosition(board, invoker, startHorizontal, startVertical, endHorizontal, endVertical);
        }

        public bool CardTypesEquals(Card card1, Card card2)
            => card1.Player.Type == card2.Player.Type;

        #endregion
    }
}
