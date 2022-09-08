using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using TMPro;

namespace LostThrone.Board
{
    public abstract class Card : MonoBehaviour
    {
        [SerializeField, Foldout("Main")] 
        protected TMP_Text _titleText;
        [SerializeField, Foldout("Main")] 
        protected Image _iconImage;
        [SerializeField, Foldout("Main")]
        protected Canvas _canvas;

        [SerializeField, Foldout("Values")] 
        protected TMP_Text _damageText;
        [SerializeField, Foldout("Values")] 
        protected TMP_Text _armorText;
        [SerializeField, Foldout("Values")] 
        protected TMP_Text _healthText;

        [SerializeField, Foldout("Modificators")] 
        protected Transform _modificatorsParent;
        [SerializeField, Foldout("Modificators")] 
        protected GameObject _modificatorPrefab;

        [SerializeField, Foldout("Additionals")] 
        protected TMP_Text _levelText;
        [SerializeField, Foldout("Additionals")] 
        protected TMP_Text _rarityText;

        [SerializeField, Foldout("Info Panel")] 
        protected GameObject _infoObject;

        protected BoardPlayer _player;
        protected Unit _unit;

        public BoardPlayer Player => _player;
        public Unit Unit => _unit;

        public abstract CardType Type { get; }

        public void OpenInfoPanel() => _infoObject.SetActive(true);

        public void CloseInfoPanel() => _infoObject.SetActive(false);

        protected void InitializeUnit(BoardPlayer player, Unit unit)
        {
            _player = player;
            _unit = unit;

            _unit.GetStatistics(StatisticsType.Damage).OnValueChanged += RefreshStatistics;
            _unit.GetStatistics(StatisticsType.Armor).OnValueChanged += RefreshStatistics;
            _unit.GetStatistics(StatisticsType.Health).OnValueChanged += RefreshStatistics;

            RefreshUI();
        }

        private void OnDestroy()
        {
            _unit.GetStatistics(StatisticsType.Damage).OnValueChanged -= RefreshStatistics;
            _unit.GetStatistics(StatisticsType.Armor).OnValueChanged -= RefreshStatistics;
            _unit.GetStatistics(StatisticsType.Health).OnValueChanged -= RefreshStatistics;
        }

        protected virtual void RefreshUI()
        {
            _titleText.text = _unit.Name;
            _iconImage.sprite = _unit.CardIcon;
            _levelText.text = _unit.Level.ToString();
            _rarityText.text = _unit.CardRarity.ToString();

            RefreshStatistics();
        }

        protected virtual void RefreshStatistics()
        {
            _damageText.text = _unit.GetStatistics(StatisticsType.Damage).Value.ToString("N0");
            _armorText.text = _unit.GetStatistics(StatisticsType.Armor).Value.ToString("N0");
            _healthText.text = _unit.GetStatistics(StatisticsType.Health).Value.ToString("N0");
        }

        public abstract void GetDamage(float value);

        /// <summary>
        /// Need to use BoardBase to destroy the card!
        /// </summary>
        public abstract void DestroyCard();
    }

    public enum CardType
    {
        Unit,
        Tower
    }
}
