using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.OpenWorld;

namespace Game.Board
{
    public abstract class Card : MonoBehaviour
    {
        [SerializeField] protected TMP_Text _damageText;
        [SerializeField] protected TMP_Text _armorText;
        [SerializeField] protected TMP_Text _healthText;

        [Space]

        [SerializeField] protected TMP_Text _titleText;
        [SerializeField] protected Image _iconImage;

        [Space]

        [SerializeField] protected Transform _modificatorsParent;
        [SerializeField] protected GameObject _modificatorPrefab;

        [Space]

        [SerializeField] protected TMP_Text _turnCostText;
        [SerializeField] protected TMP_Text _levelText;
        [SerializeField] protected TMP_Text _rarityText;

        [Space]

        [SerializeField] protected GameObject _infoObject;
        [SerializeField] protected Canvas _canvas;

        protected Unit _unit;
        public Unit Unit => _unit;

        protected Player _player;
        public Player Player => _player;

        public void OpenInfoPanel()
        {
            _infoObject.SetActive(true);
        }

        public void CloseInfoPanel()
        {
            _infoObject.SetActive(false);
        }

        public abstract void DestroyCard();

        public abstract void GetDamage(float value);

        protected abstract void RefreshUI();
    }
}
