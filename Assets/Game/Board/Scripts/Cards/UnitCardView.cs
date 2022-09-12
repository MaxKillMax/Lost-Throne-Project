using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace LostThrone.Board
{
    public class UnitCardView : CardView
    {
        [SerializeField, Foldout("Additionals")]
        protected TMP_Text TurnCostText;

        public void SetTurnCostText(int cost)
        {
            TurnCostText.text = cost.ToString();
        }
    }
}
