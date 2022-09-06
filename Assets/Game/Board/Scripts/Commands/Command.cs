
namespace LostThrone.Board
{
    public abstract class Command
    {
        protected bool _executed;

        protected Board _board;
        protected Player _player;

        public bool Executed => _executed;

        public Command(Board board, Player player)
        {
            _board = board;
            _player = player;
        }

        public abstract void Execute();
    }

    public abstract class CardCommand : Command
    {
        protected Card _card;

        public CardCommand(Board board, Player player, Card card) : base(board, player)
        {
            _card = card;
        }
    }
}
