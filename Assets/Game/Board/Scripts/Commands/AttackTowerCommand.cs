using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace LostThrone.Board
{
    public class AttackTowerCommand : CardCommand
    {
        private readonly BoardPlayer _enemy;
        private readonly TowerCard _enemyCard;
        private UnitCard _unitCard;
        private Cell _cardCell;

        public AttackTowerCommand(Board board, BoardPlayer player, Card card, BoardPlayer enemy, TowerCard enemyCard, Action onCommandEnded = null) : base(board, player, card, onCommandEnded)
        {
            _enemy = enemy;
            _enemyCard = enemyCard;
            _unitCard = Card as UnitCard;
        }

        protected override bool CanExecute()
        {
            bool result = true;

            if (!base.CanExecute())
                result = false;

            if (!Board.Base.UnitCanAttackTower(_unitCard, Player))
                result = false;

            if (Board.Base.CardTypesEquals(_unitCard, _enemyCard))
                result = false;

            _cardCell = Board.Base.GetUnitCell(_unitCard);
            Cell enemyCell = Board.Base.GetTowerCell(_enemy, _enemyCard);

            if (_cardCell != enemyCell)
                result = false;

            return result;
        }

        protected override void StartCommand()
        {
            _enemyCard.GetDamage(_unitCard.GetUnit().GetStatistics(StatisticsType.Damage).Value);

            Player.RemoveTurnPoints(_unitCard.TurnCost);
            _unitCard.DoubleCost();

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_unitCard.transform.DOMove(new Vector3(_enemyCard.transform.position.x, _enemyCard.transform.position.y, _unitCard.transform.position.z), 0.2f));
            sequence.AppendCallback(() => EndCommand());
            sequence.Play();
        }
    }
}
