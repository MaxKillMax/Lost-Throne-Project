using System;
using UnityEngine;
using DG.Tweening;

namespace LostThrone.Board
{
    public class PickupCommand : CardCommand
    {
        public PickupCommand(Board board, BoardPlayer player, UnitCard unitCard, Action onCommandEnded = null) : base(board, player, unitCard, onCommandEnded) { }

        public override void Execute()
        {
            _card.transform.position = new Vector3(_card.transform.position.x, _card.transform.position.y, -1);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_card.transform.DOScale(1.5f, 0.2f));
            sequence.AppendCallback(() => _onCommandExecuted?.Invoke());

            _executed = true;
        }
    }
}
