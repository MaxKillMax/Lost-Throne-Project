using System;
using DG.Tweening;
using UnityEngine;

namespace LostThrone.Board
{
    public class AttackUnitWithTowerCommand : TowerCommand
    {
        private UnitCard _enemy;

        public AttackUnitWithTowerCommand(Board board, BoardPlayer player, TowerCard towerCard, UnitCard enemy, Action onCommandEnded = null) : base(board, player, towerCard, onCommandEnded)
        {
            _enemy = enemy;
        }

        protected override bool CanExecute()
        {
            Board.Base.GetTowerCell(Player, TowerCard, out int horizontal, out int vertical);
            Board.Base.GetUnitCell(_enemy, out int enemyHorizontal, out int enemyVertical);
            return horizontal == enemyHorizontal && vertical == enemyVertical;
        }

        protected override void StartCommand()
        {
            TowerCard.transform.position = new Vector3(TowerCard.transform.position.x, TowerCard.transform.position.y, -1);

            Vector3 towerPosition = new (TowerCard.transform.position.x, TowerCard.transform.position.y, -1);
            Vector3 enemyPosition = new(_enemy.transform.position.x, _enemy.transform.position.y, -1);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(TowerCard.transform.DOScale(1.5f, Board.RefreshLinesTime));
            sequence.Append(TowerCard.transform.DOMove(enemyPosition, 0.2f));
            sequence.AppendCallback(() => AttackEnemy());
            sequence.Append(TowerCard.transform.DOMove(towerPosition, 0.2f));
            sequence.AppendCallback(() => TowerCard.transform.position = new Vector3(TowerCard.transform.position.x, TowerCard.transform.position.y, 0));
            sequence.AppendCallback(() => EndCommand());
        }

        private void AttackEnemy()
        {
            _enemy.GetDamage(TowerCard.GetUnit().GetStatistics(StatisticsType.Damage).Value);
        }
    }
}
