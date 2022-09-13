using System;
using DG.Tweening;
using UnityEngine;

namespace LostThrone.Board
{
    public class DropCommand : CardCommand
    {
        private Cell _unitCell;

        public DropCommand(Board board, BoardPlayer player, UnitCard unitCard, Action onCommandEnded = null) : base(board, player, unitCard, onCommandEnded) { }

        protected override void StartCommand()
        {
            _unitCell = Board.Base.GetUnitCell((UnitCard)Card);

            Card.transform.position = new Vector3(Card.transform.position.x, Card.transform.position.y, 0);

            Sequence sequence = DOTween.Sequence();
            sequence.AppendCallback(() => PutCard());
            sequence.AppendInterval(Board.RefreshLinesTime);
            sequence.AppendCallback(() => EndCommand());
        }

        private void PutCard()
        {
            Card.transform.DOScale(1.0f, Board.RefreshLinesTime);
            Board.Base.RefreshLinePositions(_unitCell.GetLine(Player.Type));
        }
    }
}
