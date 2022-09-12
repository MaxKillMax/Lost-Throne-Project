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

        public AttackTowerCommand(Board board, BoardPlayer player, Card card, BoardPlayer enemy, TowerCard enemyCard, Action onCommandEnded = null) : base(board, player, card, onCommandEnded)
        {
            _enemy = enemy;
            _enemyCard = enemyCard;
        }

        public override void Execute()
        {
            bool result = true;
            UnitCard unitCard = Card as UnitCard;

            if (unitCard.TurnCost > Card.GetPlayer().TurnPoints || Player.State != PlayerState.Attack || Player.TurnPoints <= 0)
                result = false;

            if (Board.Base.CardTypesEquals(unitCard, _enemyCard))
                result = false;

            Cell cardCell = Board.Base.GetUnitCell(unitCard);
            Cell enemyCell = Board.Base.GetTowerCell(_enemy, _enemyCard);

            if (cardCell != enemyCell)
                result = false;

            if (result)
            {
                _enemyCard.GetDamage(unitCard.GetUnit().GetStatistics(StatisticsType.Damage).Value);

                Player.RemoveTurnPoints(unitCard.TurnCost);
                unitCard.DoubleCost();

                Sequence sequence = DOTween.Sequence();
                sequence.Append(unitCard.transform.DOMove(new Vector3(_enemyCard.transform.position.x, _enemyCard.transform.position.y, unitCard.transform.position.z), 0.2f));
                sequence.AppendCallback(() => EndExecute(cardCell));
                sequence.Play();
            }
            else
            {
                Board.Base.RefreshLinePositions(cardCell.GetLine(Player.Type));
            }

            Executed = result;
        }

        private void EndExecute(Cell cardCell)
        {
            Board.Base.RefreshLinePositions(cardCell.GetLine(Player.Type));
            OnCommandExecuted?.Invoke();
        }
    }
}
