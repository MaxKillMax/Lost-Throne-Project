using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.Board
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private Line _topLine;
        [SerializeField] private Line _bottomLine;

        public Line GetLine(PositionType type)
        {
            if (_topLine.Type == type)
                return _topLine;
            else if (_bottomLine.Type == type)
                return _bottomLine;

            return default;
        }
    }

    [System.Serializable]
    public class Line
    {
        [SerializeField] private PositionType _type;
        [SerializeField] private Transform _parent;

        private List<UnitCard> _cards = new List<UnitCard>(5);

        public PositionType Type => _type;
        public Transform Parent => _parent;
        public List<UnitCard> Cards => _cards;

        public void AddCard(UnitCard card)
        {
            if (!_cards.Contains(card))
                _cards.Add(card);
        }

        public void RemoveCard(UnitCard card)
        {
            if (_cards.Contains(card))
                _cards.Remove(card);
        }
    }
}
