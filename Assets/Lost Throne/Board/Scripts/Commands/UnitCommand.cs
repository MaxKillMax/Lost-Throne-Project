using System;

namespace LostThrone.Board
{
    public abstract class UnitCommand : Command
    {
        protected UnitCard UnitCard;

        public UnitCommand(Board board, BoardPlayer player, UnitCard unitCard, Action onCommandEnded) : base(board, player, onCommandEnded)
        {
            UnitCard = unitCard;
        }
    }
}
