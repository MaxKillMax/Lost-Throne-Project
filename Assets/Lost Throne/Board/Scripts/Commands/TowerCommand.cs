using System;

namespace LostThrone.Board
{
    public abstract class TowerCommand : Command
    {
        protected TowerCard TowerCard;

        public TowerCommand(Board board, BoardPlayer player, TowerCard towerCard, Action onCommandEnded) : base(board, player, onCommandEnded)
        {
            TowerCard = towerCard;
        }
    }
}
