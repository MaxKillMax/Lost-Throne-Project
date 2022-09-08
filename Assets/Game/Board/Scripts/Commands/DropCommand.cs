using System;
using UnityEngine;
using DG.Tweening;

namespace LostThrone.Board
{
    public class DropCommand : CardCommand
    {
        public DropCommand(Board board, BoardPlayer player, UnitCard unitCard, Action onCommandEnded = null) : base(board, player, unitCard, onCommandEnded) { }

        public override void Execute()
        {
            _card.transform.position = new Vector3(_card.transform.position.x, _card.transform.position.y, 0);

            _card.transform.DOScale(1.0f, 0.1f);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_card.transform.DOScale(1.0f, 0.1f));
            sequence.AppendCallback(() => _onCommandExecuted?.Invoke());

            _executed = true;
        }
    }
}
