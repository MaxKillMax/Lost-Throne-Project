using System;
using UnityEngine;
using DG.Tweening;

namespace Game.Board
{
    public class AttackCommand : CardCommand
    {
        protected event Action _onAnimationEnded;
        protected Card _enemy;

        public AttackCommand(Board board, Player player, Card card, Card enemy, Action onAnimationEnded = null) : base(board, player, card)
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

            if (_board.CardTypesEquals(unitCard, _enemy))
                result = false;

            Cell cardCell = _board.FindCellOfCard(unitCard, out int horizontal, out int vertical);
            Cell enemyCell = _board.FindCellOfCard(_enemy, out int enemyHorizontal, out int enemyVertical);

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
                _board.RefreshLinePositions(cardCell.GetLine(_player.Type));
            else
                _board.RefreshLinePositions(_player.Hand);
        }
    }
}
