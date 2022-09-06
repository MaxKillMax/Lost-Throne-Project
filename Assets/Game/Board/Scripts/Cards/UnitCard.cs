using UnityEngine;

namespace LostThrone.Board
{
    public class UnitCard : Card
    {
        public override CardType Type => CardType.Unit;

        private int _turnCost;
        public int TurnCost => _turnCost;

        public void InitializeCard(Player player, OpenWorld.Unit unit, Camera camera)
        {
            _player = player;
            _unit = unit;
            _canvas.worldCamera = camera;

            _unit.GetStatistics(StatisticsType.Damage).OnValueChanged += RefreshStatistics;
            _unit.GetStatistics(StatisticsType.Armor).OnValueChanged += RefreshStatistics;
            _unit.GetStatistics(StatisticsType.Health).OnValueChanged += RefreshStatistics;

            RefreshCard();
            RefreshUI();
        }

        private void OnDestroy()
        {
            if (_unit != null)
            {
                _unit.GetStatistics(StatisticsType.Damage).OnValueChanged -= RefreshStatistics;
                _unit.GetStatistics(StatisticsType.Armor).OnValueChanged -= RefreshStatistics;
                _unit.GetStatistics(StatisticsType.Health).OnValueChanged -= RefreshStatistics;
            }
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
                DestroyCard();
        }

        public override void DestroyCard()
        {
            _unit.GetStatistics(StatisticsType.Health).SetValue(0);
            _player.DestroyCard(this);
        }

        protected override void RefreshUI()
        {
            _titleText.text = _unit.Name;
            _iconImage.sprite = _unit.CardIcon;
            _turnCostText.text = _turnCost.ToString();
            _levelText.text = _unit.Level.ToString();
            _rarityText.text = _unit.CardRarity.ToString();

            RefreshStatistics();
        }

        protected void RefreshStatistics()
        {
            _damageText.text = _unit.GetStatistics(StatisticsType.Damage).Value.ToString("N0");
            _armorText.text = _unit.GetStatistics(StatisticsType.Armor).Value.ToString("N0");
            _healthText.text = _unit.GetStatistics(StatisticsType.Health).Value.ToString("N0");
        }
    }
}
