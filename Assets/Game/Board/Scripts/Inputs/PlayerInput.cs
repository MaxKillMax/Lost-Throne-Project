using UnityEngine;

namespace LostThrone.Board
{
    public class PlayerInput : BoardInput
    {
        [SerializeField] private GameObject _playerTurnUI;
        [SerializeField] private Camera _camera;

        private float _minDragTime = 0.1f;
        private float _currentTime = 0.1f;

        private CardSelectionState _selectionState = CardSelectionState.Nothing;

        private void Awake()
        {
            _board.OnPlayerTurnChanged += CheckState;
        }

        private void OnDestroy()
        {
            _board.OnPlayerTurnChanged -= CheckState;
        }

        private void CheckState(PositionType type)
        {
            _playerTurnUI.SetActive(type == _player.Type);
        }

        private void Update()
        {
            if (!_board.Base.PlayerCanMove(_board, _player))
                return;

            if (Input.GetMouseButtonDown(0))
                OnInputMouseDown();

            if (Input.GetMouseButton(0))
                OnInputMouse();

            if (Input.GetMouseButtonUp(0))
                OnInputMouseUp();

            if (Input.GetKeyDown(KeyCode.W))
                _board.Base.MoveCamera(_board, Direction.Top);
            else if (Input.GetKeyDown(KeyCode.D))
                _board.Base.MoveCamera(_board, Direction.Right);
            else if (Input.GetKeyDown(KeyCode.S))
                _board.Base.MoveCamera(_board, Direction.Bottom);
            else if (Input.GetKeyDown(KeyCode.A))
                _board.Base.MoveCamera(_board, Direction.Left);
        }

        #region Inputs

        private void OnInputMouseDown()
        {
            if (_selectionState == CardSelectionState.Opened)
                _selectedCard.CloseInfoPanel();

            _selectionState = CardSelectionState.Nothing;
            _selectedCard = null;

            if (_formulas.TryGetObjectInMousePosition(out UnitCard unitCard, (unitCard) => unitCard.Player == _player))
            {
                _selectionState = CardSelectionState.Selected;
                _selectedCard = unitCard;
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
                if (_player.TurnPoints >= _selectedCard.TurnCost)
                {
                    _selectionState = CardSelectionState.Moved;
                    new PickupCommand(_board, _player, _selectedCard).Execute();
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
                mousePosition.z = _selectedCard.transform.position.z;
                _selectedCard.transform.position = mousePosition;
            }
        }

        private void OnInputMouseUp()
        {
            if (_selectionState == CardSelectionState.Selected)
                _selectedCard.OpenInfoPanel();
            else if (_selectionState == CardSelectionState.Moved)
                UseSelectedCard();
        }

        #endregion

        private void UseSelectedCard()
        {
            UnitCard cachedCard = _selectedCard;

            if (_formulas.TryGetObjectInMousePosition(out UnitCard unitCard, (unitCard) => unitCard.Player.Type == _board.EnemyPositionType))
                new AttackUnitCommand(_board, _player, _selectedCard, unitCard, () => { new DropCommand(_board, _player, cachedCard).Execute(); }).Execute();
            else if (_formulas.TryGetObjectInMousePosition(out TowerCard towerCard, (towerCard) => towerCard.Player.Type == _board.EnemyPositionType))
                new AttackTowerCommand(_board, _player, _selectedCard, _enemy, towerCard, () => { new DropCommand(_board, _player, cachedCard).Execute(); }).Execute();
            else if (_formulas.TryGetObjectInMousePosition(out Cell cell))
                new MovementCommand(_board, _player, _selectedCard, cell, () => { new DropCommand(_board, _player, cachedCard).Execute(); }).Execute();
        }

        private Vector3 GetMouseWorldPosition() => _camera.ScreenToWorldPoint(Input.mousePosition);

        private bool CanHoldCard() => _selectionState == CardSelectionState.Selected || _selectionState == CardSelectionState.Moved;
    }

    public enum CardSelectionState
    {
        Nothing,
        Selected,
        Opened,
        Moved
    }
}
