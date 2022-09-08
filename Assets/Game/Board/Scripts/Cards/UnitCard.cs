using UnityEngine;
using NaughtyAttributes;
using TMPro;

namespace LostThrone.Board
{
    public class UnitCard : Card
    {
        [SerializeField, Foldout("Additionals")]
        protected TMP_Text _turnCostText;

        public override CardType Type => CardType.Unit;

        private Board _board;

        private int _turnCost;
        public int TurnCost => _turnCost;

        public void InitializeCard(Board board, BoardPlayer player, Unit unit)
        {
            _canvas.worldCamera = Camera.current;

            _board = board;

            RefreshCard();
            InitializeUnit(player, unit);
        }

        public void DoubleCost()
        {
            _turnCost *= 2;
            _turnCostText.text = _turnCost.ToString();
        }

        public void RefreshCard()
        {
            _turnCost = 1;
            _turnCostText.text = _turnCost.ToString();
        }

        public override void GetDamage(float value)
        {
            float health = _unit.GetStatistics(StatisticsType.Health).Value - Services.GetService<Formulas>().DamageReducedByArmor(value, _unit.GetStatistics(StatisticsType.Armor).Value);
            _unit.GetStatistics(StatisticsType.Health).SetValue(health);

            if (health <= 0)
                Services.GetService<BoardBase>().DestroyUnitCard(_board, this);
        }

        public override void DestroyCard() => Destroy(gameObject);
    }
}
