using Four_in_a_row.Models.Cpu;
using System;

namespace Four_in_a_row.Models.Board
{
    public enum CellStatus
    {
        OutOfOrder,
        Void,
        Available,
        First,
        Second,
    }

    public class GameBoard : IGameBoard
    {
        private readonly SimpleBoard Board;

        public GameSetting Setting { get; private set; }

        public int ColSize => Setting.ColSize;
        public int RowSize => Setting.RowSize;

        public ISimpleBoard SimpleBoard => Board;

        public GameResult? Result { get; private set; }

        public bool Finished => Result.HasValue;


        public GameBoard(GameSetting setting)
        {
            this.Setting = setting;
            Board = new SimpleBoard(setting);
        }

        public CellStatus this[BoardIndex index] => this.Board.GetCellStatus(index);

        public void AddStone(BoardIndex index)
        {
            if (CellStatus.Available != this[index]) throw new ArgumentException($"石を配置できない場所です。");

            this.Board.PutStone(new StoneMove(index, this.Board.CurrentTurn), false);

            var connectIndex = Board.GetConnectIndex();
            if (null != connectIndex)
            {
                Result = Board[connectIndex].Value.ToResult();
            }
            else
            {
                if (!Board.ExistsAvailable)
                {
                    Result = GameResult.Draw;
                }
                else
                {
                    Result = null;
                }
            }
        }

        public void ChangeTurn() => Board.ChangeTurn();
    }
}
