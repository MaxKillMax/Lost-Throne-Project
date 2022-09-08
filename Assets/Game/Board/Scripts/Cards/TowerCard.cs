using UnityEngine;

namespace LostThrone.Board
{
    public class TowerCard : Card
    {
        private Board _board;
        private Cell _cell;

        public override CardType Type => CardType.Tower;

        public void InitializeTower(Board board, BoardPlayer player, Cell cell, Unit unit)
        {
            _canvas.worldCamera = Camera.current;
            gameObject.SetActive(true);

            _board = board;
            _cell = cell;

            InitializeUnit(player, unit);
        }

        public override void GetDamage(float value)
        {
            float health = _unit.GetStatistics(StatisticsType.Health).Value - Services.GetService<Formulas>().DamageReducedByArmor(value, _unit.GetStatistics(StatisticsType.Armor).Value);
            _unit.GetStatistics(StatisticsType.Health).SetValue(health);

            if (health <= 0)
                Services.GetService<BoardBase>().DestroyTowerCard(_board, this);
        }

        public override void DestroyCard() => gameObject.SetActive(false);
    }
}
