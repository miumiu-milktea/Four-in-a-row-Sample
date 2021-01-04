using Four_in_a_row.Cpu.Estimator.Common;
using Four_in_a_row.Models.Board;

namespace Four_in_a_row.Cpu.Estimator.Evaluator
{
    public interface IBoardEvaluator
    {
        BoardScore Evaluate(ISimpleBoard board);
    }
}