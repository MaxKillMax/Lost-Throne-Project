
namespace LostThrone
{
    public class Formulas : Service
    {
        private float _levelImportance = 0.4f;
        private float _damageImportance = 1;
        private float _armorImportance = 0.1f;
        private float _healthImportance = 0.2f;

        public float LevelImportance => _levelImportance;
        public float DamageImportance => _damageImportance;
        public float ArmorImportance => _armorImportance;
        public float HealthImportance => _healthImportance;

        public void GetCardStats(OpenWorld.Unit unit, out float damage, out float armor, out float health)
        {
            damage = unit.GetStatistics(StatisticsType.Damage).Value;
            armor = unit.GetStatistics(StatisticsType.Armor).Value;
            health = unit.GetStatistics(StatisticsType.Health).Value;
        }

        public float GetUnitImportance(OpenWorld.Unit unit)
        {
            GetCardStats(unit, out float damage, out float armor, out float health);
            return unit.Level * _levelImportance + damage * _damageImportance + armor * _armorImportance + health * _healthImportance;
        }

        public float GetUnitAttackImportance(OpenWorld.Unit unit)
        {
            GetCardStats(unit, out float damage, out float armor, out float health);
            return 50 + unit.Level * _levelImportance + damage * _damageImportance - armor * _armorImportance - health * _healthImportance;
        }

        public float DamageReducedByArmor(float damage, float armor) => damage * (100 - armor) / 100;
    }
}
