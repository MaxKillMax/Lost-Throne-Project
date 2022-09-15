using System;
using DG.Tweening;
using UnityEngine;

namespace LostThrone.Board
{
    public class DropCommand : UnitCommand
    {
        private Cell _unitCell;

        public DropCommand(Board board, BoardPlayer player, UnitCard unitCard, Action onCommandEnded = null) : base(board, player, unitCard, onCommandEnded) { }

        protected override void StartCommand()
        {
            _unitCell = Board.Base.GetUnitCell(UnitCard);

            UnitCard.transform.position = new Vector3(UnitCard.transform.position.x, UnitCard.transform.position.y, 0);

            Sequence sequence = DOTween.Sequence();
            sequence.AppendCallback(() => PutCard());
            sequence.AppendInterval(Board.RefreshLinesTime);
            sequence.AppendCallback(() => EndCommand());
        }

        private void PutCard()
        {
            UnitCard.transform.DOScale(1.0f, Board.RefreshLinesTime);
            Board.Base.RefreshLinePositions(_unitCell.GetLine(Player.Type));
        }
    }
}
