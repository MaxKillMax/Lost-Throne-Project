using System;

namespace LostThrone.Board
{
    public abstract class Command
    {
        protected Action _onCommandExecuted;

        protected bool _executed;

        protected Board _board;
        protected BoardPlayer _player;

        public bool Executed => _executed;

        public Command(Board board, BoardPlayer player, Action onCommandEnded)
        {
            _board = board;
            _player = player;
            _onCommandExecuted = onCommandEnded;
        }

        public abstract void Execute();
    }

    public abstract class CardCommand : Command
    {
        protected Card _card;

        public CardCommand(Board board, BoardPlayer player, Card card, Action onCommandEnded) : base(board, player, onCommandEnded)
        {
            _card = card;
        }
    }
}
