using System;
using DG.Tweening;

namespace LostThrone.Board
{
    public class MovementCommand : CardCommand
    {
        protected event Action _onAnimationEnded;
        protected Cell _endCell;

        public MovementCommand(Board board, BoardPlayer player, Card card, Cell endCell, Action onAnimationEnded = null) : base(board, player, card)
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

            if (!Services.GetService<BoardBase>().LineCanAcceptCard(_board, endLine, unitCard))
                result = false;

            Cell startCell = Services.GetService<BoardBase>().GetUnitCell(_board, unitCard, out int startHorizontal, out int startVertical);
            Services.GetService<BoardBase>().GetCellCoordinates(_board, _endCell, out int endHorizontal, out int endVertical);

            if (startCell && Services.GetService<BoardBase>().CellHaveEnemies(_player, startCell))
                result = false;

            if (Services.GetService<BoardBase>().HasWrongPosition(_board, _player, startHorizontal, startVertical, endHorizontal, endVertical))
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
                Services.GetService<BoardBase>().RefreshLinePositions(startCell.GetLine(_player.Type));
            else
                Services.GetService<BoardBase>().RefreshLinePositions(_player.Hand);
            Services.GetService<BoardBase>().RefreshLinePositions(_endCell.GetLine(_player.Type));

            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(_board.RefreshLinesTime);
            sequence.AppendCallback(() => { _onAnimationEnded?.Invoke(); });
            _executed = result;
        }
    }
}
