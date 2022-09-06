using System.Collections.Generic;
using UnityEngine;

namespace LostThrone.Board
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private Board _board;
        [SerializeField] private Camera _camera;

        private UnitCard _selectedCard;
        private UnitCard _openedCard;

        private bool _cardIsSelected = false;
        private bool _cardIsMoved = false;
        private bool _cardIsOpened = false;

        private float _minDragTime = 0.1f;
        private float _currentTime = 0.1f;

        private void Update()
        {
            if (!_board.GameInProgress)
                return;

            if (Input.GetMouseButtonDown(0))
                OnInputMouseDown();

            if (Input.GetMouseButton(0) && _cardIsSelected)
                OnInputMouse();

            if (Input.GetMouseButtonUp(0))
                OnInputMouseUp();

            if (Input.GetKeyDown(KeyCode.W))
                MoveCamera(Direction.Top);
            else if (Input.GetKeyDown(KeyCode.D))
                MoveCamera(Direction.Right);
            else if (Input.GetKeyDown(KeyCode.S))
                MoveCamera(Direction.Bottom);
            else if (Input.GetKeyDown(KeyCode.A))
                MoveCamera(Direction.Left);
        }

        private void OnInputMouseDown()
        {
            if (_cardIsOpened)
            {
                _openedCard.CloseInfoPanel();
                _cardIsOpened = false;
            }

            if (_player.State == PlayerState.Attack && FindCard(out UnitCard card))
            {
                if (!_player.Hand.Cards.Contains(card) && _openedCard != card)
                {
                    _cardIsOpened = true;
                    _openedCard = card;
                }

                _cardIsSelected = true;
                _selectedCard = card;
                _currentTime = _minDragTime;
            }

            if (!_cardIsOpened)
                _openedCard = null;
        }

        private void OnInputMouse()
        {
            _currentTime -= Time.deltaTime;

            if (_currentTime < 0)
            {
                if (_cardIsOpened)
                {
                    _cardIsOpened = false;
                    _openedCard = null;
                }

                if (!_cardIsMoved)
                {
                    _cardIsMoved = true;
                    new PickupCommand(_board, _player, _selectedCard).Execute();
                }

                Vector3 mousePosition = GetMouseWorldPosition();
                mousePosition.z = _selectedCard.transform.position.z;
                _selectedCard.transform.position = mousePosition;
            }
        }

        private void OnInputMouseUp()
        {
            if (_cardIsOpened)
                _openedCard.OpenInfoPanel();

            if (_cardIsSelected && !_cardIsOpened)
            {
                if (!FindObject() && _cardIsMoved)
                {
                    new DropCommand(_board, _player, _selectedCard).Execute();
                    if (_player.Hand.Cards.Contains(_selectedCard))
                        Services.GetService<BoardBase>().RefreshLinePositions(_player.Hand);
                    else
                        Services.GetService<BoardBase>().RefreshLinePositions(Services.GetService<BoardBase>().GetCardCell(_board, _selectedCard, out int h, out int v).GetLine(_player.Type));
                }
            }
            else if (_cardIsMoved)
                new DropCommand(_board, _player, _selectedCard).Execute();

            _cardIsSelected = false;
            _cardIsMoved = false;
            _selectedCard = null;
        }

        private void MoveCamera(Direction direction)
        {
            Cell[,] grid = _board.Grid;
            Cell currentCell = Services.GetService<BoardBase>().CurrentCameraCell;

            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    if (grid[x, y] == currentCell)
                    {
                        switch (direction)
                        {
                            case Direction.Top:
                                if (x + 1 < grid.GetLength(0))
                                    Services.GetService<BoardBase>().SetCameraTarget(_board.virtualCamera, grid[x + 1, y]);
                                break;
                            case Direction.Right:
                                if (y + 1 < grid.GetLength(1))
                                    Services.GetService<BoardBase>().SetCameraTarget(_board.virtualCamera, grid[x, y + 1]);
                            break;
                            case Direction.Bottom:
                                if (x - 1 >= 0)
                                    Services.GetService<BoardBase>().SetCameraTarget(_board.virtualCamera, grid[x - 1, y]);
                                break;
                            case Direction.Left:
                                if (y - 1 >= 0)
                                    Services.GetService<BoardBase>().SetCameraTarget(_board.virtualCamera, grid[x, y - 1]);
                                break;
                        }

                        return;
                    }
                }
            }
        }

        private bool FindCard(out UnitCard findedCard)
        {
            if (_player.TurnPoints <= 0 || _player.State != PlayerState.Attack)
            {
                findedCard = default;
                return false;
            }

            List<Transform> transforms = GetRayObjectsInMousePosition();

            if (transforms != null)
            {
                for (int i = 0; i < transforms.Count; i++)
                {
                    if (transforms[i].TryGetComponent(out UnitCard card))
                    {
                        if (card.TurnCost > card.Player.TurnPoints || card.Player != _player)
                            continue;
                        findedCard = card;
                        return true;
                    }
                }
            }

            findedCard = default;
            return false;
        }

        private bool FindObject()
        {
            List<Transform> transforms = GetRayObjectsInMousePosition();

            bool cellFinded = false;
            bool cardFinded = false;

            Cell cell = default;
            Card card = default;

            for (int i = 0; i < transforms.Count; i++)
            {
                if (card == default && transforms[i].TryGetComponent(out card))
                    cardFinded = true;
                else if (cell == default && transforms[i].TryGetComponent(out cell))
                    cellFinded = true;
            }

            UnitCard cashedCard = _selectedCard;

            if (cardFinded)
            {
                if (card.Type == CardType.Unit)
                    new AttackUnitCommand(_board, _player, _selectedCard, (UnitCard)card, () => { new DropCommand(_board, _player, cashedCard).Execute(); }).Execute();
                else if (card.Type == CardType.Tower)
                    new AttackTowerCommand(_board, _player, _selectedCard, (TowerCard)card, () => { new DropCommand(_board, _player, cashedCard).Execute(); }).Execute();
            }
            else if (cellFinded)
            {
                new MovementCommand(_board, _player, _selectedCard, cell, () => { new DropCommand(_board, _player, cashedCard).Execute(); }).Execute();
            }

            return cardFinded || cellFinded;
        }

        private List<Transform> GetRayObjectsInMousePosition()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            List<RaycastHit2D> raycastHits = new List<RaycastHit2D>(2);
            List<Transform> transforms = new List<Transform>(2);
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.NoFilter();

            if (Physics2D.Raycast(ray.origin, ray.direction, contactFilter, raycastHits) > 0)
            {
                for (int i = 0; i < raycastHits.Count; i++)
                    if (!_cardIsSelected || _selectedCard.transform != raycastHits[i].transform)
                        transforms.Add(raycastHits[i].transform);
                return transforms;
            }

            return default;
        }

        private Vector3 GetMouseWorldPosition() => _camera.ScreenToWorldPoint(Input.mousePosition);
    }
}
