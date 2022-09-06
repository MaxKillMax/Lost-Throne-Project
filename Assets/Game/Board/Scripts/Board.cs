using System;
using System.Linq;
using UnityEngine;
using LostThrone.OpenWorld;
using Cinemachine;

namespace LostThrone.Board
{
    public class Board : MonoBehaviour
    {
        public event Action<PositionType> OnTurnChanged;

        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private GameObject _playerTurnUI;
        [SerializeField] private Cell[] _cells;

        public CinemachineVirtualCamera virtualCamera => _virtualCamera;

        [SerializeField] private Player _player;
        [SerializeField] private Player _enemy;

        private BattleData _playerData;
        private BattleData _enemyData;

        [SerializeField] private TowerCard[] _playerTowers;
        [SerializeField] private TowerCard[] _enemyTowers;

        public TowerCard[] PlayerTowers => _playerTowers;
        public TowerCard[] EnemyTowers => _enemyTowers;

        [SerializeField] private int _turnPoints = 5;
        [SerializeField] private int _cardsLimitInLine = 3;
        [SerializeField] private int _requiredVerticalOffset = 1;
        [SerializeField] private int _maximumHorizontalOffset = 1;
        [SerializeField] private float _refreshLinesTime = 0.2f;
        public int TurnPoints => _turnPoints;
        public int CardsLimitInLine => _cardsLimitInLine;
        public int RequiredVerticalOffset => _requiredVerticalOffset;
        public int MaximumHorizontalOffset => _maximumHorizontalOffset;
        public float RefreshLinesTime => _refreshLinesTime;

        private Cell[,] _grid;

        public Cell[] Cells => _cells;
        public Cell[,] Grid => _grid;

        private PositionType _currentTurn;
        public PositionType CurrentTurn => _currentTurn;

        public int EnemyLine => _grid.GetLength(1) - 1;
        public int PlayerLine => 0;

        public bool GameInProgress { get; private set; }

        private void Start()
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
            Services.GetService<BoardBase>().SetCameraTarget(_virtualCamera, Grid[0, 1]);

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
