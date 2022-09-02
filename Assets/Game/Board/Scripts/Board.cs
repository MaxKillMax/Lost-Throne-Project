using System;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Game.OpenWorld;

namespace Game.Board
{
    public class Board : MonoBehaviour
    {
        public event Action<PositionType> OnTurnChanged;

        [SerializeField] private GameObject _playerTurnUI;
        [SerializeField] private Cell[] _cells;

        [SerializeField] private Player _player;
        [SerializeField] private Player _enemy;

        private BattleData _playerData;
        private BattleData _enemyData;

        [SerializeField] private TowerCard[] _playerTowers;
        [SerializeField] private TowerCard[] _enemyTowers;

        [SerializeField] private int _turnPoints = 4;
        [SerializeField] private int _cardsLimitInLine = 5;
        [SerializeField] private int _requiredVerticalOffset = 1;
        [SerializeField] private int _maximumHorizontalOffset = 1;

        [SerializeField] private float _refreshLinesTime = 0.2f;
        public float RefreshLinesTime => _refreshLinesTime;

        private Cell[,] _grid;
        public Cell[,] Grid => _grid;

        private PositionType _currentTurn;
        public PositionType CurrentTurn => _currentTurn;

        public int EnemyLine => _grid.GetLength(1) - 1;
        public int PlayerLine => 0;

        public bool GameInProgress { get; private set; }

        private void Awake()
        {
            _grid = new Cell[,]
            {
                { _cells[0], _cells[1], _cells[2] },
                { _cells[3], _cells[4], _cells[5] },
                { _cells[6], _cells[7], _cells[8] }
            };
        }

        #region States and Turns

        public void StartBattle(bool playerIsFirst, BattleData playerData, BattleData enemyData)
        {
            _playerData = playerData;
            _enemyData = enemyData;

            for (int i = 0; i < _playerTowers.Length; i++)
                _playerTowers[i].InitializeTower(_player, _grid[PlayerLine, i], _playerData.Tower);

            for (int i = 0; i < _enemyTowers.Length; i++)
                _enemyTowers[i].InitializeTower(_enemy, _grid[EnemyLine, i], _enemyData.Tower);

            _player.PrepareToBattle(this, _playerData.Units);
            _enemy.PrepareToBattle(this, _enemyData.Units);

            GameInProgress = true;
            SetTurn(playerIsFirst ? PositionType.Bottom : PositionType.Top);
        }

        public void EndTurn() => SetTurn(_currentTurn == PositionType.Top ? PositionType.Bottom : PositionType.Top);

        private void SetTurn(PositionType type)
        {
            _currentTurn = type;
            SetPlayerStates();
            OnTurnChanged?.Invoke(_currentTurn);

            _playerTurnUI.SetActive(type == PositionType.Top ? false : true);
        }

        public bool TryEndBattle(out Player winner) 
        {
            winner = default;
            bool ended = false;

            if (_player.Cards.Count == 0 || _playerTowers.Any(t => t.IsDestroyed) || (_playerData.HaveMainUnit && !_playerData.Units.Contains(_playerData.MainUnit)))
            {
                winner = _enemy;
                ended = true;
            }
            else if (_enemy.Cards.Count == 0 || _enemyTowers.Any(t => t.IsDestroyed) || (_enemyData.HaveMainUnit && !_enemyData.Units.Contains(_enemyData.MainUnit)))
            {
                winner = _player;
                ended = true;
            }

            if (ended)
            {
                _player.EndBattle();
                _enemy.EndBattle();
                GameInProgress = false;
                Debug.LogError("End");
            }

            return ended;
        }

        private void SetPlayerStates()
        {
            if (_currentTurn == PositionType.Top)
            {
                _player.SetState(PlayerState.Defend);
                _enemy.SetState(PlayerState.Attack);
                _enemy.RefreshTurnPoints(_turnPoints);
            }
            else
            {
                _player.SetState(PlayerState.Attack);
                _enemy.SetState(PlayerState.Defend);
                _player.RefreshTurnPoints(_turnPoints);
            }
        }

