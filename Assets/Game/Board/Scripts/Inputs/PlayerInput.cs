using UnityEngine;

namespace LostThrone.Board
{
    public class PlayerInput : BoardInput
    {
        [SerializeField] private GameObject _playerTurnUI;
        [SerializeField] private Camera _camera;

        private readonly float _minDragTime = 0.1f;
        private float _currentTime = 0.1f;

        private CardSelectionState _selectionState = CardSelectionState.Nothing;

        private void Awake()
        {
            Board.OnPlayerTurnChanged += CheckState;
        }

        private void OnDestroy()
        {
            Board.OnPlayerTurnChanged -= CheckState;
        }

        private void CheckState(PositionType type)
        {
            _playerTurnUI.SetActive(type == Player.Type);
        }

        private void Update()
        {
            if (!Board.Base.PlayerCanMove(Player))
                return;

            if (Input.GetMouseButtonDown(0))
                OnInputMouseDown();

            if (Input.GetMouseButton(0))
                OnInputMouse();

            if (Input.GetMouseButtonUp(0))
                OnInputMouseUp();

            if (Input.GetKeyDown(KeyCode.W))
                Board.Base.MoveCamera(Direction.Top);
            else if (Input.GetKeyDown(KeyCode.D))
                Board.Base.MoveCamera(Direction.Right);
            else if (Input.GetKeyDown(KeyCode.S))
                Board.Base.MoveCamera(Direction.Bottom);
            else if (Input.GetKeyDown(KeyCode.A))
                Board.Base.MoveCamera(Direction.Left);
        }

        #region Inputs

        private void OnInputMouseDown()
        {
            if (_selectionState == CardSelectionState.Opened)
                SelectedCard.GetCardView().CloseInfoPanel();

            _selectionState = CardSelectionState.Nothing;
            SelectedCard = null;

            if (Formulas.TryGetObjectInMousePosition(out UnitCard unitCard, (unitCard) => unitCard.GetPlayer() == Player))
            {
                _selectionState = CardSelectionState.Selected;
                SelectedCard = unitCard;
                _currentTime = _minDragTime;
            }
        }

        private void OnInputMouse()
        {
            if (!CanHoldCard())
                return;

            _currentTime -= Time.deltaTime;

            if (_currentTime > 0)
                return;

            if (_selectionState == CardSelectionState.Selected)
            {
                if (Player.TurnPoints >= SelectedCard.TurnCost)
                {
                    _selectionState = CardSelectionState.Moved;
                    new PickupCommand(Board, Player, SelectedCard).Execute();
                }
                else
                {
                    _selectionState = CardSelectionState.Nothing;
                    return;
                }
            }

            if (_selectionState == CardSelectionState.Moved)
            {
                Vector3 mousePosition = GetMouseWorldPosition();
                mousePosition.z = SelectedCard.transform.position.z;
                SelectedCard.transform.position = mousePosition;
            }
        }

        private void OnInputMouseUp()
        {
            if (_selectionState == CardSelectionState.Selected)
                SelectedCard.GetCardView().OpenInfoPanel();
            else if (_selectionState == CardSelectionState.Moved)
                UseSelectedCard();
        }

        #endregion

        private void UseSelectedCard()
        {
            UnitCard cachedCard = SelectedCard;

            if (Formulas.TryGetObjectInMousePosition(out UnitCard unitCard, (unitCard) => unitCard.GetPlayer().Type == Board.EnemyPositionType))
                new AttackUnitCommand(Board, Player, SelectedCard, unitCard, () => { new DropCommand(Board, Player, cachedCard).Execute(); }).Execute();
            else if (Formulas.TryGetObjectInMousePosition(out TowerCard towerCard, (towerCard) => towerCard.GetPlayer().Type == Board.EnemyPositionType))
                new AttackTowerCommand(Board, Player, SelectedCard, Enemy, towerCard, () => { new DropCommand(Board, Player, cachedCard).Execute(); }).Execute();
            else if (Formulas.TryGetObjectInMousePosition(out Cell cell))
                new MovementCommand(Board, Player, SelectedCard, cell, () => { new DropCommand(Board, Player, cachedCard).Execute(); }).Execute();
        }

        private Vector3 GetMouseWorldPosition() => _camera.ScreenToWorldPoint(Input.mousePosition);

        private bool CanHoldCard() => _selectionState is CardSelectionState.Selected or CardSelectionState.Moved;
    }
}
