using System;

namespace LostThrone.Board
{
    public abstract class CardCommand : Command
    {
        protected readonly Card Card;

        public CardCommand(Board board, BoardPlayer player, Card card, Action onCommandEnded) : base(board, player, onCommandEnded)
        {
            Card = card;
        }
    }
}
