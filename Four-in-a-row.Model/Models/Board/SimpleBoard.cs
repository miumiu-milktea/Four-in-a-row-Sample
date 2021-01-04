using Four_in_a_row.Models.Cpu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Four_in_a_row.Models.Board
{
    public class SimpleBoard : ISimpleBoard
    {
        public GameSetting Setting { get; }

        private GameUser?[,] Board { get; set; }

        public int RowSize => Board.GetLength(0);
        public int ColSize => Board.GetLength(1);

        public GameUser? this[BoardIndex index]
        {
            get => ContainsIndex(index)
                ? Board[index.RowIndex, index.ColIndex]
                : null;
            private set
            {
                if (ContainsIndex(index))
                    Board[index.RowIndex, index.ColIndex] = value;
            }
        }

        public CellStatus GetCellStatus(BoardIndex index)
        {
            if (!ContainsIndex(index)) return CellStatus.OutOfOrder;

            var user = this[index];
            if (!user.HasValue)
            {
                var downIndex = index.DownIndex;
                return !this.ContainsIndex(downIndex) || this[downIndex].HasValue
                    ? CellStatus.Available
                    : CellStatus.Void;
            }

            switch (user.Value)
            {
                case GameUser.First:
                    return CellStatus.First;
                case GameUser.Second:
                    return CellStatus.Second;
                default:
                    throw new ArgumentException("予期しないUserです。");
            }
        }


        public GameUser CurrentTurn { get; private set; }

        public int TurnCount => CreateEnumerableIndex().Where(idx => null != this[idx]).Count();

        public bool ExistsAvailable => CreateEnumerableIndex().Where(idx => null == this[idx]).Any();


        public SimpleBoard(GameSetting setting)
        {
            this.Setting = setting;
            this.Board = new GameUser?[setting.RowSize, setting.ColSize];
            this.CurrentTurn = GameUser.First;
        }

        public SimpleBoard(ISimpleBoard board) : this(board.Setting, board.CreateBoardArrayCopy(), board.CurrentTurn) { }

        private SimpleBoard(GameSetting setting, GameUser?[,] board, GameUser turn)
        {
            this.Setting = setting;
            this.Board = board;
            this.CurrentTurn = turn;
        }

        public bool ContainsIndex(BoardIndex index)
        {
            if (index.RowIndex < 0 || RowSize <= index.RowIndex) return false;
            if (index.ColIndex < 0 || ColSize <= index.ColIndex) return false;

            return true;
        }

        public GameUser?[,] CreateBoardArrayCopy() => this.Board.Clone() as GameUser?[,];

        public SimpleBoard CreateCopy()
        {
            return new SimpleBoard(Setting, CreateBoardArrayCopy(), this.CurrentTurn);
        }

        public IEnumerable<BoardIndex> CreateEnumerableIndex()
        {
            return Enumerable.Range(0, RowSize)
                    .SelectMany(row => Enumerable.Range(0, ColSize).Select(col => new BoardIndex(row, col)));
        }

        public IEnumerable<StoneMove> GetCandidateStoneMove()
        {
            return Enumerable.Range(0, ColSize)
                .Select(col =>
                {
                    return Enumerable.Range(0, RowSize)
                        .Reverse()
                        .Select(row => new BoardIndex(row, col))
                        .FirstOrDefault(idx => null == this[idx]);
                })
                .Where(idx => null != idx)
                .Select(idx => new StoneMove(idx, CurrentTurn));
        }

        public BoardIndex GetConnectIndex()
        {
            foreach (var index in CreateEnumerableIndex())
            {
                if (CheckCellConnect(index))
                {
                    return index;
                }
            }
            return null;
        }

        private bool CheckCellConnect(BoardIndex index)
        {
            var baseStatus = this[index];
            if (!baseStatus.HasValue)
            {
                return false;
            }

            foreach (var direction in new IndexDirection[] { IndexDirection.Down, IndexDirection.Right, IndexDirection.DownLeft, IndexDirection.DownRight })
            {
                var tmpIndex = index;
                var isConnect = true;
                for (var ii = 1; ii < Setting.ConnectCount; ii++)
                {
                    tmpIndex = tmpIndex.GetIndex(direction);
                    if (baseStatus != this[tmpIndex])
                    {
                        isConnect = false;
                        break;
                    }
                }

                if (isConnect)
                {
                    return true;
                }
            }
            return false;
        }

        public void ChangeTurn()
        {
            CurrentTurn = CurrentTurn.GetNextUser();
        }

        public SimpleBoard CreatePutStoneBoard(StoneMove move)
        {
            var board = CreateCopy();
            board.PutStone(move);

            return board;
        }

        public void PutStone(StoneMove move, bool turnChange = true)
        {
            if (null == move) throw new ArgumentNullException("move is null.");
            if (move.Turn != CurrentTurn) throw new ArgumentException("turn is invalid.");

            this[move.Index] = move.Turn;

            if (turnChange)
            {
                ChangeTurn();
            }
        }
    }
}
