using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Roguelike
{
    using Chess.Core;
    using Chess.Rules;
    using Chess.AI;
    using Chess.Roguelike.Core;
    using Chess.Roguelike.Progression;
    using Chess.Roguelike.Economy;
    using Chess.Roguelike.Gambits;
    using Chess.Roguelike.Scrolls;
    using Chess.Roguelike.Army;

    /// <summary>
    /// Game phases in the roguelike loop
    /// </summary>
    public enum GamePhase
    {
        MainMenu,
        Shop,
        Setup,      // Army arrangement before trial
        Trial,      // Active chess gameplay
        Reward,     // After winning a trial
        GameOver    // Run ended (win or loss)
    }

    /// <summary>
    /// Main manager for the Chess Balatro roguelike game loop.
    /// Coordinates all systems: progression, economy, trials, gambits, scrolls.
    /// </summary>
    public class RoguelikeGameManager : MonoBehaviour
    {
        public static RoguelikeGameManager Instance { get; private set; }

        public event Action<GamePhase> OnPhaseChanged;
        public event Action<WinConditionChecker.TrialResult> OnTrialEnded;
        public event Action OnRunStarted;
        public event Action<bool> OnRunEnded; // true = victory, false = defeat
        public event Action<Move, Color> OnMoveExecuted; // Fired after any move (player or AI)

        [Header("Configuration")]
        [SerializeField] private int startingMoney = 10;
        [SerializeField] private int aiDifficulty = 3;

        [Header("Win Conditions")]
        [SerializeField] private bool enableCheckmate = true;
        [SerializeField] private bool enableElimination = true;
        [SerializeField] private bool enableValueDominance = true;
        [SerializeField] private int valueDominanceThreshold = 13;
        [SerializeField] private int valueDominanceRounds = 2;

        [Header("Databases (Assign in Inspector)")]
        [SerializeField] private List<Gambit> allGambits = new();
        [SerializeField] private List<Scroll> allScrolls = new();

        // Core state
        private RunState runState;
        private GamePhase currentPhase = GamePhase.MainMenu;

        // Managers
        private GambitManager gambitManager;
        private ScrollManager scrollManager;
        private Shop shop;
        private BoardSetup boardSetup;
        private WinConditionChecker winChecker;

        // Trial state
        private Board trialBoard;
        private ChessRules trialRules;
        private ChessAI trialAI;
        private Color playerColor = Color.White;
        private Color currentTurn = Color.White;
        private List<PieceInstance> currentEnemyArmy;
        private int turnCount = 0;

        // Properties
        public RunState RunState => runState;
        public GamePhase CurrentPhase => currentPhase;
        public GambitManager Gambits => gambitManager;
        public ScrollManager Scrolls => scrollManager;
        public Shop Shop => shop;
        public BoardSetup BoardSetup => boardSetup;
        public Board TrialBoard => trialBoard;
        public ChessRules TrialRules => trialRules;
        public Color PlayerColor => playerColor;
        public Color CurrentTurn => currentTurn;
        public bool IsPlayerTurn => currentTurn == playerColor;
        public int TurnCount => turnCount;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSystems();
        }

        private void InitializeSystems()
        {
            runState = new RunState();
            gambitManager = new GambitManager();
            scrollManager = new ScrollManager();
            shop = new Shop(runState, gambitManager, scrollManager);
            boardSetup = new BoardSetup(runState);
            winChecker = new WinConditionChecker(
                valueDominanceThreshold, 
                valueDominanceRounds,
                enableCheckmate,
                enableElimination,
                enableValueDominance
            );

            // Connect economy hooks to gambits
            gambitManager.SetEconomyHooks(
                amount => runState.AddMoney(amount),
                () => runState.Money
            );

            gambitManager.SetBoardHooks(
                size => runState.SetBoardSize(size),
                () => runState.BoardSize
            );

            // Set available items in shop
            shop.SetAvailableGambits(allGambits);
            shop.SetAvailableScrolls(allScrolls);
        }

        #region Run Management

        /// <summary>
        /// Start a new roguelike run
        /// </summary>
        public void StartNewRun()
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("  STARTING NEW RUN - CHESS BALATRO");
            Debug.Log("═══════════════════════════════════════");

            runState.InitializeNewRun();
            runState.SetMoney(startingMoney);
            
            gambitManager.ClearAllGambits();
            
            winChecker.Reset();

            OnRunStarted?.Invoke();
            
            // Start at shop phase
            EnterShopPhase();
        }

        /// <summary>
        /// End the current run
        /// </summary>
        public void EndRun(bool victory)
        {
            Debug.Log($"═══════════════════════════════════════");
            Debug.Log($"  RUN ENDED - {(victory ? "VICTORY!" : "DEFEAT")}");
            Debug.Log($"  Reached: Cycle {runState.CurrentCycle}, Trial {runState.CurrentTrial}");
            Debug.Log($"═══════════════════════════════════════");

            SetPhase(GamePhase.GameOver);
            OnRunEnded?.Invoke(victory);
        }

        #endregion

        #region Phase Management

        private void SetPhase(GamePhase phase)
        {
            currentPhase = phase;
            Debug.Log($"[Phase] → {phase}");
            OnPhaseChanged?.Invoke(phase);
        }

        public void EnterShopPhase()
        {
            SetPhase(GamePhase.Shop);
            shop.RefreshShop();
            runState.ResetShopRerolls();

            // Update scroll context
            scrollManager.UpdateContext(null, runState, playerColor, false, true, false);
        }

        public void EnterSetupPhase()
        {
            SetPhase(GamePhase.Setup);
            
            boardSetup.InitializeSetup();
            boardSetup.PlaceDefaultFormation();

            // Generate and place enemy army
            currentEnemyArmy = boardSetup.PlaceEnemyArmy(runState.CurrentCycle, runState.CurrentTrial);

            // Update scroll context
            scrollManager.UpdateContext(boardSetup.Board, runState, playerColor, false, false, true);
        }

        public void StartTrial()
        {
            if (!boardSetup.ValidateSetup())
            {
                Debug.LogError("[Trial] Setup validation failed!");
                return;
            }

            SetPhase(GamePhase.Trial);

            // Get the prepared board
            trialBoard = boardSetup.GetFinalBoard();
            trialRules = new ChessRules(trialBoard);
            trialAI = new ChessAI(trialBoard, aiDifficulty);

            // Reset trial state
            currentTurn = Color.White; // Player (white) always goes first
            turnCount = 0;
            winChecker.Reset();

            // Update contexts
            gambitManager.UpdateContext(trialBoard, playerColor, currentTurn);
            scrollManager.UpdateContext(trialBoard, runState, playerColor, true, false, false);

            // Trigger trial start gambits
            gambitManager.TriggerOnTrialStart();

            Debug.Log($"═══════════════════════════════════════");
            Debug.Log($"  TRIAL START - Cycle {runState.CurrentCycle}, Trial {runState.CurrentTrial}");
            Debug.Log($"  Player army value: {DifficultyScaler.CalculateBoardValue(trialBoard, playerColor)}");
            Debug.Log($"  Enemy army value: {DifficultyScaler.CalculateBoardValue(trialBoard, playerColor.Opposite())}");
            Debug.Log($"═══════════════════════════════════════");
        }

        #endregion

        #region Trial Gameplay

        /// <summary>
        /// Execute a move during a trial
        /// </summary>
        public bool ExecuteTrialMove(Move move)
        {
            if (currentPhase != GamePhase.Trial)
                return false;

            if (currentTurn != playerColor)
            {
                Debug.LogWarning("[Trial] Not player's turn!");
                return false;
            }

            // Validate move - find matching legal move
            var legalMoves = trialRules.GetLegalMoves(playerColor);
            
            // Find exact match including promotion piece
            var actualMove = legalMoves.Find(m => 
                m.From == move.From && 
                m.To == move.To && 
                m.PromotionPiece == move.PromotionPiece);
            
            // If no exact match, try to find any matching move (for basic moves)
            if (actualMove == null)
            {
                actualMove = legalMoves.Find(m => m.From == move.From && m.To == move.To);
            }
            
            if (actualMove == null)
            {
                Debug.LogWarning($"[Trial] Illegal move: {move.From} → {move.To}");
                return false;
            }
            
            ExecuteMoveInternal(actualMove, playerColor);
            return true;
        }

        private void ExecuteMoveInternal(Move move, Color color)
        {
            // Track capture before executing
            var capturedPiece = trialBoard.GetPiece(move.To);
            var movingPiece = trialBoard.GetPiece(move.From);

            // Execute the move
            trialRules.ExecuteMove(move, color);

            // Notify listeners that a move was executed
            OnMoveExecuted?.Invoke(move, color);

            // Trigger gambit events
            if (capturedPiece != null)
            {
                gambitManager.TriggerOnPieceCaptured(move, capturedPiece, movingPiece, move.To);
            }

            if (move.PromotionPiece != PieceType.None)
            {
                gambitManager.TriggerOnPromotion(move.PromotionPiece);
            }

            // Check for check
            Color opponent = color.Opposite();
            if (trialRules.IsInCheck(trialBoard, opponent))
            {
                if (color == playerColor)
                    gambitManager.TriggerOnCheckGiven();
                else
                    gambitManager.TriggerOnCheckReceived();
            }

            // End turn
            gambitManager.TriggerOnTurnEnd();

            // Check win conditions
            bool afterPlayerMove = color == playerColor;
            var result = winChecker.CheckWinConditions(trialBoard, trialRules, playerColor, afterPlayerMove);

            if (result != WinConditionChecker.TrialResult.Ongoing)
            {
                EndTrial(result);
                return;
            }

            // Switch turns
            currentTurn = opponent;
            gambitManager.UpdateContext(trialBoard, playerColor, currentTurn);
            gambitManager.TriggerOnTurnStart();

            if (color == playerColor)
            {
                turnCount++;
            }

            // If AI's turn, trigger AI move
            if (currentTurn != playerColor && currentPhase == GamePhase.Trial)
            {
                StartCoroutine(ExecuteAITurn());
            }
        }

        private IEnumerator ExecuteAITurn()
        {
            yield return new WaitForSeconds(0.3f);

            var aiMove = trialAI.FindBestMove(playerColor.Opposite());
            
            if (aiMove != null)
            {
                ExecuteMoveInternal(aiMove, playerColor.Opposite());
            }
            else
            {
                Debug.LogError("[Trial] AI returned null move!");
                // Check if it's checkmate or stalemate
                var result = winChecker.CheckWinConditions(trialBoard, trialRules, playerColor, false);
                if (result != WinConditionChecker.TrialResult.Ongoing)
                {
                    EndTrial(result);
                }
            }
        }

        private void EndTrial(WinConditionChecker.TrialResult result)
        {
            bool playerWon = result == WinConditionChecker.TrialResult.PlayerWin_Checkmate ||
                            result == WinConditionChecker.TrialResult.PlayerWin_Elimination ||
                            result == WinConditionChecker.TrialResult.PlayerWin_ValueDominance;

            Debug.Log($"═══════════════════════════════════════");
            Debug.Log($"  TRIAL ENDED - {result}");
            Debug.Log($"═══════════════════════════════════════");

            // Trigger gambit events
            gambitManager.TriggerOnTrialEnd(playerWon);

            OnTrialEnded?.Invoke(result);

            if (playerWon)
            {
                EnterRewardPhase();
            }
            else
            {
                // For now, loss ends the run
                // Could implement lives/continues later
                EndRun(false);
            }
        }

        #endregion

        #region Rewards

        private void EnterRewardPhase()
        {
            SetPhase(GamePhase.Reward);

            // Calculate reward
            bool flawless = true; // TODO: Track if any pieces were lost
            int reward = DifficultyScaler.GetTrialReward(
                runState.CurrentCycle, 
                runState.CurrentTrial, 
                flawless
            );

            runState.AddMoney(reward);
            Debug.Log($"[Reward] Earned ${reward}!");

            // Auto-advance to next trial after short delay
            // In real game, this would show reward screen
        }

        /// <summary>
        /// Call this to proceed after reward phase
        /// </summary>
        public void ProceedFromReward()
        {
            // Advance progression
            runState.AdvanceToNextTrial();

            // Check for victory (completed all cycles)
            // For now, let's say winning cycle 8 trial 3 is victory
            if (runState.CurrentCycle > 8)
            {
                EndRun(true);
                return;
            }

            // Back to shop
            EnterShopPhase();
        }

        #endregion

        #region Shop Actions

        public void FinishShopping()
        {
            if (currentPhase != GamePhase.Shop)
                return;

            EnterSetupPhase();
        }

        #endregion

        #region Debug

        public void DebugPrintState()
        {
            Debug.Log($"═══════════════════════════════════════");
            Debug.Log($"  RUN STATE");
            Debug.Log($"  Cycle: {runState.CurrentCycle}, Trial: {runState.CurrentTrial}");
            Debug.Log($"  Money: ${runState.Money}");
            Debug.Log($"  Army: {runState.OwnedPieces.Count} pieces, value: {runState.GetTotalArmyValue()}");
            Debug.Log($"  Gambits: {gambitManager.Count}/{runState.MaxGambits}");
            Debug.Log($"  Scrolls: {scrollManager.Count}/{runState.MaxScrolls}");
            Debug.Log($"  Phase: {currentPhase}");
            Debug.Log($"═══════════════════════════════════════");
        }

        #endregion
    }
}
