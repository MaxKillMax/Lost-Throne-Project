using System;
using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.Board
{
    [Serializable]
    public class Line
    {
        [SerializeField] private PositionType _type;
        [SerializeField] private Transform _parent;
        private Cell _cell;

        private readonly List<UnitCard> _cards = new(2);

        public PositionType Type => _type;
        public Transform Parent => _parent;
        public Cell Cell => _cell;
        public List<UnitCard> Cards => _cards;

        public void SetCell(Cell cell)
        {
            _cell = cell;
        }

        public void AddCard(UnitCard card)
        {
            if (_cards.Contains(card))
                _cards.Add(card);
        }

        public void RemoveCard(UnitCard card)
        {
            if (_cards.Contains(card))
                _cards.Remove(card);
        }
    }
}
