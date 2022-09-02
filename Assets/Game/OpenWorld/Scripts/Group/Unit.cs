using System;
using System.Linq;
using UnityEngine;

namespace Game.OpenWorld
{
    public class Unit
    {
        private string _name;
        private Sprite _icon;

        private bool _attributesAreNotNeeded;
        public bool AttributesAreNotNeeded => _attributesAreNotNeeded;

        private Game.Board.CardRarity _cardRarity;
        private Sprite _cardIcon;

        private Statistics[] _statistics;
        private Attribute[] _attributes;

        public Statistics[] Statistics => _statistics;
        public Attribute[] Attributes => _attributes;

        public string Name => _name;
        public Sprite Icon => _icon;

        public Game.Board.CardRarity CardRarity => _cardRarity;
        public Sprite CardIcon => _cardIcon;

        public int Level { get { int level = 0; for (int i = 0; i < _attributes.Length; i++) level += (int)_attributes[i].value; return level; } }

        public Unit(UnitData data)
        {
            _name = data.Name;
            _icon = data.Icon;
            _cardRarity = data.CardRarity;
            _cardIcon = data.CardIcon;
            _attributesAreNotNeeded = data.AttributesAreNotNeeded;

            _statistics = new Statistics[data.Statistics.Length];
            for (int i = 0; i < _statistics.Length; i++)
                _statistics[i] = data.Statistics[i].GetCopy();

            _attributes = new Attribute[data.Attributes.Length];
            for (int i = 0; i < _attributes.Length; i++)
                _attributes[i] = data.Attributes[i].GetCopy();

            RefreshStatistics();
        }

        public Statistics GetStatistics(StatisticsType type) => _statistics.FirstOrDefault(s => s.Type == type);

        public Attribute GetAttribute(AttributeType type) => _attributes.FirstOrDefault(a => a.type == type);

        public void SetAttributeValue(AttributeType type, float value)
        {
            for (int i = 0; i < _attributes.Length; i++)
                if (_attributes[i].type == type)
                    _attributes[i].value = value;
        }

        public void IncreaseAttributeValue(AttributeType type, float value)
        {
            for (int i = 0; i < _attributes.Length; i++)
                if (_attributes[i].type == type)
                    _attributes[i].value += value;
        }

        public void DecreaseAttributeValue(AttributeType type, float value)
        {
            for (int i = 0; i < _attributes.Length; i++)
                if (_attributes[i].type == type)
                    _attributes[i].value -= value;
        }

        public void AddLevel()
        {
            GetStatistics(StatisticsType.Experience).SetValue(0);
            _attributes[UnityEngine.Random.Range(0, _attributes.Length)].value += 1;
            RefreshStatistics();
        }

        public void RefreshStatistics()
        {
            if (_attributesAreNotNeeded)
                return;

            for (int i = 0; i < _statistics.Length; i++)
                _statistics[i].RefreshStatistics(_attributes);
        }
    }
}

[System.Serializable]
public class Attribute
{
    public AttributeType type;
    public float value;

    public Attribute(AttributeType type, float value)
    {
        this.type = type;
        this.value = value;
    }

    public Attribute GetCopy() => new (type, value);
}

[System.Serializable]
public class Statistics
{
    public event Action OnBelowTheLimit;
    public event Action OnAboveTheLimit;

    public event Action OnValueChanged;

    [SerializeField] private StatisticsType _type;
    [Space]
    [SerializeField] private bool _valueEqualsMaxValue;
    [Space]
    [SerializeField] private float _basicMaxValue;
    [SerializeField] private Attribute[] _maxValuePerAttributes;
    [Space]
    [SerializeField] private float _increaseMultiply;
    [SerializeField] private float _decreaseMultiply;
    [Space]
    [SerializeField] private float _maxValue;
    [SerializeField] private float _value;

    public float MaxValue => _maxValue;
    public float Value => _value;
    public StatisticsType Type => _type;

    public Statistics(StatisticsType type, bool valueEqualsMaxValue, float basicMaxValue, Attribute[] maxValuePerAttributes, float increaseMultiply, float decreaseMultiply, float maxValue, float value)
    {
        _type = type;
        _valueEqualsMaxValue = valueEqualsMaxValue;
        _basicMaxValue = basicMaxValue;
        _maxValuePerAttributes = maxValuePerAttributes;
        _increaseMultiply = increaseMultiply;
        _decreaseMultiply = decreaseMultiply;
        _maxValue = maxValue;
        _value = value;
    }

    public Statistics GetCopy() => new (_type, _valueEqualsMaxValue, _basicMaxValue, _maxValuePerAttributes, _increaseMultiply, _decreaseMultiply, _maxValue, _value);

    public void RefreshStatistics(Attribute[] attributes)
    {
        float additionByAttributes = 0;
        for (int x = 0; x < attributes.Length; x++)
        {
            for (int y = 0; y < _maxValuePerAttributes.Length; y++)
            {
                if (attributes[x].type == _maxValuePerAttributes[y].type)
                {
                    additionByAttributes += attributes[x].value * _maxValuePerAttributes[y].value;
                    break;
                }
            }
        }

        _maxValue = _basicMaxValue + additionByAttributes;
        if (_valueEqualsMaxValue)
            _value = _maxValue;

        OnValueChanged?.Invoke();
    }

    private void CheckValue()
    {
        if (_value > _maxValue)
        {
            _value = _maxValue;
            OnBelowTheLimit?.Invoke();
        }
        else if (_value <= 0)
        {
            _value = 0;
            OnAboveTheLimit?.Invoke();
        }

        if (_valueEqualsMaxValue)
            _maxValue = _value;

        OnValueChanged?.Invoke();
    }

    public void SetValue(float value)
    {
        _value = value;
        CheckValue();
    }

    public void AddValue(float value)
    {
        _value += value * _increaseMultiply;
        CheckValue();
    }

    public void DowngradeValue(float value)
    {
        _value -= value * _decreaseMultiply;
        CheckValue();
    }

    public bool HaveValue(float value) => _value >= value;
}

public enum AttributeType
{
    Strength,
    Vitality,
    Communication,
    Speed,
    Adaptability,
    Fortune
}

public enum StatisticsType
{
    Experience,
    Health,
    Stamina,
    Hunger,
    Cold,
    Comfort,
    Weight,
    Damage,
    DiseaseChance,
    Speed,
    Discount,
    Fortune,
    Armor
}
