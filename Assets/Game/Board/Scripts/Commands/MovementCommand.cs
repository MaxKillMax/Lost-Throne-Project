using System;
using DG.Tweening;

namespace Game.Board
{
    public class MovementCommand : CardCommand
    {
        protected event Action _onAnimationEnded;
        protected Cell _endCell;

        public MovementCommand(Board board, Player player, Card card, Cell endCell, Action onAnimationEnded = null) : base(board, player, card)
        {
            _endCell = endCell;
            _onAnimationEnded = onAnimationEnded;
        }

        public override void Execute()
        {
            bool result = true;
            UnitCard unitCard = _card as UnitCard;

            if (unitCard.TurnCost > _card.Player.TurnPoints || _player.State != PlayerState.Attack || _player.TurnPoints <= 0)
                result = false;

            Line endLine = _endCell.GetLine(_player.Type);

            if (!_board.LineCanAcceptCard(endLine, unitCard))
                result = false;

            Cell startCell = _board.FindCellOfCard(unitCard, out int startHorizontal, out int startVertical);
            _board.FindCellCoordinates(_endCell, out int endHorizontal, out int endVertical);

            if (startCell && _board.CellHaveEnemies(_player, startCell))
                result = false;

            if (_board.HasWrongPosition(_player, startHorizontal, startVertical, endHorizontal, endVertical))
                result = false;

            if (result)
            {
                if (!startCell)
                    _player.Hand.RemoveCard(unitCard);
                else
                    startCell.GetLine(_player.Type).RemoveCard(unitCard);

                _endCell.GetLine(_player.Type).AddCard(unitCard);
                _player.UseCard(unitCard);

                unitCard.transform.SetParent(endLine.Parent);
            }

            if (startCell)
                _board.RefreshLinePositions(startCell.GetLine(_player.Type));
            else
                _board.RefreshLinePositions(_player.Hand);
            _board.RefreshLinePositions(_endCell.GetLine(_player.Type));

            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(_board.RefreshLinesTime);
            sequence.AppendCallback(() => { _onAnimationEnded?.Invoke(); });
            _executed = result;
        }
    }
}
