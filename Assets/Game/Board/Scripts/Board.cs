using System;
using System.Collections.Generic;
using UnityEngine;
using LostThrone.OpenWorld;
using Cinemachine;
using NaughtyAttributes;

namespace LostThrone.Board
{
    public class Board : MonoBehaviour
    {
        #region Fields

        public event Action<PositionType> OnPlayerTurnChanged;
        public event Action<BoardState> OnGameStateChanged;

        private BoardBase _base;
        public BoardBase Base => _base;

        [SerializeField, Foldout("Grid")] 
        private Cell[] _cells;
        [SerializeField, Foldout("Grid")] 
        private TowerCard[] _playerTowers;
        [SerializeField, Foldout("Grid")] 
        private TowerCard[] _enemyTowers;
        [SerializeField, Foldout("Grid")] 
        private CinemachineVirtualCamera _virtualCamera;
        private Cell[,] _grid;

        public Cell[] Cells => _cells;
        public CinemachineVirtualCamera virtualCamera => _virtualCamera;
        public Cell[,] Grid => _grid;

        [SerializeField, Foldout("Players")] 
        private BoardPlayer _player;
        [SerializeField, Foldout("Players")] 
        private BoardPlayer _enemy;
        [SerializeField, Foldout("Players")] 
        private UnitCard _cardPrefab;

        [SerializeField, Foldout("Parameters")] 
        private int _turnPoints = 5;
        [SerializeField, Foldout("Parameters")] 
        private int _cardsLimitInLine = 3;
        [SerializeField, Foldout("Parameters")] 
        private int _requiredVerticalOffset = 1;
        [SerializeField, Foldout("Parameters")] 
        private int _maximumHorizontalOffset = 1;
        [SerializeField, Foldout("Parameters")] 
        private float _refreshLinesTime = 0.2f;

        public int TurnPoints => _turnPoints;
        public int CardsLimitInLine => _cardsLimitInLine;
        public int RequiredVerticalOffset => _requiredVerticalOffset;
        public int MaximumHorizontalOffset => _maximumHorizontalOffset;
        public float RefreshLinesTime => _refreshLinesTime;

        private BoardState _gameState = BoardState.Not;
        public BoardState GameState => _gameState;

        private PositionType _turnPosition = PositionType.Nothing;
        public PositionType TurnPosition => _turnPosition;

        public int EnemyLine => _grid.GetLength(1) - 1;
        public PositionType EnemyPositionType => PositionType.Top;
        public int PlayerLine => 0;
        public PositionType PlayerPositionType => PositionType.Bottom;
        public int StandardTowersCount => 3;

        #endregion

        #region Initialization

        private void Awake()
        {
            _grid = new Cell[,]
            {
                { _cells[0], _cells[1], _cells[2] },
                { _cells[3], _cells[4], _cells[5] },
                { _cells[6], _cells[7], _cells[8] }
            };
        }

        private void Start()
        {
            _base = Services.GetService<BoardBase>();
        }

        private void InititializePlayer(BoardPlayer player, BattleData battleData, TowerCard[] towersArray, int towerHorizontalPosition)
        {
            List<TowerCard> towers = new List<TowerCard>(towersArray.Length);

            for (int i = 0; i < towersArray.Length; i++)
            {
                towersArray[i].InitializeTower(this, _player, _grid[towerHorizontalPosition, i], battleData.Tower);
                towers.Add(towersArray[i]);
            }

            List<UnitCard> playerCards = InitializeUnits(battleData.Units, player);
            player.StartBattle(battleData, playerCards, towers);
        }

        private List<UnitCard> InitializeUnits(List<Unit> units, BoardPlayer player)
        {
            List<UnitCard> cards = new List<UnitCard>(units.Count);

            for (int i = 0; i < units.Count; i++)
                cards.Add(InitializeUnit(units[i], player));

            return cards;
        }

        private UnitCard InitializeUnit(Unit unitData, BoardPlayer player)
        {
            UnitCard card = Instantiate(_cardPrefab, player.Hand.Parent);
            card.InitializeCard(this, player, unitData);
            player.Hand.AddCard(card);
            return card;
        }

        public void StartBattle(bool playerIsFirst, BattleData playerData, BattleData enemyData)
        {
            _base.SetCameraTarget(_virtualCamera, Grid[0, 1]);

            InititializePlayer(_player, playerData, _playerTowers, PlayerLine);
            InititializePlayer(_enemy, enemyData, _enemyTowers, EnemyLine);

            SetBattleState(BoardState.InProcess);
            SetTurnPosition(playerIsFirst ? PlayerPositionType : EnemyPositionType);
        }

        #endregion

        #region Ending

        public void EndBattle()
        {
            if (!_base.PlayerLost(this, _player) && !_base.PlayerLost(this, _enemy))
                return;

            EndPlayer(_player);
            EndPlayer(_enemy);

            SetNothingTurn();
            RefreshPlayerStates(PlayerState.Win, PlayerState.Lose);
            SetBattleState(BoardState.Not);
        }

        private void EndPlayer(BoardPlayer player)
        {
            for (int i = 0; i < player.Cards.Count; i++)
                Destroy(player.Cards[i].gameObject);

            player.EndBattle();
        }

        private void SetNothingTurn()
        {
            _turnPosition = PositionType.Nothing;
        }

        #endregion

        #region Turn positions

        public void SwitchTurn()
        {
            PositionType nextTurn = _turnPosition == PlayerPositionType ? EnemyPositionType : _turnPosition == EnemyPositionType ? PlayerPositionType : PositionType.Nothing;
            SetTurnPosition(nextTurn);
        }

        private void SetTurnPosition(PositionType type)
        {
            _turnPosition = type;
            RefreshPlayerStates(PlayerState.Attack, PlayerState.Defend);
            OnPlayerTurnChanged?.Invoke(_turnPosition);
        }

        private void SetBattleState(BoardState state)
        {
            _gameState = state;
            OnGameStateChanged?.Invoke(_gameState);
        }

        #endregion

        #region Player states

        private void RefreshPlayerStates(PlayerState currentState, PlayerState lastState)
        {
            BoardPlayer currentPlayer = default;
            BoardPlayer lastPlayer = default;

            if (_turnPosition == EnemyPositionType)
            {
                currentPlayer = _enemy;
                lastPlayer = _player;
            }
            else if (_turnPosition == PlayerPositionType)
            {
                currentPlayer = _player;
                lastPlayer = _enemy;
            }

            currentPlayer.SetState(currentState);
            lastPlayer.SetState(lastState);

            RefreshPlayers();
        }

        private void RefreshPlayers()
        {
            _player.RefreshCardsAndTurns(_turnPoints);
            _enemy.RefreshCardsAndTurns(_turnPoints);
        }

        #endregion
    }

    #region Enums

    public enum BoardState
    {
        InProcess,
        InVictory,
        Not
    }

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
        Bottom,
        Nothing
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
