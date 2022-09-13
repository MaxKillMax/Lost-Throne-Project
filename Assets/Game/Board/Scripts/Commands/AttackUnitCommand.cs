using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace LostThrone.Board
{
    public class AttackUnitCommand : CardCommand
    {
        private UnitCard _enemy;
        private UnitCard _unitCard;
        private Cell _cardCell;

        public AttackUnitCommand(Board board, BoardPlayer player, Card card, UnitCard enemy, Action onCommandEnded = null) : base(board, player, card, onCommandEnded)
        {
            _enemy = enemy;
            _unitCard = Card as UnitCard;
        }

        protected override bool CanExecute()
        {
            bool result = true;

            if (!base.CanExecute())
                result = false;

            if (!Board.Base.UnitCanAttack(_unitCard, Player))
                result = false;

            if (Board.Base.CardTypesEquals(_unitCard, _enemy))
                result = false;

            _cardCell = Board.Base.GetUnitCell(_unitCard);
            Cell enemyCell = Board.Base.GetUnitCell(_enemy);

            if (_cardCell != enemyCell)
                result = false;

            return result;
        }

        protected override void StartCommand()
        {
            _enemy.GetDamage(_unitCard.GetUnit().GetStatistics(StatisticsType.Damage).Value);

            Player.RemoveTurnPoints(_unitCard.TurnCost);
            _unitCard.DoubleCost();

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_unitCard.transform.DOMove(new Vector3(_enemy.transform.position.x, _enemy.transform.position.y, _unitCard.transform.position.z), 0.2f));
            sequence.AppendCallback(() => EndCommand());
            sequence.Play();
        }
    }
}
