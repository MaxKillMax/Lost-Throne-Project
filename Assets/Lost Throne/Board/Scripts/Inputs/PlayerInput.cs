using UnityEngine;

namespace LostThrone.Board
{
    public class PlayerInput : BoardInput
    {
        [SerializeField] private GameObject _playerTurnUI;
        [SerializeField] private BoardCamera _boardCamera;
        [SerializeField] private Camera _camera;

        protected UnitCard SelectedCard;

        private readonly float _minDragTime = 0.13f;
        private float _currentTime = 0.1f;

        private bool _canOpenCard = true;
        private CardSelectionState _selectionState = CardSelectionState.Nothing;

        private void Awake()
        {
            Board.OnPlayerTurnChanged += CheckState;
        }

        private void OnDestroy()
        {
            Board.OnPlayerTurnChanged -= CheckState;
        }

        protected override void Start()
        {
            base.Start();
        }
        
        private void CheckState(PositionType type)
        {
            if (type == Player.Type)
            {
                _playerTurnUI.SetActive(true);
            }
            else
            {
                _playerTurnUI.SetActive(false);

                if(_selectionState == CardSelectionState.Opened)
                {
                    SelectedCard.GetCardView().CloseInfoPanel();
                    RefreshState();
                }

                _boardCamera.ActivateOverviewMode();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
                _boardCamera.MoveCamera(Direction.Top);
            else if (Input.GetKeyDown(KeyCode.D))
                _boardCamera.MoveCamera(Direction.Right);
            else if (Input.GetKeyDown(KeyCode.S))
                _boardCamera.MoveCamera(Direction.Bottom);
            else if (Input.GetKeyDown(KeyCode.A))
                _boardCamera.MoveCamera(Direction.Left);

            if (!Base.PlayerCanMove(Player))
                return;

            if (Input.GetKeyDown(KeyCode.Space))
                _boardCamera.SwitchOverviewMode();

            if (Input.GetMouseButtonDown(0))
                OnInputMouseDown();

            if (Input.GetMouseButton(0))
                OnInputMouse();

            if (Input.GetMouseButtonUp(0))
                OnInputMouseUp();
        }

        #region Inputs

        private void OnInputMouseDown()
        {
            if (_selectionState == CardSelectionState.Opened)
                SelectedCard.GetCardView().CloseInfoPanel();

            if (Formulas.TryGetObjectInMousePosition(out UnitCard unitCard, (unitCard) => unitCard.GetPlayer() == Player))
            {
                _canOpenCard = unitCard != SelectedCard || _selectionState != CardSelectionState.Opened;
                _selectionState = CardSelectionState.Selected;
                SelectedCard = unitCard;
                _currentTime = _minDragTime;
            }
            else if (Formulas.TryGetObjectInMousePosition(out UnitCard enemyCard, (unitCard) => unitCard.GetPlayer() != Player))
            {
                _canOpenCard = enemyCard != SelectedCard || _selectionState != CardSelectionState.Opened;
                _selectionState = CardSelectionState.Opened;
                SelectedCard = enemyCard;

                if (_canOpenCard)
                    SelectedCard.GetCardView().OpenInfoPanel();
                else
                    RefreshState();
            }
            else
            {
                RefreshState();
            }
        }

        private void OnInputMouse()
        {
            if (!(_selectionState is CardSelectionState.Selected or CardSelectionState.Moved))
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
            else if (_selectionState == CardSelectionState.Moved)
            {
                Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = SelectedCard.transform.position.z;
                SelectedCard.transform.position = mousePosition;
            }
        }

        private void OnInputMouseUp()
        {
            if (_selectionState == CardSelectionState.Selected && _canOpenCard)
            {
                SelectedCard.GetCardView().OpenInfoPanel();
                _selectionState = CardSelectionState.Opened;
            }
            else if (_selectionState == CardSelectionState.Moved)
            {
                UseSelectedCard();
            }
        }

        private void RefreshState()
        {
            _selectionState = CardSelectionState.Nothing;
            SelectedCard = null;
        }

        #endregion

        private void UseSelectedCard()
        {
            UnitCard cachedCard = SelectedCard;

            if (Base.UnitCanAttack(cachedCard, Player) && Formulas.TryGetObjectInMousePosition(out UnitCard unitCard, (unitCard) => unitCard.GetPlayer().Type == Board.EnemyPositionType))
                new AttackUnitCommand(Board, Player, SelectedCard, unitCard, () => { new DropCommand(Board, Player, cachedCard).Execute(); }).Execute();
            else if (Base.UnitCanAttackTower(cachedCard, Player) && Formulas.TryGetObjectInMousePosition(out TowerCard towerCard, (towerCard) => towerCard.GetPlayer().Type == Board.EnemyPositionType))
                new AttackTowerCommand(Board, Player, SelectedCard, Enemy, towerCard, () => { new DropCommand(Board, Player, cachedCard).Execute(); }).Execute();
            else if (Base.UnitCanMove(cachedCard, Player) && Formulas.TryGetObjectInMousePosition(out Cell cell))
                new MovementCommand(Board, Player, SelectedCard, cell, () => { new DropCommand(Board, Player, cachedCard).Execute(); }).Execute();
            else
                new DropCommand(Board, Player, cachedCard).Execute();
        }
    }
}
