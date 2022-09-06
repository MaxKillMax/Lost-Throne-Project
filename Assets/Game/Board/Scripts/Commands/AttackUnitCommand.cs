using System;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace LostThrone.Board
{
    public class AttackUnitCommand : CardCommand
    {
        protected event Action _onAnimationEnded;
        protected UnitCard _enemy;

        public AttackUnitCommand(Board board, Player player, Card card, UnitCard enemy, Action onAnimationEnded = null) : base(board, player, card)
        {
            _enemy = enemy;
            _onAnimationEnded = onAnimationEnded;
        }

        public override void Execute()
        {
            bool result = true;
            UnitCard unitCard = _card as UnitCard;

            if (unitCard.TurnCost > _card.Player.TurnPoints || _player.State != PlayerState.Attack || _player.TurnPoints <= 0)
                result = false;

            if (Services.GetService<BoardBase>().CardTypesEquals(unitCard, _enemy))
                result = false;

            Cell cardCell = Services.GetService<BoardBase>().GetCardCell(_board, unitCard, out int horizontal, out int vertical);
            Cell enemyCell = Services.GetService<BoardBase>().GetCardCell(_board, _enemy, out int enemyHorizontal, out int enemyVertical);

            if (cardCell != enemyCell)
                result = false;

            if (result)
            {
                _enemy.GetDamage(unitCard.Unit.GetStatistics(StatisticsType.Damage).Value);
                _player.UseCard(unitCard);

                _enemy.transform.position = new Vector3(_enemy.transform.position.x, _enemy.transform.position.y, -1);
                Sequence sequence = DOTween.Sequence();
                sequence.Append(unitCard.transform.DOMove(new Vector3(_enemy.transform.position.x, _enemy.transform.position.y, unitCard.transform.position.z), 0.2f));
                sequence.AppendCallback(() => { if (_enemy) _enemy.transform.DOShakeScale(0.2f); RefreshLine(cardCell); _onAnimationEnded?.Invoke(); });
                sequence.Play();
            }
            else
            {
                RefreshLine(cardCell);
            }

            _executed = result;
        }

        private void RefreshLine(Cell cardCell)
        {
            if (cardCell)
                Services.GetService<BoardBase>().RefreshLinePositions(cardCell.GetLine(_player.Type));
            else
                Services.GetService<BoardBase>().RefreshLinePositions(_player.Hand);
        }
    }
}
