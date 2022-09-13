using UnityEngine;

namespace LostThrone.Board
{
    public class PlayerInput : BoardInput
    {
        [SerializeField] private GameObject _playerTurnUI;
        [SerializeField] private Camera _camera;

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
            // UNDONE: Camera initialized in the wrong place
            Formulas.InitializeCamera(_camera);
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

            if (Formulas.TryGetObjectInMousePosition(out UnitCard unitCard, (unitCard) => unitCard.GetPlayer() == Player))
            {
                _canOpenCard = unitCard != SelectedCard || _selectionState != CardSelectionState.Opened;
                _selectionState = CardSelectionState.Selected;
                SelectedCard = unitCard;
                _currentTime = _minDragTime;
            }
            else
            {
                SelectedCard = null;
                _selectionState = CardSelectionState.Nothing;
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
