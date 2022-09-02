
namespace Game.Board
{
    public class TowerCard : Card
    {
        protected Cell _cell;

        public bool IsDestroyed => !gameObject.activeSelf;

        public void InitializeTower(Player player, Cell cell, OpenWorld.Unit unit)
        {
            gameObject.SetActive(true);
            _player = player;
            _unit = unit;
            _cell = cell;
        }

        public override void GetDamage(float value)
        {
            float health = _unit.GetStatistics(StatisticsType.Health).Value - value * (100 - _unit.GetStatistics(StatisticsType.Armor).Value) / 100;
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
