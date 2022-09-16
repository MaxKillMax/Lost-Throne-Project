using UnityEngine;

namespace LostThrone.Board
{
    [RequireComponent(typeof(UnitCardView))]
    public class UnitCard : Card
    {
        private UnitCardView _unitCardView;

        public override CardType Type => CardType.Unit;

        private Board _board;

        private int _basicTurnCost;
        private int _turnCost;
        public int TurnCost => _turnCost;

        public void InitializeUnitCard(Board board, BoardPlayer player, Unit unit)
        {
            _board = board;
            _unitCardView = CardView as UnitCardView;
            _basicTurnCost = Mathf.RoundToInt(Services.GetService<Formulas>().GetUnitImportance(unit));

            RefreshCard();
            InitializeUnit(player, unit);
        }

        public void DoubleCost()
        {
            _turnCost *= 2;
            _unitCardView.SetTurnCostText(_turnCost);
        }

        public void RefreshCard()
        {
            _turnCost = _basicTurnCost;
            _unitCardView.SetTurnCostText(_turnCost);
        }

        public override void GetDamage(float value)
        {
            float health = Unit.GetStatistics(StatisticsType.Health).Value - Services.GetService<Formulas>().DamageReducedByArmor(value, Unit.GetStatistics(StatisticsType.Armor).Value);
            Unit.GetStatistics(StatisticsType.Health).SetValue(health);

            if (health <= 0)
                DestroyCard();
        }

        public override void DestroyCard()
        {
            Cell cell = _board.Base.GetUnitCell(this);
            cell.GetLine(GetPlayer().Type).RemoveCard(this);
            _board.Base.RefreshLinePositions(cell.GetLine(GetPlayer().Type));

            base.DestroyCard();
            Destroy(gameObject);
        }
    }
}
