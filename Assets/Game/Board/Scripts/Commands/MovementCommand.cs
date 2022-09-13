using System;

namespace LostThrone.Board
{
    public class MovementCommand : CardCommand
    {
        private Cell _endCell;
        private UnitCard _unitCard;
        private Cell _startCell;
        private Line _endLine;

        public MovementCommand(Board board, BoardPlayer player, Card card, Cell endCell, Action onCommandEnded = null) : base(board, player, card, onCommandEnded)
        {
            _endCell = endCell;
            _unitCard = Card as UnitCard;
        }

        protected override bool CanExecute()
        {
            bool result = true;

            if (!base.CanExecute())
                result = false;

            if (!Board.Base.UnitCanMove(_unitCard, Player))
                result = false;

            _endLine = _endCell.GetLine(Player.Type);

            if (!Board.Base.LineCanAcceptCard(_endLine, _unitCard))
                result = false;

            _startCell = Board.Base.GetUnitCell(_unitCard, out int startHorizontal, out int startVertical);
            Board.Base.GetCellCoordinates(_endCell, out int endHorizontal, out int endVertical);

            if (_startCell != Player.Hand.Cell && Board.Base.CellHaveEnemies(Player, _startCell))
                result = false;

            if (Board.Base.HasWrongPosition(Player, startHorizontal, startVertical, endHorizontal, endVertical))
                result = false;

            return result;
        }

        protected override void StartCommand()
        {
            Player.RemoveTurnPoints(_unitCard.TurnCost);
            _unitCard.DoubleCost();

            _startCell.GetLine(Player.Type).RemoveCard(_unitCard);
            Board.Base.RefreshLinePositions(_startCell.GetLine(Player.Type));

            _endCell.GetLine(Player.Type).AddCard(_unitCard);
            Board.Base.RefreshLinePositions(_endCell.GetLine(Player.Type));

            _unitCard.transform.SetParent(_endLine.Parent);

            EndCommand();
        }

        protected override void EndCommand()
        {
            Board.Base.RefreshLinePositions(_startCell.GetLine(Player.Type));
            base.EndCommand();
        }
    }
}
