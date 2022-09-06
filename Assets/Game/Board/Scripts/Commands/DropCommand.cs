using UnityEngine;
using DG.Tweening;

namespace LostThrone.Board
{
    public class DropCommand : CardCommand
    {
        public DropCommand(Board board, Player player, UnitCard unitCard) : base(board, player, unitCard)
        {

        }

        public override void Execute()
        {
            bool result = true;

            _card.transform.position = new Vector3(_card.transform.position.x, _card.transform.position.y, 0);
            _card.transform.DOScale(1.0f, 0.1f);

            _executed = result;
        }
    }
}
