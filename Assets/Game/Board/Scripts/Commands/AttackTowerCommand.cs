using System;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace LostThrone.Board
{
    public class AttackTowerCommand : CardCommand
    {
        protected BoardPlayer _enemy;
        protected TowerCard _enemyCard;

        public AttackTowerCommand(Board board, BoardPlayer player, Card card, BoardPlayer enemy, TowerCard enemyCard, Action onCommandEnded = null) : base(board, player, card, onCommandEnded)
        {
            _enemy = enemy;
            _enemyCard = enemyCard;
        }

        public override void Execute()
        {
            bool result = true;
            UnitCard unitCard = _card as UnitCard;

            if (unitCard.TurnCost > _card.Player.TurnPoints || _player.State != PlayerState.Attack || _player.TurnPoints <= 0)
                result = false;

            if (_board.Base.CardTypesEquals(unitCard, _enemyCard))
                result = false;

            Cell cardCell = _board.Base.GetUnitCell(_board, unitCard);
            Cell enemyCell = _board.Base.GetTowerCell(_board, _enemy, _enemyCard);

            if (cardCell != enemyCell)
                result = false;

            if (result)
            {
                _enemyCard.GetDamage(unitCard.Unit.GetStatistics(StatisticsType.Damage).Value);

                _player.RemoveTurnPoints(unitCard.TurnCost);
                unitCard.DoubleCost();

                Sequence sequence = DOTween.Sequence();
                sequence.Append(unitCard.transform.DOMove(new Vector3(_enemyCard.transform.position.x, _enemyCard.transform.position.y, unitCard.transform.position.z), 0.2f));
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
