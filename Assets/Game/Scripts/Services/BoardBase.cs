using System;
using System.Collections.Generic;
using LostThrone.Board;
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

        public void DestroyUnitCard(Board.Board board, UnitCard card)
        {
            Cell cell = board.Base.GetUnitCell(board, card);
            cell.GetLine(card.Player.Type).RemoveCard(card);
            board.Base.RefreshLinePositions(cell.GetLine(card.Player.Type));
            card.Unit.GetStatistics(StatisticsType.Health).SetValue(0);
            card.Player.RemoveCard(card);
            card.DestroyCard();

            board.EndBattle();
        }

        public void DestroyTowerCard(Board.Board board, TowerCard card)
        {
            card.Unit.GetStatistics(StatisticsType.Health).SetValue(0);
            card.Player.RemoveTower(card);
            card.DestroyCard();

            board.EndBattle();
        }

        public void RefreshLinePositions(Line line)
        {
            float middle = (line.Cards.Count - 1f) / 2;
            for (int i = 0; i < line.Cards.Count; i++)
                line.Cards[i].transform.DOLocalMove(new Vector3((i - middle), 0, 0), 0.2f);
        }

        public void MoveCamera(Board.Board board, Direction direction)
        {
            Cell[,] grid = board.Grid;

            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    if (grid[x, y] == _currentCameraCell)
                    {
                        switch (direction)
                        {
                            case Direction.Top:
                                if (x + 1 < grid.GetLength(0))
                                    SetCameraTarget(board.virtualCamera, grid[x + 1, y]);
                                break;
                            case Direction.Right:
                                if (y + 1 < grid.GetLength(1))
                                    SetCameraTarget(board.virtualCamera, grid[x, y + 1]);
                                break;
                            case Direction.Bottom:
                                if (x - 1 >= 0)
                                    SetCameraTarget(board.virtualCamera, grid[x - 1, y]);
                                break;
                            case Direction.Left:
                                if (y - 1 >= 0)
                                    SetCameraTarget(board.virtualCamera, grid[x, y - 1]);
                                break;
                        }

                        return;
                    }
                }
            }
        }

        public void SetCameraTarget(CinemachineVirtualCamera virtualCamera, Cell cell)
        {
            _currentCameraCell = cell;
            virtualCamera.Follow = _currentCameraCell.transform;
        }

        #endregion

        #region Get something (positions)

        public List<Cell> GetMovementCells(Board.Board board, BoardPlayer player, UnitCard unitCard)
        {
            List<Cell> cells = new List<Cell>(board.Cells);
            GetUnitCell(board, unitCard, out int cardHorizontal, out int cardVertical);

            for (int i = cells.Count - 1; i >= 0; i--)
            {
                GetCellCoordinates(board, cells[i], out int cellHorizontal, out int cellVertical);
                if (HasWrongPosition(board, player, cardHorizontal, cardVertical, cellHorizontal, cellVertical))
                    cells.RemoveAt(i);
            }

            return cells;
        }

        public Cell GetTowerCell(Board.Board board, BoardPlayer playerSide, Card card, out int horizontal, out int vertical)
        {
            vertical = playerSide.Type == board.PlayerPositionType ? board.PlayerLine : playerSide.Type == board.EnemyPositionType ? board.EnemyLine : -1;

            for (int i = 0; i < playerSide.Towers.Count; i++)
            {
                if (playerSide.Towers[i] == card)
                {
                    horizontal = i;
                    return board.Grid[vertical, horizontal];
                }
            }

            horizontal = default;
            return default;
        }

        public Cell GetTowerCell(Board.Board board, BoardPlayer playerSide, Card card) => GetTowerCell(board, playerSide, card, out int h, out int v);

        public Cell GetUnitCell(Board.Board board, UnitCard card, out int horizontal, out int vertical)
        {
            PositionType type = card.Player.Type;
            Cell cell = GetCellWithCondition(board, out horizontal, out vertical, (findedCell, h, v) => findedCell.GetLine(type).Cards.Contains(card));

            if (card.Player.Hand.Cards.Contains(card))
                return card.Player.Hand.Cell;

            return cell;
        }

        public Cell GetUnitCell(Board.Board board, UnitCard card) => GetUnitCell(board, card, out int h, out int v);

        public void GetCellCoordinates(Board.Board board, Cell cell, out int horizontal, out int vertical)
        {
            GetCellWithCondition(board, out horizontal, out vertical, (findedCell, h, v) => findedCell == cell);
        }

        public Cell GetCellWithCondition(Board.Board board, out int horizontal, out int vertical, Func<Cell, float, float, bool> condition)
        {
            for (int x = 0; x < board.Grid.GetLength(0); x++)
            {
                for (int y = 0; y < board.Grid.GetLength(1); y++)
                {
                    if (condition.Invoke(board.Grid[x, y], y, x))
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

        public Cell GetCellWithCondition(Board.Board board, Func<Cell, float, float, bool> condition) => GetCellWithCondition(board, out int h, out int v, condition);

        public List<Cell> GetCells(Board.Board board, Func<Cell, float, float, bool> condition)
        {
            List<Cell> cards = new List<Cell>(1);

            for (int x = 0; x < board.Grid.GetLength(0); x++)
            {
                for (int y = 0; y < board.Grid.GetLength(1); y++)
                {
                    if (condition.Invoke(board.Grid[x, y], y, x))
                        cards.Add(board.Grid[x, y]);
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
                if (!UnitCanDie(unitCards[i], attacking))
                    unitCards.RemoveAt(i);

            return units;
        }

        public bool UnitCanDie(Card unitCard, Card attacking)
        {
            float health = unitCard.Unit.GetStatistics(StatisticsType.Health).Value;
            float armor = unitCard.Unit.GetStatistics(StatisticsType.Armor).Value;
            float damage = attacking.Unit.GetStatistics(StatisticsType.Damage).Value;

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

        public UnitCard GetUnitWithBestCondition(List<UnitCard> units, bool moreIsBetter, Func<UnitCard, float> condition) => GetUnitWithBestCondition(units, moreIsBetter, condition, out float value);

        public List<UnitCard> GetUnitsWithBestCondition(List<UnitCard> units, bool moreIsBetter, Func<UnitCard, float> condition)
        {
            List<UnitCard> unitCards = new List<UnitCard>(1);
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
                if (condition.Invoke(units[i]))
                    return units[i];

            return default;
        }

        public List<UnitCard> GetUnitsWithCondition(List<UnitCard> units, Func<UnitCard, bool> condition)
        {
            List<UnitCard> unitCards = new List<UnitCard>(1);

            for (int i = 0; i < units.Count; i++)
                if (condition.Invoke(units[i]))
                    unitCards.Add(units[i]);

            return unitCards;
        }

        #endregion

        #region Checks

        public bool PlayerLost(Board.Board board, BoardPlayer player) => player.Cards.Count == 0 || player.Towers.Count != board.StandardTowersCount || (player.BattleData.HaveMainUnit && !player.BattleData.Units.Contains(player.BattleData.MainUnit));

        public bool PlayerCanMove(Board.Board board, BoardPlayer player) => player.Type == board.TurnPosition && board.GameState == BoardState.InProcess;

        public bool UnitCanAttack(Board.Board board, UnitCard unit, BoardPlayer player) => player.TurnPoints >= unit.TurnCost && GetUnitCell(board, unit).GetLine(player.Type == PositionType.Bottom ? PositionType.Top : PositionType.Bottom).Cards.Count > 0;

        public bool UnitCanMove(Board.Board board, UnitCard unit, BoardPlayer player) => player.TurnPoints >= unit.TurnCost && GetUnitCell(board, unit).GetLine(player.Type == PositionType.Bottom ? PositionType.Top : PositionType.Bottom).Cards.Count == 0;

        public bool LineCanAcceptCard(Board.Board board, Line line, UnitCard card)
            => !line.Cards.Contains(card) && line.Cards.Count < board.CardsLimitInLine;

        public bool CellHaveEnemies(BoardPlayer invoker, Cell cell)
            => cell.GetLine(invoker.Type == PositionType.Top ? PositionType.Bottom : PositionType.Top).Cards.Count != 0;

        public bool HasWrongPosition(Board.Board board, BoardPlayer invoker, int startHorizontal, int startVertical, int endHorizontal, int endVertical)
            => startVertical == endVertical || startVertical != endVertical + (invoker.Type == PositionType.Top ? board.RequiredVerticalOffset : -board.RequiredVerticalOffset) || Mathf.Abs(endHorizontal - startHorizontal) > board.MaximumHorizontalOffset;

        public bool HasWrongPosition(Board.Board board, BoardPlayer invoker, Cell startCell, Cell endCell)
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
