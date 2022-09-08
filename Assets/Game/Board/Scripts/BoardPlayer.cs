using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.Board
{
    public class BoardPlayer : MonoBehaviour
    {
        #region Fields

        [SerializeField] private PositionType _type;
        private PlayerState _state;

        public PlayerState State => _state;
        public PositionType Type => _type;

        private int _turnPoints;

        public int TurnPoints => _turnPoints;
        public bool HaveTurns => _turnPoints > 0;

        [SerializeField] private Cell _handCell;
        private OpenWorld.BattleData _battleData;

        public Line Hand => _handCell.GetLine(_type);
        public OpenWorld.BattleData BattleData => _battleData;

        private List<UnitCard> _cards;
        private List<TowerCard> _towers;

        public List<UnitCard> Cards => _cards;
        public List<TowerCard> Towers => _towers;

        #endregion

        public void StartBattle(OpenWorld.BattleData battleData, List<UnitCard> cards, List<TowerCard> towers)
        {
            _battleData = battleData;
            _cards = cards;
            _towers = towers;
        }

        public void EndBattle()
        {
            _battleData = default;
            _cards = default;
            _towers = default;
        }

        public void SetState(PlayerState state)
        {
            _state = state;
        }

        public void RefreshCardsAndTurns(int turnPoints)
        {
            _turnPoints = turnPoints;

            for (int i = 0; i < _cards.Count; i++)
                _cards[i].RefreshCard();
        }

        public void RemoveTurnPoints(int value) => _turnPoints -= value;

        public void RemoveCard(UnitCard card) => _cards.Remove(card);

        public void RemoveTower(TowerCard card) => _towers.Remove(card);

        //public void UseCard(UnitCard card)
        //{
        //    _turnPoints -= card.TurnCost;
        //    card.DoubleCost();
        //}
    }
}
