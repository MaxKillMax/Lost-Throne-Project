
namespace LostThrone.Board
{
    public enum BoardState
    {
        InProcess,
        InVictory,
        Not
    }

    public enum PlayerState
    {
        Attack,
        Defend,
        Win,
        Lose
    }

    public enum PositionType
    {
        Top,
        Bottom,
        Nothing
    }

    public enum Direction
    {
        Top,
        Right,
        Bottom,
        Left
    }

    public enum CardRarity
    {
        Common,
        Rare,
        Legendary
    }

    public enum CardSelectionState
    {
        Nothing,
        Selected,
        Opened,
        Moved
    }

    public enum CardType
    {
        Unit,
        Tower
    }
}