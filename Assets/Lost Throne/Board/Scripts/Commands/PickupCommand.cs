using System;
using DG.Tweening;
using UnityEngine;

namespace LostThrone.Board
{
    public class PickupCommand : UnitCommand
    {
        public PickupCommand(Board board, BoardPlayer player, UnitCard unitCard, Action onCommandEnded = null) : base(board, player, unitCard, onCommandEnded) { }

        protected override void StartCommand()
        {
            UnitCard.transform.position = new Vector3(UnitCard.transform.position.x, UnitCard.transform.position.y, -1);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(UnitCard.transform.DOScale(1.5f, Board.RefreshLinesTime));
            sequence.AppendCallback(() => EndCommand());
        }
    }
}
