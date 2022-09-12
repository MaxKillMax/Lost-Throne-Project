using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace LostThrone.Board
{
    public class AttackUnitCommand : CardCommand
    {
        private readonly UnitCard _enemy;

        public AttackUnitCommand(Board board, BoardPlayer player, Card card, UnitCard enemy, Action onCommandEnded = null) : base(board, player, card, onCommandEnded)
        {
            _enemy = enemy;
        }

        public override void Execute()
        {
            bool result = true;
            UnitCard unitCard = Card as UnitCard;

            if (unitCard.TurnCost > Card.GetPlayer().TurnPoints || Player.State != PlayerState.Attack || Player.TurnPoints <= 0)
                result = false;

            if (Board.Base.CardTypesEquals(unitCard, _enemy))
                result = false;

            Cell cardCell = Board.Base.GetUnitCell(unitCard);
            Cell enemyCell = Board.Base.GetUnitCell(_enemy);

            if (cardCell != enemyCell)
                result = false;

            if (result)
            {
                _enemy.GetDamage(unitCard.GetUnit().GetStatistics(StatisticsType.Damage).Value);

                Player.RemoveTurnPoints(unitCard.TurnCost);
                unitCard.DoubleCost();

                Sequence sequence = DOTween.Sequence();
                sequence.Append(unitCard.transform.DOMove(new Vector3(_enemy.transform.position.x, _enemy.transform.position.y, unitCard.transform.position.z), 0.2f));
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
