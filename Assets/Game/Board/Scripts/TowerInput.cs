using UnityEngine;

namespace LostThrone.Board
{
    public class TowerInput : MonoBehaviour
    {
        public void TryAttack(Board board, Player player, TowerCard towerCard)
        {
            Cell cell = Services.GetService<BoardBase>().GetTowerCell(board, towerCard, out int horizontal, out int vertical);
            Line enemyLine = cell.GetLine(player.Type == PositionType.Top ? PositionType.Bottom : PositionType.Top);

            
        }
    }
}
