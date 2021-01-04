using System;

namespace Four_in_a_row.Models.Board
{
    public enum IndexDirection
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        DownLeft,
        UpRight,
        DownRight,
    }

    public class BoardIndex
    {
        public int RowIndex { get; }
        public int ColIndex { get; }

        public BoardIndex(int rowIndex, int colIndex)
        {
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
        }

        public BoardIndex UpIndex => GetIndex(IndexDirection.Up);
        public BoardIndex DownIndex => GetIndex(IndexDirection.Down);
        public BoardIndex LeftIndex => GetIndex(IndexDirection.Left);
        public BoardIndex RightIndex => GetIndex(IndexDirection.Right);
        public BoardIndex UpLeftIndex => GetIndex(IndexDirection.UpLeft);
        public BoardIndex DownLeftIndex => GetIndex(IndexDirection.DownLeft);
        public BoardIndex UpRightIndex => GetIndex(IndexDirection.UpRight);
        public BoardIndex DownRightIndex => GetIndex(IndexDirection.DownRight);

        public BoardIndex GetIndex(IndexDirection direction)
        {
            switch (direction)
            {
                case IndexDirection.Up:
                    return new BoardIndex(RowIndex - 1, ColIndex);
                case IndexDirection.Down:
                    return new BoardIndex(RowIndex + 1, ColIndex);
                case IndexDirection.Left:
                    return new BoardIndex(RowIndex, ColIndex - 1);
                case IndexDirection.Right:
                    return new BoardIndex(RowIndex, ColIndex + 1);
                case IndexDirection.UpLeft:
                    return new BoardIndex(RowIndex - 1, ColIndex - 1);
                case IndexDirection.DownLeft:
                    return new BoardIndex(RowIndex + 1, ColIndex - 1);
                case IndexDirection.UpRight:
                    return new BoardIndex(RowIndex - 1, ColIndex + 1);
                case IndexDirection.DownRight:
                    return new BoardIndex(RowIndex + 1, ColIndex + 1);
                default:
                    throw new ArgumentException("無効な方向です。");
            }
        }
    }
}
