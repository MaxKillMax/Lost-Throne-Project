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
}
