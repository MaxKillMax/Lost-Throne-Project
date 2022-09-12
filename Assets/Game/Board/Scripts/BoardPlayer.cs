using System;
using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.Board
{
    public class BoardPlayer : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Cell _handCell;
        public Line Hand => _handCell.GetLine(_type);

        [SerializeField] private PositionType _type;
        public PositionType Type => _type;

        private PlayerState _state;
        public PlayerState State => _state;

        private int _turnPoints;
        public int TurnPoints => _turnPoints;

        private OpenWorld.BattleData _battleData;
        public OpenWorld.BattleData BattleData => _battleData;

        private List<UnitCard> _cards;
        public List<UnitCard> Cards => _cards;

        private List<TowerCard> _towers;
        public List<TowerCard> Towers => _towers;

        #endregion

        public void StartBattle(OpenWorld.BattleData battleData, List<UnitCard> cards, List<TowerCard> towers)
        {
            _battleData = battleData;
            _cards = cards;
            _towers = towers;

            ListenArrayOfCards(cards, RemoveUnit);
            ListenArrayOfCards(towers, RemoveTower);
        }

        private void ListenArrayOfCards<T>(List<T> cards, Action<Card> action) where T : Card
        {
            for (int i = 0; i < cards.Count; i++)
                cards[i].OnCardDestroyed += action;
        }

        private void RemoveUnit(Card card)
        {
            _cards.Remove((UnitCard)card);
            card.OnCardDestroyed -= RemoveUnit;
        }

        private void RemoveTower(Card card)
        {
            _towers.Remove((TowerCard)card);
            card.OnCardDestroyed -= RemoveTower;
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
    }
}
