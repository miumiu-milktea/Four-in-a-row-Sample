using Four_in_a_row.Models.Cpu;
using System.Collections.Generic;

namespace Four_in_a_row.Models.Board
{
    public interface ISimpleBoard
    {
        GameSetting Setting { get; }

        int RowSize { get; }
        int ColSize { get; }

        GameUser CurrentTurn { get; }
        int TurnCount { get; }

        GameUser? this[BoardIndex index] { get; }

        bool ExistsAvailable { get; }

        bool ContainsIndex(BoardIndex index);
        BoardIndex GetConnectIndex();

        CellStatus GetCellStatus(BoardIndex index);

        IEnumerable<BoardIndex> CreateEnumerableIndex();

        IEnumerable<StoneMove> GetCandidateStoneMove();

        GameUser?[,] CreateBoardArrayCopy();

        SimpleBoard CreateCopy();
        SimpleBoard CreatePutStoneBoard(StoneMove move);
    }
}