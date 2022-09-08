using UnityEngine;

namespace LostThrone.Board
{
    public class BoardInput : MonoBehaviour
    {
        [SerializeField] protected BoardPlayer _player;
        [SerializeField] protected BoardPlayer _enemy;
        [SerializeField] protected Board _board;

        protected UnitCard _selectedCard;

        protected Formulas _formulas;

        private void Start()
        {
            _formulas = Services.GetService<Formulas>();
        }
    }
}
