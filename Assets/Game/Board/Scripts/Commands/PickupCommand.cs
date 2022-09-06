using System;
using UnityEngine;
using DG.Tweening;

namespace LostThrone.Board
{
    public class PickupCommand : CardCommand
    {
        protected event Action _onAnimationEnded;

        public PickupCommand(Board board, Player player, UnitCard unitCard, Action onAnimationEnded = null) : base(board, player, unitCard)
        {
            _onAnimationEnded = onAnimationEnded;
        }

        public override void Execute()
        {
            bool result = true;

            Sequence sequence = DOTween.Sequence();
            _card.transform.position = new Vector3(_card.transform.position.x, _card.transform.position.y, -1);
            sequence.Append(_card.transform.DOScale(1.5f, 0.2f));
            sequence.AppendCallback(() => _onAnimationEnded?.Invoke());

            _executed = result;
        }
    }
}
