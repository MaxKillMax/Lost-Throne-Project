using System;
using DG.Tweening;

namespace LostThrone.Board
{
    public class MovementCommand : CardCommand
    {
        private readonly Cell _endCell;

        public MovementCommand(Board board, BoardPlayer player, Card card, Cell endCell, Action onCommandEnded = null) : base(board, player, card, onCommandEnded)
        {
            _endCell = endCell;
        }

        public override void Execute()
        {
            bool result = true;
            UnitCard unitCard = Card as UnitCard;

            if (unitCard.TurnCost > Card.GetPlayer().TurnPoints || Player.State != PlayerState.Attack || Player.TurnPoints <= 0)
                result = false;

            Line endLine = _endCell.GetLine(Player.Type);

            if (!Board.Base.LineCanAcceptCard(endLine, unitCard))
                result = false;

            Cell startCell = Board.Base.GetUnitCell(unitCard, out int startHorizontal, out int startVertical);
            Board.Base.GetCellCoordinates(_endCell, out int endHorizontal, out int endVertical);

            if (startCell && Board.Base.CellHaveEnemies(Player, startCell))
                result = false;

            if (Board.Base.HasWrongPosition(Player, startHorizontal, startVertical, endHorizontal, endVertical))
                result = false;

            if (result)
            {
                Player.RemoveTurnPoints(unitCard.TurnCost);
                unitCard.DoubleCost();

                if (!startCell)
                {
                    Player.Hand.RemoveCard(unitCard);
                }
                else
                {
                    startCell.GetLine(Player.Type).RemoveCard(unitCard);
                    Board.Base.RefreshLinePositions(startCell.GetLine(Player.Type));
                }

                _endCell.GetLine(Player.Type).AddCard(unitCard);
                Board.Base.RefreshLinePositions(_endCell.GetLine(Player.Type));

                unitCard.transform.SetParent(endLine.Parent);

                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(Board.RefreshLinesTime);
                sequence.AppendCallback(() => EndExecute(startCell));
            }
            else
            {
                if (startCell)
                    Board.Base.RefreshLinePositions(startCell.GetLine(Player.Type));
            }

            if (startCell)
                Board.Base.RefreshLinePositions(startCell.GetLine(Player.Type));
            else
                Board.Base.RefreshLinePositions(Player.Hand);

            Board.Base.RefreshLinePositions(_endCell.GetLine(Player.Type));

            Executed = result;
        }

        private void EndExecute(Cell startCell)
        {
            Board.Base.RefreshLinePositions(startCell.GetLine(Player.Type));
            Board.Base.RefreshLinePositions(_endCell.GetLine(Player.Type));
            OnCommandExecuted?.Invoke();
        }
    }
}
