
namespace LostThrone.Board
{
    public class TowerCard : Card
    {
        public override CardType Type => CardType.Tower;

        protected Cell _cell;

        public bool IsDestroyed => !gameObject.activeSelf;

        public void InitializeTower(Player player, Cell cell, OpenWorld.Unit unit)
        {
            gameObject.SetActive(true);
            _player = player;
            _unit = unit;
            _cell = cell;

            _unit.GetStatistics(StatisticsType.Damage).OnValueChanged += RefreshStatistics;
            _unit.GetStatistics(StatisticsType.Armor).OnValueChanged += RefreshStatistics;
            _unit.GetStatistics(StatisticsType.Health).OnValueChanged += RefreshStatistics;

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
            gameObject.SetActive(false);
        }

        protected override void RefreshUI()
        {
            _titleText.text = _unit.Name;
            _iconImage.sprite = _unit.CardIcon;
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
