using System;
using DG.Tweening;
using UnityEngine;

namespace LostThrone.Board
{
    public class DropCommand : CardCommand
    {
        public DropCommand(Board board, BoardPlayer player, UnitCard unitCard, Action onCommandEnded = null) : base(board, player, unitCard, onCommandEnded) { }

        public override void Execute()
        {
            Card.transform.position = new Vector3(Card.transform.position.x, Card.transform.position.y, 0);

            Card.transform.DOScale(1.0f, 0.1f);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(Card.transform.DOScale(1.0f, 0.1f));
            sequence.AppendCallback(() => OnCommandExecuted?.Invoke());

            Executed = true;
        }
    }
}
