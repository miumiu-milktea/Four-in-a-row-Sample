namespace Four_in_a_row.Models.Board
{
    public interface IGameBoard
    {
        GameSetting Setting { get; }

        int RowSize { get; }
        int ColSize { get; }

        ISimpleBoard SimpleBoard { get; }
        CellStatus this[BoardIndex index] { get; }

        bool Finished { get; }
        GameResult? Result { get; }
    }
}