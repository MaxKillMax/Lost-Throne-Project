using System;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace LostThrone.Board
{
    public class AttackUnitCommand : CardCommand
    {
        protected UnitCard _enemy;

        public AttackUnitCommand(Board board, BoardPlayer player, Card card, UnitCard enemy, Action onCommandEnded = null) : base(board, player, card, onCommandEnded)
        {
            _enemy = enemy;
        }

        public override void Execute()
        {
            bool result = true;
            UnitCard unitCard = _card as UnitCard;

            if (unitCard.TurnCost > _card.Player.TurnPoints || _player.State != PlayerState.Attack || _player.TurnPoints <= 0)
                result = false;

            if (_board.Base.CardTypesEquals(unitCard, _enemy))
                result = false;

            Cell cardCell = _board.Base.GetUnitCell(_board, unitCard, out int horizontal, out int vertical);
            Cell enemyCell = _board.Base.GetUnitCell(_board, _enemy, out int enemyHorizontal, out int enemyVertical);

            if (cardCell != enemyCell)
                result = false;

            if (result)
            {
                _enemy.GetDamage(unitCard.Unit.GetStatistics(StatisticsType.Damage).Value);

                _player.RemoveTurnPoints(unitCard.TurnCost);
                unitCard.DoubleCost();

                Sequence sequence = DOTween.Sequence();
                sequence.Append(unitCard.transform.DOMove(new Vector3(_enemy.transform.position.x, _enemy.transform.position.y, unitCard.transform.position.z), 0.2f));
                sequence.AppendCallback(() => EndExecute(cardCell));
                sequence.Play();
            }
            else
            {
                _board.Base.RefreshLinePositions(cardCell.GetLine(_player.Type));
            }

            _executed = result;
        }

        private void EndExecute(Cell cardCell)
        {
            _board.Base.RefreshLinePositions(cardCell.GetLine(_player.Type));
            _onCommandExecuted?.Invoke();
        }
    }
}
