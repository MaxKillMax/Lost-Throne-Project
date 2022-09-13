using System;

namespace LostThrone.Board
{
    public abstract class Command
    {
        protected readonly Action OnCommandEnded;

        protected readonly Board Board;
        protected readonly BoardPlayer Player;

        private bool _result;
        public bool Result => _result;

        public Command(Board board, BoardPlayer player, Action onCommandEnded)
        {
            Board = board;
            Player = player;
            OnCommandEnded = onCommandEnded;
        }

        public void Execute()
        {
            _result = CanExecute();

            if (_result)
                StartCommand();
            else
                EndCommand();
        }

        protected virtual bool CanExecute()
        {
            return Board.Base.PlayerCanMove(Player);
        }

        protected virtual void StartCommand()
        {
            UnityEngine.Debug.Log("Command not overrided");
            EndCommand();
        }

        protected virtual void EndCommand()
        {
            OnCommandEnded?.Invoke();
        }
    }
}
