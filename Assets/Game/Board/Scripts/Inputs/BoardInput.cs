using UnityEngine;

namespace LostThrone.Board
{
    public class BoardInput : MonoBehaviour
    {
        [SerializeField] protected BoardPlayer Player;
        [SerializeField] protected BoardPlayer Enemy;
        [SerializeField] protected Board Board;

        protected UnitCard SelectedCard;

        protected BoardBase Base;
        protected Formulas Formulas;

        private void Start()
        {
            Base = Services.GetService<BoardBase>();
            Formulas = Services.GetService<Formulas>();
        }
    }
}
