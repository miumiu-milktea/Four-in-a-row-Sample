using Four_in_a_row.Models;
using Four_in_a_row.Models.Board;
using Four_in_a_row.Models.Cpu;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Four_in_a_row.Cpu.Estimator.Common
{
    internal class EstimateMoveNode : IEstimateMoveNode
    {
        private readonly object _lock = new object();

        public int Depth { get; private set; }

        public EstimateMoveNode Parent { get; }
        public EstimateMoveNode Root => Parent?.Root ?? this;
        public bool IsRoot => null == Parent;


        public GameUser Turn => Move?.Turn ?? RawBoard?.CurrentTurn.GetPrevUser() ?? throw new InvalidProgramException("state is invalid.");
        public bool IsAbsoluteNextTurnMine => Turn == Root.Turn;

        public StoneMove Move { get; }

        /// <summary>
        /// Nodeの保持する生のBoard
        /// </summary>
        public ISimpleBoard RawBoard { get; private set; }

        public GameResult? Result { get; private set; }
        public bool Finished => Result.HasValue;

        public BoardScore AtomicScore { get; set; }
        public BoardScore ExpectedScore { get; set; }

        public ISimpleBoard Board
        {
            get
            {
                if (null == RawBoard)
                {
                    RawBoard = Parent?.Board.CreatePutStoneBoard(this.Move) ?? throw new InvalidProgramException("state is invalid.");
                }
                return RawBoard;
            }
        }

        private List<EstimateMoveNode> _Children { get; }
        public IReadOnlyList<EstimateMoveNode> Children => _Children;

        public EstimateMoveNode(ISimpleBoard currentBoard)
        {
            if (null == currentBoard) throw new ArgumentNullException("board is null.");

            this.Depth = 0;
            this.Parent = null;
            this.RawBoard = currentBoard;
            this._Children = new List<EstimateMoveNode>();

            InitializeState();
        }

        private EstimateMoveNode(EstimateMoveNode parent, StoneMove move)
        {
            if (null == parent) throw new ArgumentNullException("parent is null.");
            if (null == move) throw new ArgumentNullException("move is null.");

            this.Parent = parent;
            this.Depth = Parent.Depth + 1;
            this.Move = move;
            this._Children = new List<EstimateMoveNode>();

            InitializeState();
        }

        private void InitializeState()
        {
            var board = Board;
            var connectIndex = board.GetConnectIndex();
            if (null != connectIndex)
            {
                Result = board[connectIndex].Value.ToResult();
            }
        }

        public EstimateMoveNode AddChild(StoneMove move)
        {
            var child = new EstimateMoveNode(this, move);
            lock (_lock)
            {
                this._Children.Add(child);
            }

            return child;
        }

        public void ReleaseData()
        {
            if (!IsRoot)
            {
                this.RawBoard = null;
            }
            foreach (var child in Children)
            {
                child.ReleaseData();
            }
        }

        [ConditionalAttribute("DEBUG")]
        public void DebugOutputEstimateInternal(int? maxDepth = null)
        {
            if (maxDepth.HasValue && this.Depth > maxDepth) return;

            Console.WriteLine($"{new string('　', Depth)}{(Turn == GameUser.First ? '●' : '〇')} ({Move?.Index.RowIndex},{Move?.Index.ColIndex}) {ExpectedScore?.ToString()} diff:{ExpectedScore.GetScore(GameUser.First) - (Parent?.ExpectedScore.GetScore(GameUser.First) ?? 0)} {(Finished ? Result.ToString() : string.Empty)}");
            foreach (var c in _Children)
            {
                c.DebugOutputEstimateInternal(maxDepth);
            }
        }
    }
}
