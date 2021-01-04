using Four_in_a_row.Models.Board;
using System.Collections.Generic;

namespace Four_in_a_row.Models.Cpu
{
    public interface IMoveEstimator
    {
        IList<StoneMove> EstimateStoneMove(ISimpleBoard board, int fetchCount = 1);
    }
}
