using System;

namespace LostThrone.Board
{
    public abstract class Command
    {
        protected readonly Action OnCommandExecuted;

        protected readonly Board Board;
        protected readonly BoardPlayer Player;

        protected bool Executed;

        public bool IsExecuted() => Executed;

        public Command(Board board, BoardPlayer player, Action onCommandEnded)
        {
            Board = board;
            Player = player;
            OnCommandExecuted = onCommandEnded;
        }

        public abstract void Execute();
    }
}
