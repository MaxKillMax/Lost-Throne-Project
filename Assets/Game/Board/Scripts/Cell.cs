using System;
using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.Board
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private Line _topLine;
        [SerializeField] private Line _bottomLine;

        private void Awake()
        {
            _topLine.SetCell(this);
            _bottomLine.SetCell(this);
        }

        public Line GetLine(PositionType type) => type == _topLine.Type ? _topLine : _bottomLine;
    }

    [Serializable]
    public class Line
    {
        [SerializeField] private PositionType _type;
        [SerializeField] private Transform _parent;
        [SerializeField] private Cell _cell;

        private List<UnitCard> _cards = new List<UnitCard>(2);

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
