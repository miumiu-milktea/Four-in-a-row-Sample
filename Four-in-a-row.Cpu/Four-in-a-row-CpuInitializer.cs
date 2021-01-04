using Four_in_a_row.Cpu.Estimator;
using Four_in_a_row.Cpu.Estimator.Evaluator;
using Four_in_a_row.Cpu.Estimator.MoveNominator;
using Four_in_a_row.Models;
using Four_in_a_row.Models.Cpu;

namespace Four_in_a_row.Cpu
{
    public static class Four_in_a_row_CpuInitializer
    {
        public static IMoveEstimator CreateMoveEvaluator(GameSetting settning)
        {
            return new SimpleMoveEstimator(settning, new SimpleBoardEvaluator(settning), new SimpleMoveNominator(settning), Constant.MaxEstimateDepth);
        }
    }
}
