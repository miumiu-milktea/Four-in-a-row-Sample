using Four_in_a_row.Models.Board;
using Four_in_a_row.Models.Cpu;
using System;
using System.Linq;

namespace Four_in_a_row.Models
{
    public class Game
    {
        public GameSetting Setting { get; private set; }
        public GameStatus Status { get; private set; }

        private GameBoard _board;
        public IGameBoard Board => _board;
        public GameUser CurrentTurn => Board.SimpleBoard.CurrentTurn;

        public int Count => Board.SimpleBoard.TurnCount;

        public bool IsFirstCpu { get; private set; }
        public bool IsSecondCpu { get; private set; }

        public bool IsUserTurn => (!IsFirstCpu && CurrentTurn == GameUser.First) || (!IsSecondCpu && CurrentTurn == GameUser.Second);

        private IMoveEstimator moveEvaluator;

        public Game(GameSetting setting)
        {
            this.Setting = setting;
        }

        public void SetCpu(IMoveEstimator moveEvaluator, GameUser? user)
        {
            if (this.Status != GameStatus.Setting) return;

            this.moveEvaluator = moveEvaluator;
            switch (user)
            {
                case GameUser.First:
                    IsFirstCpu = true;
                    IsSecondCpu = false;
                    break;
                case GameUser.Second:
                    IsFirstCpu = false;
                    IsSecondCpu = true;
                    break;
                default:
                    IsFirstCpu = false;
                    IsSecondCpu = false;
                    break;
            }
        }

        public void GameStart()
        {
            Status = GameStatus.InGame;
            _board = new GameBoard(Setting);
        }

        public void AddUserStone(BoardIndex index)
        {
            if (!Status.IsInGame() || !IsUserTurn) return;
            AddStone(index);
        }

        public StoneMove AddCpuStone(GameUser turn)
        {
            if (!Status.IsInGame() || IsUserTurn) return null;

            var move = this.moveEvaluator.EstimateStoneMove(this.Board.SimpleBoard).FirstOrDefault();
            AddStone(move.Index);

            return move;
        }

        private void AddStone(BoardIndex index)
        {
            if (!Status.IsInGame()) return;
            if (CellStatus.Available != Board[index]) return;

            _board.AddStone(index);

            if (Board.Finished)
            {
                Status = GameStatus.GameSet;
                return;
            }
            _board.ChangeTurn();
        }
    }

    public enum GameStatus
    {
        Setting,
        InGame,
        GameSet,
    }

    public static class GameStatusExtensions
    {
        public static bool IsSetting(this GameStatus? status) => status?.IsSetting() ?? true;
        public static bool IsInGame(this GameStatus? status) => status?.IsInGame() ?? false;
        public static bool IsGameSet(this GameStatus? status) => status?.IsGameSet() ?? false;

        public static bool IsSetting(this GameStatus status) => GameStatus.Setting == status;
        public static bool IsInGame(this GameStatus status) => GameStatus.InGame == status;
        public static bool IsGameSet(this GameStatus status) => GameStatus.GameSet == status;
    }
}
