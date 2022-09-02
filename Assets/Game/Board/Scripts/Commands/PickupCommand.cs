using System;
using UnityEngine;
using DG.Tweening;

namespace Game.Board
{
    public class PickupCommand : CardCommand
    {
        public PickupCommand(Board board, Player player, UnitCard unitCard) : base(board, player, unitCard)
        {

        }

        public override void Execute()
        {
            bool result = true;

            _card.transform.position = new Vector3(_card.transform.position.x, _card.transform.position.y, -1);
            _card.transform.DOScale(1.5f, 0.2f);

            _executed = result;
        }
    }
}
