using Four_in_a_row.Models.Board;
using Four_in_a_row.Models.Cpu;
using System.Collections.Generic;

namespace Four_in_a_row.Cpu.Estimator.MoveNominator
{
    public interface IMoveNominator
    {
        IEnumerable<StoneMove> GetCandidateStoneMove(ISimpleBoard board);
    }
}
