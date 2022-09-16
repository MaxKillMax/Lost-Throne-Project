using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LostThrone.Board
{
    public class CardInfoWindow : Window
    {
        [SerializeField, Foldout("Main")]
        private TMP_Text _titleText;
        [SerializeField, Foldout("Main")]
        private Image _iconImage;

        [SerializeField, Foldout("Values")]
        private TMP_Text _damageText;
        [SerializeField, Foldout("Values")]
        private TMP_Text _armorText;
        [SerializeField, Foldout("Values")]
        private TMP_Text _healthText;

        [SerializeField, Foldout("Modificators")]
        private Transform _modificatorsParent;
        [SerializeField, Foldout("Modificators")]
        private GameObject _modificatorPrefab;

        [SerializeField, Foldout("Additionals")]
        private TMP_Text _levelText;
        [SerializeField, Foldout("Additionals")]
        private TMP_Text _rarityText;

        // TODO: Modificators
        public void SetUnit(Unit unit)
        {
            _titleText.text = unit.Name;
            _iconImage.sprite = unit.CardIcon;

            _levelText.text = unit.Level.ToString();
            _rarityText.text = unit.CardRarity.ToString();

            _damageText.text = unit.GetStatistics(StatisticsType.Damage).Value.ToString("N0");
            _armorText.text = unit.GetStatistics(StatisticsType.Armor).Value.ToString("N0");
            _healthText.text = unit.GetStatistics(StatisticsType.Health).Value.ToString("N0");
        }
    }
}
