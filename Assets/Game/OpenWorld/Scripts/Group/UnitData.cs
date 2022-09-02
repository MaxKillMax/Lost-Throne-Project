using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "Unit Data", menuName = "Unit Data", order = 51)]
public class UnitData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private Sprite _icon;

    [SerializeField] private Game.Board.CardRarity _cardRarity;
    [SerializeField] private Sprite _cardIcon;

    public string Name => _name;
    public Sprite Icon => _icon;

    public Game.Board.CardRarity CardRarity => _cardRarity;
    public Sprite CardIcon => _cardIcon;

    [SerializeField] private Statistics[] _statistics;
    [SerializeField] private bool _attributesAreNotNeeded;
    [SerializeField, HideIf("_attributesAreNotNeeded")] private Attribute[] _attributes;

    public Statistics[] Statistics => _statistics;
    public bool AttributesAreNotNeeded => _attributesAreNotNeeded;
    public Attribute[] Attributes => _attributes;
}