        #endregion

        #region Card Positions

        public bool TryGetTower(int horizontal, int vertical, out TowerCard tower)
        {
            tower = default;

            if (vertical == PlayerLine || vertical == EnemyLine)
            {
                tower = vertical == PlayerLine ? _playerTowers[horizontal] : _enemyTowers[horizontal];
                if (tower.IsDestroyed)
                    return false;

                return true;
            }

            return false;
        }

        public void RefreshLinePositions(Line line)
        {
            float middle = (line.Cards.Count - 1f) / 2;
            for (int i = 0; i < line.Cards.Count; i++)
                line.Cards[i].transform.DOLocalMove(new Vector3((i - middle), 0, 0), 0.2f);
        }

        public Cell FindCellOfCard(Card card, out int horizontal, out int vertical)
        {
            if (_playerTowers.Contains(card) || _enemyTowers.Contains(card))
                return GetCellOfTower(card, out horizontal, out vertical);

            PositionType type = card.Player.Type; 

            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                for (int y = 0; y < _grid.GetLength(1); y++)
                {
                    if (_grid[x, y].GetLine(type).Cards.Contains(card))
                    {
                        horizontal = y;
                        vertical = x;
                        return _grid[x, y];
                    }
                }
            }

            horizontal = 1;
            vertical = card.Player.Type == PositionType.Bottom ? -1 : 3;
            return null;
        }

        public Cell GetCellOfTower(Card card, out int horizontal, out int vertical)
        {
            TowerCard[] array;
            if (_playerTowers.Contains(card))
            {
                array = _playerTowers;
                vertical = PlayerLine;
            }
            else if (_enemyTowers.Contains(card))
            {
                array = _enemyTowers;
                vertical = EnemyLine;
            }
            else
            {
                horizontal = default;
                vertical = default;
                return default;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == card)
                {
                    horizontal = i;
                    return Grid[horizontal, vertical];
                }
            }

            horizontal = default;
            return default;
        }

        public void FindCellCoordinates(Cell cell, out int horizontal, out int vertical)
        {
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                for (int y = 0; y < _grid.GetLength(1); y++)
                {
                    if (_grid[x, y] == cell)
                    {
                        horizontal = y;
                        vertical = x;
                        return;
                    }
                }
            }

            horizontal = -1;
            vertical = -1;
            return;
        }

        #endregion

        #region Checks

        public bool LineCanAcceptCard(Line line, UnitCard card) 
            => !line.Cards.Contains(card) && line.Cards.Count < _cardsLimitInLine;

        public bool CellHaveEnemies(Player invoker, Cell cell) 
            => cell.GetLine(invoker.Type == PositionType.Top ? PositionType.Bottom : PositionType.Top).Cards.Count != 0;

        public bool HasWrongPosition(Player invoker, int startHorizontal, int startVertical, int endHorizontal, int endVertical) 
            => startVertical == endVertical || startVertical != endVertical + (invoker.Type == PositionType.Top ? _requiredVerticalOffset : -_requiredVerticalOffset)  || Mathf.Abs(endHorizontal - startHorizontal) > _maximumHorizontalOffset;

        public bool HasWrongPosition(Player invoker, Cell startCell, Cell endCell)
        {
            FindCellCoordinates(startCell, out int startHorizontal, out int startVertical);
            FindCellCoordinates(endCell, out int endHorizontal, out int endVertical);
            return HasWrongPosition(invoker, startHorizontal, startVertical, endHorizontal, endVertical);
        }

        public bool CardTypesEquals(Card card1, Card card2) 
            => card1.Player.Type == card2.Player.Type;

        #endregion
    }

    #region Enums

    public enum PlayerState
    {
        Attack,
        Defend,
        Win,
        Lose
    }

    public enum PositionType
    {
        Top,
        Bottom
    }

    public enum Direction
    {
        Top,
        Right,
        Bottom,
        Left
    }

    public enum CardRarity
    {
        Common,
        Rare,
        Legendary
    }

    #endregion
}
