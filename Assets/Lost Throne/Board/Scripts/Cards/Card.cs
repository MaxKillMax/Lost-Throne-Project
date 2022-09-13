using System;
using UnityEngine;

namespace LostThrone.Board
{
    [RequireComponent(typeof(CardView))]
    public abstract class Card : MonoBehaviour
    {
        public event Action<Card> OnCardDestroyed;

        [SerializeField] private Canvas _canvas;
        [SerializeField] protected CardView CardView;

        protected BoardPlayer Player;
        protected Unit Unit;

        public abstract CardType Type { get; }

        public BoardPlayer GetPlayer() => Player;

        public Unit GetUnit() => Unit;

        public CardView GetCardView() => CardView;

        protected void InitializeUnit(BoardPlayer player, Unit unit)
        {
            _canvas.worldCamera = Camera.current;

            Player = player;
            Unit = unit;

            CardView.InitializeUnit(unit);

            Unit.GetStatistics(StatisticsType.Damage).OnValueChanged += CardView.RefreshStatistics;
            Unit.GetStatistics(StatisticsType.Armor).OnValueChanged += CardView.RefreshStatistics;
            Unit.GetStatistics(StatisticsType.Health).OnValueChanged += CardView.RefreshStatistics;

            CardView.RefreshUI();
        }

        private void OnDestroy()
        {
            Unit.GetStatistics(StatisticsType.Damage).OnValueChanged -= CardView.RefreshStatistics;
            Unit.GetStatistics(StatisticsType.Armor).OnValueChanged -= CardView.RefreshStatistics;
            Unit.GetStatistics(StatisticsType.Health).OnValueChanged -= CardView.RefreshStatistics;
        }

        public abstract void GetDamage(float value);

        public virtual void DestroyCard()
        {
            Unit.GetStatistics(StatisticsType.Health).SetValue(0);
            OnCardDestroyed?.Invoke(this);
        }
    }
}
