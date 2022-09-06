using System.Collections.Generic;
using UnityEngine;
using LostThrone.OpenWorld;

namespace LostThrone.Board
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Board _board;
        [SerializeField] private UnitCard _cardPrefab;

        [SerializeField] private Line _hand;
        public Line Hand => _hand;

        private List<UnitCard> _cards;
        public List<UnitCard> Cards => _cards;

        [SerializeField] private PositionType _type;
        public PositionType Type => _type;

        private PlayerState _state;
        public PlayerState State => _state;

        private int _turnPoints;
        public int TurnPoints => _turnPoints;

        public void PrepareToBattle(Board board, List<Unit> units)
        {
            _cards = new List<UnitCard>(units.Count);
            for (int i = 0; i < units.Count; i++)
                InitializeUnit(units[i]);
            Services.GetService<BoardBase>().RefreshLinePositions(_hand);
        }

        public void EndBattle()
        {
            for (int i = 0; i < _cards.Count; i++)
                Destroy(_cards[i].gameObject);
            _cards.Clear();
        }

        private void InitializeUnit(Unit unit)
        {
            UnitCard card = Instantiate(_cardPrefab, _hand.Parent);
            card.InitializeCard(this, unit, _camera);
            _hand.AddCard(card);
            _cards.Add(card);
        }

        public void SetState(PlayerState state)
        {
            _state = state;
        }

        public void RefreshTurnPoints(int turnPoints)
        {
            _turnPoints = turnPoints;

            for (int i = 0; i < _cards.Count; i++)
                _cards[i].RefreshCard();
        }

        public void UseCard(UnitCard card)
        {
            _turnPoints -= card.TurnCost;
            card.DoubleCost();
        }

        public void DestroyCard(UnitCard card)
        {
            Cell cell = Services.GetService<BoardBase>().GetCardCell(_board, card, out int h, out int v);
            cell.GetLine(Type).RemoveCard(card);
            Services.GetService<BoardBase>().RefreshLinePositions(cell.GetLine(Type));
            _cards.Remove(card);
            Destroy(card.gameObject);
        }
    }
}
