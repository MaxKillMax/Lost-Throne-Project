using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace LostThrone.Board
{
    public class AttackTowerCommand : UnitCommand
    {
        private readonly BoardPlayer _enemy;
        private readonly TowerCard _enemyCard;
        private Cell _cardCell;

        public AttackTowerCommand(Board board, BoardPlayer player, UnitCard unitCard, BoardPlayer enemy, TowerCard enemyCard, Action onCommandEnded = null) : base(board, player, unitCard, onCommandEnded)
        {
            _enemy = enemy;
            _enemyCard = enemyCard;
        }

        protected override bool CanExecute()
        {
            bool result = true;

            if (!base.CanExecute())
                result = false;
            
            if (!Board.Base.UnitCanAttackTower(UnitCard, Player))
                result = false;

            if (Board.Base.CardTypesEquals(UnitCard, _enemyCard))
                result = false;

            _cardCell = Board.Base.GetUnitCell(UnitCard);
            Cell enemyCell = Board.Base.GetTowerCell(_enemy, _enemyCard);

            if (_cardCell != enemyCell)
                result = false;

            return result;
        }

        protected override void StartCommand()
        {
            Player.RemoveTurnPoints(UnitCard.TurnCost);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(UnitCard.transform.DOMove(new Vector3(_enemyCard.transform.position.x, _enemyCard.transform.position.y, UnitCard.transform.position.z), 0.2f));
            sequence.AppendCallback(() => AttackEnemy());
            sequence.AppendCallback(() => EndCommand());
            sequence.Play();
        }

        private void AttackEnemy()
        {
            _enemyCard.GetDamage(UnitCard.GetUnit().GetStatistics(StatisticsType.Damage).Value);
            UnitCard.DoubleCost();
        }
    }
}
