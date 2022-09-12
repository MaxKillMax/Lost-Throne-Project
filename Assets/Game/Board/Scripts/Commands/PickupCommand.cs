using System;
using DG.Tweening;
using UnityEngine;

namespace LostThrone.Board
{
    public class PickupCommand : CardCommand
    {
        public PickupCommand(Board board, BoardPlayer player, UnitCard unitCard, Action onCommandEnded = null) : base(board, player, unitCard, onCommandEnded) { }

        public override void Execute()
        {
            Card.transform.position = new Vector3(Card.transform.position.x, Card.transform.position.y, -1);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(Card.transform.DOScale(1.5f, 0.2f));
            sequence.AppendCallback(() => OnCommandExecuted?.Invoke());

            Executed = true;
        }
    }
}
