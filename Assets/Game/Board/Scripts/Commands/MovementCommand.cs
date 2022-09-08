using System;
using DG.Tweening;

namespace LostThrone.Board
{
    public class MovementCommand : CardCommand
    {
        protected Cell _endCell;

        public MovementCommand(Board board, BoardPlayer player, Card card, Cell endCell, Action onCommandEnded = null) : base(board, player, card, onCommandEnded)
        {
            _endCell = endCell;
        }

        public override void Execute()
        {
            bool result = true;
            UnitCard unitCard = _card as UnitCard;

            if (unitCard.TurnCost > _card.Player.TurnPoints || _player.State != PlayerState.Attack || _player.TurnPoints <= 0)
                result = false;

            Line endLine = _endCell.GetLine(_player.Type);

            if (!_board.Base.LineCanAcceptCard(_board, endLine, unitCard))
                result = false;

            Cell startCell = _board.Base.GetUnitCell(_board, unitCard, out int startHorizontal, out int startVertical);
            _board.Base.GetCellCoordinates(_board, _endCell, out int endHorizontal, out int endVertical);

            if (startCell && _board.Base.CellHaveEnemies(_player, startCell))
                result = false;

            if (_board.Base.HasWrongPosition(_board, _player, startHorizontal, startVertical, endHorizontal, endVertical))
                result = false;

            if (result)
            {
                _player.RemoveTurnPoints(unitCard.TurnCost);
                unitCard.DoubleCost();

                if (!startCell)
                {
                    _player.Hand.RemoveCard(unitCard);
                }
                else
                {
                    startCell.GetLine(_player.Type).RemoveCard(unitCard);
                    _board.Base.RefreshLinePositions(startCell.GetLine(_player.Type));
                }

                _endCell.GetLine(_player.Type).AddCard(unitCard);
                _board.Base.RefreshLinePositions(_endCell.GetLine(_player.Type));

                unitCard.transform.SetParent(endLine.Parent);

                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(_board.RefreshLinesTime);
                sequence.AppendCallback(() => EndExecute(startCell));
            }
            else
            {
                if (startCell)
                    _board.Base.RefreshLinePositions(startCell.GetLine(_player.Type));
            }

            if (startCell)
                _board.Base.RefreshLinePositions(startCell.GetLine(_player.Type));
            else
                _board.Base.RefreshLinePositions(_player.Hand);

            _board.Base.RefreshLinePositions(_endCell.GetLine(_player.Type));

            _executed = result;
        }

        private void EndExecute(Cell startCell)
        {
            _board.Base.RefreshLinePositions(startCell.GetLine(_player.Type));
            _board.Base.RefreshLinePositions(_endCell.GetLine(_player.Type));
            _onCommandExecuted?.Invoke();
        }
    }
}
