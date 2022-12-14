using System;
using System.Collections.Generic;
using LostThrone.OpenWorld;
using NaughtyAttributes;
using UnityEngine;

namespace LostThrone.Board
{
    public class Board : MonoBehaviour
    {
        #region Fields

        public event Action<PositionType> OnPlayerTurnChanged;
        public event Action<BoardState> OnGameStateChanged;

        private BoardBase _base;
        public BoardBase Base => _base;

        [SerializeField]
        private Cell[] _cells;
        private Cell[,] _grid;

        public Cell[] Cells => _cells;
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

        [SerializeField, Foldout("Towers")]
        private TowerInput _towerInput;
        [SerializeField, Foldout("Towers")]
        private TowerCard[] _playerTowers;
        [SerializeField, Foldout("Towers")]
        private TowerCard[] _enemyTowers;

        [SerializeField, Foldout("Components")]
        private BoardCamera _boardCamera;

        public int TurnPoints => _turnPoints;
        public int CardsLimitInLine => _cardsLimitInLine;
        public int RequiredVerticalOffset => _requiredVerticalOffset;
        public int MaximumHorizontalOffset => _maximumHorizontalOffset;
        public float RefreshLinesTime => _refreshLinesTime;

        private BoardState _gameState = BoardState.Not;
        public BoardState GameState => _gameState;

        private PositionType _nextTurn = PositionType.Nothing;

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

            _towerInput.OnTowersAttacked += OnTowersAttacked;
        }

        private void Start()
        {
            _base = Services.GetService<BoardBase>();
            _base.InitializeBoard(this);
            _boardCamera.InitializeGrid(_grid);
        }

        private void OnDestroy()
        {
            UnSubscribeAll();
            _towerInput.OnTowersAttacked -= OnTowersAttacked;
        }

        private void UnSubscribeAll()
        {
            UnSubscribeUnits(_player);
            UnSubscribeUnits(_enemy);
            UnSubscribeTowers(_player.Towers);
            UnSubscribeTowers(_enemy.Towers);
        }

        private void UnSubscribeUnits(BoardPlayer player)
        {
            for (int i = 0; i < player.Cards.Count; i++)
                player.Cards[i].OnCardDestroyed -= UnlistenCard;
        }

        private void UnSubscribeTowers(List<TowerCard> towers)
        {
            for (int i = 0; i < towers.Count; i++)
                towers[i].OnCardDestroyed -= UnlistenCard;
        }

        private void InititializePlayer(BoardPlayer player, BattleData battleData, TowerCard[] towersArray)
        {
            List<TowerCard> towers = new(towersArray.Length);

            for (int i = 0; i < towersArray.Length; i++)
            {
                towersArray[i].InitializeTowerCard(player, battleData.Tower);
                towersArray[i].OnCardDestroyed += UnlistenCard;
                towers.Add(towersArray[i]);
            }

            List<UnitCard> playerCards = InitializeUnits(battleData.Units, player);
            player.StartBattle(battleData, playerCards, towers);

            _base.RefreshLinePositions(player.Hand);
        }

        private List<UnitCard> InitializeUnits(List<Unit> units, BoardPlayer player)
        {
            List<UnitCard> cards = new(units.Count);

            for (int i = 0; i < units.Count; i++)
                cards.Add(InitializeUnit(units[i], player));

            return cards;
        }

        private UnitCard InitializeUnit(Unit unitData, BoardPlayer player)
        {
            UnitCard card = Instantiate(_cardPrefab, player.Hand.Parent);
            card.InitializeUnitCard(this, player, unitData);
            card.OnCardDestroyed += UnlistenCard;
            player.Hand.AddCard(card);
            return card;
        }

        public void StartBattle(bool playerIsFirst, BattleData playerData, BattleData enemyData)
        {
            _boardCamera.SetCameraTarget(Grid[0, 1]);

            InititializePlayer(_player, playerData, _playerTowers);
            InititializePlayer(_enemy, enemyData, _enemyTowers);

            SetBattleState(BoardState.InProcess);
            SetTurnPosition(playerIsFirst ? PlayerPositionType : EnemyPositionType);
        }

        #endregion

        #region Ending

        public void UnlistenCard(Card card)
        {
            card.OnCardDestroyed -= UnlistenCard;
            TryEndBattle();
        }

        public void TryEndBattle()
        {
            if (!_base.PlayerLost(_player) && !_base.PlayerLost(_enemy))
                return;

            EndPlayer(_player);
            EndPlayer(_enemy);

            SetNothingTurn();
            RefreshPlayerStates(PlayerState.Win, PlayerState.Lose);
            SetBattleState(BoardState.Not);
            UnSubscribeAll();
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

        #region Towers

        private void ActivateTowerAttack()
        {
            BoardPlayer currentPlayer;
            BoardPlayer nextPlayer;

            if (_nextTurn == PositionType.Top)
            {
                currentPlayer = _player;
                nextPlayer = _enemy;
            }
            else
            {
                currentPlayer = _enemy;
                nextPlayer = _player;
            }

            _towerInput.InitializePlayer(currentPlayer, nextPlayer);
            _towerInput.AttackEnemies();
        }

        #endregion

        #region Turn positions

        public void SwitchTurn()
        {
            _turnPosition = PositionType.Nothing;
            ActivateTowerAttack();
        }

        private void OnTowersAttacked()
        {
            SetTurnPosition(_nextTurn);
        }

        private void SetTurnPosition(PositionType type)
        {
            _turnPosition = type;
            _nextTurn = _turnPosition == PositionType.Top ? PositionType.Bottom : PositionType.Top;
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
}
