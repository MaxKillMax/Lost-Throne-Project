using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace LostThrone.Board
{
    public class AttackUnitCommand : UnitCommand
    {
        private UnitCard _enemy;
        private Cell _cardCell;

        public AttackUnitCommand(Board board, BoardPlayer player, UnitCard unitCard, UnitCard enemy, Action onCommandEnded = null) : base(board, player, unitCard, onCommandEnded)
        {
            _enemy = enemy;
        }

        protected override bool CanExecute()
        {
            bool result = true;

            if (!base.CanExecute())
                result = false;

            if (!Board.Base.UnitCanAttack(UnitCard, Player))
                result = false;

            if (Board.Base.CardTypesEquals(UnitCard, _enemy))
                result = false;

            _cardCell = Board.Base.GetUnitCell(UnitCard);
            Cell enemyCell = Board.Base.GetUnitCell(_enemy);

            if (_cardCell != enemyCell)
                result = false;

            return result;
        }

        protected override void StartCommand()
        {
            Player.RemoveTurnPoints(UnitCard.TurnCost);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(UnitCard.transform.DOMove(new Vector3(_enemy.transform.position.x, _enemy.transform.position.y, UnitCard.transform.position.z), 0.2f));
            sequence.AppendCallback(() => AttackEnemy());
            sequence.AppendCallback(() => EndCommand());
            sequence.Play();
        }

        private void AttackEnemy()
        {
            _enemy.GetDamage(UnitCard.GetUnit().GetStatistics(StatisticsType.Damage).Value);
            UnitCard.DoubleCost();
        }
    }
}
