
namespace LostThrone.Board
{
    public class TowerCard : Card
    {
        public override CardType Type => CardType.Tower;

        public void InitializeTowerCard(BoardPlayer player, Unit unit)
        {
            gameObject.SetActive(true);
            InitializeUnit(player, unit);
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
            base.DestroyCard();
            gameObject.SetActive(false);
        }
    }
}
