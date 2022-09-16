using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LostThrone.Board
{
    public class CardView : MonoBehaviour
    {
        [SerializeField, Foldout("Main")]
        protected TMP_Text TitleText;
        [SerializeField, Foldout("Main")]
        protected Image IconImage;

        [SerializeField, Foldout("Values")]
        protected TMP_Text DamageText;
        [SerializeField, Foldout("Values")]
        protected TMP_Text ArmorText;
        [SerializeField, Foldout("Values")]
        protected TMP_Text HealthText;

        [SerializeField, Foldout("Additionals")]
        protected TMP_Text LevelText;

        private Unit _unit;

        public void InitializeUnit(Unit unit)
        {
            _unit = unit;
        }

        public virtual void RefreshUI()
        {
            TitleText.text = _unit.Name;
            IconImage.sprite = _unit.CardIcon;
            LevelText.text = _unit.Level.ToString();

            RefreshStatistics();
        }

        public virtual void RefreshStatistics()
        {
            DamageText.text = _unit.GetStatistics(StatisticsType.Damage).Value.ToString("N0");
            ArmorText.text = _unit.GetStatistics(StatisticsType.Armor).Value.ToString("N0");
            HealthText.text = _unit.GetStatistics(StatisticsType.Health).Value.ToString("N0");
        }
    }
}
